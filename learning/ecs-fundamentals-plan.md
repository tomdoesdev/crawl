# Entity Component System (ECS) Learning Plan

## What is an ECS?

An Entity Component System is an architectural pattern used in game development that separates data (Components) from behavior (Systems) while using simple IDs (Entities) to tie them together.

### The Three Pillars:

1. **Entity**: A unique identifier (usually just an ID number)
2. **Component**: Pure data containers (no behavior)
3. **System**: Logic that operates on entities with specific components

## Why Use ECS?

- **Performance**: Data-oriented design improves cache locality
- **Flexibility**: Easy to add/remove behaviors without inheritance hierarchies
- **Composition over Inheritance**: Mix and match components freely
- **Decoupling**: Systems are independent and can run in parallel

## Learning Steps

### Phase 1: Core ECS Foundation ✅ In Progress
- [ ] Understand ECS principles and benefits
- [ ] Design Entity interface (simple ID)
- [ ] Create Component marker interface
- [ ] Implement basic System interface
- [ ] Create World/Registry for managing everything

### Phase 2: Basic Components
- [ ] Position component (X, Y coordinates)
- [ ] Health component (current/max HP)
- [ ] Renderable component (sprite/texture info)
- [ ] Velocity component (movement speed)

### Phase 3: Core Systems
- [ ] Movement system (updates positions based on velocity)
- [ ] Health system (manages HP, death events)
- [ ] Rendering system (draws entities with Renderable + Position)

### Phase 4: Game-Specific Components
- [ ] Player component (marks player entity)
- [ ] Enemy component (AI behavior data)
- [ ] Weapon component (damage, range, attack speed)
- [ ] Inventory component (list of items)
- [ ] Door component (locked/unlocked state)
- [ ] Key component (which doors it opens)

### Phase 5: Advanced Systems
- [ ] Combat system (weapon attacks, damage dealing)
- [ ] AI system (enemy movement and behavior)
- [ ] Collision system (prevent walking through walls)
- [ ] Inventory system (pickup/drop items)

## Key ECS Concepts to Master

1. **Query Pattern**: Systems find entities by component combinations
2. **Archetyping**: Grouping entities with same component sets
3. **Event Systems**: Communication between systems
4. **Component Pools**: Memory management for components

## Real-World Analogy

Think of ECS like a database:
- **Entities** are row IDs
- **Components** are columns of data
- **Systems** are queries that process specific combinations of columns

Example: "Find all entities with Position AND Velocity components" = "SELECT * FROM entities WHERE has_position = true AND has_velocity = true"

## Progress Tracking

- [x] Created learning plan
- [ ] Core ECS interfaces implemented
- [ ] World/Registry system built
- [ ] Basic components created
- [ ] Movement system working
- [ ] Ready for 2D rendering integration