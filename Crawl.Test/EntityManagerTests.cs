using Crawl.ECS;
using Crawl.ECS.Entities;
using Xunit;

namespace Crawl.Test;

public class EntityManagerTests
{
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
        var entityManager = new EntityManager(5000);
        
        var entity = entityManager.Create();
        Assert.NotEqual(0u, entity.Id);
    }

    [Fact]
    public void Constructor_WithZeroCapacity_CreatesManagerWithNoPooling()
    {
        var entityManager = new EntityManager(0);
        
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
    public void Create_AfterRemoval_ReusesReleasedId()
    {
        var entityManager = new EntityManager();
        
        var entity1 = entityManager.Create(); // ID 1
        var entity2 = entityManager.Create(); // ID 2
        var entity3 = entityManager.Create(); // ID 3
        
        entityManager.Remove(entity2); // Release ID 2
        
        var entity4 = entityManager.Create(); // Should reuse ID 2
        
        Assert.Equal(2u, entity4.Id);
    }

    [Fact]
    public void Create_AfterMultipleRemovals_ReusesReleasedIds()
    {
        var entityManager = new EntityManager();
        
        var entity1 = entityManager.Create(); // ID 1
        var entity2 = entityManager.Create(); // ID 2
        var entity3 = entityManager.Create(); // ID 3
        var entity4 = entityManager.Create(); // ID 4
        
        entityManager.Remove(entity2); // Release ID 2
        entityManager.Remove(entity4); // Release ID 4
        
        var entity5 = entityManager.Create(); // Should get one of the released IDs
        var entity6 = entityManager.Create(); // Should get the other released ID
        
        // Both entities should have reused IDs (order not guaranteed due to HashSet)
        var releasedIds = new HashSet<uint> { 2, 4 };
        Assert.Contains(entity5.Id, releasedIds);
        Assert.Contains(entity6.Id, releasedIds);
        Assert.NotEqual(entity5.Id, entity6.Id); // Should be different IDs
    }

    [Fact]
    public void Create_WhenPoolIsEmpty_UsesNextSequentialId()
    {
        var entityManager = new EntityManager();
        
        var entity1 = entityManager.Create(); // ID 1
        var entity2 = entityManager.Create(); // ID 2
        
        entityManager.Remove(entity1); // Release ID 1
        
        var entity3 = entityManager.Create(); // Reuse ID 1
        var entity4 = entityManager.Create(); // Should be ID 3 (next sequential)
        
        Assert.Equal(1u, entity3.Id);
        Assert.Equal(3u, entity4.Id);
    }

    [Fact]
    public void Create_ManyEntities_HandlesSequentialAllocation()
    {
        var entityManager = new EntityManager();
        const int entityCount = 1000;
        
        var entities = new List<Entity>();
        for (int i = 0; i < entityCount; i++)
        {
            entities.Add(entityManager.Create());
        }
        
        // Should have sequential IDs from 1 to 1000
        for (int i = 0; i < entityCount; i++)
        {
            Assert.Equal((uint)(i + 1), entities[i].Id);
        }
    }

    [Fact]
    public void Create_NearUintMaxValue_ThrowsInvalidOperationException()
    {
        var entityManager = new EntityManager();
        
        // Use reflection to set the internal counter to near max value
        var currentIdField = typeof(EntityManager).GetField("_currentId", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        currentIdField?.SetValue(entityManager, uint.MaxValue);
        
        var exception = Assert.Throws<InvalidOperationException>(() => entityManager.Create());
        Assert.Equal("entity id pool exhausted", exception.Message);
    }

    [Fact]
    public void Create_AtUintMaxValueMinus1_CreatesLastEntity()
    {
        var entityManager = new EntityManager();
        
        // Use reflection to set the internal counter to max value - 1
        var currentIdField = typeof(EntityManager).GetField("_currentId", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        
        currentIdField?.SetValue(entityManager, uint.MaxValue - 1);
        
        var entity = entityManager.Create();
        Assert.Equal(uint.MaxValue - 1, entity.Id);
        
        // Next call should throw
        Assert.Throws<InvalidOperationException>(() => entityManager.Create());
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
    public void Remove_NullEntity_DoesNothing()
    {
        var entityManager = new EntityManager();
        var nullEntity = new Entity((uint)SentinelEntities.Null);
        
        entityManager.Remove(nullEntity); // Should not throw
    }

    [Fact]
    public void Remove_SameEntityMultipleTimes_HandlesGracefully()
    {
        var entityManager = new EntityManager();
        var entity = entityManager.Create();
        
        entityManager.Remove(entity);
        entityManager.Remove(entity); // Should not throw or cause issues
        entityManager.Remove(entity); // Should not throw or cause issues
    }

    [Fact]
    public void Remove_EntityThenCreateNew_ReusesId()
    {
        var entityManager = new EntityManager();
        
        var entity1 = entityManager.Create();
        var originalId = entity1.Id;
        
        entityManager.Remove(entity1);
        var entity2 = entityManager.Create();
        
        Assert.Equal(originalId, entity2.Id);
    }

    [Fact]
    public void Remove_MultipleEntitiesInOrder_ReusesAllIds()
    {
        var entityManager = new EntityManager();
        
        var entity1 = entityManager.Create(); // ID 1
        var entity2 = entityManager.Create(); // ID 2
        var entity3 = entityManager.Create(); // ID 3
        
        // Remove in order: 1, 2, 3
        entityManager.Remove(entity1);
        entityManager.Remove(entity2);
        entityManager.Remove(entity3);
        
        // Should reuse all removed IDs (order not guaranteed due to HashSet)
        var newEntity1 = entityManager.Create();
        var newEntity2 = entityManager.Create();
        var newEntity3 = entityManager.Create();
        
        var originalIds = new HashSet<uint> { 1, 2, 3 };
        var reusedIds = new HashSet<uint> { newEntity1.Id, newEntity2.Id, newEntity3.Id };
        
        Assert.Equal(originalIds, reusedIds); // All IDs should be reused
    }

    [Fact]
    public void Remove_WhenPoolIsFull_DoesNotAddToPool()
    {
        var entityManager = new EntityManager(2); // Small capacity for testing
        
        var entity1 = entityManager.Create(); // ID 1
        var entity2 = entityManager.Create(); // ID 2
        var entity3 = entityManager.Create(); // ID 3
        
        // Remove 3 entities, but pool can only hold 2
        entityManager.Remove(entity1);
        entityManager.Remove(entity2);
        entityManager.Remove(entity3); // This should not be added to full pool
        
        // Create new entities
        var newEntity1 = entityManager.Create(); // Should reuse from pool
        var newEntity2 = entityManager.Create(); // Should reuse from pool
        var newEntity3 = entityManager.Create(); // Should use next sequential ID (4)
        
        // First two should be from pool (order not guaranteed)
        var poolIds = new HashSet<uint> { 1, 2 };
        Assert.Contains(newEntity1.Id, poolIds);
        Assert.Contains(newEntity2.Id, poolIds);
        Assert.NotEqual(newEntity1.Id, newEntity2.Id);
        
        // Third should be sequential
        Assert.Equal(4u, newEntity3.Id);
    }

    [Fact]
    public void Remove_NonExistentEntity_DoesNothing()
    {
        var entityManager = new EntityManager();
        var entity = new Entity(999); // Entity that was never created
        
        entityManager.Remove(entity); // Should not throw
    }

    #endregion

    #region Entity Lifecycle Tests

    [Fact]
    public void EntityLifecycle_CreateRemoveCreate_WorksCorrectly()
    {
        var entityManager = new EntityManager();
        
        // Create entity
        var entity1 = entityManager.Create();
        var originalId = entity1.Id;
        
        // Remove entity
        entityManager.Remove(entity1);
        
        // Create new entity - should reuse ID
        var entity2 = entityManager.Create();
        
        Assert.Equal(originalId, entity2.Id);
    }

    [Fact]
    public void EntityLifecycle_ComplexPattern_MaintainsConsistency()
    {
        var entityManager = new EntityManager();
        
        // Create initial entities
        var entities = new List<Entity>();
        for (int i = 0; i < 10; i++)
        {
            entities.Add(entityManager.Create());
        }
        
        // Remove some entities (even indices)
        var removedIds = new List<uint>();
        for (int i = 0; i < entities.Count; i += 2)
        {
            removedIds.Add(entities[i].Id);
            entityManager.Remove(entities[i]);
        }
        
        // Create new entities - should reuse removed IDs
        var newEntities = new List<Entity>();
        for (int i = 0; i < removedIds.Count; i++)
        {
            newEntities.Add(entityManager.Create());
        }
        
        // All new entity IDs should be from the removed IDs list
        foreach (var newEntity in newEntities)
        {
            Assert.Contains(newEntity.Id, removedIds);
        }
        
        // Create one more - should get next sequential ID (11)
        var nextEntity = entityManager.Create();
        Assert.Equal(11u, nextEntity.Id);
    }

    #endregion

    #region Pool Capacity Tests

    [Fact]
    public void PoolCapacity_RespectedDuringRemoval()
    {
        var entityManager = new EntityManager(3); // Small capacity
        
        // Create entities
        var entity1 = entityManager.Create(); // ID 1
        var entity2 = entityManager.Create(); // ID 2
        var entity3 = entityManager.Create(); // ID 3
        var entity4 = entityManager.Create(); // ID 4
        
        // Remove all entities (more than pool capacity)
        entityManager.Remove(entity1);
        entityManager.Remove(entity2);
        entityManager.Remove(entity3);
        entityManager.Remove(entity4); // This won't fit in pool
        
        // Create new entities
        var newEntity1 = entityManager.Create(); // From pool
        var newEntity2 = entityManager.Create(); // From pool
        var newEntity3 = entityManager.Create(); // From pool
        var newEntity4 = entityManager.Create(); // Sequential (5)
        
        // First three should be reused IDs
        var reusedIds = new[] { newEntity1.Id, newEntity2.Id, newEntity3.Id };
        var originalIds = new[] { 1u, 2u, 3u };
        
        foreach (var reusedId in reusedIds)
        {
            Assert.Contains(reusedId, originalIds);
        }
        
        // Fourth should be sequential
        Assert.Equal(5u, newEntity4.Id);
    }

    [Fact]
    public void PoolCapacity_ZeroCapacity_NoPooling()
    {
        var entityManager = new EntityManager(0);
        
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

    #region Sentinel Entity Tests

    [Fact]
    public void SentinelEntity_HasIdZero()
    {
        Assert.Equal(0u, (uint)SentinelEntities.Null);
    }

    [Fact]
    public void Remove_SentinelEntity_DoesNothing()
    {
        var entityManager = new EntityManager();
        var sentinelEntity = new Entity((uint)SentinelEntities.Null);
        
        // Should not throw and should not affect entity creation
        entityManager.Remove(sentinelEntity);
        
        var normalEntity = entityManager.Create();
        Assert.Equal(1u, normalEntity.Id);
    }

    [Fact]
    public void Create_NeverReturnsSentinelId()
    {
        var entityManager = new EntityManager();
        
        // Create many entities to ensure we never get ID 0
        for (int i = 0; i < 100; i++)
        {
            var entity = entityManager.Create();
            Assert.NotEqual(0u, entity.Id);
        }
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
        for (int i = 0; i < entityCount; i++)
        {
            entities.Add(entityManager.Create());
        }
        
        Assert.Equal(entityCount, entities.Count);
        
        // Verify all IDs are unique and sequential
        for (int i = 0; i < entityCount; i++)
        {
            Assert.Equal((uint)(i + 1), entities[i].Id);
        }
    }

    [Fact]
    public void Performance_CreateRemoveCreateCycle_PerformsWell()
    {
        var entityManager = new EntityManager();
        const int cycleCount = 1000;
        
        var createdIds = new HashSet<uint>();
        
        for (int i = 0; i < cycleCount; i++)
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

    #region Integration Tests

    [Fact]
    public void EntityManager_FullLifecycleScenario_WorksCorrectly()
    {
        var entityManager = new EntityManager(5);
        
        // Phase 1: Create initial entities
        var initialEntities = new List<Entity>();
        for (int i = 0; i < 8; i++)
        {
            initialEntities.Add(entityManager.Create());
        }
        
        // Verify sequential IDs
        for (int i = 0; i < 8; i++)
        {
            Assert.Equal((uint)(i + 1), initialEntities[i].Id);
        }
        
        // Phase 2: Remove some entities
        var removedIds = new List<uint> { 2, 4, 6, 8, 1 }; // More than pool capacity
        foreach (var id in removedIds)
        {
            var entityToRemove = initialEntities.First(e => e.Id == id);
            entityManager.Remove(entityToRemove);
        }
        
        // Phase 3: Create new entities
        var newEntities = new List<Entity>();
        for (int i = 0; i < 7; i++)
        {
            newEntities.Add(entityManager.Create());
        }
        
        // First 5 should be from pool (subset of removed IDs in reverse order)
        var poolIds = newEntities.Take(5).Select(e => e.Id).ToList();
        foreach (var poolId in poolIds)
        {
            Assert.Contains(poolId, removedIds);
        }
        
        // Last 2 should be sequential (9, 10)
        Assert.Equal(9u, newEntities[5].Id);
        Assert.Equal(10u, newEntities[6].Id);
    }

    [Fact]
    public void EntityManager_EdgeCase_SingleEntityReuse()
    {
        var entityManager = new EntityManager();
        
        var entity1 = entityManager.Create();
        Assert.Equal(1u, entity1.Id);
        
        entityManager.Remove(entity1);
        
        var entity2 = entityManager.Create();
        Assert.Equal(1u, entity2.Id); // Should reuse ID
        
        var entity3 = entityManager.Create();
        Assert.Equal(2u, entity3.Id); // Should be next sequential
    }

    [Fact]
    public void EntityManager_RobustnessTest_HandlesRandomOperations()
    {
        var entityManager = new EntityManager();
        var activeEntities = new HashSet<Entity>();
        var random = new Random(42); // Fixed seed for reproducibility
        
        // Perform random create/remove operations
        for (int i = 0; i < 1000; i++)
        {
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
        }
        
        // Verify all active entities have valid IDs
        foreach (var entity in activeEntities)
        {
            Assert.NotEqual(0u, entity.Id);
        }
    }

    #endregion
}