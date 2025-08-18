using Crawl.DataStructures;
using Crawl.ECS;
using Xunit;

namespace Crawl.Test;

public class PoolTests
{
    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultCapacity_CreatesEmptyPool()
    {
        var pool = new Pool<int>();
        
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Constructor_WithCustomCapacity_CreatesEmptyPool()
    {
        var pool = new Pool<int>(500);
        
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Constructor_WithZeroCapacity_CreatesPoolWithZeroCapacity()
    {
        var pool = new Pool<int>(0);
        
        Assert.Equal(0, pool.Count);
        
        // Should not be able to add anything
        var result = pool.Add(42);
        Assert.False(result);
    }

    #endregion

    #region Add Tests

    [Fact]
    public void Add_SingleItem_ReturnsTrue()
    {
        var pool = new Pool<int>();
        
        var result = pool.Add(42);
        
        Assert.True(result);
        Assert.Equal(1, pool.Count);
        Assert.True(pool.Contains(42));
    }

    [Fact]
    public void Add_MultipleItems_ReturnsTrue()
    {
        var pool = new Pool<int>();
        
        var result1 = pool.Add(10);
        var result2 = pool.Add(20);
        var result3 = pool.Add(30);
        
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.Equal(3, pool.Count);
        Assert.True(pool.Contains(10));
        Assert.True(pool.Contains(20));
        Assert.True(pool.Contains(30));
    }

    [Fact]
    public void Add_DuplicateItem_ReturnsFalse()
    {
        var pool = new Pool<int>();
        
        var result1 = pool.Add(42);
        var result2 = pool.Add(42);
        
        Assert.True(result1);
        Assert.False(result2);
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Add_AtCapacityLimit_ReturnsFalse()
    {
        var pool = new Pool<int>(2);
        
        var result1 = pool.Add(10);
        var result2 = pool.Add(20);
        var result3 = pool.Add(30); // Should fail - at capacity
        
        Assert.True(result1);
        Assert.True(result2);
        Assert.False(result3);
        Assert.Equal(2, pool.Count);
        Assert.True(pool.Contains(10));
        Assert.True(pool.Contains(20));
        Assert.False(pool.Contains(30));
    }

    [Fact]
    public void Add_ToFullPool_ReturnsFalseForAllSubsequentAdds()
    {
        var pool = new Pool<int>(1);
        
        pool.Add(42);
        
        for (int i = 0; i < 10; i++)
        {
            var result = pool.Add(i);
            Assert.False(result);
        }
        
        Assert.Equal(1, pool.Count);
        Assert.True(pool.Contains(42));
    }

    [Fact]
    public void Add_WithStringType_WorksCorrectly()
    {
        var pool = new Pool<string>();
        
        var result1 = pool.Add("hello");
        var result2 = pool.Add("world");
        var result3 = pool.Add("hello"); // Duplicate
        
        Assert.True(result1);
        Assert.True(result2);
        Assert.False(result3);
        Assert.Equal(2, pool.Count);
    }

    #endregion

    #region Pluck Tests

    [Fact]
    public void Pluck_FromPoolWithSingleItem_ReturnsItem()
    {
        var pool = new Pool<int>();
        pool.Add(42);
        
        var result = pool.Pluck();
        
        Assert.Equal(42, result);
        Assert.Equal(0, pool.Count);
        Assert.False(pool.Contains(42));
    }

    [Fact]
    public void Pluck_FromPoolWithMultipleItems_ReturnsAnItem()
    {
        var pool = new Pool<int>();
        pool.Add(10);
        pool.Add(20);
        pool.Add(30);
        
        var result = pool.Pluck();
        
        // Should return one of the items (order not guaranteed with HashSet)
        Assert.True(result == 10 || result == 20 || result == 30);
        Assert.Equal(2, pool.Count);
        Assert.False(pool.Contains(result));
    }

    [Fact]
    public void Pluck_FromEmptyPool_ThrowsInvalidOperationException()
    {
        var pool = new Pool<int>();
        
        var exception = Assert.Throws<InvalidOperationException>(() => pool.Pluck());
        Assert.Equal("Pool is empty", exception.Message);
    }

    [Fact]
    public void Pluck_UntilEmpty_EventuallyThrows()
    {
        var pool = new Pool<int>();
        pool.Add(10);
        pool.Add(20);
        
        // Pluck all items
        pool.Pluck();
        pool.Pluck();
        
        // Next pluck should throw
        Assert.Throws<InvalidOperationException>(() => pool.Pluck());
    }

    [Fact]
    public void Pluck_RepeatedCalls_ReturnsAllUniqueItems()
    {
        var pool = new Pool<int>();
        var originalItems = new HashSet<int> { 10, 20, 30, 40, 50 };
        
        foreach (var item in originalItems)
        {
            pool.Add(item);
        }
        
        var pluckedItems = new HashSet<int>();
        while (pool.Count > 0)
        {
            pluckedItems.Add(pool.Pluck());
        }
        
        Assert.Equal(originalItems, pluckedItems);
    }

    #endregion

    #region TryPluck Tests

    [Fact]
    public void TryPluck_FromPoolWithItems_ReturnsTrueWithItem()
    {
        var pool = new Pool<int>();
        pool.Add(42);
        
        var result = pool.TryPluck(out var value);
        
        Assert.True(result);
        Assert.Equal(42, value);
        Assert.Equal(0, pool.Count);
        Assert.False(pool.Contains(42));
    }

    [Fact]
    public void TryPluck_FromEmptyPool_ReturnsFalseWithDefault()
    {
        var pool = new Pool<int>();
        
        var result = pool.TryPluck(out var value);
        
        Assert.False(result);
        Assert.Equal(default(int), value);
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void TryPluck_WithReferenceType_ReturnsNullWhenEmpty()
    {
        var pool = new Pool<string>();
        
        var result = pool.TryPluck(out var value);
        
        Assert.False(result);
        Assert.Null(value);
    }

    [Fact]
    public void TryPluck_RepeatedCalls_EventuallyReturnsFalse()
    {
        var pool = new Pool<int>();
        pool.Add(10);
        pool.Add(20);
        
        // First two should succeed
        Assert.True(pool.TryPluck(out _));
        Assert.True(pool.TryPluck(out _));
        
        // Third should fail
        Assert.False(pool.TryPluck(out _));
    }

    [Fact]
    public void TryPluck_WithMultipleItems_ReturnsValidItem()
    {
        var pool = new Pool<int>();
        var originalItems = new HashSet<int> { 10, 20, 30 };
        
        foreach (var item in originalItems)
        {
            pool.Add(item);
        }
        
        var result = pool.TryPluck(out var value);
        
        Assert.True(result);
        Assert.True(originalItems.Contains(value));
        Assert.Equal(2, pool.Count);
    }

    #endregion

    #region Contains Tests

    [Fact]
    public void Contains_ExistingItem_ReturnsTrue()
    {
        var pool = new Pool<int>();
        pool.Add(42);
        
        Assert.True(pool.Contains(42));
    }

    [Fact]
    public void Contains_NonExistentItem_ReturnsFalse()
    {
        var pool = new Pool<int>();
        pool.Add(42);
        
        Assert.False(pool.Contains(99));
    }

    [Fact]
    public void Contains_EmptyPool_ReturnsFalse()
    {
        var pool = new Pool<int>();
        
        Assert.False(pool.Contains(42));
    }

    [Fact]
    public void Contains_AfterPluck_ReturnsFalse()
    {
        var pool = new Pool<int>();
        pool.Add(42);
        
        pool.Pluck();
        
        Assert.False(pool.Contains(42));
    }

    [Fact]
    public void Contains_WithStringType_WorksCorrectly()
    {
        var pool = new Pool<string>();
        pool.Add("hello");
        
        Assert.True(pool.Contains("hello"));
        Assert.False(pool.Contains("world"));
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_WithItems_RemovesAll()
    {
        var pool = new Pool<int>();
        pool.Add(10);
        pool.Add(20);
        pool.Add(30);
        
        pool.Clear();
        
        Assert.Equal(0, pool.Count);
        Assert.False(pool.Contains(10));
        Assert.False(pool.Contains(20));
        Assert.False(pool.Contains(30));
    }

    [Fact]
    public void Clear_EmptyPool_DoesNothing()
    {
        var pool = new Pool<int>();
        
        pool.Clear(); // Should not throw
        
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Clear_ThenAdd_WorksCorrectly()
    {
        var pool = new Pool<int>();
        pool.Add(42);
        
        pool.Clear();
        var result = pool.Add(99);
        
        Assert.True(result);
        Assert.Equal(1, pool.Count);
        Assert.True(pool.Contains(99));
        Assert.False(pool.Contains(42));
    }

    [Fact]
    public void Clear_ResetsCapacityConstraints()
    {
        var pool = new Pool<int>(2);
        pool.Add(10);
        pool.Add(20);
        
        // Pool is now full
        Assert.False(pool.Add(30));
        
        pool.Clear();
        
        // Should be able to add again up to capacity
        Assert.True(pool.Add(40));
        Assert.True(pool.Add(50));
        Assert.False(pool.Add(60));
    }

    #endregion

    #region Count Tests

    [Fact]
    public void Count_EmptyPool_ReturnsZero()
    {
        var pool = new Pool<int>();
        
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Count_AfterAdding_IncrementsCorrectly()
    {
        var pool = new Pool<int>();
        
        Assert.Equal(0, pool.Count);
        
        pool.Add(10);
        Assert.Equal(1, pool.Count);
        
        pool.Add(20);
        Assert.Equal(2, pool.Count);
        
        pool.Add(30);
        Assert.Equal(3, pool.Count);
    }

    [Fact]
    public void Count_AfterDuplicateAdd_DoesNotIncrement()
    {
        var pool = new Pool<int>();
        pool.Add(42);
        
        Assert.Equal(1, pool.Count);
        
        pool.Add(42); // Duplicate
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Count_AfterPluck_Decrements()
    {
        var pool = new Pool<int>();
        pool.Add(10);
        pool.Add(20);
        
        Assert.Equal(2, pool.Count);
        
        pool.Pluck();
        Assert.Equal(1, pool.Count);
        
        pool.Pluck();
        Assert.Equal(0, pool.Count);
    }

    [Fact]
    public void Count_AfterClear_ReturnsZero()
    {
        var pool = new Pool<int>();
        pool.Add(10);
        pool.Add(20);
        pool.Add(30);
        
        pool.Clear();
        
        Assert.Equal(0, pool.Count);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void Pool_ComplexScenario_WorksCorrectly()
    {
        var pool = new Pool<int>(5);
        
        // Add some items
        Assert.True(pool.Add(1));
        Assert.True(pool.Add(2));
        Assert.True(pool.Add(3));
        
        // Pluck one
        var plucked = pool.Pluck();
        Assert.True(plucked is >= 1 and <= 3);
        Assert.Equal(2, pool.Count);
        
        // Add more until capacity
        Assert.True(pool.Add(4));
        Assert.True(pool.Add(5));
        Assert.True(pool.Add(6));
        
        // Should be at capacity now
        Assert.False(pool.Add(7));
        Assert.Equal(5, pool.Count);
        
        // Clear and start fresh
        pool.Clear();
        Assert.Equal(0, pool.Count);
        
        // Should be able to add again
        Assert.True(pool.Add(100));
        Assert.Equal(1, pool.Count);
    }

    [Fact]
    public void Pool_WithEntityIds_WorksAsExpected()
    {
        var pool = new Pool<uint>();
        
        // Simulate entity ID pool usage
        var entityIds = new uint[] { 1, 5, 10, 15, 20 };
        
        foreach (var id in entityIds)
        {
            pool.Add(id);
        }
        
        // Pluck some IDs (simulating entity creation)
        var id1 = pool.Pluck();
        var id2 = pool.Pluck();
        
        Assert.Equal(3, pool.Count);
        Assert.True(Array.Exists(entityIds, x => x == id1));
        Assert.True(Array.Exists(entityIds, x => x == id2));
        Assert.NotEqual(id1, id2);
        
        // Return an ID (simulating entity destruction)
        pool.Add(id1);
        Assert.Equal(4, pool.Count);
        Assert.True(pool.Contains(id1));
    }

    [Fact]
    public void Pool_ThreadSafety_BasicOperations()
    {
        // Note: This is a basic test - Pool<T> is not thread-safe by design
        // This test just ensures basic operations don't corrupt state
        var pool = new Pool<int>(1000);
        
        // Add many items sequentially
        for (int i = 0; i < 500; i++)
        {
            pool.Add(i);
        }
        
        Assert.Equal(500, pool.Count);
        
        // Pluck many items
        for (int i = 0; i < 250; i++)
        {
            pool.TryPluck(out _);
        }
        
        Assert.Equal(250, pool.Count);
    }

    [Fact]
    public void Pool_WithCustomObjects_WorksCorrectly()
    {
        var pool = new Pool<string>();
        
        var items = new[] { "apple", "banana", "cherry", "date" };
        
        foreach (var item in items)
        {
            pool.Add(item);
        }
        
        Assert.Equal(4, pool.Count);
        
        var plucked = new List<string>();
        while (pool.TryPluck(out var item))
        {
            plucked.Add(item);
        }
        
        Assert.Equal(4, plucked.Count);
        Assert.Equal(0, pool.Count);
        
        // All original items should have been plucked
        foreach (var originalItem in items)
        {
            Assert.Contains(originalItem, plucked);
        }
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    public void Pool_WithVariousCapacities_RespectLimits(uint capacity)
    {
        var pool = new Pool<int>(capacity);
        
        // Try to add more items than capacity
        for (int i = 0; i < capacity + 10; i++)
        {
            pool.Add(i);
        }
        
        // Should not exceed capacity
        Assert.True(pool.Count <= capacity);
        Assert.Equal((int)Math.Min(capacity, capacity + 10), pool.Count);
    }

    #endregion
}