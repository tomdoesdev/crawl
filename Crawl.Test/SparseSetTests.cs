using Crawl.DataStructures;
using Crawl.ECS;
using Crawl.ECS.Components;
using Crawl.ECS.Entities;
using Xunit;

namespace Crawl.Test;

// Test component for our tests
public readonly struct TestComponent(int value) : IComponent
{
    public int Value { get; } = value;
    public ComponentType ComponentType => ComponentType.Position; // Using Position for simplicity

    public override bool Equals(object? obj)
    {
        return obj is TestComponent other && Value == other.Value;
    }

    public override int GetHashCode() => Value.GetHashCode();
}

public class SparseSetTests
{
    private readonly SparseSet<TestComponent> _sparseSet = new();
    private readonly Entity _entity1 = new(1);
    private readonly Entity _entity2 = new(2);
    private readonly Entity _entity3 = new(3);

    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultCapacity_CreatesEmptySet()
    {
        var sparseSet = new SparseSet<TestComponent>();
        
        // No public way to check capacity, but we can verify it's empty
        Assert.Equal(0, sparseSet.GetAll().Length);
    }

    [Fact]
    public void Constructor_WithCustomCapacity_DoesNotThrow()
    {
        var sparseSet = new SparseSet<TestComponent>(500, 250);
        
        Assert.Equal(0, sparseSet.GetAll().Length);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(-1, 100)]
    [InlineData(100, 0)]
    [InlineData(100, -1)]
    public void Constructor_WithInvalidCapacity_ThrowsArgumentException(int initialCapacity, int growCapacity)
    {
        Assert.Throws<ArgumentException>(() => new SparseSet<TestComponent>(initialCapacity, growCapacity));
    }

    #endregion

    #region Add Tests

    [Fact]
    public void Add_SingleComponent_StoresCorrectly()
    {
        var component = new TestComponent(42);
        
        _sparseSet.Add(_entity1, component);
        
        Assert.True(_sparseSet.Has(_entity1));
        Assert.Equal(component, _sparseSet.Get(_entity1));
        Assert.Equal(1, _sparseSet.GetAll().Length);
    }

    [Fact]
    public void Add_MultipleComponents_StoresAllCorrectly()
    {
        var component1 = new TestComponent(10);
        var component2 = new TestComponent(20);
        var component3 = new TestComponent(30);
        
        _sparseSet.Add(_entity1, component1);
        _sparseSet.Add(_entity2, component2);
        _sparseSet.Add(_entity3, component3);
        
        Assert.True(_sparseSet.Has(_entity1));
        Assert.True(_sparseSet.Has(_entity2));
        Assert.True(_sparseSet.Has(_entity3));
        
        Assert.Equal(component1, _sparseSet.Get(_entity1));
        Assert.Equal(component2, _sparseSet.Get(_entity2));
        Assert.Equal(component3, _sparseSet.Get(_entity3));
        
        Assert.Equal(3, _sparseSet.GetAll().Length);
    }

    [Fact]
    public void Add_DuplicateEntity_ThrowsDuplicateComponentException()
    {
        var component1 = new TestComponent(10);
        var component2 = new TestComponent(20);
        
        _sparseSet.Add(_entity1, component1);
        
        var exception = Assert.Throws<DuplicateComponentException>(() => _sparseSet.Add(_entity1, component2));
        Assert.Contains("Entity 1 already has component TestComponent", exception.Message);
    }

    [Fact]
    public void Add_ManyComponents_GrowsCapacityCorrectly()
    {
        // Add more components than initial capacity (1000) to test growth
        for (uint i = 1; i <= 1500; i++)
        {
            _sparseSet.Add(new Entity(i), new TestComponent((int)i));
        }
        
        Assert.Equal(1500, _sparseSet.GetAll().Length);
        
        // Verify all components are still accessible
        for (uint i = 1; i <= 1500; i++)
        {
            Assert.True(_sparseSet.Has(new Entity(i)));
            Assert.Equal(new TestComponent((int)i), _sparseSet.Get(new Entity(i)));
        }
    }

    #endregion

    #region TryAdd Tests

    [Fact]
    public void TryAdd_NewEntity_ReturnsTrue()
    {
        var component = new TestComponent(42);
        
        var result = _sparseSet.TryAdd(_entity1, component);
        
        Assert.True(result);
        Assert.True(_sparseSet.Has(_entity1));
        Assert.Equal(component, _sparseSet.Get(_entity1));
    }

    [Fact]
    public void TryAdd_DuplicateEntity_ReturnsFalse()
    {
        var component1 = new TestComponent(10);
        var component2 = new TestComponent(20);
        
        _sparseSet.TryAdd(_entity1, component1);
        var result = _sparseSet.TryAdd(_entity1, component2);
        
        Assert.False(result);
        Assert.Equal(component1, _sparseSet.Get(_entity1)); // Original component unchanged
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_ExistingEntity_RemovesComponent()
    {
        var component = new TestComponent(42);
        _sparseSet.Add(_entity1, component);
        
        _sparseSet.Remove(_entity1);
        
        Assert.False(_sparseSet.Has(_entity1));
        Assert.Equal(0, _sparseSet.GetAll().Length);
    }

    [Fact]
    public void Remove_NonExistentEntity_DoesNothing()
    {
        _sparseSet.Remove(_entity1); // Should not throw
        
        Assert.False(_sparseSet.Has(_entity1));
        Assert.Equal(0, _sparseSet.GetAll().Length);
    }

    [Fact]
    public void Remove_FromEmptySet_DoesNothing()
    {
        _sparseSet.Remove(_entity1); // Should not throw
        
        Assert.Equal(0, _sparseSet.GetAll().Length);
    }

    [Fact]
    public void Remove_SwapWithLast_MaintainsPacking()
    {
        var component1 = new TestComponent(10);
        var component2 = new TestComponent(20);
        var component3 = new TestComponent(30);
        
        _sparseSet.Add(_entity1, component1);
        _sparseSet.Add(_entity2, component2);
        _sparseSet.Add(_entity3, component3);
        
        // Remove middle entity
        _sparseSet.Remove(_entity2);
        
        Assert.False(_sparseSet.Has(_entity2));
        Assert.True(_sparseSet.Has(_entity1));
        Assert.True(_sparseSet.Has(_entity3));
        Assert.Equal(2, _sparseSet.GetAll().Length);
        
        // Verify remaining components are still accessible
        Assert.Equal(component1, _sparseSet.Get(_entity1));
        Assert.Equal(component3, _sparseSet.Get(_entity3));
    }

    [Fact]
    public void Remove_LastEntity_DoesNotSwap()
    {
        var component1 = new TestComponent(10);
        var component2 = new TestComponent(20);
        
        _sparseSet.Add(_entity1, component1);
        _sparseSet.Add(_entity2, component2);
        
        // Remove last entity (no swap needed)
        _sparseSet.Remove(_entity2);
        
        Assert.True(_sparseSet.Has(_entity1));
        Assert.False(_sparseSet.Has(_entity2));
        Assert.Equal(1, _sparseSet.GetAll().Length);
        Assert.Equal(component1, _sparseSet.Get(_entity1));
    }

    #endregion

    #region Get Tests

    [Fact]
    public void Get_ExistingEntity_ReturnsComponent()
    {
        var component = new TestComponent(42);
        _sparseSet.Add(_entity1, component);
        
        var result = _sparseSet.Get(_entity1);
        
        Assert.Equal(component, result);
    }

    [Fact]
    public void Get_NonExistentEntity_ThrowsComponentNotFoundException()
    {
        var exception = Assert.Throws<ComponentNotFoundException>(() => _sparseSet.Get(_entity1));
        Assert.Contains("Entity 1 does not exist", exception.Message);
    }

    #endregion

    #region TryGet Tests

    [Fact]
    public void TryGet_ExistingEntity_ReturnsTrueWithComponent()
    {
        var component = new TestComponent(42);
        _sparseSet.Add(_entity1, component);
        
        var result = _sparseSet.TryGet(_entity1, out var retrievedComponent);
        
        Assert.True(result);
        Assert.Equal(component, retrievedComponent);
    }

    [Fact]
    public void TryGet_NonExistentEntity_ReturnsFalseWithDefault()
    {
        var result = _sparseSet.TryGet(_entity1, out var retrievedComponent);
        
        Assert.False(result);
        Assert.Equal(default(TestComponent), retrievedComponent);
    }

    #endregion

    #region Has Tests

    [Fact]
    public void Has_ExistingEntity_ReturnsTrue()
    {
        _sparseSet.Add(_entity1, new TestComponent(42));
        
        Assert.True(_sparseSet.Has(_entity1));
    }

    [Fact]
    public void Has_NonExistentEntity_ReturnsFalse()
    {
        Assert.False(_sparseSet.Has(_entity1));
    }

    [Fact]
    public void Has_RemovedEntity_ReturnsFalse()
    {
        _sparseSet.Add(_entity1, new TestComponent(42));
        _sparseSet.Remove(_entity1);
        
        Assert.False(_sparseSet.Has(_entity1));
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_EmptySet_ReturnsEmptySpan()
    {
        var result = _sparseSet.GetAll();
        
        Assert.Equal(0, result.Length);
    }

    [Fact]
    public void GetAll_WithComponents_ReturnsAllComponents()
    {
        var component1 = new TestComponent(10);
        var component2 = new TestComponent(20);
        var component3 = new TestComponent(30);
        
        _sparseSet.Add(_entity1, component1);
        _sparseSet.Add(_entity2, component2);
        _sparseSet.Add(_entity3, component3);
        
        var result = _sparseSet.GetAll();
        
        Assert.Equal(3, result.Length);
        Assert.Contains(component1, result.ToArray());
        Assert.Contains(component2, result.ToArray());
        Assert.Contains(component3, result.ToArray());
    }

    [Fact]
    public void GetAll_AfterRemoval_ReturnsUpdatedComponents()
    {
        var component1 = new TestComponent(10);
        var component2 = new TestComponent(20);
        var component3 = new TestComponent(30);
        
        _sparseSet.Add(_entity1, component1);
        _sparseSet.Add(_entity2, component2);
        _sparseSet.Add(_entity3, component3);
        
        _sparseSet.Remove(_entity2);
        
        var result = _sparseSet.GetAll();
        
        Assert.Equal(2, result.Length);
        Assert.Contains(component1, result.ToArray());
        Assert.Contains(component3, result.ToArray());
        Assert.DoesNotContain(component2, result.ToArray());
    }

    #endregion

    #region Clear Tests

    [Fact]
    public void Clear_WithComponents_RemovesAll()
    {
        _sparseSet.Add(_entity1, new TestComponent(10));
        _sparseSet.Add(_entity2, new TestComponent(20));
        _sparseSet.Add(_entity3, new TestComponent(30));
        
        _sparseSet.Clear();
        
        Assert.Equal(0, _sparseSet.GetAll().Length);
        Assert.False(_sparseSet.Has(_entity1));
        Assert.False(_sparseSet.Has(_entity2));
        Assert.False(_sparseSet.Has(_entity3));
    }

    [Fact]
    public void Clear_EmptySet_DoesNothing()
    {
        _sparseSet.Clear(); // Should not throw
        
        Assert.Equal(0, _sparseSet.GetAll().Length);
    }

    [Fact]
    public void Clear_ThenAdd_WorksCorrectly()
    {
        _sparseSet.Add(_entity1, new TestComponent(10));
        _sparseSet.Clear();
        
        var newComponent = new TestComponent(42);
        _sparseSet.Add(_entity1, newComponent);
        
        Assert.True(_sparseSet.Has(_entity1));
        Assert.Equal(newComponent, _sparseSet.Get(_entity1));
        Assert.Equal(1, _sparseSet.GetAll().Length);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void SparseSet_ComplexScenario_WorksCorrectly()
    {
        // Add several components
        _sparseSet.Add(_entity1, new TestComponent(10));
        _sparseSet.Add(_entity2, new TestComponent(20));
        _sparseSet.Add(_entity3, new TestComponent(30));
        
        // Remove middle one
        _sparseSet.Remove(_entity2);
        
        // Add new one (should reuse space)
        var entity4 = new Entity(4);
        _sparseSet.Add(entity4, new TestComponent(40));
        
        // Verify state
        Assert.True(_sparseSet.Has(_entity1));
        Assert.False(_sparseSet.Has(_entity2));
        Assert.True(_sparseSet.Has(_entity3));
        Assert.True(_sparseSet.Has(entity4));
        
        Assert.Equal(3, _sparseSet.GetAll().Length);
    }

    [Fact]
    public void SparseSet_HighEntityIds_WorksCorrectly()
    {
        // Create a sparse set that can handle reasonably high entity IDs
        var highCapacitySparseSet = new SparseSet<TestComponent>(100, 100, 1_000_000);
        var highIdEntity = new Entity(999_999);
        var component = new TestComponent(999);
        
        highCapacitySparseSet.Add(highIdEntity, component);
        
        Assert.True(highCapacitySparseSet.Has(highIdEntity));
        Assert.Equal(component, highCapacitySparseSet.Get(highIdEntity));
    }

    [Fact]
    public void TryRemove_ExistingEntity_ReturnsTrue()
    {
        _sparseSet.Add(_entity1, new TestComponent(100));
        
        var result = _sparseSet.TryRemove(_entity1);
        
        Assert.True(result);
        Assert.False(_sparseSet.Has(_entity1));
        Assert.Equal(0, _sparseSet.Count);
    }

    [Fact]
    public void TryRemove_NonExistentEntity_ReturnsFalse()
    {
        var result = _sparseSet.TryRemove(_entity1);
        
        Assert.False(result);
        Assert.Equal(0, _sparseSet.Count);
    }

    [Fact]
    public void RemoveRange_MultipleEntities_RemovesAll()
    {
        _sparseSet.Add(_entity1, new TestComponent(1));
        _sparseSet.Add(_entity2, new TestComponent(2));
        _sparseSet.Add(_entity3, new TestComponent(3));
        
        var entitiesToRemove = new[] { _entity1, _entity3 };
        _sparseSet.RemoveRange(entitiesToRemove);
        
        Assert.False(_sparseSet.Has(_entity1));
        Assert.True(_sparseSet.Has(_entity2));
        Assert.False(_sparseSet.Has(_entity3));
        Assert.Equal(1, _sparseSet.Count);
    }

    [Fact]
    public void RemoveWhere_WithPredicate_RemovesMatchingEntities()
    {
        _sparseSet.Add(_entity1, new TestComponent(10));
        _sparseSet.Add(_entity2, new TestComponent(20));
        _sparseSet.Add(_entity3, new TestComponent(30));
        
        var removedCount = _sparseSet.RemoveWhere((entity, component) => component.Value >= 20);
        
        Assert.Equal(2, removedCount);
        Assert.True(_sparseSet.Has(_entity1));
        Assert.False(_sparseSet.Has(_entity2));
        Assert.False(_sparseSet.Has(_entity3));
        Assert.Equal(1, _sparseSet.Count);
    }

    #endregion
}