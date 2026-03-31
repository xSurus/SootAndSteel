# Gamelab Game

Number of players: 1–4 (cooperative)

Input method: Primarily Gamepad (Keyboard also available for debugging)

## Overview

A cooperative top-down train defense game. Players work together aboard a moving train to keep it running by managing coal fuel, crafting ammunition from raw materials, firing a cannon at enemies, repairing broken walls, and controlling the train's speed. Your objective is to deliver the train to the level's target distance while defending it on the way.

## Controls

| Action          | Gamepad            | Keyboard (Debug) |
| --------------- | ------------------ | ---------------- |
| Move            | Left Stick / D-Pad | W / A / S / D    |
| Interact (hold) | X                  | E                |
| Grab (hold)     | Y                  | R                |
| Pickup          | A                  | Space            |
| Pause           | Start              | Escape           |
| Start / Confirm | Start              | Enter            |

On the **Join Screen**, press **A** (gamepad) or **Space** (keyboard debug input) to join. Press **Start** or **Enter** to begin.

## Station Guide (Color + Function)

| Station | Color | Function |
| ------- | ----- | -------- |
| Coal Resource | Black | Gives coal when picked up (if train has coal). Putting coal back here returns it to the train supply. |
| Coal Oven | Dark Red | Consumes loaded coal as fuel. If it runs empty, the train stops. |
| Speed Lever | Light Green | Cycles train speed: Stopped -> Default -> Double -> Quadruple. |
| Cannon | Dark Red | Load bullets, aim with the aiming bar, then fire to damage enemies. |
| Copper Resource | Orange | Infinite source of Copper. |
| Gunpowder Resource | Dark Gray | Infinite source of Gunpowder. |
| Counter | Saddle Brown | Temporary storage/swap spot for items. Multiple counters are available for parallel workflows. |
| Anvil | Dark Slate Gray | Crafting workbench for processing materials and making Standard/Heavy bullets. Two anvils are available. |

## How to Play

### Keeping the Train Moving

- The train runs on coal. Pick up coal from the **Coal Resource** station and deposit it in the **Coal Oven** to keep the engine fueled.
- Use the **Speed Lever** to cycle through four speed settings: Stopped, Default, Double, and Quadruple. Higher speeds burn coal faster.
- If the oven runs out of fuel, the train stops.

### Crafting Ammunition

Raw material stations (Copper, Gunpowder) provide infinite supplies. Use either **Anvil** workbench to craft bullets:

| Recipe                            | Craft Time | Result                   |
| --------------------------------- | ---------- | ------------------------ |
| Copper                            | 2.0s       | Hammered Copper          |
| Hammered Copper                   | 2.0s       | Core                     |
| Copper + Core                     | 3.0s       | Cored Hammered Copper    |
| Hammered Copper + Gunpowder       | 1.5s       | Standard Bullet (50 dmg) |
| Cored Hammered Copper + Gunpowder | 2.5s       | Heavy Bullet (100 dmg)   |

Place materials on an Anvil and hold **Interact** to craft. Use the counters as intermediate storage when juggling multiple items.

### Firing the Cannon

1. Load a crafted bullet into the **Cannon** using Pickup.
2. **Grab** the cannon's aiming bar and rotate it to aim.
3. Press **Interact** to fire.

### Defending the Train

- **Shooter enemies** (red, on horseback) ride alongside the train and shoot at the walls. Destroy them with the cannon.
- **Thief enemies** (purple) sneak up from above or below to steal coal from your supply. They turn yellow while stealing and green while fleeing.
- **Repair walls** by holding Interact on a damaged wall segment.

### Win & Lose Conditions

- **Win:** Reach the level's distance goal and clear remaining enemies.
- **Lose:** If temperature reaches maximum, the train freezes and you fail.
- **Lose:** If coal reaches 0 while the train is still moving, you fail to deliver.

The HUD shows key status values such as Coal, Speed, Temperature, Cannon Ammo, and Distance progress.
