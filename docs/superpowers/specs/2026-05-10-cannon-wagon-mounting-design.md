# Cannon Wagon Mounting — Design

## Problem

`CannonSlot` physics bodies (the interactive seats inside `CannonWagon`) are positioned 48 px outside the train's top/bottom boundaries. With `TrainHeight=6`, `TileSize=80`, the wagon center lands at `Ty+240` (train center), but the `heightPixels * 0.45` offset pushes slots to `Ty-48` (top) and `Ty+528` (bottom) — 8 px past each train edge. The player's horizontal raycast from inside the train never reaches them.

## Fix

Change the Y offset factor in `CannonWagon` from `0.45f` to `0.25f`.

With `0.25 * heightPixels` (= 160 px = 2 tiles):
- Top slot: `Ty + 80` — 1 tile inside the train top edge
- Bottom slot: `Ty + 400` — 1 tile inside the train bottom edge

Players walk left from the train into the cannon wagon and can now raycast-reach the slots with the existing 80 px interact distance.

## Interaction flow

| Action | Input |
|--------|-------|
| Mount slot | Walk into wagon, face slot, press **Pickup** |
| Aim cannon | Movement stick (while seated) |
| Fire cannon | **Interact** (while seated) |
| Load ammo | Hold bullet, press **Pickup** (while seated) |
| Dismount | **Grab** (while seated) |

No changes to interaction logic — only the position constant changes.

## Files changed

- `Src/PhysicalEntities/Structures/CannonWagon.cs`: `0.45f` → `0.25f` (2 occurrences)
