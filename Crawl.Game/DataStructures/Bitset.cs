using System.Runtime.CompilerServices;

namespace Crawl.DataStructures;

public struct Bitset
{
    private ulong[] _bits;
    private int _capacity;

    public Bitset(int initialCapacity = 64)
    {
        var ulongCount = (initialCapacity + 63) / 64;
        _bits = new ulong[ulongCount];
        _capacity = ulongCount * 64;
    }

    /// <summary>
    ///     Sets the bit at the specified index to true
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void SetBit(int index)
    {
        if (index >= _capacity)
            Grow(index + 1);

        var ulongIndex = index / 64;
        var bitIndex = index % 64;
        _bits[ulongIndex] |= 1UL << bitIndex;
    }

    /// <summary>
    ///     Clears the bit at the specified index (sets to false)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ClearBit(int index)
    {
        if (index >= _capacity) return;

        var ulongIndex = index / 64;
        var bitIndex = index % 64;
        _bits[ulongIndex] &= ~(1UL << bitIndex);
    }

    /// <summary>
    ///     Checks if the bit at the specified index is set
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsSet(int index)
    {
        if (index >= _capacity) return false;
        var ulongIndex = index / 64;
        var bitIndex = index % 64;
        return (_bits[ulongIndex] & (1UL << bitIndex)) != 0;
    }

    /// <summary>
    ///     Clears all bits
    /// </summary>
    public void Clear()
    {
        if (_bits != null)
            Array.Clear(_bits, 0, _bits.Length);
    }

    /// <summary>
    ///     Returns true if no bits are set
    /// </summary>
    public bool IsEmpty
    {
        get
        {
            if (_bits == null) return true;
            for (var i = 0; i < _bits.Length; i++)
                if (_bits[i] != 0)
                    return false;
            return true;
        }
    }

    /// <summary>
    ///     Iterates over all set bit indices efficiently
    /// </summary>
    public IEnumerable<int> GetSetBits()
    {
        if (_bits == null) yield break;

        for (var ulongIndex = 0; ulongIndex < _bits.Length; ulongIndex++)
        {
            var bits = _bits[ulongIndex];
            if (bits == 0) continue;

            // Find each set bit in this ulong
            for (var bitIndex = 0; bitIndex < 64; bitIndex++)
                if ((bits & (1UL << bitIndex)) != 0)
                    yield return ulongIndex * 64 + bitIndex;
        }
    }

    /// <summary>
    ///     Performs bitwise OR with another bitset
    /// </summary>
    public void Or(ref Bitset other)
    {
        // Ensure we're large enough
        if (other._capacity > _capacity)
            Grow(other._capacity);

        var minLength = Math.Min(_bits.Length, other._bits.Length);
        for (var i = 0; i < minLength; i++) _bits[i] |= other._bits[i];
    }

    /// <summary>
    ///     Performs bitwise AND with another bitset
    /// </summary>
    public void And(ref Bitset other)
    {
        if (_bits == null) return;

        var minLength = Math.Min(_bits.Length, other._bits.Length);
        for (var i = 0; i < minLength; i++) _bits[i] &= other._bits[i];

        // Clear remaining bits if we're larger
        for (var i = minLength; i < _bits.Length; i++) _bits[i] = 0;
    }

    private void Grow(int newMinCapacity)
    {
        var newUlongCount = (newMinCapacity + 63) / 64;

        if (_bits == null)
            _bits = new ulong[newUlongCount];
        else
            Array.Resize(ref _bits, newUlongCount);

        _capacity = newUlongCount * 64;
    }
}