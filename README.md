# Crawl - A Learning-Focused Dungeon Crawler

A educational game development project designed to teach modern C# and OpenGL through hands-on implementation of a dungeon crawling game.

## 🎯 Project Purpose

This project serves as a comprehensive learning platform for:
- **Modern C# programming** - Advanced language features, design patterns, and best practices
- **OpenGL graphics programming** - Using OpenTK for rendering and graphics pipeline management
- **Game architecture** - Entity Component System (ECS), game loops, and modular design
- **Progressive complexity** - Starting with 2D tile-based rendering, advancing to 3D first-person gameplay

## 🎮 Game Overview

Crawl implements a dungeon crawler with two distinct rendering modes:

### 2D Mode (Current Focus)
- Tile-based orthographic view
- Simple sprite rendering
- Easy-to-understand game mechanics

### 3D Mode (Future Implementation)
- First-person perspective in the style of classic Doom
- True 3D rendering with modern OpenGL
- Advanced lighting and texture mapping

## 🏗️ Architecture

### Entity Component System (ECS)
The game uses a custom-built ECS architecture featuring:
- **Entities**: Unique identifiers for game objects
- **Components**: Pure data containers (Position, Health, Inventory, etc.)
- **Systems**: Logic processors that operate on component combinations
- **Sparse Sets**: High-performance component storage for cache-friendly iteration

### Project Structure
```
Crawl/
├── Crawl.Game/          # Core game logic and ECS implementation
│   └── ECS/             # Entity Component System foundation
├── Crawl.Renderer/      # Graphics rendering (2D and future 3D)
│   └── 2D/              # 2D tile-based rendering system
├── learning/            # Educational guides and learning plans
└── res/                 # Game resources (textures, models, etc.)
```

## 🎲 Gameplay Features

### Core Game Elements
- **Hero Character**: Player-controlled entity with health and inventory systems
- **Weapons**: Various weapon types (Long Sword, Bow, etc.) with unique stats
  - Damage values
  - Attack speed
  - Attack range
- **Enemies**: AI-controlled opponents that attack the player and drop items
- **Loot System**: Enemies drop weapons and keys when defeated
- **Locked Doors**: Require keys obtained from defeated enemies
- **Win Condition**: Successfully unlock and pass through the final door

### Planned Systems
- Combat system with weapon variety
- Inventory management
- AI behavior for enemies
- Collision detection
- Audio system
- Save/load functionality

## 🛠️ Technology Stack

- **Language**: C# (.NET)
- **Graphics**: OpenTK (OpenGL wrapper for .NET)
- **IDE**: Compatible with Visual Studio, Rider, or VS Code
- **Target Platform**: Cross-platform (Windows, macOS, Linux)

## 📚 Learning Approach

This project emphasizes:
- **Teaching from first principles** - Each concept is explained thoroughly
- **Incremental complexity** - Start simple, add features progressively  
- **Hands-on implementation** - Learn by building, not just reading
- **Best practices** - Modern C# patterns and game development techniques
- **Performance awareness** - Understanding why certain approaches are faster

## 🚀 Getting Started

### Prerequisites
- .NET 8.0 or later
- OpenTK (automatically restored via NuGet)

### Building and Running
```bash
# Clone the repository
git clone <repository-url>
cd Crawl

# Restore dependencies
dotnet restore

# Build the solution
dotnet build

# Run the game
dotnet run --project Crawl.Game
```

## 📖 Learning Resources

Check the `learning/` directory for:
- ECS fundamentals and implementation guides
- OpenGL and graphics programming tutorials
- C# advanced features explanations
- Game development best practices
- Step-by-step implementation plans

## 🎯 Current Status

- ✅ Core ECS architecture implemented
- ✅ Sparse set component storage system
- ✅ Basic entity and component framework
- 🔄 2D rendering system (in progress)
- 📋 Game logic implementation (planned)
- 📋 3D rendering system (future)

## 🤝 Contributing

This is primarily an educational project. Feel free to:
- Suggest improvements to the learning materials
- Report bugs or issues
- Propose additional features that would enhance the learning experience

## 📝 License

This project is created for educational purposes. See LICENSE file for details.

---

*"The best way to learn game development is to build games."* - This project embodies that philosophy through practical, hands-on implementation of fundamental game development concepts.