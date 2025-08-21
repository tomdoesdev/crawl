using System.Collections.Immutable;
using Crawl.ECS.Component;
using Crawl.ECS.DataStructure;
using Crawl.ECS.Exception;
using Xunit.Abstractions;

namespace Crawl.Testing;

public class SparseSetTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public SparseSetTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void Constructor_ShouldInitializeCorrectly()
    {
        // Arrange & Act
        var sparseSet = new SparseSet<TestComponent>(5000, 100);

        // Assert
        Assert.Equal(0, sparseSet.Count);
        Assert.Equal(100, sparseSet.Capacity);
        Assert.Equal(5000u, sparseSet.MaxEntityId);
    }

    [Fact]
    public void Add_SingleEntity_ShouldWork()
    {
        // Arrange
        var sparseSet = new SparseSet<TestComponent>();
        var component = new TestComponent(42, "Test");

        // Act
        var result = sparseSet.Add(1u, component);

        // Assert
        Assert.True(sparseSet.Contains(1u));
        Assert.Equal(1, sparseSet.Count);

        var retrieved = sparseSet.Get(1u);
        Assert.Equal(1u, retrieved.Entity);
        Assert.Equal(42, retrieved.Component.Value);
        Assert.Equal("Test", retrieved.Component.Name);
    }

    [Theory]
    [InlineData(0u)] // First entity
    [InlineData(1023u)] // Last in page 0
    [InlineData(1024u)] // First in page 1  
    [InlineData(2048u)] // First in page 2
    [InlineData(3071u)] // Last in page 2
    public void Add_DifferentPages_ShouldWork(uint entityId)
    {
        // Arrange
        var sparseSet = new SparseSet<TestComponent>();
        var component = new TestComponent((int)entityId, $"Entity{entityId}");

        // Act
        sparseSet.Add(entityId, component);

        // Assert
        Assert.True(sparseSet.Contains(entityId));
        var retrieved = sparseSet.Get(entityId);
        Assert.Equal(entityId, retrieved.Entity);
        Assert.Equal((int)entityId, retrieved.Component.Value);

        _testOutputHelper.WriteLine($"Entity {entityId} successfully stored and retrieved from page system");
    }

    [Fact]
    public void Add_DuplicateEntity_ShouldThrow()
    {
        // Arrange
        var sparseSet = new SparseSet<TestComponent>();
        var component = new TestComponent(42, "Test");
        sparseSet.Add(1u, component);

        // Act & Assert
        var exception = Assert.Throws<ConflictException>(() => sparseSet.Add(1u, component));
        Assert.Contains("entity with id 1 already exists", exception.Message);
    }

    [Fact]
    public void Remove_SwapAndPop_ShouldMaintainDenseArray()
    {
        // Arrange
        var sparseSet = new SparseSet<TestComponent>();
        sparseSet.Add(10u, new TestComponent(100, "Entity10"));
        sparseSet.Add(20u, new TestComponent(200, "Entity20")); // This will be removed
        sparseSet.Add(30u, new TestComponent(300, "Entity30")); // This should move to middle

        _testOutputHelper.WriteLine("Before removal:");
        LogSparseSetState(sparseSet);

        // Act - Remove middle element (should trigger swap-and-pop)
        var result = sparseSet.Remove(20u);

        // Assert
        Assert.True(result);
        Assert.Equal(2, sparseSet.Count);

        _testOutputHelper.WriteLine("After removal:");
        LogSparseSetState(sparseSet);

        // Verify remaining entities still work
        Assert.True(sparseSet.Contains(10u));
        Assert.False(sparseSet.Contains(20u)); // Removed
        Assert.True(sparseSet.Contains(30u)); // Should still be accessible

        // Verify the swapped entity (30) still has correct data
        var entity30 = sparseSet.Get(30u);
        Assert.Equal(30u, entity30.Entity);
        Assert.Equal(300, entity30.Component.Value);
        Assert.Equal("Entity30", entity30.Component.Name);
    }

    [Fact]
    public void Remove_LastElement_OptimizationShouldWork()
    {
        // Arrange
        var sparseSet = new SparseSet<TestComponent>();
        sparseSet.Add(1u, new TestComponent(100, "Entity1"));
        sparseSet.Add(2u, new TestComponent(200, "Entity2"));
        sparseSet.Add(3u, new TestComponent(300, "Entity3"));

        // Act - Remove last element (should use optimization path)
        var result = sparseSet.Remove(3u);

        // Assert
        Assert.True(result);
        Assert.Equal(2, sparseSet.Count);
        Assert.True(sparseSet.Contains(1u));
        Assert.True(sparseSet.Contains(2u));
        Assert.False(sparseSet.Contains(3u));

        _testOutputHelper.WriteLine("Last element removal optimization worked correctly");
    }

    [Fact]
    public void GetAll_WithEntities_ShouldReturnDenseArray()
    {
        // Arrange
        var sparseSet = new SparseSet<TestComponent>();
        sparseSet.Add(5u, new TestComponent(100, "Entity5"));
        sparseSet.Add(42u, new TestComponent(200, "Entity42"));
        sparseSet.Add(2048u, new TestComponent(300, "Entity2048"));

        // Act
        var all = sparseSet.GetAll();

        // Assert
        Assert.Equal(3, all.Length);

        _testOutputHelper.WriteLine("Dense array contents:");
        for (var i = 0; i < all.Length; i++)
        {
            var entry = all[i];
            _testOutputHelper.WriteLine($"  [{i}] Entity: {entry.Entity}, Value: {entry.Component.Value}");
        }

        // Verify all entities are present (order doesn't matter in dense array)
        var entityIds = all.ToImmutableArray().Select(e => e.Entity).ToHashSet();
        Assert.Contains(5u, entityIds);
        Assert.Contains(42u, entityIds);
        Assert.Contains(2048u, entityIds);
    }

    [Fact]
    public void StressTest_ManyEntitiesAcrossPages()
    {
        // Arrange
        var sparseSet = new SparseSet<TestComponent>(50000, 10);
        var testEntities = new List<uint> { 1, 1023, 1024, 2048, 5000, 10000, 20000, 49999 };

        // Act - Add entities across different pages
        foreach (var entityId in testEntities)
            sparseSet.Add(entityId, new TestComponent((int)entityId, $"Entity{entityId}"));

        // Assert
        Assert.Equal(testEntities.Count, sparseSet.Count);

        foreach (var entityId in testEntities)
        {
            Assert.True(sparseSet.Contains(entityId), $"Entity {entityId} should exist");
            var entry = sparseSet.Get(entityId);
            Assert.Equal(entityId, entry.Entity);
            Assert.Equal((int)entityId, entry.Component.Value);
        }

        _testOutputHelper.WriteLine($"Successfully handled {testEntities.Count} entities across multiple pages");
        _testOutputHelper.WriteLine($"Final capacity: {sparseSet.Capacity}");
    }

    private void LogSparseSetState(SparseSet<TestComponent> sparseSet)
    {
        _testOutputHelper.WriteLine($"Count: {sparseSet.Count}, Capacity: {sparseSet.Capacity}");
        var all = sparseSet.GetAll();
        for (var i = 0; i < all.Length && i < sparseSet.Count; i++)
        {
            var entry = all[i];
            _testOutputHelper.WriteLine($"  Dense[{i}]: Entity {entry.Entity}, Value {entry.Component.Value}");
        }
    }

    // Test component for our tests - uses the old IComponent interface
    public struct TestComponent : IComponent
    {
        public int Value { get; set; }
        public string Name { get; set; }

        public TestComponent(int value, string name)
        {
            ComponentId = 0;
            EntityId = 0;
            Value = value;
            Name = name;
        }

        public uint ComponentId { get; set; }
        public uint EntityId { get; set; }
    }
}