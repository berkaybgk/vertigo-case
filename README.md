# Vertigo Games Case Study - Wheel of Fortune

A polished Unity mobile game demonstrating a "Wheel of Fortune" progression loop. Players spin to collect various rewards, manage risk across escalating zones, and decide when to walk away with their loot or risk it all.

---

## 🔗 Demo & Assets
* **Demo Video & Screenshots:** [Google Drive Folder](https://drive.google.com/drive/folders/1uy2BkaxFzPJLxis3_wh6MufZfubGx6uq?usp=sharing)
* **Latest APK Build:** Available under the [GitHub Releases](https://github.com/berkaybgk/vertigo-case/releases) page of this repository.

<p align="center">
  <img width="800" alt="Gameplay Showcase" src="https://github.com/user-attachments/assets/3d40343f-ccfa-418e-8fac-47d19c268529" />
</p>

---

## 🎮 Gameplay Features

* **Escalating Zones:** Players progress through sequential zones. Every zone features a wheel spin with distinct rewards.
* **Safe vs. Danger Zones:** Special milestone zones guarantee safe spins (no bombs), while regular zones introduce risk.
* **Risk & Reward (Bombs):** Hitting a bomb presents a crucial choice:
  * Spend Gold to revive and continue.
  * Forfeit accumulated rewards and exit.
* **Smart Walkaway:** Players can choose to walk away and secure all collected rewards (disabled in the initial zone to ensure gameplay begins).
* **Claim Summary:** Decoupled animation and presentation of accumulated rewards upon a successful walkaway or game completion.

---

## 🛠 Software Architecture & Patterns

The codebase is built on **SOLID principles** and clean architectural patterns optimized for Unity:

* **Interface Decoupling (`IGameManager`):** The core game logic is separated via a clear interface, allowing components to interact with the game state without direct dependencies, facilitating testability.
* **Event-Driven UI:** UI panels (such as `ClaimPanel`, `TopBarUI`, and `ZoneIndicatorUI`) subscribe to game events, ensuring a decoupled architecture where the UI only responds to state changes rather than polling.
* **Scriptable Object Configurations:** Game values, wheel items, slice distributions, and zone weights are driven by custom ScriptableObjects (`WheelConfigData`, `ZoneConfigData`), allowing easy balancing changes without modifying code.
* **DOTween Integrations:** Smooth micro-animations for UI elements, wheel rotations, and panel scaling to offer a premium, game-feel experience.

---

## 📦 How to Build the APK

A custom editor build script is included in the project to compile the Android APK in a single click:

1. Open the project in the **Unity Editor (version 2021.3.45f1)**.
2. In the top Unity menu bar, select **Build -> Build Android APK**.
3. The script will automatically configure the build settings for Android, target the `SampleScene`, compile the APK, and save it in the `<Project-Root>/Builds/` folder.
4. Once completed, your finder/explorer window will open directly to the generated `vertigo-case.apk`.

> [!NOTE]
> Make sure you have the **Android Build Support** package installed in Unity Hub for version `2021.3.45f1`.

---

## 🚀 How to Install and Run the APK

1. Go to the [Releases](https://github.com/berkaybgk/vertigo-case/releases) section of this GitHub repository.
2. Download the `vertigo-case.apk` file on your Android device (or copy it from your computer).
3. Locate the file on your device and tap to install (enable "Install from Unknown Sources" if prompted by your Android OS).
4. Run the application and enjoy!
