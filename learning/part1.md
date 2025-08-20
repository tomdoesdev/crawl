Value Types vs Reference Types: The Performance Reality

Memory Layout & Allocation

Structs (Value Types):

HealthComponentStruct health = new HealthComponentStruct(); // Stack allocated (usually)
- Location: Stack (local variables) or inline in containing object
- Allocation cost: ~0 - just stack pointer adjustment
- GC pressure: Zero - no heap objects created
- Memory overhead: Exactly the size of your fields (16 bytes for your HealthComponent)

Classes (Reference Types):

HealthComponentClass health = new HealthComponentClass(); // Heap allocated
- Location: Always on the heap
- Allocation cost: Heap allocation + GC tracking
- GC pressure: High - every instance creates work for GC
- Memory overhead: 24+ bytes (16 bytes data + 8 byte object header + alignment)

Copy Semantics - The Game Changer

The Critical Difference:

// STRUCT - copies the entire value
var health1 = player.GetComponent<HealthComponentStruct>(); // COPY made here
health1.CurrentHealth = 50; // Modifies the COPY
// Original in GameObject unchanged! 😱

// CLASS - copies the reference
var health2 = player.GetComponent<HealthComponentClass>(); // Reference copied
health2.CurrentHealth = 50; // Modifies the ORIGINAL object ✅

Performance Implications You Discovered

What Your Benchmarks Should Show:

Memory Allocation:
- Structs: ~0 bytes allocated for 1M creations
- Classes: ~24+ MB allocated for 1M creations
- Winner: Structs by a massive margin

Access Speed:
- Structs: Faster creation, but copying overhead in GetComponent()
- Classes: Slower creation, but no copying in GetComponent()
- Winner: Depends on usage pattern

ComponentRef Pattern:
- Trade-off: Delegation overhead vs copy avoidance
- Real cost: ~2-3x slower than direct access, but solves mutation problem

The .NET Runtime Deep Dive

Why This Matters for Your Dungeon Crawler:

// In your 30 TPS game loop:
foreach (var enemy in enemies) // Potentially 100+ enemies
{
// With structs - copies component data each access
var health = enemy.GetComponent<HealthComponent>(); // Copy 16 bytes
var position = enemy.GetComponent<PositionComponent>(); // Copy 8 bytes
var weapon = enemy.GetComponent<WeaponComponent>(); // Copy 20 bytes

      // 44 bytes copied per enemy per tick = 4.4KB per frame at 100 enemies
      // At 30 TPS = 132KB/second just in component copying!
}

Garbage Collection Impact:

// Classes create GC pressure:
var health = new HealthComponentClass(); // Heap allocation
// Later: GC must track, mark, and sweep this object
// In tight game loops, this causes frame drops!

// Structs create zero GC pressure:
var health = new HealthComponentStruct(); // Stack allocation
// No GC involvement whatsoever

The Fundamental Trade-off

| Aspect             | Struct (Value Type)    | Class (Reference Type) |
  |--------------------|------------------------|------------------------|
| Memory Efficiency  | ✅ Excellent            | ❌ Overhead             |
| GC Pressure        | ✅ Zero                 | ❌ High                 |
| Mutation Semantics | ❌ Copy confusion       | ✅ Intuitive            |
| Cache Performance  | ✅ Great (when grouped) | ❌ Scattered            |
| Large Data         | ❌ Expensive copying    | ✅ Reference sharing    |

Your Architectural Decision Framework

Choose Structs When:

- ✅ Component is small (< 32 bytes recommended)
- ✅ Component is data-focused (minimal behavior)
- ✅ Performance is critical (game loops, hot paths)
- ✅ You can handle copy semantics properly

Choose Classes When:

- ✅ Component is large (copying would be expensive)
- ✅ Component has complex behavior or lifecycle
- ✅ Reference semantics are more natural
- ✅ Component needs polymorphism or inheritance

Modern C# Best Practices

Your component design hits the sweet spot:
- Small structs for game components (16-32 bytes)
- ComponentRef pattern for safe mutation
- ID-based references instead of object references
- Pure data + behavior separation