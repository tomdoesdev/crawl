using Crawl.ECS.DataStructure;

namespace Crawl.Testing;

public class FastDictionaryTests
{
    [Fact]
    public void Add_SingleItem_IncreasesCount()
    {
        var dict = new FastDictionary<string, int>();

        dict.Add("key1", 42);

        Assert.Equal(1, dict.Count);
    }

    [Fact]
    public void Add_MultipleItems_IncreasesCountCorrectly()
    {
        var dict = new FastDictionary<string, int>();

        dict.Add("key1", 1);
        dict.Add("key2", 2);
        dict.Add("key3", 3);

        Assert.Equal(3, dict.Count);
    }

    [Fact]
    public void Add_DuplicateKey_UpdatesValue()
    {
        var dict = new FastDictionary<string, int>();

        dict.Add("key1", 42);
        dict.Add("key1", 99); // Should update, not add

        Assert.Equal(1, dict.Count);
        Assert.True(dict.TryGet("key1", out var value));
        Assert.Equal(99, value); // Should have updated value
    }

    [Fact]
    public void Add_NullKey_ThrowsArgumentNullException()
    {
        var dict = new FastDictionary<string, int>();

        Assert.Throws<ArgumentNullException>(() => dict.Add(null!, 42));
    }

    [Fact]
    public void Add_ManyItems_TriggersResize()
    {
        var dict = new FastDictionary<int, int>(4); // Small initial capacity
        var initialCapacity = dict.Capacity;

        // Add enough items to trigger resize (beyond 90% load factor)
        for (var i = 0; i < 10; i++) dict.Add(i, i * 10);

        Assert.True(dict.Capacity > initialCapacity);
        Assert.Equal(10, dict.Count);
    }

    [Fact]
    public void Add_ItemsWithCollisions_HandlesRobinHoodCorrectly()
    {
        // Use small capacity to force collisions
        var dict = new FastDictionary<int, string>(4);

        // These keys should hash to same initial position with small capacity
        dict.Add(1, "first");
        dict.Add(5, "second"); // Likely collision with key 1
        dict.Add(9, "third"); // Likely collision with keys 1 and 5

        Assert.Equal(3, dict.Count);
        // All items should be stored despite collisions
    }

    [Fact]
    public void Add_LoadFactorCalculation_IsAccurate()
    {
        var dict = new FastDictionary<int, int>(8); // Capacity will be rounded to power of 2

        dict.Add(1, 1);
        dict.Add(2, 2);

        var expectedLoadFactor = 2.0 / dict.Capacity;
        Assert.Equal(expectedLoadFactor, dict.LoadFactor, 3);
    }

    [Fact]
    public void Add_ResizePreservesAllEntries()
    {
        var dict = new FastDictionary<int, int>(4);
        var itemsToAdd = new Dictionary<int, int>();

        // Add items that will trigger resize
        for (var i = 0; i < 8; i++)
        {
            itemsToAdd[i] = i * 100;
            dict.Add(i, i * 100);
        }

        Assert.Equal(itemsToAdd.Count, dict.Count);

        // Verify all items are still present after resize
        foreach (var kvp in itemsToAdd)
        {
            Assert.True(dict.TryGet(kvp.Key, out var value));
            Assert.Equal(kvp.Value, value);
        }
    }

    [Fact]
    public void Constructor_WithInitialCapacity_SetsCapacityToPowerOfTwo()
    {
        var dict1 = new FastDictionary<int, int>(10);
        var dict2 = new FastDictionary<int, int>(16);
        var dict3 = new FastDictionary<int, int>(17);

        Assert.Equal(16, dict1.Capacity); // Next power of 2 after 10
        Assert.Equal(16, dict2.Capacity); // Already power of 2
        Assert.Equal(32, dict3.Capacity); // Next power of 2 after 17
    }

    [Fact]
    public void Constructor_DefaultCapacity_IsCorrect()
    {
        var dict = new FastDictionary<string, int>();

        Assert.Equal(64, dict.Capacity); // Default initial capacity
        Assert.Equal(0, dict.Count);
    }

    [Fact]
    public void Add_StressTest_HandlesLargeNumberOfItems()
    {
        var dict = new FastDictionary<int, int>();
        const int itemCount = 1000;

        for (var i = 0; i < itemCount; i++) dict.Add(i, i * 2);

        Assert.Equal(itemCount, dict.Count);
        Assert.True(dict.LoadFactor < 0.9); // Should have resized to maintain load factor
    }

    [Fact]
    public void Add_WithStringKeys_WorksCorrectly()
    {
        var dict = new FastDictionary<string, int>();

        dict.Add("apple", 1);
        dict.Add("banana", 2);
        dict.Add("cherry", 3);

        Assert.Equal(3, dict.Count);
    }

    [Fact]
    public void Add_UpdateExistingKey_DoesNotIncreaseCount()
    {
        var dict = new FastDictionary<int, string>();

        dict.Add(42, "original");
        var countAfterFirst = dict.Count;

        dict.Add(42, "updated");

        Assert.Equal(countAfterFirst, dict.Count);
        Assert.Equal(1, dict.Count);
        
        // Verify the value was actually updated
        Assert.True(dict.TryGet(42, out var value));
        Assert.Equal("updated", value);
    }

    // TryGet-specific tests
    [Fact]
    public void TryGet_ExistingKey_ReturnsTrue()
    {
        var dict = new FastDictionary<string, int>();
        dict.Add("test", 42);

        var found = dict.TryGet("test", out var value);

        Assert.True(found);
        Assert.Equal(42, value);
    }

    [Fact]
    public void TryGet_NonExistentKey_ReturnsFalse()
    {
        var dict = new FastDictionary<string, int>();
        dict.Add("existing", 42);

        var found = dict.TryGet("nonexistent", out var value);

        Assert.False(found);
        Assert.Equal(default(int), value);
    }

    [Fact]
    public void TryGet_EmptyDictionary_ReturnsFalse()
    {
        var dict = new FastDictionary<string, int>();

        var found = dict.TryGet("any", out var value);

        Assert.False(found);
        Assert.Equal(default(int), value);
    }

    [Fact]
    public void TryGet_WithCollisions_FindsCorrectValues()
    {
        var dict = new FastDictionary<int, string>(4); // Force collisions

        dict.Add(1, "first");
        dict.Add(5, "second"); // Likely collision
        dict.Add(9, "third");  // Likely collision

        Assert.True(dict.TryGet(1, out var value1));
        Assert.Equal("first", value1);

        Assert.True(dict.TryGet(5, out var value2));
        Assert.Equal("second", value2);

        Assert.True(dict.TryGet(9, out var value3));
        Assert.Equal("third", value3);
    }

    [Fact]
    public void TryGet_AfterResize_StillFindsValues()
    {
        var dict = new FastDictionary<int, string>(4);
        
        // Add items that will trigger resize
        for (int i = 0; i < 10; i++)
        {
            dict.Add(i, $"value{i}");
        }

        // Verify all items are still findable after resize
        for (int i = 0; i < 10; i++)
        {
            Assert.True(dict.TryGet(i, out var value));
            Assert.Equal($"value{i}", value);
        }
    }

    [Fact]
    public void TryGet_NullKey_ThrowsArgumentNullException()
    {
        var dict = new FastDictionary<string, int>();

        Assert.Throws<ArgumentNullException>(() => dict.TryGet(null!, out _));
    }

    [Fact]
    public void TryGet_UpdatedValue_ReturnsNewValue()
    {
        var dict = new FastDictionary<string, int>();
        
        dict.Add("key", 100);
        dict.Add("key", 200); // Update

        Assert.True(dict.TryGet("key", out var value));
        Assert.Equal(200, value);
    }

    [Fact]
    public void TryGet_MixedOperations_WorksCorrectly()
    {
        var dict = new FastDictionary<string, int>();
        
        // Add some initial values
        dict.Add("a", 1);
        dict.Add("b", 2);
        dict.Add("c", 3);
        
        // Test retrieval
        Assert.True(dict.TryGet("a", out var valueA));
        Assert.Equal(1, valueA);
        
        // Update and test
        dict.Add("b", 20);
        Assert.True(dict.TryGet("b", out var valueB));
        Assert.Equal(20, valueB);
        
        // Test non-existent
        Assert.False(dict.TryGet("d", out _));
    }

    [Fact]
    public void TryGet_StressTest_AllItemsRetrievable()
    {
        var dict = new FastDictionary<int, string>();
        const int itemCount = 1000;
        
        // Add many items
        for (int i = 0; i < itemCount; i++)
        {
            dict.Add(i, $"item_{i}");
        }
        
        // Verify all are retrievable
        for (int i = 0; i < itemCount; i++)
        {
            Assert.True(dict.TryGet(i, out var value));
            Assert.Equal($"item_{i}", value);
        }
        
        // Verify non-existent keys return false
        Assert.False(dict.TryGet(itemCount, out _));
        Assert.False(dict.TryGet(-1, out _));
    }
}