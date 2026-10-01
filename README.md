# 🐍 Snake Game - Diluted Checkerboard Edition

A native Windows desktop application built with C# and .NET 9 Windows Forms. It runs completely offline with no web browser, Node.js, or Electron dependencies.

---

## ✨ Features

- **Diluted Checkerboard Background**:
  - Soft, low-contrast alternating tiles designed for optimal focus and eye comfort (inspired by classic meadow greens).
  - 4 selectable diluted checkerboard themes:
    - *Classic Meadow (Diluted)*
    - *Dark Slate (Diluted)*
    - *Soft Pastel (Diluted)*
    - *Warm Sand (Diluted)*
- **Point Counter & High Score Persistence**:
  - Live in-game score tracking.
  - Persistent High Score saved automatically to `%LocalAppData%\AntigravitySnakeGame\highscore.json` (survives app restarts and game overs).
  - Real-time record breaking banner (`🏆 Best: X (NEW!)`) when you beat your previous best!
- **Dynamic Gameplay & Bonus Food**:
  - Smooth 60 FPS double-buffered rendering.
  - Responsive buffered keyboard handling (never drops rapid turns).
  - Cute animated snake head with directional cartoon eyes.
  - Regular food (Apples) and rare bonus items (Golden Apples with countdown indicator).
  - Speed settings: Relaxed, Normal, Speedy.
- **Pure Native Audio**:
  - Built-in retro synthesized sound effects (eating chimes, bonus fanfares, death sounds, and high score celebrations) generated in-memory with zero external audio assets.
  - Audio mute toggle button (`M` key or header button).

---

## 🎮 Controls

| Key | Action |
| :--- | :--- |
| **Arrow Keys** or **W, A, S, D** | Move Snake (Up, Down, Left, Right) |
| **Spacebar** | Pause / Resume (or Restart on Game Over) |
| **R** | Restart Game |
| **M** | Mute / Unmute Sound Effects |
| **Mouse Click** | Start game / Restart from Game Over screen |

---

## 🚀 How to Run

### Option 1: Double-click Launcher
Double-click **`run.bat`** in this folder to launch the game immediately.

### Option 2: Run the Executable
Open the `publish/` folder and double-click **`publish\SnakeGame.exe`**.

### Option 3: Terminal Command
From this directory, run:
```bash
dotnet run
```
