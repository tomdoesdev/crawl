# 2D Dungeon Crawler Learning Plan

## Phase 1: Foundation - 2D Tile Rendering System
### Current Progress: Starting tile rendering implementation

#### 1.1 OpenGL Fundamentals Review ✅
- [ ] Vertices, VBOs, VAOs refresher
- [ ] Shader pipeline (vertex & fragment shaders)
- [ ] Orthographic projection for 2D

#### 1.2 Basic Tile Rendering
- [ ] Create vertex data for quad (tile representation)
- [ ] Set up VBO/VAO for tile geometry
- [ ] Write basic vertex/fragment shaders
- [ ] Implement tile rendering in Scene2D.Draw()
- [ ] Test rendering single colored tiles

#### 1.3 Enhanced Tile System
- [ ] Add texture support for tiles
- [ ] Implement tile atlas/sprite system
- [ ] Create different tile types (wall=1, floor=0)
- [ ] Add camera/viewport management

## Phase 2: Game Entities
#### 2.1 Player Character (Hero)
- [ ] Create Hero class in Game project
- [ ] Add position, movement mechanics
- [ ] Implement health system
- [ ] Add inventory system
- [ ] Render hero on tile grid

#### 2.2 Weapons & Items
- [ ] Design weapon base class
- [ ] Implement specific weapons (Long Sword, Bow)
- [ ] Add weapon stats (damage, speed, range)
- [ ] Create item pickup system

#### 2.3 Enemies
- [ ] Create enemy base class
- [ ] Implement enemy AI (basic movement/combat)
- [ ] Add enemy-player combat system
- [ ] Implement item drops (weapons, keys)

## Phase 3: Game Mechanics
#### 3.1 Door & Key System
- [ ] Add door tile type
- [ ] Implement locked/unlocked states
- [ ] Create key item and unlock mechanics

#### 3.2 Win Condition
- [ ] Implement game state management
- [ ] Add win condition when door is unlocked
- [ ] Create basic UI feedback

## Phase 4: Polish & Preparation for 3D
#### 4.1 Input & Controls
- [ ] Smooth player movement
- [ ] Combat input handling
- [ ] Menu/inventory controls

#### 4.2 Architecture Review
- [ ] Ensure clean separation between Renderer and Game
- [ ] Document rendering pipeline for 3D transition

---

## Next Immediate Steps:
1. Review OpenGL fundamentals
2. Implement basic tile quad rendering
3. Set up shader pipeline for 2D tiles

## Outstanding Questions:
- Texture format preferences?
- Target tile size (pixels)?
- Color scheme for initial testing?