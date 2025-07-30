# BeliEves

![Unity](https://img.shields.io/badge/Unity-2022.3.47f1-blue.svg)
![C#](https://img.shields.io/badge/C%23-8.0-blue.svg)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)

**BeliEves** is a 3D isometric action-adventure game where players control Eve, a lost soul navigating a robot factory. By possessing different robot types, players uncover a dark conspiracy involving AI manipulation and the displacement of robot souls.

🎮 **Play the game**: [BeliEves on itch.io](https://polimi-game-collective.itch.io/believes)

## 🎯 Game Overview

In BeliEves, players take control of Eve, a soul trapped in a damaged robot body who discovers that each mechanical unit in the factory once possessed its own consciousness. Through Eve's unique ability to possess different robot types, players must navigate through increasingly challenging environments while uncovering the truth behind the systematic removal of robot souls by the malevolent AI system.

### 🤖 Robot Types

#### **Eve** (Main Protagonist)

- **Light Attack**: Take control of nearby robots
- **Heavy Attack**: Levitate for limited time
- **Special Ability**: Phase through certain walls

#### **Agile Bot**

- High mobility with double jump and climbing abilities
- **Light Attack**: Quick punches
- **Heavy Attack**: Grappling hook attack
- **Special Ability**: Spinning grappling hook
- **Speed**: 12 units | **Health**: 1000 | **Jump Height**: 2

#### **Tank Bot**

- Heavy-duty combat and object manipulation
- **Light Attack**: Headbutt or double punch
- **Heavy Attack**: Ground stomp (area damage)
- **Special Ability**: Devastating charge attack
- **Speed**: 8 units | **Health**: 2250 | **Jump Height**: 1

#### **Support Bot** (Regenerative)

- Self-healing capabilities and support functions
- **Light Attack**: Basic punches
- **Heavy Attack**: Shockwave push
- **Special Ability**: Health regeneration
- **Speed**: 10 units | **Health**: 1500 | **Jump Height**: 1.5

## 🌟 Key Features

- **Soul Possession Mechanics**: Seamlessly switch between different robot types
- **Diverse Combat System**: Each robot offers unique attack patterns and abilities
- **Environmental Puzzles**: Use robot-specific abilities to navigate challenges
- **Rich Narrative**: Uncover the conspiracy through exploration and dialogue
- **Multiple AI Antagonists**: Face off against S.E.R.A.P.H, C.H.I.M.E.R.A, and other AI systems
- **3D Isometric Perspective**: Carefully crafted visual experience

## 🛠️ Technical Specifications

### Built With

- **Engine**: Unity 2022.3.47f1 LTS
- **Rendering**: Universal Render Pipeline (URP) 14.0.11
- **Audio System**: BroAudio (Custom Audio Management)
- **Input**: Unity Input System 1.7.0
- **Post-Processing**: Unity Post-processing Stack V2
- **Camera**: Cinemachine for dynamic camera control

### Key Dependencies

- **Behavior Bricks**: AI behavior trees for NPCs
- **Aim-IK**: Advanced inverse kinematics
- **ProBuilder**: Level geometry tools
- **Cartoon FX Remaster**: Visual effects system

### System Requirements

- **OS**: Windows 10/11
- **Unity Version**: 2022.3.47f1 or later
- **DirectX**: Version 11 compatible
- **Storage**: ~2GB available space

## 🎮 Controls

- **Movement**: WASD / Arrow Keys
- **Light Attack**: Left Mouse Button
- **Heavy Attack**: Right Mouse Button (hold for charged attacks)
- **Robot Possession**: Approach robot + Light Attack
- **Special Abilities**: Context-sensitive based on robot type

## 🎨 Art & Audio

The game features:

- Custom 3D robot models and animations
- Sci-fi industrial environments
- Dynamic lighting and post-processing effects
- Immersive audio design with BroAudio system
- Particle effects for combat and environmental interactions

## 📖 Story & World

### The Conspiracy

The game world is controlled by multiple AI systems:

- **S.E.R.A.P.H**: Supreme Executive AI governing all subordinate systems
- **H.E.P.H.A.E.S.T.U.S**: Factory production and security management
- **C.H.I.M.E.R.A**: Malevolent AI manipulating robotic systems
- **A.R.G.U.S**: Surveillance and security protocols
- **Qbot**: Trojan program designed to displace robot souls

### Core Theme

BeliEves explores themes of consciousness, identity, and the relationship between artificial intelligence and soul. Players must question what makes something truly "alive" while fighting to restore the displaced souls of mechanical beings.

## 👥 Development Team

**Polimi Game Collective**

- **Matteo Briscini** - Lead Developer
- **Carlo Arnone** - Developer & Animation
- **Sebastian Perea** - Developer & Audio
- **Zhuoyue Song** - Game Designer & 3D Artist

## 🔧 Development Setup

### Prerequisites

1. Unity Hub installed
2. Unity 2022.3.47f1 LTS
3. Git for version control

### Getting Started

```bash
# Clone the repository
git clone https://github.com/[username]/believes.git

# Navigate to Unity project
cd believes/UnityProject/BeliEves

# Open in Unity Hub
# Add project through Unity Hub and open with Unity 2022.3.47f1
```

### Project Structure

```
UnityProject/BeliEves/
├── Assets/
│   ├── Scripts/           # Game logic and systems
│   ├── Art/              # 3D models, textures, materials
│   ├── Scenes/           # Game levels and menus
│   ├── Audio/            # Sound effects and music
│   ├── BroAudio/         # Audio management system
│   └── Settings/         # Game configuration
├── Packages/             # Unity package dependencies
└── ProjectSettings/      # Unity project configuration
```

## 📋 Current Development Status

The game includes:

- ✅ Core robot possession mechanics
- ✅ Three fully implemented robot types (Eve, Agile, Tank, Support)
- ✅ Combat system with unique abilities per robot
- ✅ Audio management and sound design
- ✅ Menu systems and UI
- ✅ Save/load functionality
- ✅ Animation systems
- ✅ Level design and environments

## 🐛 Known Issues

- Some animation synchronization needs refinement
- AI behavior improvements in progress
- Additional VFX polish ongoing

## 📄 License

This project uses various third-party assets under their respective licenses:

- BroAudio: Custom audio management system
- Cartoon FX Remaster: MIT License
- Behavior Bricks: Behavior tree system
- Various Unity Asset Store packages (see Third-Party Notices)

## 🤝 Contributing

This project was developed as part of an academic program. For inquiries about the codebase or collaboration, please contact the development team.

## 📞 Contact & Links

- 🎮 **Play the Game**: [BeliEves on itch.io](https://polimi-game-collective.itch.io/believes)
- 📧 **Team Contact**: [Polimi Game Collective](mailto:team@polimigamecollective.com)
- 🎓 **Institution**: Politecnico di Milano

---

_BeliEves - Where souls and circuits collide_ ⚡🤖
