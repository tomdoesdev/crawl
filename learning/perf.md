1. Remove Operation Performance Issues

Problem: Remove_Random was consistently the slowest operation (7.2ms for 100k entities)
- Root Cause: The swap-with-last algorithm creates unpredictable memory access patterns
- Current Implementation: When removing a random entity, we swap with the last entity, which can cause cache misses
- Additional Issue: The dictionary update _sparseEntities[movedEntity.Id] = deletingIdx happens for every removal except the last element

Potential Fix: Consider batch removal operations or optimize the swap logic.

2. Dictionary Performance Bottlenecks

Problem: All operations rely on Dictionary<uint, int> lookups
- Hash Collisions: With large entity counts, hash collisions become more likely
- Memory Layout: Dictionary entries aren't cache-friendly for sequential access
- Overhead: Dictionary has inherent overhead compared to direct array access

Alternative Approach: Consider a true sparse array implementation where entity IDs directly map to array indices (if entity IDs are dense enough).

3. Memory Layout and Cache Performance

Problem: Sequential operations showed poor scaling
- Entity Array: Separate Entity[] array may not be necessary for most operations
- False Sharing: Components and entities stored in separate arrays can cause cache line issues
- Memory Fragmentation: Multiple allocations (_components, _entities, dictionary) spread across memory

Potential Optimization: Structure-of-Arrays vs Array-of-Structures consideration.

4. Churn Performance (Add/Remove Cycles)

Problem: Churn_AddRemoveCycle allocated 34MB consistently
- Dictionary Resize: Frequent dictionary growth during churn
- Capacity Planning: No shrinking mechanism when entities are removed
- Memory Pressure: High allocation rate during realistic usage patterns

Solution: Implement dictionary capacity pre-sizing and consider shrinking strategies.

5. Entity Creation Overhead in Benchmarks

Observation: Random benchmarks create new Entity(_randomEntityIds[i]) in tight loops
- Struct Allocation: While structs are stack-allocated, the constructor call has overhead
- Real-world Impact: This reflects actual usage where entities are frequently created

Note: This is more of a benchmark artifact, but it highlights that Entity creation isn't free.

6. Lack of Bulk Operations

Missing Feature: No bulk add/remove operations
- Cache Efficiency: Single-item operations don't optimize for cache locality
- Dictionary Updates: Each operation touches the dictionary separately
- Use Case: Game engines often need to process many entities at once

Potential Addition: AddRange(), RemoveRange(), Clear() optimizations.

7. Growth Strategy Inefficiency

Problem: Linear growth (capacity + growCapacity) vs exponential
- Frequent Resizing: With large datasets, linear growth causes more resize operations
- Memory Copying: More frequent Array.Copy operations
- Performance Impact: Visible in Add_WithGrowth being slower than Add_Sequential

Solution: We partially addressed this with the Math.Max(_capacity + _growCapacity, _capacity * 2) approach, but could be further optimized.

The most impactful issues to address next would be:
1. Dictionary optimization (biggest bottleneck)
2. Remove operation efficiency (worst single-operation performance)
3. Bulk operations (real-world usage patterns)