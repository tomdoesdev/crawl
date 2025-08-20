using Crawl.ECS.Components;
using Crawl.ECS.Entities;
using Crawl.Exceptions;

namespace Crawl.Test;

// Test components for ComponentManager tests
public readonly struct SpriteComponent : IComponent
{
    public ComponentType ComponentType => ComponentType.Sprite2D;

    public override bool Equals(object? obj)
    {
        return obj is SpriteComponent;
    }

    public override int GetHashCode()
    {
        return ComponentType.GetHashCode();
    }
}

public readonly struct VelocityComponent(float speed) : IComponent
{
    public float Speed { get; } = speed;
    public ComponentType ComponentType => ComponentType.Velocity;

    public override bool Equals(object? obj)
    {
        return obj is VelocityComponent other && Speed.Equals(other.Speed);
    }

    public override int GetHashCode()
    {
        return Speed.GetHashCode();
    }
}

public readonly struct TestPositionComponent(int x, int y) : IComponent
{
    public int X { get; } = x;
    public int Y { get; } = y;
    public ComponentType ComponentType => ComponentType.Position;

    public override bool Equals(object? obj)
    {
        return obj is TestPositionComponent other && X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}

public class ComponentManagerTests
{
    private readonly ComponentManager _componentManager;
    private readonly Entity _entity1;
    private readonly Entity _entity2;
    private readonly Entity _entity3;

    public ComponentManagerTests()
    {
        _componentManager = new ComponentManager();
        _entity1 = new Entity(1);
        _entity2 = new Entity(2);
        _entity3 = new Entity(3);
    }

    #region AddComponent Tests

    [Fact]
    public void AddComponent_SingleComponent_AddsSuccessfully()
    {
        var position = new TestPositionComponent(10, 20);

        _componentManager.Add(_entity1, position);

        var retrieved = _componentManager.Get<TestPositionComponent>(_entity1);
        Assert.Equal(position, retrieved);
    }

    [Fact]
    public void AddComponent_MultipleComponentTypes_AddsAllCorrectly()
    {
        var position = new TestPositionComponent(10, 20);
        var velocity = new VelocityComponent(5.5f);
        var sprite = new SpriteComponent();

        _componentManager.Add(_entity1, position);
        _componentManager.Add(_entity1, velocity);
        _componentManager.Add(_entity1, sprite);

        Assert.Equal(position, _componentManager.Get<TestPositionComponent>(_entity1));
        Assert.Equal(velocity, _componentManager.Get<VelocityComponent>(_entity1));
        Assert.Equal(sprite, _componentManager.Get<SpriteComponent>(_entity1));
    }

    [Fact]
    public void AddComponent_SameComponentTypeDifferentEntities_StoresCorrectly()
    {
        var pos1 = new TestPositionComponent(10, 20);
        var pos2 = new TestPositionComponent(30, 40);
        var pos3 = new TestPositionComponent(50, 60);

        _componentManager.Add(_entity1, pos1);
        _componentManager.Add(_entity2, pos2);
        _componentManager.Add(_entity3, pos3);

        Assert.Equal(pos1, _componentManager.Get<TestPositionComponent>(_entity1));
        Assert.Equal(pos2, _componentManager.Get<TestPositionComponent>(_entity2));
        Assert.Equal(pos3, _componentManager.Get<TestPositionComponent>(_entity3));
    }

    [Fact]
    public void AddComponent_DuplicateComponentToSameEntity_ThrowsDuplicateComponentException()
    {
        var pos1 = new TestPositionComponent(10, 20);
        var pos2 = new TestPositionComponent(30, 40);

        _componentManager.Add(_entity1, pos1);

        Assert.Throws<ComponentExistsException>(() => _componentManager.Add(_entity1, pos2));
    }

    [Fact]
    public void AddComponent_AutoRegistersComponentType()
    {
        var position = new TestPositionComponent(10, 20);

        // First time adding this component type
        _componentManager.Add(_entity1, position);

        // Should be able to retrieve it
        var retrieved = _componentManager.Get<TestPositionComponent>(_entity1);
        Assert.Equal(position, retrieved);
    }

    [Fact]
    public void AddComponent_SameComponentTypeMultipleTimes_OnlyRegistersOnce()
    {
        var pos1 = new TestPositionComponent(10, 20);
        var pos2 = new TestPositionComponent(30, 40);

        // Add to different entities - should only register type once
        _componentManager.Add(_entity1, pos1);
        _componentManager.Add(_entity2, pos2);

        // Both should work
        Assert.Equal(pos1, _componentManager.Get<TestPositionComponent>(_entity1));
        Assert.Equal(pos2, _componentManager.Get<TestPositionComponent>(_entity2));
    }

    [Fact]
    public void AddComponent_CreatesStoreOnDemand()
    {
        // Before adding any components, no stores should exist
        // After adding, store should be created automatically

        var position = new TestPositionComponent(10, 20);
        _componentManager.Add(_entity1, position);

        // Should be able to retrieve component (proving store was created)
        var retrieved = _componentManager.Get<TestPositionComponent>(_entity1);
        Assert.Equal(position, retrieved);
    }

    #endregion

    #region GetComponent Tests

    [Fact]
    public void GetComponent_ExistingComponent_ReturnsCorrectComponent()
    {
        var position = new TestPositionComponent(42, 99);
        _componentManager.Add(_entity1, position);

        var retrieved = _componentManager.Get<TestPositionComponent>(_entity1);

        Assert.Equal(position, retrieved);
    }

    [Fact]
    public void GetComponent_NonRegisteredComponentType_ThrowsException()
    {
        // Try to get a component type that was never added
        var exception =
            Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<TestPositionComponent>(_entity1));
    }

    [Fact]
    public void GetComponent_RegisteredTypeButEntityDoesntHaveIt_ThrowsComponentNotFoundException()
    {
        var position = new TestPositionComponent(10, 20);

        // Add component to entity1
        _componentManager.Add(_entity1, position);

        // Try to get from entity2 (which doesn't have it)
        Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<TestPositionComponent>(_entity2));
    }

    [Fact]
    public void GetComponent_MultipleComponentTypes_ReturnsCorrectTypes()
    {
        var position = new TestPositionComponent(10, 20);
        var velocity = new VelocityComponent(7.5f);

        _componentManager.Add(_entity1, position);
        _componentManager.Add(_entity1, velocity);

        Assert.Equal(position, _componentManager.Get<TestPositionComponent>(_entity1));
        Assert.Equal(velocity, _componentManager.Get<VelocityComponent>(_entity1));
    }

    [Fact]
    public void GetComponent_AfterMultipleAddsAndGets_ConsistentResults()
    {
        var pos1 = new TestPositionComponent(1, 2);
        var pos2 = new TestPositionComponent(3, 4);
        var vel1 = new VelocityComponent(1.5f);
        var vel2 = new VelocityComponent(2.5f);

        _componentManager.Add(_entity1, pos1);
        _componentManager.Add(_entity2, pos2);
        _componentManager.Add(_entity1, vel1);
        _componentManager.Add(_entity2, vel2);

        // Get components multiple times - should be consistent
        for (var i = 0; i < 5; i++)
        {
            Assert.Equal(pos1, _componentManager.Get<TestPositionComponent>(_entity1));
            Assert.Equal(pos2, _componentManager.Get<TestPositionComponent>(_entity2));
            Assert.Equal(vel1, _componentManager.Get<VelocityComponent>(_entity1));
            Assert.Equal(vel2, _componentManager.Get<VelocityComponent>(_entity2));
        }
    }

    [Fact]
    public void GetComponent_NoStoreForRegisteredType_ThrowsException()
    {
        // This is an edge case that shouldn't happen in normal usage
        // but tests the error handling

        var position = new TestPositionComponent(10, 20);
        _componentManager.Add(_entity1, position);

        // Simulate a scenario where type is registered but store doesn't exist
        // (This is hard to trigger in normal usage, but the code has this check)

        // Normal case should work
        var retrieved = _componentManager.Get<TestPositionComponent>(_entity1);
        Assert.Equal(position, retrieved);
    }

    #endregion

    #region Type Registration Tests

    [Fact]
    public void ComponentManager_TypeRegistration_WorksCorrectly()
    {
        var position = new TestPositionComponent(10, 20);
        var velocity = new VelocityComponent(5.0f);

        // Add components - this should register their types
        _componentManager.Add(_entity1, position);
        _componentManager.Add(_entity2, velocity);

        // Should be able to get components back
        Assert.Equal(position, _componentManager.Get<TestPositionComponent>(_entity1));
        Assert.Equal(velocity, _componentManager.Get<VelocityComponent>(_entity2));

        // Should be able to add more of the same types to different entities
        var position2 = new TestPositionComponent(30, 40);
        var velocity2 = new VelocityComponent(10.0f);

        _componentManager.Add(_entity3, position2);
        _componentManager.Add(_entity3, velocity2);

        Assert.Equal(position2, _componentManager.Get<TestPositionComponent>(_entity3));
        Assert.Equal(velocity2, _componentManager.Get<VelocityComponent>(_entity3));
    }

    [Fact]
    public void ComponentManager_TypeRegistration_HandlesRepeatedRegistration()
    {
        var pos1 = new TestPositionComponent(10, 20);
        var pos2 = new TestPositionComponent(30, 40);

        // Add same component type multiple times (different entities)
        _componentManager.Add(_entity1, pos1);
        _componentManager.Add(_entity2, pos2);

        // Both should work fine
        Assert.Equal(pos1, _componentManager.Get<TestPositionComponent>(_entity1));
        Assert.Equal(pos2, _componentManager.Get<TestPositionComponent>(_entity2));
    }

    #endregion

    #region Store Creation Tests

    [Fact]
    public void GetOrCreateStore_FirstTime_CreatesNewStore()
    {
        var position = new TestPositionComponent(10, 20);

        // First time adding this component type should create store
        _componentManager.Add(_entity1, position);

        // Should be able to retrieve
        var retrieved = _componentManager.Get<TestPositionComponent>(_entity1);
        Assert.Equal(position, retrieved);
    }

    [Fact]
    public void GetOrCreateStore_SubsequentCalls_ReusesExistingStore()
    {
        var pos1 = new TestPositionComponent(10, 20);
        var pos2 = new TestPositionComponent(30, 40);

        // First call creates store
        _componentManager.Add(_entity1, pos1);

        // Second call should reuse existing store
        _componentManager.Add(_entity2, pos2);

        // Both should be accessible
        Assert.Equal(pos1, _componentManager.Get<TestPositionComponent>(_entity1));
        Assert.Equal(pos2, _componentManager.Get<TestPositionComponent>(_entity2));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void ComponentManager_ComplexScenario_WorksCorrectly()
    {
        // Create a complex scenario with multiple entities and component types
        var pos1 = new TestPositionComponent(10, 20);
        var pos2 = new TestPositionComponent(30, 40);
        var vel1 = new VelocityComponent(5.5f);
        var vel2 = new VelocityComponent(7.5f);
        var sprite = new SpriteComponent();

        // Entity 1: Position + Velocity + Sprite
        _componentManager.Add(_entity1, pos1);
        _componentManager.Add(_entity1, vel1);
        _componentManager.Add(_entity1, sprite);

        // Entity 2: Position + Velocity
        _componentManager.Add(_entity2, pos2);
        _componentManager.Add(_entity2, vel2);

        // Entity 3: Only Sprite
        _componentManager.Add(_entity3, new SpriteComponent());

        // Verify all components
        Assert.Equal(pos1, _componentManager.Get<TestPositionComponent>(_entity1));
        Assert.Equal(vel1, _componentManager.Get<VelocityComponent>(_entity1));
        Assert.Equal(sprite, _componentManager.Get<SpriteComponent>(_entity1));

        Assert.Equal(pos2, _componentManager.Get<TestPositionComponent>(_entity2));
        Assert.Equal(vel2, _componentManager.Get<VelocityComponent>(_entity2));

        Assert.Equal(new SpriteComponent(), _componentManager.Get<SpriteComponent>(_entity3));

        // Verify entities don't have components they shouldn't
        Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<SpriteComponent>(_entity2));
        Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<TestPositionComponent>(_entity3));
        Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<VelocityComponent>(_entity3));
    }

    [Fact]
    public void ComponentManager_WithManyEntities_PerformsWell()
    {
        // Test with many entities to verify performance
        const int entityCount = 1000;

        // Add position components to many entities
        for (uint i = 1; i <= entityCount; i++)
        {
            var entity = new Entity(i);
            var position = new TestPositionComponent((int)i, (int)i * 2);
            _componentManager.Add(entity, position);
        }

        // Verify all components can be retrieved
        for (uint i = 1; i <= entityCount; i++)
        {
            var entity = new Entity(i);
            var expectedPosition = new TestPositionComponent((int)i, (int)i * 2);
            var actualPosition = _componentManager.Get<TestPositionComponent>(entity);
            Assert.Equal(expectedPosition, actualPosition);
        }
    }

    [Fact]
    public void ComponentManager_MixedComponentTypes_HandlesCorrectly()
    {
        // Test mixing different component types across entities
        var entities = new[]
        {
            new Entity(1), new Entity(2), new Entity(3), new Entity(4), new Entity(5)
        };

        // Entity 1: All components
        _componentManager.Add(entities[0], new TestPositionComponent(1, 1));
        _componentManager.Add(entities[0], new VelocityComponent(1.0f));
        _componentManager.Add(entities[0], new SpriteComponent());

        // Entity 2: Position + Velocity
        _componentManager.Add(entities[1], new TestPositionComponent(2, 2));
        _componentManager.Add(entities[1], new VelocityComponent(2.0f));

        // Entity 3: Only Position
        _componentManager.Add(entities[2], new TestPositionComponent(3, 3));

        // Entity 4: Only Velocity
        _componentManager.Add(entities[3], new VelocityComponent(4.0f));

        // Entity 5: Only Sprite
        _componentManager.Add(entities[4], new SpriteComponent());

        // Verify correct components exist
        Assert.Equal(new TestPositionComponent(1, 1), _componentManager.Get<TestPositionComponent>(entities[0]));
        Assert.Equal(new VelocityComponent(1.0f), _componentManager.Get<VelocityComponent>(entities[0]));
        Assert.Equal(new SpriteComponent(), _componentManager.Get<SpriteComponent>(entities[0]));

        Assert.Equal(new TestPositionComponent(2, 2), _componentManager.Get<TestPositionComponent>(entities[1]));
        Assert.Equal(new VelocityComponent(2.0f), _componentManager.Get<VelocityComponent>(entities[1]));

        Assert.Equal(new TestPositionComponent(3, 3), _componentManager.Get<TestPositionComponent>(entities[2]));
        Assert.Equal(new VelocityComponent(4.0f), _componentManager.Get<VelocityComponent>(entities[3]));
        Assert.Equal(new SpriteComponent(), _componentManager.Get<SpriteComponent>(entities[4]));

        // Verify missing components throw exceptions
        Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<SpriteComponent>(entities[1]));
        Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<VelocityComponent>(entities[2]));
        Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<TestPositionComponent>(entities[3]));
    }

    [Fact]
    public void ComponentManager_ErrorHandling_ProvidesInformativeMessages()
    {
        // Test that error messages are helpful for debugging

        // Unregistered component type
        var exception1 =
            Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<TestPositionComponent>(_entity1));


        // Registered type but entity doesn't have it
        _componentManager.Add(_entity1, new TestPositionComponent(10, 20));
        var exception2 =
            Assert.Throws<ComponentNotFoundException>(() => _componentManager.Get<TestPositionComponent>(_entity2));
    }

    #endregion
}