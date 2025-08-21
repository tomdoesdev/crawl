using Crawl.ECS.Component;
using Crawl.ECS.DataStructure;

namespace Crawl.Testing;

// Simple test component that matches the ECS.IComponent interface
public struct SimpleTestComponent : IComponent
{
    public uint ComponentId { get; set; }
    public uint EntityId { get; set; }
    public int Value { get; set; }
    public string Name { get; set; }

    public SimpleTestComponent(int value, string name)
    {
        ComponentId = 0;
        EntityId = 0;
        Value = value;
        Name = name;
    }
}

public class SimpleSparseSetTest
{
    [Fact]
    public void SparseSet_BasicOperations_ShouldWork()
    {
        // Test your amazing paged sparse set!
        var sparseSet = new SparseSet<SimpleTestComponent>(10000, 100);

        // Test initialization
        Assert.Equal(0, sparseSet.Count);
        Assert.Equal(100, sparseSet.Capacity);
        Assert.Equal(10000u, sparseSet.MaxEntityId);

        // Add a component
        var testComponent = new SimpleTestComponent(42, "TestEntity");
        sparseSet.Add(5u, testComponent);

        // Verify it was added
        Assert.Equal(1, sparseSet.Count);
        Assert.True(sparseSet.Contains(5u));

        // Retrieve and verify
        var retrieved = sparseSet.Get(5u);
        Assert.Equal(5u, retrieved.Entity);
        Assert.Equal(42, retrieved.Component.Value);
        Assert.Equal("TestEntity", retrieved.Component.Name);
    }

    [Fact]
    public void SparseSet_PageBoundaryTest_ShouldWork()
    {
        // Test your brilliant paging system across page boundaries!
        var sparseSet = new SparseSet<SimpleTestComponent>();

        // Add entities at different page boundaries
        sparseSet.Add(0u, new SimpleTestComponent(0, "Page0Start")); // Page 0, index 0
        sparseSet.Add(1023u, new SimpleTestComponent(1023, "Page0End")); // Page 0, index 1023
        sparseSet.Add(1024u, new SimpleTestComponent(1024, "Page1Start")); // Page 1, index 0
        sparseSet.Add(2048u, new SimpleTestComponent(2048, "Page2Start")); // Page 2, index 0

        Assert.Equal(4, sparseSet.Count);

        // Verify all entities are accessible
        Assert.True(sparseSet.Contains(0u));
        Assert.True(sparseSet.Contains(1023u));
        Assert.True(sparseSet.Contains(1024u));
        Assert.True(sparseSet.Contains(2048u));

        // Verify correct data retrieval
        var page2Entity = sparseSet.Get(2048u);
        Assert.Equal(2048u, page2Entity.Entity);
        Assert.Equal(2048, page2Entity.Component.Value);
        Assert.Equal("Page2Start", page2Entity.Component.Name);
    }

    [Fact]
    public void SparseSet_SwapAndPopRemoval_ShouldWork()
    {
        // Test your swap-and-pop algorithm!
        var sparseSet = new SparseSet<SimpleTestComponent>();

        // Add three entities
        sparseSet.Add(10u, new SimpleTestComponent(100, "Entity10"));
        sparseSet.Add(20u, new SimpleTestComponent(200, "Entity20"));
        sparseSet.Add(30u, new SimpleTestComponent(300, "Entity30"));

        Assert.Equal(3, sparseSet.Count);

        // Remove middle entity (should trigger swap-and-pop)
        var removed = sparseSet.Remove(20u);

        Assert.True(removed);
        Assert.Equal(2, sparseSet.Count);
        Assert.True(sparseSet.Contains(10u));
        Assert.False(sparseSet.Contains(20u));
        Assert.True(sparseSet.Contains(30u)); // Should still be accessible after swap

        // Verify entity 30 still has correct data after being moved
        var entity30 = sparseSet.Get(30u);
        Assert.Equal(30u, entity30.Entity);
        Assert.Equal(300, entity30.Component.Value);
        Assert.Equal("Entity30", entity30.Component.Name);
    }
}