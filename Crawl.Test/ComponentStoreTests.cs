using Crawl.ECS.Components;
using Crawl.ECS.Entities;
using Crawl.Exceptions;

namespace Crawl.Test;

// Test components for ComponentStore tests
public readonly struct PositionComponent(float x, float y) : IComponent
{
    private float X { get; } = x;
    private float Y { get; } = y;
    public ComponentType ComponentType => ComponentType.Position;

    public override bool Equals(object? obj)
    {
        return obj is PositionComponent other && X.Equals(other.X) && Y.Equals(other.Y);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}

public struct HealthComponent(int current, int max) : IComponent
{
    public int Current { get; private set; } = current;
    public int Max { get; } = max;
    public ComponentType ComponentType => ComponentType.Health;

    public void SetCurrent(int newCurrent)
    {
        Current = newCurrent;
    }

    public override bool Equals(object? obj)
    {
        return obj is HealthComponent other && Current == other.Current && Max == other.Max;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Current, Max);
    }
}

public class ComponentStoreTests
{
    private readonly Entity _entity1 = new(1);
    private readonly Entity _entity2 = new(2);
    private readonly Entity _entity3 = new(3);
    private readonly ComponentStore<HealthComponent> _healthStore = new();
    private readonly ComponentStore<PositionComponent> _positionStore = new();

    #region Add Tests

    [Fact]
    public void Add_CorrectComponentType_AddsSuccessfully()
    {
        var position = new PositionComponent(10.5f, 20.3f);

        _positionStore.Add(_entity1, position);

        Assert.True(_positionStore.Has(_entity1));
        var retrieved = _positionStore.Get<PositionComponent>(_entity1);
        Assert.Equal(position, retrieved);
    }

    [Fact]
    public void Add_WrongComponentType_ThrowsArgumentException()
    {
        var health = new HealthComponent(100, 100);

        var exception = Assert.Throws<ArgumentException>(() => _positionStore.Add(_entity1, health));
        Assert.Contains("Expected component of type Crawl.Test.PositionComponent, got Crawl.Test.HealthComponent",
            exception.Message);
    }

    [Fact]
    public void Add_MultipleEntities_StoresAllCorrectly()
    {
        var pos1 = new PositionComponent(1, 2);
        var pos2 = new PositionComponent(3, 4);
        var pos3 = new PositionComponent(5, 6);

        _positionStore.Add(_entity1, pos1);
        _positionStore.Add(_entity2, pos2);
        _positionStore.Add(_entity3, pos3);

        Assert.True(_positionStore.Has(_entity1));
        Assert.True(_positionStore.Has(_entity2));
        Assert.True(_positionStore.Has(_entity3));

        Assert.Equal(pos1, _positionStore.Get<PositionComponent>(_entity1));
        Assert.Equal(pos2, _positionStore.Get<PositionComponent>(_entity2));
        Assert.Equal(pos3, _positionStore.Get<PositionComponent>(_entity3));
    }

    [Fact]
    public void Add_DuplicateEntity_ThrowsDuplicateComponentException()
    {
        var pos1 = new PositionComponent(1, 2);
        var pos2 = new PositionComponent(3, 4);

        _positionStore.Add(_entity1, pos1);

        Assert.Throws<ComponentExistsException>(() => _positionStore.Add(_entity1, pos2));
    }

    [Fact]
    public void Add_ViaInterface_WorksWithCorrectType()
    {
        IComponentStore store = _positionStore;
        var position = new PositionComponent(10, 20);

        store.Add(_entity1, position);

        Assert.True(store.Has(_entity1));
    }

    #endregion

    #region TryAdd Tests

    [Fact]
    public void TryAdd_CorrectComponentType_ReturnsTrue()
    {
        var position = new PositionComponent(10.5f, 20.3f);

        var result = _positionStore.TryAdd(_entity1, position);

        Assert.True(result);
        Assert.True(_positionStore.Has(_entity1));
        Assert.Equal(position, _positionStore.Get<PositionComponent>(_entity1));
    }

    [Fact]
    public void TryAdd_WrongComponentType_ReturnsFalse()
    {
        var health = new HealthComponent(100, 100);

        var result = _positionStore.TryAdd(_entity1, health);

        Assert.False(result);
        Assert.False(_positionStore.Has(_entity1));
    }

    [Fact]
    public void TryAdd_DuplicateEntity_ReturnsFalse()
    {
        var pos1 = new PositionComponent(1, 2);
        var pos2 = new PositionComponent(3, 4);

        _positionStore.TryAdd(_entity1, pos1);
        var result = _positionStore.TryAdd(_entity1, pos2);

        Assert.False(result);
        Assert.Equal(pos1, _positionStore.Get<PositionComponent>(_entity1)); // Original unchanged
    }

    [Fact]
    public void TryAdd_ViaInterface_WorksCorrectly()
    {
        IComponentStore store = _positionStore;
        var position = new PositionComponent(10, 20);
        var health = new HealthComponent(100, 100);

        var result1 = store.TryAdd(_entity1, position);
        var result2 = store.TryAdd(_entity2, health); // Wrong type

        Assert.True(result1);
        Assert.False(result2);
        Assert.True(store.Has(_entity1));
        Assert.False(store.Has(_entity2));
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_ExistingEntity_RemovesComponent()
    {
        var position = new PositionComponent(10, 20);
        _positionStore.Add(_entity1, position);

        _positionStore.Remove(_entity1);

        Assert.False(_positionStore.Has(_entity1));
    }

    [Fact]
    public void Remove_NonExistentEntity_DoesNothing()
    {
        _positionStore.Remove(_entity1); // Should not throw

        Assert.False(_positionStore.Has(_entity1));
    }

    [Fact]
    public void Remove_ViaInterface_WorksCorrectly()
    {
        IComponentStore store = _positionStore;
        var position = new PositionComponent(10, 20);
        store.Add(_entity1, position);

        store.Remove(_entity1);

        Assert.False(store.Has(_entity1));
    }

    [Fact]
    public void Remove_OneOfMany_OnlyRemovesSpecified()
    {
        var pos1 = new PositionComponent(1, 2);
        var pos2 = new PositionComponent(3, 4);
        var pos3 = new PositionComponent(5, 6);

        _positionStore.Add(_entity1, pos1);
        _positionStore.Add(_entity2, pos2);
        _positionStore.Add(_entity3, pos3);

        _positionStore.Remove(_entity2);

        Assert.True(_positionStore.Has(_entity1));
        Assert.False(_positionStore.Has(_entity2));
        Assert.True(_positionStore.Has(_entity3));
    }

    #endregion

    #region Get Tests

    [Fact]
    public void Get_ExistingEntity_ReturnsCorrectComponent()
    {
        var position = new PositionComponent(42.5f, 99.1f);
        _positionStore.Add(_entity1, position);

        var result = _positionStore.Get<PositionComponent>(_entity1);

        Assert.Equal(position, result);
    }

    [Fact]
    public void Get_NonExistentEntity_ThrowsComponentNotFoundException()
    {
        Assert.Throws<ComponentNotFoundException>(() => _positionStore.Get<PositionComponent>(_entity1));
    }

    [Fact]
    public void Get_WrongGenericType_ThrowsInvalidCastException()
    {
        var position = new PositionComponent(10, 20);
        _positionStore.Add(_entity1, position);

        // Trying to get HealthComponent from PositionComponent store
        Assert.Throws<InvalidCastException>(() => _positionStore.Get<HealthComponent>(_entity1));
    }

    [Fact]
    public void Get_MultipleEntities_ReturnsCorrectComponents()
    {
        var pos1 = new PositionComponent(1, 2);
        var pos2 = new PositionComponent(3, 4);

        _positionStore.Add(_entity1, pos1);
        _positionStore.Add(_entity2, pos2);

        Assert.Equal(pos1, _positionStore.Get<PositionComponent>(_entity1));
        Assert.Equal(pos2, _positionStore.Get<PositionComponent>(_entity2));
    }

    #endregion

    #region TryGet Tests

    [Fact]
    public void TryGet_ExistingEntity_ReturnsTrueWithComponent()
    {
        var position = new PositionComponent(10, 20);
        _positionStore.Add(_entity1, position);

        var result = _positionStore.TryGet(_entity1, out var component);

        Assert.True(result);
        Assert.Equal(position, component);
    }

    [Fact]
    public void TryGet_NonExistentEntity_ReturnsFalseWithNull()
    {
        var result = _positionStore.TryGet(_entity1, out var component);

        Assert.False(result);
        Assert.Null(component);
    }

    [Fact]
    public void TryGet_ViaInterface_WorksCorrectly()
    {
        IComponentStore store = _positionStore;
        var position = new PositionComponent(10, 20);
        store.Add(_entity1, position);

        var result = store.TryGet(_entity1, out var component);

        Assert.True(result);
        Assert.Equal(position, component);
    }

    [Fact]
    public void TryGet_ReturnsBoxedComponent()
    {
        var position = new PositionComponent(10, 20);
        _positionStore.Add(_entity1, position);

        _positionStore.TryGet(_entity1, out var component);

        // Component should be boxed to IComponent
        Assert.IsAssignableFrom<IComponent>(component);
        Assert.IsType<PositionComponent>(component);
    }

    #endregion

    #region Has Tests

    [Fact]
    public void Has_ExistingEntity_ReturnsTrue()
    {
        var position = new PositionComponent(10, 20);
        _positionStore.Add(_entity1, position);

        Assert.True(_positionStore.Has(_entity1));
    }

    [Fact]
    public void Has_NonExistentEntity_ReturnsFalse()
    {
        Assert.False(_positionStore.Has(_entity1));
    }

    [Fact]
    public void Has_AfterRemoval_ReturnsFalse()
    {
        var position = new PositionComponent(10, 20);
        _positionStore.Add(_entity1, position);
        _positionStore.Remove(_entity1);

        Assert.False(_positionStore.Has(_entity1));
    }

    [Fact]
    public void Has_ViaInterface_WorksCorrectly()
    {
        IComponentStore store = _positionStore;
        var position = new PositionComponent(10, 20);
        store.Add(_entity1, position);

        Assert.True(store.Has(_entity1));
    }

    #endregion

    #region GetAll Tests

    [Fact]
    public void GetAll_EmptyStore_ReturnsEmptySpan()
    {
        var result = _positionStore.GetAll();

        Assert.Equal(0, result.Length);
    }

    [Fact]
    public void GetAll_WithComponents_ReturnsAllComponents()
    {
        var pos1 = new PositionComponent(1, 2);
        var pos2 = new PositionComponent(3, 4);
        var pos3 = new PositionComponent(5, 6);

        _positionStore.Add(_entity1, pos1);
        _positionStore.Add(_entity2, pos2);
        _positionStore.Add(_entity3, pos3);

        var result = _positionStore.GetAll();

        Assert.Equal(3, result.Length);

        var resultArray = result.ToArray();
        Assert.Contains(pos1, resultArray);
        Assert.Contains(pos2, resultArray);
        Assert.Contains(pos3, resultArray);
    }

    [Fact]
    public void GetAll_AfterRemoval_ReturnsUpdatedComponents()
    {
        var pos1 = new PositionComponent(1, 2);
        var pos2 = new PositionComponent(3, 4);
        var pos3 = new PositionComponent(5, 6);

        _positionStore.Add(_entity1, pos1);
        _positionStore.Add(_entity2, pos2);
        _positionStore.Add(_entity3, pos3);

        _positionStore.Remove(_entity2);

        var result = _positionStore.GetAll();

        Assert.Equal(2, result.Length);

        var resultArray = result.ToArray();
        Assert.Contains(pos1, resultArray);
        Assert.Contains(pos3, resultArray);
        Assert.DoesNotContain(pos2, resultArray);
    }

    [Fact]
    public void GetAll_ViaInterface_ReturnsBoxedComponents()
    {
        IComponentStore store = _positionStore;
        var position = new PositionComponent(10, 20);
        store.Add(_entity1, position);

        var result = store.GetAll();

        Assert.Equal(1, result.Length);
        Assert.IsAssignableFrom<IComponent>(result[0]);
        Assert.IsType<PositionComponent>(result[0]);
        Assert.Equal(position, result[0]);
    }

    [Fact]
    public void GetAll_ReturnsNewArrayEachTime()
    {
        var position = new PositionComponent(10, 20);
        _positionStore.Add(_entity1, position);

        var result1 = _positionStore.GetAll();
        var result2 = _positionStore.GetAll();

        // Should be different array instances but same content
        Assert.NotSame(result1.ToArray(), result2.ToArray());
        Assert.Equal(result1.ToArray(), result2.ToArray());
    }

    #endregion

    #region Interface Abstraction Tests

    [Fact]
    public void ComponentStore_ImplementsInterface_Correctly()
    {
        IComponentStore store = _positionStore;

        Assert.IsAssignableFrom<IComponentStore>(store);
    }

    [Fact]
    public void Interface_AllMethods_WorkAsExpected()
    {
        IComponentStore store = _positionStore;
        var position = new PositionComponent(10, 20);

        // Add
        store.Add(_entity1, position);
        Assert.True(store.Has(_entity1));

        // TryAdd duplicate
        Assert.False(store.TryAdd(_entity1, position));

        // TryGet
        Assert.True(store.TryGet(_entity1, out var component));
        Assert.Equal(position, component);

        // GetAll
        var all = store.GetAll();
        Assert.Equal(1, all.Length);

        // Remove
        store.Remove(_entity1);
        Assert.False(store.Has(_entity1));
    }

    [Fact]
    public void TypeSafety_PreventsMixingComponentTypes()
    {
        var positionStore = new ComponentStore<PositionComponent>();
        var healthStore = new ComponentStore<HealthComponent>();

        var position = new PositionComponent(10, 20);
        var health = new HealthComponent(100, 100);

        // Correct types work
        positionStore.Add(_entity1, position);
        healthStore.Add(_entity1, health);

        // Wrong types fail
        Assert.Throws<ArgumentException>(() => positionStore.Add(_entity2, health));
        Assert.Throws<ArgumentException>(() => healthStore.Add(_entity2, position));

        // TryAdd with wrong types return false
        Assert.False(positionStore.TryAdd(_entity2, health));
        Assert.False(healthStore.TryAdd(_entity2, position));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ComponentStore_ComplexScenario_WorksCorrectly()
    {
        var pos1 = new PositionComponent(10, 20);
        var pos2 = new PositionComponent(30, 40);
        var pos3 = new PositionComponent(50, 60);

        // Add components
        _positionStore.Add(_entity1, pos1);
        _positionStore.Add(_entity2, pos2);
        _positionStore.Add(_entity3, pos3);

        // Verify all added
        Assert.Equal(3, _positionStore.GetAll().Length);

        // Remove middle entity
        _positionStore.Remove(_entity2);

        // Verify removal
        Assert.Equal(2, _positionStore.GetAll().Length);
        Assert.False(_positionStore.Has(_entity2));

        // Try to get removed entity
        Assert.Throws<ComponentNotFoundException>(() => _positionStore.Get<PositionComponent>(_entity2));

        // Verify others still exist
        Assert.Equal(pos1, _positionStore.Get<PositionComponent>(_entity1));
        Assert.Equal(pos3, _positionStore.Get<PositionComponent>(_entity3));

        // Add component back to same entity
        var newPos2 = new PositionComponent(100, 200);
        _positionStore.Add(_entity2, newPos2);

        Assert.Equal(3, _positionStore.GetAll().Length);
        Assert.Equal(newPos2, _positionStore.Get<PositionComponent>(_entity2));
    }

    [Fact]
    public void ComponentStore_WithManyEntities_PerformsWell()
    {
        // Add many components
        for (uint i = 1; i <= 1000; i++)
        {
            var entity = new Entity(i);
            var position = new PositionComponent(i, i * 2);
            _positionStore.Add(entity, position);
        }

        Assert.Equal(1000, _positionStore.GetAll().Length);

        // Verify random access
        var testEntity = new Entity(500);
        var expectedPosition = new PositionComponent(500, 1000);
        Assert.Equal(expectedPosition, _positionStore.Get<PositionComponent>(testEntity));

        // Remove some entities
        for (uint i = 1; i <= 100; i++) _positionStore.Remove(new Entity(i));

        Assert.Equal(900, _positionStore.GetAll().Length);
        Assert.False(_positionStore.Has(new Entity(50)));
        Assert.True(_positionStore.Has(new Entity(150)));
    }

    #endregion
}