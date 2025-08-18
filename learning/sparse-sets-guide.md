# Sparse Sets: A Complete Guide

## What Problem Do Sparse Sets Solve?

In an ECS, we need to:
1. **Quickly find** if entity X has component Y
2. **Efficiently iterate** through all components of type Y
3. **Fast add/remove** components from entities

Traditional approaches have trade-offs:
- **Array indexed by Entity ID**: Fast lookup, but wastes memory for sparse entity IDs
- **Dictionary only**: No memory waste, but slower iteration
- **List of components**: Memory efficient, but slow lookups

Sparse sets give us the best of all worlds!

## The Sparse Set Structure

A sparse set uses **two arrays working together**:

```
Dense Arrays (packed, no gaps):
_denseComponents: [Pos(10,5), Pos(20,15), Pos(0,30)]
_denseEntities:   [Entity-7,  Entity-3,   Entity-12]
                      ↑          ↑           ↑
                   index 0    index 1    index 2

Sparse Lookup (Entity ID → Dense Index):
_sparse: {7→0, 3→1, 12→2}
```

## Step-by-Step Example

Let's build a sparse set for Position components:

### Step 1: Start Empty
```
_denseComponents: []
_denseEntities:   []
_sparse:          {}
_count:           0
```

### Step 2: Add Position to Entity 7
```
_denseComponents: [Pos(10,5)]
_denseEntities:   [Entity-7]
_sparse:          {7→0}
_count:           1
```

### Step 3: Add Position to Entity 3
```
_denseComponents: [Pos(10,5), Pos(20,15)]
_denseEntities:   [Entity-7,  Entity-3]
_sparse:          {7→0, 3→1}
_count:           2
```

### Step 4: Add Position to Entity 12
```
_denseComponents: [Pos(10,5), Pos(20,15), Pos(0,30)]
_denseEntities:   [Entity-7,  Entity-3,   Entity-12]
_sparse:          {7→0, 3→1, 12→2}
_count:           3
```

## Key Operations

### Has Component
```csharp
public bool Has(Entity entity)
{
    return _sparse.ContainsKey(entity.Id);
}
```

### Get Component
```csharp
public ref T Get(Entity entity)
{
    int index = _sparse[entity.Id];
    return ref _denseComponents[index];
}
```

### Add Component
```csharp
public void Add(Entity entity, T component)
{
    // Don't allow duplicates
    if (_sparse.ContainsKey(entity.Id))
        throw new InvalidOperationException("Entity already has this component");
    
    // Add to end of dense arrays
    int newIndex = _count;
    _denseComponents[newIndex] = component;
    _denseEntities[newIndex] = entity;
    _sparse[entity.Id] = newIndex;
    _count++;
}
```

### The Tricky Part: Remove Component

This is where "swap-with-last" shines:

**Before removing Entity 3:**
```
_denseComponents: [Pos(10,5), Pos(20,15), Pos(0,30)]
_denseEntities:   [Entity-7,  Entity-3,   Entity-12]
                      ↑          ↑           ↑
                   index 0    index 1    index 2
_sparse:          {7→0, 3→1, 12→2}
_count:           3
```

**Step 1: Find the entity to remove (Entity 3 at index 1)**

**Step 2: Swap with last element (index 2)**
```
_denseComponents: [Pos(10,5), Pos(0,30),  Pos(20,15)]
_denseEntities:   [Entity-7,  Entity-12,  Entity-3]
                      ↑          ↑           ↑
                   index 0    index 1    index 2
```

**Step 3: Update sparse mapping for moved entity**
```
_sparse: {7→0, 3→1, 12→1}  // Entity 12 moved to index 1
```

**Step 4: Remove the entity and decrement count**
```
_denseComponents: [Pos(10,5), Pos(0,30)]
_denseEntities:   [Entity-7,  Entity-12]
                      ↑          ↑
                   index 0    index 1
_sparse:          {7→0, 12→1}  // Remove Entity 3
_count:           2
```

### Remove Implementation
```csharp
public void Remove(Entity entity)
{
    if (!_sparse.TryGetValue(entity.Id, out int index))
        return; // Entity doesn't have this component
    
    int lastIndex = _count - 1;
    
    if (index != lastIndex)
    {
        // Swap with last element
        _denseComponents[index] = _denseComponents[lastIndex];
        _denseEntities[index] = _denseEntities[lastIndex];
        
        // Update sparse mapping for the moved entity
        Entity movedEntity = _denseEntities[index];
        _sparse[movedEntity.Id] = index;
    }
    
    // Remove from sparse and decrement count
    _sparse.Remove(entity.Id);
    _count--;
}
```

## Why This Is Brilliant

### Fast Lookup: O(1)
```csharp
// Is this fast? YES!
bool hasPosition = positions.Has(entity);
Position pos = positions.Get(entity);
```

### Fast Iteration: O(components)
```csharp
// Iterate only through entities that HAVE Position components
for (int i = 0; i < positions.Count; i++)
{
    Entity entity = positions.GetEntity(i);
    ref Position pos = ref positions.GetComponent(i);
    // Process...
}
```

### Memory Efficient
- No wasted space for entities without this component
- Dense arrays are cache-friendly
- Only grows with actual usage

### Fast Add/Remove: O(1)
- Add: append to end
- Remove: swap-with-last (no shifting required)

## Real-World Analogy

Think of a sparse set like a **restaurant reservation system**:

- **Dense Arrays**: The actual tables (packed, no empty tables between customers)
- **Sparse Dictionary**: The reservation book (maps customer name → table number)

When a customer leaves:
1. Find their table number in the reservation book
2. Move the last customer to the empty table
3. Update the reservation book
4. Mark one fewer table as occupied

No need to shift all customers down one table!

## Common Gotchas

1. **Order doesn't matter**: Sparse sets don't preserve insertion order
2. **One component per entity**: Each entity can have at most one component of each type
3. **Index invalidation**: After removal, indices may change due to swap-with-last
4. **Memory vs speed**: Uses more memory than pure dictionary, but much faster iteration

## When to Use Sparse Sets

✅ **Perfect for ECS component storage**
✅ **Fast iteration over active elements**
✅ **Frequent add/remove operations**
✅ **When you need both random access AND iteration**

❌ **Not good when order matters**
❌ **Overkill for small, static collections**
❌ **Not suitable when you need stable indices**

Sparse sets are the sweet spot for ECS performance!