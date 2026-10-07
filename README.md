# 1v1 MGE — Multiplayer Game Engine

> A 1v1 competitive multiplayer FPS built with Unity and FishNet networking.

![Unity Version](https://img.shields.io/badge/Unity-2022.3.31f1-orange)
![Static Badge](https://img.shields.io/badge/Networking-FishNet_v4.6-blue)
![Static Badge](https://img.shields.io/badge/Render_Pipeline-URP_14.0-purple)
![Language](https://img.shields.io/badge/Language-C%23%209.0-cyan)

## 🎬 Watch the demo on Youtube 🎬
[![Watch the demo](https://img.youtube.com/vi/2dVTkQYogB0/maxresdefault.jpg)](https://www.youtube.com/watch?v=2dVTkQYogB0)

---

## 🎮 Overview

**1v1 MGE** is a multiplayer-first-person-shooter game engine designed for 1v1 competitive matches. It features a complete movement system, weapon framework, networked health/damage, score tracking, and a round-based gameplay loop — all synced over the network using FishNet.

The project serves as both a **fully playable game prototype** and a **demonstration of C# and Unity practices** for networking, architecture, and code quality.

---

## 🌐 Hosting

Hosting requires enabling port-forwarding in network settings.

Port used for hosting: 7770.

---

## ✨ Features

### ⚔️ Weapons System
- 2 realized weapon types (shotgun, rocket launcher)
- Modular weapon architecture with separate shooting and ammo management functionality
- Status effects system
- UniTask-based reloading

### 🎨 Visual Effects
- Level blockout made with Realtime-CSG plugin
- Pre-baked level lighting
- Weapon muzzle flashes, impact decals and materials, weapon trail effects, status effects, etc.

### ⚙️ Settings & Configuration
- JSON-based settings save/load
- Game fully utilizes new Unity Input System
- Volume controls (master, music, SFX)
- Mouse sensitivity and camera FOV
- Resolution and window mode configuration

### 🌐 Networking (FishNet)
- Client-authoritative movement and shooting
- Server shutdown handling and scene reset

### 📱 UI
- Event-channel based HUD decoupling (`UIEventChannelSO` ScriptableObject)
- Health, ammo, and score displays.
- Connection/disconnection lobby UI
- Game start countdown text, score board, victory screen
- Game settings menu

<img width="1920" height="1080" alt="vlcsnap-2026-09-27-23h51m17s716" src="https://github.com/user-attachments/assets/9234ab77-7b92-4b45-acb1-b9459ebbe365" />

---

## 📦 Dependencies

| Package | Version | Type |
|---|---|---|
| **FishNet** | 4.6.20 | Networking |
| **Unity Render Pipeline Universal** | 14.0.11 | Rendering |
| **Unity Input System** | 1.7.0 | Input |
| **TextMeshPro** | 3.0.6 | UI Text |
| **UniTask** | Latest | Async/await |
| **ParrelSync** | Latest | Multiplayer testing |
| **realtime-CSG** | Latest | CSG geometry |
| **Fragsurf** | Latest | FPS Movement project |
| **GameKit** | Latest | FishNet dependencies |

---

## 🚀 Getting Started

### Prerequisites
- Unity Hub with **Unity 2022.3** or later installed
- Visual Studio 2022 with Unity development workload
- .NET Framework 4.7.1+

### Local Multiplayer Testing
Use **ParrelSync** to run two instances of the game on the same machine:
1. Install ParrelSync from the Package Manager
2. Open two Unity instances using ParrelSync
3. One instance acts as the **Host**, the other as the **Client**
4. Connect via the IP settings in the lobby

---

## 📝 License

This project is part of a portfolio demo. See the `LICENSE` file for details.

Third-party assets used:
- [FishNet](https://github.com/FirstGearGames/FishNet) — MIT License
- [TextMeshPro](https://unity.com/textmeshpro) — Unity License
- [QuickOutline](https://assetstore.unity.com) — Chris Nolet
- [UnitySourceMovement](https://github.com/Olezen/UnitySourceMovement) — Olezen, based on Fragsurf by cr4yz (Jake E.)

---

## 🎨 Art Credits

- **Art**: Unity Asset Store samples (Toon Shader, Props, VFX)
- **Fonts**: Team Fortress 2 DIY Kit (Valve Software)
- **Sound**: Team Fortress 2 (Valve Software)
- **Music**: Rocket Jump Waltz Remix [NO HEAVY] (Hidey0shi, Martin Chlud)
