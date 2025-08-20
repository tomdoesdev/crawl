using System.Reflection;
using Crawl.ECS;
using Crawl.ECS.Entities;

namespace Crawl.Test;

public class WorldTests : IDisposable
{
    private readonly World _world = new();

    public void Dispose()
    {
        // TODO release managed resources here
    }

    #region Helpers

    private void SetNextId(uint value)
    {
        var entityManager = _world.GetType()
            .GetField("_entityManager", BindingFlags.NonPublic | BindingFlags.Instance)
            ?.GetValue(_world);

        var nextIdField = entityManager?.GetType()
            .GetField("_nextId", BindingFlags.NonPublic | BindingFlags.Instance);

        nextIdField?.SetValue(entityManager, value);
    }

    #endregion

    #region Components

    [Fact]
    public void AddComponent_Single()
    {
        var health = new HealthComponent(10, 100);
        var entity = _world.CreateEntity();

        _world.AddComponent(entity, health);

        Assert.Equal(1, _world.ComponentCount);
    }

    [Fact]
    public void AddComponent_Many()
    {
        var health = new HealthComponent(10, 100);
        var entities = _world.CreateEntity(1000);

        _world.AddComponent(entities, health);

        Assert.Equal(1000, _world.ComponentCount);
    }

    [Fact]
    public void GetComponent_Single()
    {
        var health = new HealthComponent(10, 100);
        var entity = _world.CreateEntity(5);
        _world.AddComponent(entity[0], health);
        _world.AddComponent(entity[1], health);

        var c1 = _world.GetComponent<HealthComponent>(entity[0]);
        c1.SetCurrent(0);
        var c2 = _world.GetComponent<HealthComponent>(entity[1]);
    }

    #endregion


    #region Entity

    [Fact]
    public void CreateEntity_Single()
    {
        _world.CreateEntity();
        Assert.Equal(1, _world.EntityCount);
    }

    [Fact]
    public void CreateEntity_Single_ThrowsIdsExhausted()
    {
        SetNextId(uint.MaxValue);

        Assert.Throws<EntityPoolExhaustedException>(() => _world.CreateEntity());
    }

    [Fact]
    public void CreateEntity_Single_OneIdLeft()
    {
        SetNextId(uint.MaxValue - 1);

        _world.CreateEntity();
    }

    [Fact]
    public void CreateEntity_Many()
    {
        var entities = _world.CreateEntity(5);
        Assert.Equal(5, entities.Length);
        Assert.Equal(5, _world.EntityCount);
    }

    [Fact]
    public void CreateEntity_Many_ThrowsIdsExhausted()
    {
        SetNextId(uint.MaxValue);

        Assert.Throws<EntityPoolExhaustedException>(() => _world.CreateEntity(10));
    }

    [Fact]
    public void CreateEntity_Many_CapacityIdsLeft()
    {
        SetNextId(uint.MaxValue - 10);

        _world.CreateEntity(10);
    }

    [Fact]
    public void DestroyEntity_Single()
    {
        var entity = _world.CreateEntity();
        Assert.Equal(1, _world.EntityCount);
        _world.DestroyEntity(entity);
        Assert.Equal(0, _world.EntityCount);
    }

    [Fact]
    public void HasEntity_Single_Exists()
    {
        var entity = _world.CreateEntity();
        Assert.True(_world.ContainsEntity(entity));
    }

    [Fact]
    public void HasEntity_Single_DoesNotExist()
    {
        var entity = new Entity(999);
        Assert.False(_world.ContainsEntity(entity));
    }


    [Fact]
    public void DestroyEntity_Many()
    {
        var entities = _world.CreateEntity(100);
        Assert.Equal(100, entities.Length);

        var toDestroy = entities.Take(50).ToArray();
        var destroyed = _world.DestroyEntity(toDestroy);
        Assert.True(destroyed);

        Assert.Equal(50, _world.EntityCount);
    }

    #endregion
}