using System.Numerics;
using System.Runtime.CompilerServices;

namespace Crawl.ECS.DataStructure;

public class FastDictionary<TKey, TValue> where TKey : IEquatable<TKey>
{
    /// <summary>
    ///     Resize when X% full. Includes tombstones.
    /// </summary>
    private const double MaxLoadFactor = 0.9;

    /// <summary>
    ///     Shrink when X% full.
    /// </summary>
    private const double MinLoadFactor = 0.25;

    /// <summary>
    ///     Max Tombstone ratio before cleanup.
    /// </summary>
    private const double MaxTombstoneRatio = 0.4; // Clean up when X% of occupied slots are tombstones

    private const int InitialCapacity = 64;

    /// <summary>
    ///     Maximum Probe Distance
    /// </summary>
    private const byte MaxProbeDistance = 255; // Max probe distance. 

    private readonly IEqualityComparer<TKey> _comparer;

    private Entry[] _buckets;
    private int _occupiedSlots;

    public FastDictionary(int initialCapacity = InitialCapacity)
    {
        _comparer = _comparer ?? EqualityComparer<TKey>.Default;
        Capacity = IsPowerOfTwo(initialCapacity) ? initialCapacity : NextPowerOfTwo(initialCapacity);
        _buckets = new Entry[Capacity];
        Count = 0;
        _occupiedSlots = 0;
    }

    public int Count { get; private set; }

    public int Capacity { get; private set; }

    public double LoadFactor => (double)_occupiedSlots / Capacity;
    public double TombstoneRatio => _occupiedSlots > 0 ? (double)(_occupiedSlots - Count) / _occupiedSlots : 0;

    public void Add(TKey key, TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));

        if (_occupiedSlots >= Capacity * MaxLoadFactor || TombstoneRatio > MaxTombstoneRatio) Resize(Capacity * 2);

        InsertInternal(key, value);
    }

    private int GetIdealIndex(TKey key)
    {
        var hash = (uint)_comparer.GetHashCode(key);
        return (int)(hash & (Capacity - 1));
    }


    public bool TryGet(TKey key, out TValue value)
    {
        if (key == null) throw new ArgumentNullException(nameof(key));
        var idealIndex = GetIdealIndex(key);

        var bucket = _buckets[idealIndex];

        if (bucket.IsLive && _comparer.Equals(bucket.Key, key))
        {
            value = bucket.Value;
            return true;
        }

        for (var probeDistance = 1; probeDistance < MaxProbeDistance; probeDistance++)
        {
            var probeIndex = (idealIndex + probeDistance) & (Capacity - 1);
            var probeBucket = _buckets[probeIndex];

            if (!probeBucket.IsOccupied)
            {
                //Item doesn't exist
                value = default!;
                return false;
            }

            switch (probeBucket.IsLive)
            {
                case true when _comparer.Equals(probeBucket.Key, key):
                    value = probeBucket.Value;
                    return true;
                case true when probeDistance > probeBucket.DistanceFromIdeal:
                    value = default!;
                    return false;
            }
        }

        value = default!;
        return false;
    }

    private void Resize(int newCapacity)
    {
        var oldBuckets = _buckets;
        var oldCount = Count;

        Capacity = NextPowerOfTwo(newCapacity);
        _buckets = new Entry[Capacity];
        Count = 0;
        _occupiedSlots = 0; // Reset both counters

        // Rehash all live entries (automatically cleans up tombstones)
        for (var i = 0; i < oldBuckets.Length; i++)
            if (oldBuckets[i].IsLive)
                InsertInternal(oldBuckets[i].Key, oldBuckets[i].Value);
    }

    /// <summary>
    ///     Utility method to find next power of 2
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int NextPowerOfTwo(int value)
    {
        if (value <= 0) return 1;
        value--;
        value |= value >> 1;
        value |= value >> 2;
        value |= value >> 4;
        value |= value >> 8;
        value |= value >> 16;
        return value + 1;
    }

    private void InsertInternal(TKey key, TValue value)
    {
        var idealIndex = GetIdealIndex(key);

        var candidateEntry = new Entry(key, value, 0);

        for (var i = 0; i < Capacity; i++)
        {
            var currentIndex = (idealIndex + i) & (Capacity - 1); // Wrap around
            ref var bucket = ref _buckets[currentIndex];

            // Available slot found (empty or tombstone) - insert here
            if (bucket.IsAvailable)
            {
                candidateEntry.DistanceFromIdeal = (byte)i;

                // Track whether we're reusing a tombstone or taking a new slot
                var reusingTombstone = bucket.IsDeleted;
                bucket = candidateEntry;

                Count++;
                if (!reusingTombstone) _occupiedSlots++; // Only increment if we're taking a new slot
                return;
            }

            // Key already exists - update value
            if (bucket.IsLive && _comparer.Equals(bucket.Key, candidateEntry.Key))
            {
                bucket.Value = candidateEntry.Value;
                return;
            }

            // Robin Hood principle: if our desired index distance is greater than the occupant's,
            // evict the occupant and take their place
            if (i > bucket.DistanceFromIdeal)
            {
                // Swap entries
                var temp = bucket;
                candidateEntry.DistanceFromIdeal = (byte)i;
                bucket = candidateEntry;
                candidateEntry = temp;

                // Continue inserting the evicted entry
                i = candidateEntry.DistanceFromIdeal;
            }

            // Safety valve - if we've probed too far, resize and retry
            if (i < MaxProbeDistance) continue;

            Resize(Capacity * 2);
            InsertInternal(candidateEntry.Key, candidateEntry.Value);
            return;
        }

        // Should never reach here with proper load factor management
        throw new InvalidOperationException("Hash table insertion failed - this should not happen");
    }

    private bool IsPowerOfTwo(int value)
    {
        return BitOperations.IsPow2(value);
    }

    private struct Entry(TKey key, TValue value, byte distance)
    {
        public readonly TKey Key = key;
        public TValue Value = value;
        public byte DistanceFromIdeal = distance;
        public readonly bool IsOccupied = true;
        public readonly bool IsDeleted = false;

        /// <summary>
        ///     returns true if the entry is not occupied or deleted. The entry is considered available for storage.
        /// </summary>
        public bool IsAvailable => !IsOccupied || IsDeleted;

        /// <summary>
        ///     returns true if the entry is Occupied and not deleted
        /// </summary>
        public bool IsLive => IsOccupied && !IsDeleted;
    }
}