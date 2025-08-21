# Component-Based Object Design in C# - Learning Plan

## Overview
This plan guides you through building a performant component-based object system for your dungeon crawler, focusing on modern C# patterns and runtime performance considerations.

## Phase 1: Foundation - Object Composition vs Inheritance ✅
**Goal**: Understand when and how to use composition over inheritance in C#

### Tasks:
- [x] **1.1** Study the difference between inheritance and composition patterns
- [x] **1.2** Create a simple GameObject base class
- [x] **1.3** Design your first component interface (IComponent)
- [x] **1.4** Implement a basic HealthComponent using struct vs class
- [x] **1.5** Test: Write unit tests comparing struct vs class performance

**Key Learning**: Understanding value types vs reference types impact on memory and performance ✅

---

## Phase 2: ECS Architecture with Advanced Data Structures ✅
**Goal**: Build a high-performance Entity Component System with sparse sets

### Tasks:
- [x] **2.1** Discovered boxing issues with Dictionary<Type, IComponent> storage ✅
- [x] **2.2** Transitioned from GameObject to Entity-Component-System architecture ✅
- [x] **2.3** Designed and implemented sophisticated paged SparseSet<T> data structure ✅
- [x] **2.4** Implemented bit manipulation for efficient page/index calculations ✅
- [x] **2.5** Created swap-and-pop removal algorithm for dense array maintenance ✅
- [x] **2.6** Added comprehensive unit tests validating all sparse set operations ✅
- [x] **2.7** Used ReadOnlySpan<T> for zero-allocation data access ✅

**Key Learning**: Advanced data structures, bit manipulation, memory layout optimization, ECS patterns ✅

---

## Phase 3: ECS Game Components ⏳
**Goal**: Create game-specific components that leverage your sparse set architecture

### Tasks:
- [x] **3.1** Design ECS-compatible component interfaces (empty IComponent marker) ✅
- [x] **3.2** PositionComponent (world coordinates with 3D support) ✅
- [ ] **3.3** HealthComponent (HP management with damage/healing methods)
- [ ] **3.4** StatusEffectComponent (Boons/Banes system for buffs, debuffs, regen, DoT)
- [ ] **3.5** SpriteComponent (rendering data)
- [ ] **3.6** InventoryComponent (item storage)
- [ ] **3.7** WeaponComponent (attack stats)
- [ ] **3.8** MovementComponent (speed, collision)
- [ ] **3.9** Test: Create Hero and Enemy entities with multiple components in sparse sets

**Key Learning**: ECS component design, data-oriented patterns, entity composition

---

## Phase 4: Performance Optimization ⏳
**Goal**: Apply data-oriented design principles for better performance

### Tasks:
- [ ] **4.1** Component pooling to reduce garbage collection
- [ ] **4.2** Batch component updates (all HealthComponents together)
- [ ] **4.3** Memory layout optimization (struct packing)
- [ ] **4.4** Cache-friendly data access patterns
- [ ] **4.5** Benchmark: Before/after performance measurements

**Key Learning**: .NET garbage collector behavior, memory locality, hot paths

---

## Phase 5: Game Loop Integration ⏳
**Goal**: Connect components to your 30 TPS tick system

### Tasks:
- [ ] **5.1** ComponentUpdateSystem base class
- [ ] **5.2** MovementUpdateSystem (handle position changes)
- [ ] **5.3** CombatUpdateSystem (weapon attacks, health changes)
- [ ] **5.4** InventoryUpdateSystem (item pickup, key management)
- [ ] **5.5** Test: Full gameplay loop with multiple entities

**Key Learning**: System architecture, tick-based updates, frame independence

---

## Phase 6: Advanced Patterns ⏳
**Goal**: Explore advanced C# features for component systems

### Tasks:
- [ ] **6.1** Component messaging/events (C# events vs delegates)
- [ ] **6.2** Component serialization for save/load
- [ ] **6.3** Component reflection and editor tools
- [ ] **6.4** Memory profiling and optimization
- [ ] **6.5** Final integration with OpenTK rendering

**Key Learning**: Advanced C# features, profiling tools, production considerations

---

## Key Questions to Explore Throughout:

1. **When to use struct vs class for components?**
   - Memory allocation patterns
   - Boxing/unboxing implications
   - Performance trade-offs

2. **How does the .NET runtime handle your object graphs?**
   - Garbage collection impact
   - Reference vs value type storage
   - Memory fragmentation

3. **What's the performance cost of generic methods?**
   - JIT compilation behavior
   - Generic constraint benefits
   - Type system integration

4. **How do C# features compare to patterns you know from TypeScript/Node.js?**
   - Interfaces vs duck typing
   - Strong typing benefits
   - Compilation vs runtime checks

---

## Success Metrics:
- [ ] Hero character with Health, Position, Sprite, Inventory components
- [ ] Enemy objects that can attack and drop items
- [ ] Weapon components with different stats
- [ ] Door/Key interaction system
- [ ] 30 TPS game loop running smoothly
- [ ] Understanding of C#/.NET memory and performance implications

---

## Next Steps:
After completing this plan, you'll be ready to integrate with OpenTK for rendering and build out the full dungeon crawler gameplay!