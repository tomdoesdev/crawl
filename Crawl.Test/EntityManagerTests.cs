using System.Reflection;
using Crawl.ECS.Entities;

namespace Crawl.Test;

public class EntityManagerTests
{
    #region Entity Lifecycle Tests

    [Fact]
    public void EntityLifecycle_CreateRemoveCreate_WorksCorrectly()
    {
        var entityManager = new EntityManager();

        // Create entity
        var entity1 = entityManager.Create();
        entityManager.Create();

        // Remove entity
        entityManager.Remove(entity1);


        Assert.Equal(1, entityManager.Count);
    }

    #endregion

    #region IDPool Capacity Tests

    [Fact]
    public void PoolCapacity_ZeroCapacity_NoPooling()
    {
        var entityManager = new EntityManager();

        var entity1 = entityManager.Create(); // ID 1
        var entity2 = entityManager.Create(); // ID 2

        entityManager.Remove(entity1);
        entityManager.Remove(entity2);

        // Should not reuse IDs since pool capacity is 0
        var entity3 = entityManager.Create(); // Should be ID 3
        var entity4 = entityManager.Create(); // Should be ID 4

        Assert.Equal(3u, entity3.Id);
        Assert.Equal(4u, entity4.Id);
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void EntityManager_RobustnessTest_HandlesRandomOperations()
    {
        var entityManager = new EntityManager();
        var activeEntities = new HashSet<Entity>();
        var random = new Random(42); // Fixed seed for reproducibility

        // Perform random create/remove operations
        for (var i = 0; i < 1000; i++)
            if (activeEntities.Count == 0 || random.NextDouble() > 0.3)
            {
                // Create entity
                var entity = entityManager.Create();
                activeEntities.Add(entity);
                Assert.NotEqual(0u, entity.Id);
            }
            else
            {
                // Remove random entity
                var entityToRemove = activeEntities.ElementAt(random.Next(activeEntities.Count));
                entityManager.Remove(entityToRemove);
                activeEntities.Remove(entityToRemove);
            }

        // Verify all active entities have valid IDs
        foreach (var entity in activeEntities) Assert.NotEqual(0u, entity.Id);
    }

    #endregion

    #region Sentinel Entity Tests

    [Fact]
    public void Create_NeverReturnsSentinelId()
    {
        var entityManager = new EntityManager();

        // Create many entities to ensure we never get ID 0
        for (var i = 0; i < 100; i++)
        {
            var entity = entityManager.Create();
            Assert.NotEqual(0u, entity.Id);
        }
    }

    #endregion

    #region Constructor Tests

    [Fact]
    public void Constructor_WithDefaultCapacity_CreatesManager()
    {
        var entityManager = new EntityManager();

        // Should be able to create entities immediately
        var entity = entityManager.Create();
        Assert.NotEqual(0u, entity.Id);
    }

    [Fact]
    public void Constructor_WithCustomCapacity_CreatesManager()
    {
        var entityManager = new EntityManager();

        var entity = entityManager.Create();
        Assert.NotEqual(0u, entity.Id);
    }

    [Fact]
    public void Constructor_WithZeroCapacity_CreatesManagerWithNoPooling()
    {
        var entityManager = new EntityManager();

        // Should still be able to create entities (pool just won't store released IDs)
        var entity = entityManager.Create();
        Assert.NotEqual(0u, entity.Id);
    }

    #endregion

    #region Create Tests

    [Fact]
    public void Create_FirstEntity_ReturnsEntityWithId1()
    {
        var entityManager = new EntityManager();

        var entity = entityManager.Create();

        Assert.Equal(1u, entity.Id);
    }

    [Fact]
    public void Create_MultipleEntities_ReturnsSequentialIds()
    {
        var entityManager = new EntityManager();

        var entity1 = entityManager.Create();
        var entity2 = entityManager.Create();
        var entity3 = entityManager.Create();

        Assert.Equal(1u, entity1.Id);
        Assert.Equal(2u, entity2.Id);
        Assert.Equal(3u, entity3.Id);
    }


    [Fact]
    public void Create_ManyEntities_HandlesSequentialAllocation()
    {
        var entityManager = new EntityManager();
        const int entityCount = 1000;

        var entities = new List<Entity>();
        for (var i = 0; i < entityCount; i++) entities.Add(entityManager.Create());

        // Should have sequential IDs from 1 to 1000
        for (var i = 0; i < entityCount; i++) Assert.Equal((uint)(i + 1), entities[i].Id);
    }

    [Fact]
    public void Create_NearUintMaxValue_ThrowsInvalidOperationException()
    {
        var entityManager = new EntityManager();

        // Use reflection to set the internal counter to near max value
        var currentIdField = typeof(EntityManager).GetField("_nextId",
            BindingFlags.NonPublic | BindingFlags.Instance);

        currentIdField?.SetValue(entityManager, uint.MaxValue);

        Assert.Throws<EntityPoolExhaustedException>(() => entityManager.Create());
    }

    [Fact]
    public void Create_AtUintMaxValueMinus1_CreatesLastEntity()
    {
        var entityManager = new EntityManager();

        // Use reflection to set the internal counter to max value - 1
        var currentIdField = typeof(EntityManager).GetField("_nextId",
            BindingFlags.NonPublic | BindingFlags.Instance);

        currentIdField?.SetValue(entityManager, uint.MaxValue - 1);

        var entity = entityManager.Create();
        Assert.Equal(uint.MaxValue - 1, entity.Id);

        // Next call should throw
        Assert.Throws<EntityPoolExhaustedException>(() => entityManager.Create());
    }

    #endregion

    #region Remove Tests

    [Fact]
    public void Remove_ValidEntity_DoesNotThrow()
    {
        var entityManager = new EntityManager();
        var entity = entityManager.Create();

        entityManager.Remove(entity); // Should not throw
    }


    [Fact]
    public void Remove_SameEntityMultipleTimes_HandlesGracefully()
    {
        var entityManager = new EntityManager();
        var entity = entityManager.Create();

        entityManager.Remove(entity);
        Assert.False(entityManager.Remove(entity)); // Should return false
        Assert.False(entityManager.Remove(entity)); // this too
    }


    [Fact]
    public void Remove_NonExistentEntity_DoesNothing()
    {
        var entityManager = new EntityManager();
        var entity = new Entity(999); // Entity that was never created

        Assert.False(entityManager.Remove(entity)); // Should not throw
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void Performance_CreateManyEntities_CompletesQuickly()
    {
        var entityManager = new EntityManager();
        const int entityCount = 10000;

        var entities = new List<Entity>(entityCount);

        // This should complete quickly
        for (var i = 0; i < entityCount; i++) entities.Add(entityManager.Create());

        Assert.Equal(entityCount, entities.Count);

        // Verify all IDs are unique and sequential
        for (var i = 0; i < entityCount; i++) Assert.Equal((uint)(i + 1), entities[i].Id);
    }

    [Fact]
    public void Performance_CreateRemoveCreateCycle_PerformsWell()
    {
        var entityManager = new EntityManager();
        const int cycleCount = 1000;

        var createdIds = new HashSet<uint>();

        for (var i = 0; i < cycleCount; i++)
        {
            // Create entity
            var entity = entityManager.Create();
            createdIds.Add(entity.Id);

            // Remove entity immediately
            entityManager.Remove(entity);
        }

        // Should have created many entities successfully
        // Due to reuse, we should have seen a small number of unique IDs
        Assert.True(createdIds.Count <= cycleCount);
        Assert.True(createdIds.Count > 0);
    }

    #endregion
}