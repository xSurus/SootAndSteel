# ENEMY Implementation Plan

This document is the implementation plan for the full enemy system for this branch.

It is intentionally based on the current repo style and architecture:
- straightforward OO design
- file-scoped namespaces
- fixed-step gameplay updates
- config-first tuning through `GameplayConfig` + `Src/Content/Data/gameplay.json`
- explicit state machines over generic AI frameworks
- small helper classes instead of deep inheritance trees

This revised plan reflects the following branch-specific decisions:
- **breaking authored enemy content is allowed**
- **no legacy `Shooter` / `Thief` compatibility layer is needed**
- **`TrainNose` must not become damageable**
- **enemy terminology is `EnemyMovement`, not `EnemyLocomotion`**
- **enemy variety comes from explicit enemy types, not modifiers**
- **the system should be vehicle-ready, but vehicles are not implemented yet**

The goals are:
1. Replace the current enemy system with the new one.
2. Implement the exact enemy roles from the game design document.
3. Make enemy movement feel physical: acceleration, braking, max speed.
4. Keep the new system clean, testable, and aligned with the repo style.

---

## 1. Hard decisions for this branch

These are now fixed constraints for the plan.

### 1.1 No compatibility layer
We are **not** preserving old enemy serialization.

That means:
- remove the old enemy assumptions
- remove `Shooter` / `Thief` compatibility logic
- update level definitions to the new enemy type strings
- authored content in this branch can change freely

### 1.2 Replace the current enemies instead of preserving them
The old enemies are not a permanent layer.

Practical meaning:
- `ShooterEnemy` is replaced by the new rifle-style enemies
- `ThiefEnemy` is replaced by `MounterEnemy`
- the new factory / parser / spawn logic only supports the new system

### 1.3 No enemy modifier system
We are **not** building a generic modifier framework.

So there is no:
- `Shield` modifier field
- `Fire` modifier field
- `Ice` modifier field
- “any enemy can have any status package” system

Instead, the enemy roster is exactly the GDD table:
- `Mounter`
- `Rifle`
- `Shield`
- `Anchor`
- `Molotov`
- `TarThrower`

### 1.4 Build with vehicles in mind, but do not implement vehicles yet
We should avoid painting ourselves into a corner.

So the code should be structured so that vehicles can be added later,
but this branch should **not** add:
- vehicle enums to content
- vehicle variants in gameplay
- vehicle balance/content rollout

The system should only leave clean seams for future vehicle support.

### 1.5 `TrainNose` is not damageable
The train engine front should **not** be part of the equipment damage system.

Equipment damage applies to gameplay stations like:
- cannon
- anvil
- speed lever

But **not**:
- `TrainNose`

---

## 2. Exact enemy scope from the GDD

This branch should implement these enemy types and nothing more speculative.

| Enemy Type | Required behavior |
|---|---|
| `Mounter` | Tries to mount the train to steal coal |
| `Rifle` | Shoots equipment, can hit players, stuns players |
| `Shield` | Worse rifle enemy, alternates between shielding and shooting |
| `Anchor` | Anchors train to the ground, slows/stops progress, must be cut |
| `Molotov` | Throws fire hazards, makes areas temporarily inaccessible, can stun players in fire |
| `TarThrower` | Throws tar onto train sides, blocks vision until cleaned |

This table is the source of truth for enemy implementation in this branch.

---

## 3. Current repo snapshot

### Already implemented
- `EnemyManager`
- slot reservation via `EnemySlotManager`
- enemy projectiles
- wall damage and repair
- train speed / fuel / temperature systems
- procedural level generation and enemy budget

### Current problems
- current enemy motion is direct-position stepping and feels unintuitive
- current code only models `Shooter` and `Thief`
- no stun/revive loop
- no repairable equipment state for stations
- no anchor / fire / tar hazards
- no side-obscuring tar system
- spawn data is too limited for the full enemy set

---

## 4. High-level architecture

## 4.1 Keep the repo's simple OO style
Do **not** build:
- a full ECS
- a behavior tree framework
- a generic status/modifier system for enemies

Instead, build:
- one explicit class per enemy type
- one shared `EnemyMovementController`
- one spawn/factory path
- one hazard manager for persistent enemy-created hazards
- one small repair system for breakable stations
- one small stun/revive system for players

That matches the codebase much better.

## 4.2 Enemy type is the core serialized concept
The main content-level identity should be `EnemyType`.

Recommended enum:
- `Mounter`
- `Rifle`
- `Shield`
- `Anchor`
- `Molotov`
- `TarThrower`

That enum should drive:
- factory creation
- budget cost
- authored level definitions
- future debug tools

## 4.3 Vehicle-ready without implementing vehicles
We do not want to add vehicles now, but we do want clean future extensibility.

Recommended approach:
- keep movement tuning in `EnemyMovementProfile`
- keep visuals/identity in enemy classes for now
- keep factory creation centralized
- avoid hardwiring movement math directly into each enemy class

Then later, if vehicles are added, the likely extension becomes:
- `EnemyType` decides behavior
- `VehicleType` decides movement/body profile

But for this branch, **do not add `VehicleType` to content or gameplay**.

---

## 5. Repo conventions to preserve

These should be followed in all implementation work.

### 5.1 Namespace and file style
- Use file-scoped namespaces.
- Keep file names explicit and domain-based.
- Keep enemy files under `Src/Enemies/*`.
- Put persistent hazards under `Src/PhysicalEntities/Hazards/*`.
- Put new projectiles under `Src/PhysicalEntities/Projectiles/*`.

### 5.2 Update model
- All gameplay behavior stays in the fixed-step loop already used in `GameplayScreen`.
- No gameplay logic in `Draw`.
- No side-effecting timing logic hidden in rendering.

### 5.3 Physics safety
- Do not drive active enemies by writing `Position` every frame.
- Use `PhysicsBody.LinearVelocity` through `EnemyMovementController`.
- Continue using deferred physics changes when mutation is triggered from collisions.

### 5.4 Config style
- Put tuning values in `GameplayConfig`.
- Mirror them in `Src/Content/Data/gameplay.json`.
- Keep defaults in code robust.

### 5.5 Behavior style
- Use small explicit enums for enemy states.
- Use guard clauses.
- Prefer readable logic over abstract AI patterns.
- Put shared math in helpers; keep gameplay decisions in enemy classes.

### 5.6 Serialization/content style for this branch
- Breaking changes are acceptable.
- Remove old enemy type assumptions.
- Update all authored levels in this branch to use the new enemy names.
- Do not add compatibility parsing.

### 5.7 Testability
- Put pure math / selection / budget logic in plain helpers.
- Add focused NUnit tests for those helpers.

---

## 6. File plan

## 6.1 New enemy foundation files
- `Src/Enemies/EnemyType.cs`
- `Src/Enemies/EnemyDefinition.cs`
- `Src/Enemies/EnemyIds.cs`
- `Src/Enemies/EnemyMovementProfile.cs`
- `Src/Enemies/EnemyMovementController.cs`
- `Src/Enemies/EnemyFactory.cs`
- `Src/Enemies/EnemyCatalog.cs`
- `Src/Enemies/EnemyHazardManager.cs`

## 6.2 New concrete enemy files
- `Src/Enemies/MounterEnemy.cs`
- `Src/Enemies/RifleEnemy.cs`
- `Src/Enemies/ShieldEnemy.cs`
- `Src/Enemies/AnchorEnemy.cs`
- `Src/Enemies/MolotovEnemy.cs`
- `Src/Enemies/TarThrowerEnemy.cs`

## 6.3 New hazard/runtime support files
- `Src/PhysicalEntities/Hazards/FireZone.cs`
- `Src/PhysicalEntities/Hazards/TarPatch.cs`
- `Src/PhysicalEntities/Hazards/AnchorCable.cs`
- `Src/PhysicalEntities/Projectiles/MolotovProjectile.cs`
- `Src/PhysicalEntities/Projectiles/TarProjectile.cs`

## 6.4 New player/equipment support files
- `Src/Players/PlayerCondition.cs`
- `Src/PhysicalEntities/IRepairable.cs`
- `Src/PhysicalEntities/RepairState.cs`

## 6.5 Existing files to refactor heavily
- `Src/Enemies/AbstractEnemy.cs`
- `Src/Enemies/EnemyManager.cs`
- `Src/Enemies/EnemyTrainSlot.cs`
- `Src/Enemies/EnemySlotManager.cs`
- `Src/Levels/SpawnEvent.cs`
- `Src/Levels/ProceduralLevelGenerator.cs`
- `Src/Levels/RunDifficultyConfig.cs`
- `Src/Config/GameplayConfig.cs`
- `Src/Content/Data/gameplay.json`
- `Src/Players/Player.cs`
- `Src/Screens/GameplayScreen.cs`
- `Src/Levels/LevelManager.cs`
- `Src/PhysicalEntities/Stations/Cannon/Cannon.cs`
- `Src/PhysicalEntities/Stations/Workbenches/AbstractWorkbench.cs`
- `Src/PhysicalEntities/Stations/SpeedLever.cs`

## 6.6 Existing files to remove once migration is done
- `Src/Enemies/ShooterEnemy.cs`
- `Src/Enemies/ThiefEnemy.cs`

These should not survive as legacy parallel systems.

---

## 7. Enemy movement plan

This is the foundation that everything else should sit on.

## 7.1 Why the current motion feels wrong
Right now enemies are moved by direct position stepping.
That causes:
- instant speed changes
- no visible acceleration
- no braking feel
- unnatural holding behavior
- custom ad-hoc logic per enemy

## 7.2 Target movement model
Every enemy should move like it has real momentum:
- accelerate toward a desired velocity
- decelerate when slowing down
- respect a max speed
- naturally fail to keep up if train speed is too high

## 7.3 Train-relative drift model
In this game, the train moves through the world while the camera stays train-relative.
The cleanest enemy model is:

- `TrainFrameDrift = new Vector2(-gameplayContext.State.actualSpeed, 0f)`
- `SelfPropelledVelocity = enemy movement chosen by AI`
- `FinalVelocity = TrainFrameDrift + SelfPropelledVelocity`

Meaning:
- off-train enemies drift left if they do not move hard enough to keep up
- enemies that can match the train hold position naturally
- enemies fall behind naturally when train speed outpaces them

This is the correct basis for intuitive movement in this codebase.

## 7.4 Attached states disable drift
When an enemy or hazard is attached to the train, it should not be treated like a free world-space pursuer.

Examples:
- mounter latched to the train
- anchor cable attached to train side
- tar patch stuck on a train side
- fire zone attached to a train area

In those states:
- disable train-frame drift for that object
- operate in train-relative coordinates instead

## 7.5 `EnemyMovementController`
Build one shared controller for the enemy system.

Suggested responsibilities:
- store self-propelled velocity intent
- accelerate toward desired self velocity
- brake toward zero
- clamp by profile max speed
- support arrival behavior near target points
- apply train-frame drift when appropriate
- write final `PhysicsBody.LinearVelocity`

Suggested config in `EnemyMovementProfile`:
- `MaxForwardSpeed`
- `MaxReverseSpeed`
- `MaxLateralSpeed`
- `Acceleration`
- `Deceleration`
- `ArrivalRadius`
- `BrakeRadius`
- `TurnResponsiveness`

Suggested API:
- `UpdateTowardPoint(Vector2 targetPosition, float dt, bool includeTrainDrift = true)`
- `UpdateTowardDirection(Vector2 direction, float desiredSpeed, float dt, bool includeTrainDrift = true)`
- `UpdateStop(float dt, bool includeTrainDrift = true)`
- `UpdateHoldPosition(Vector2 anchor, float dt, bool includeTrainDrift = true)`

Keep it simple and explicit.

## 7.6 Movement implementation rule
During active movement, enemy classes should **not** do this anymore:
- `Position = ...`
- old-style `MoveTowards(...)` stepping

Direct `Position` writes should be reserved for:
- spawn placement
- one-time snapping during controlled transitions if absolutely necessary

## 7.7 `AbstractEnemy` refactor
`AbstractEnemy` should become the shared foundation for:
- health
- removal
- slot ownership
- enemy definition/type metadata
- access to `EnemyMovementController`
- common off-screen cleanup helpers

The old `MoveTowards(...)` helper should be removed or retired.

## 7.8 Definition of success for movement
Movement is correct when:
- enemies visibly accelerate into motion
- enemies do not snap unnaturally into slots
- enemies brake instead of instantly stopping
- high train speed naturally causes weaker enemies to lag behind
- no enemy jitters around its target anchor

---

## 8. Enemy type design

These are the concrete enemy behaviors to implement.

## 8.1 Mounter enemy

### Purpose
Steal coal by mounting the train.

### Recommended states
- `Approaching`
- `Mounting`
- `MountedStealing`
- `Escaping`

### Behavior
1. Spawn on top or bottom approach side.
2. Physically approach a reserved mount slot.
3. Enter a short mount transition.
4. Attach to the train side.
5. Steal coal from `TrainState.CoalAmount`.
6. Escape off the train after stealing or if interrupted.

### Important design choice
Do **not** add interior train pathfinding in this branch.

Recommended interpretation:
- the mounter steals from the shared train coal pool while mounted to the train side

That fits the current game geometry and current systems.

---

## 8.2 Rifle enemy

### Purpose
Apply ranged pressure to equipment and players.

### Recommended states
- `ApproachingSideAttackSlot`
- `HoldingSideAttackSlot`
- `Aiming`
- `Recovering`

### Required behavior
- shoot breakable equipment
- shoot players
- shots can stun players
- use walls and breaches in a readable way

### Locked wall/breach targeting rule
Rifle enemies should **not** directly target interior equipment or players through intact walls.

Their targeting flow should be:
1. if there is **no clear interior shot** on their current lane, attack the wall segment on that lane to create a breach
2. once there is a **clear interior shot**, target interior gameplay objects in priority order
3. if players repair the wall and the interior shot is no longer clear, go back to wall pressure
4. if nothing better is available, fall back to wall pressure

### Interior target priority after a breach / clear shot
1. cannon
2. speed lever
3. anvil
4. active player
5. intact wall

This keeps walls meaningful and makes rifle pressure understandable.

### Notes
This is the clean replacement for the current `ShooterEnemy` role.

---

## 8.3 Shield enemy

### Purpose
A defensive ranged enemy that alternates between offense and shielding.

### Recommended states
- `ApproachingSideAttackSlot`
- `Shielding`
- `Aiming`
- `Recovering`

### Required behavior
- hold shield up for a time window
- while shielding, block incoming cannon projectiles from the protected side
- while shielding, do not shoot
- while exposed, shoot like a weaker/slower rifle enemy
- use the same wall/breach targeting rule as the rifle enemy when choosing whether to pressure walls or interior targets

### Required gameplay feel
It should be “worse” offensively than the rifle guy, but harder to safely pick off.

Recommended tuning differences vs rifle:
- lower fire cadence
- longer recovery
- more idle time
- maybe slightly lower movement responsiveness

### Hit logic
Projectile blocking should happen in enemy `OnHit(...)` logic based on:
- whether shield is active
- whether the projectile comes from the shielded angle

This keeps the rule readable and localized.

---

## 8.4 Anchor enemy

### Purpose
Force players to clear an anchor before progress can continue.

### Recommended states
- `ApproachingDeployPoint`
- `Deploying`
- `AnchorActive`
- `Retreating`

### Required runtime pieces
- `AnchorEnemy`
- `AnchorCable` or equivalent train-side cut target

### Train effect
Do **not** teleport train speed.
Instead add explicit train debuff state to `TrainState`, for example:
- `ActiveAnchorCount`
- `ExternalSpeedCap`
- or `AnchorDragMultiplier`

Recommended first pass:
- an active anchor reduces allowed train speed sharply
- enough anchors can force the train to effectively stop

### Player interaction
Players must be able to cut the anchor by interacting with the anchor attachment/cable.

### Level-completion rule
An active anchor counts as an unresolved threat.
Progress should not be considered clear while an anchor is still active.

---

## 8.5 Molotov enemy

### Purpose
Create temporary fire denial zones.

### Recommended states
- `ApproachingThrowSlot`
- `AimingThrow`
- `Throwing`
- `Repositioning`

### Required runtime pieces
- `MolotovEnemy`
- `MolotovProjectile`
- `FireZone`

### Fire zone behavior
- spawns where the molotov lands
- persists for a fixed time
- makes standing in the area unsafe
- builds up player stun if players remain inside
- can make interaction in that area impractical or blocked

### Accessibility recommendation
Keep it clear and gamey:
- fire is an area denial hazard
- prolonged exposure stuns players
- it should not create permanent lock states

---

## 8.6 Tar thrower enemy

### Purpose
Blind one side of the train until players clean it.

### Recommended states
- `ApproachingThrowSlot`
- `AimingThrow`
- `Throwing`
- `Repositioning`

### Required runtime pieces
- `TarThrowerEnemy`
- `TarProjectile`
- `TarPatch`

### Tar patch behavior
- attaches to `Top` or `Bottom` side of the train
- creates a strong visual occlusion overlay on that side
- remains until cleaned by the players

### Required gameplay effect
Tar is not just cosmetic.
It should materially remove information from that side until players deal with it.

### Completion recommendation
Tar patches should not soft-lock the run forever once combat is done.
They can persist until cleaned, but cleanup should also happen safely on level transition if necessary.

---

## 9. Player stun and revive

This is required by the GDD and by the rifle/molotov enemy designs.

## 9.1 `PlayerCondition`
Add a small player condition model.

Suggested enum:
- `Active`
- `Stunned`

Suggested player fields:
- `Condition`
- `StunTimer` if needed
- `ReviveProgress`

## 9.2 Stun rules
When stunned:
- player cannot move
- player cannot interact
- player cannot pickup/grab
- player should drop held item if needed

## 9.3 Revive rules
A second player should revive by holding interact near the stunned player.

Recommended implementation style:
- same “hold to fill” feel as wall repair
- no complex mini-game

## 9.4 Fail state
The GDD explicitly calls for:
- lose when all players are stunned

Recommended implementation detail:
- use a short grace window before failure to avoid frame-perfect frustrating losses

## 9.5 Files to touch
- `Src/Players/Player.cs`
- `Src/Screens/GameplayScreen.cs`
- new `Src/Players/PlayerCondition.cs`

---

## 10. Equipment damage and repair

This is required for the rifle enemy.

## 10.1 Introduce repairable equipment
Create a small repair system instead of custom broken-state code in every station.

Suggested pieces:
- `IRepairable`
- `RepairState`

`RepairState` should manage:
- current health
- broken/usable state
- repair progress behavior if shared

## 10.2 First equipment set to support
Recommended first breakable equipment:
- `CannonStation`
- `SpeedLever`
- `Anvil`

Possible later extension:
- other stations if needed

## 10.3 Explicit exclusion
Do **not** add damage/repair handling to:
- `TrainNose`

That is a firm branch requirement.

## 10.4 Gameplay behavior
When broken:
- cannon cannot fire
- speed lever cannot change speed
- anvil cannot craft

Players restore these by repairing them.

## 10.5 Walls remain separate
`ShootHoleWall` already has its own damage/repair logic.
That can remain separate unless later consolidation becomes useful.

---

## 11. Hazard system

Anchor, molotov, and tar all require runtime objects that outlive a single shot or animation.

## 11.1 `EnemyHazardManager`
Add a dedicated manager parallel to `ProjectileManager`.

Responsibilities:
- own active hazards
- update them
- draw them
- clean them up
- expose whether any hazards still count as active threats

## 11.2 Why a hazard manager is necessary
Without it, hazard ownership gets messy:
- enemies may die while hazards remain
- level completion needs to know about unresolved anchor threats
- cleanup on screen/level transitions becomes fragile

## 11.3 Hazard categories
### Threat-blocking hazards
- active anchors

These should count as unresolved threats.

### Temporary or informational hazards
- fire zones
- tar patches

These should be tracked and cleaned, but should not permanently block completion if they are the only remaining objects.

---

## 12. Spawn data and content model

## 12.1 New spawn definition direction
The old serialized concept of `Shooter` / `Thief` is removed.

### Recommended `SpawnEvent` direction
Keep `SpawnEvent`, but make its `Type` represent the new enemy types only:
- `Mounter`
- `Rifle`
- `Shield`
- `Anchor`
- `Molotov`
- `TarThrower`

If more metadata is needed later, extend it directly for the new system.

## 12.2 No compatibility parsing
Do **not** add code that translates:
- `Shooter -> Rifle`
- `Thief -> Mounter`

Instead:
- update level JSON files
- update procedural generation
- keep runtime code clean

## 12.3 Side support
The side system should explicitly support the needs of the new enemies.

For this branch, all enemies should use only:
- `Top`
- `Bottom`

That includes:
- `Mounter`
- `Rifle`
- `Shield`
- `Anchor`
- `Molotov`
- `TarThrower`

So there is no `Right` enemy side in this plan.
The runtime, authored levels, and procedural generation should all assume top/bottom attacks only.

## 12.4 Slot system update
The slot manager is still a good abstraction, but it needs richer slot groups.

Recommended slot groups:
- side attack slots for rifle/shield/molotov/tar enemies
- mount slots for mounters
- anchor deploy slots for anchors on the top/bottom lanes

Avoid full pathfinding if slots can solve the encounter cleanly.

---

## 13. Budget system plan

The enemy budget system should stay, but it should become type-based.

## 13.1 Cost by enemy type
Replace the old two-enemy cost model with explicit costs per new enemy type.

Suggested first-pass costs:
- `Mounter = 2`
- `Rifle = 3`
- `Shield = 4`
- `Anchor = 5`
- `Molotov = 4`
- `TarThrower = 4`

These are tuning values, not API guarantees.

## 13.2 Vehicle-ready note
Because vehicles are not in this branch, the budget should **not** have vehicle surcharges yet.

Future-friendly recommendation:
- keep cost calculation centralized in `EnemyCatalog`
- when vehicles are added later, a vehicle surcharge can be layered in there

## 13.3 Procedural rollout and testing requirement
The procedural generator is not just final content.
It is also one of the fastest ways to test the new enemy roster repeatedly.

That means the old two-way logic in `ProceduralLevelGenerator`:
- `Shooter`
- `Thief`

must be replaced with new enemy-type selection for this branch.

### Required generator change
Rewrite the current procedural budget spend loop so it:
- works on the new enemy type list
- filters by currently affordable enemy types
- chooses from those types with weighted randomness
- writes new `SpawnEvent.Type` values directly:
  - `Mounter`
  - `Rifle`
  - `Shield`
  - `Anchor`
  - `Molotov`
  - `TarThrower`
- chooses a valid side for the chosen enemy type

### Practical recommendation
Move enemy cost and selection logic into `EnemyCatalog` or a similarly centralized helper.
Then `ProceduralLevelGenerator` becomes:
- compute budget
- ask the catalog for affordable candidates at this level
- roll one candidate
- subtract its cost
- emit the matching `SpawnEvent`

### Testing requirement
For this branch, the procedural generator should be usable to test the new enemies as soon as they exist.
So even if the long-term progression curve is conservative, there should be an easy way to expose all six enemy types during development.

Recommended options:
- unlock by level as normal, but keep thresholds low during development
- or expose a config-driven test mode that allows all enemy types early

### Recommended rollout curve
For normal progression, this still makes sense:
- early: `Mounter`, `Rifle`
- mid: `Shield`, `Molotov`, `TarThrower`
- later: `Anchor`

But for branch testing, do not rely only on authored levels.
The procedural generator should actively participate in testing the new roster.

---

## 14. Detailed implementation phases

## Phase 0 — replace foundations

### Goal
Remove the old enemy assumptions and prepare the new architecture.

### Tasks
- add `EnemyType`
- add `EnemyDefinition`
- add `EnemyMovementProfile`
- add `EnemyMovementController`
- add `EnemyFactory`
- add `EnemyCatalog`
- update `SpawnEvent` semantics to the new enemy types
- update slot system for new enemy roles and side support

### Important branch rule
At this phase, it is acceptable to break old enemy content.
Do **not** spend time on compatibility.

### Acceptance criteria
- runtime no longer depends on `Shooter` / `Thief`
- factory and content model are based on the new enemy types

---

## Phase 1 — physical movement

### Goal
Get all enemy movement onto `EnemyMovementController` before adding full enemy variety.

### Tasks
- refactor `AbstractEnemy`
- remove direct active-motion position stepping
- implement movement controller with train drift model
- prove out the controller on one or two enemy types first

### Acceptance criteria
- enemies accelerate
- enemies brake
- enemies respect max speed
- enemies naturally fall behind when train speed is too high

This phase is the foundation for the whole rest of the system.

---

## Phase 2 — player stun and equipment repair

### Goal
Build the support systems required by the GDD enemies.

### Tasks
- implement player stun/revive
- implement fail-if-all-players-stunned
- implement repairable equipment
- make cannon / speed lever / anvil breakable
- keep `TrainNose` explicitly non-damageable

### Acceptance criteria
- players can be stunned and revived
- rifle-style threats can disable stations
- all-players-stunned loss works correctly

---

## Phase 3 — implement enemy types

### Goal
Add the full GDD enemy roster.

### Tasks
- implement `MounterEnemy`
- implement `RifleEnemy`
- implement `ShieldEnemy`
- implement `AnchorEnemy`
- implement `MolotovEnemy`
- implement `TarThrowerEnemy`
- add hazard manager and hazard objects

### Acceptance criteria
- every enemy type from the GDD exists in gameplay
- they use the new movement foundation
- anchor/fire/tar objects clean up correctly

---

## Phase 4 — content and budget rollout

### Goal
Make authored and procedural content use the new roster.

### Tasks
- rewrite authored level definitions to the new enemy types
- update procedural generator to spend budget on the new type list
- make sure procedural generation can be used to test all new enemy types during development
- tune spawn pacing and enemy costs
- remove old `ShooterEnemy` / `ThiefEnemy` files once replacement is complete

### Acceptance criteria
- level content only uses the new system
- budget rollout is stable and deterministic
- no legacy enemy path remains in active use

---

## 15. Enemy-specific implementation notes

## 15.1 Mounter
- should feel opportunistic, not tanky
- should use a clean attach/detach flow
- should steal from shared coal amount
- should not need complex onboard pathfinding

## 15.2 Rifle
- should create pressure even when not directly kill-focused
- must make equipment damage readable
- should have clean target selection priorities

## 15.3 Shield
- should be readable from animation/state timing
- should not become a boring bullet sponge
- the shield window needs strong telegraphing

## 15.4 Anchor
- should be high-priority and structurally important
- should feel like a “solve this now” threat
- its cut interaction must be obvious and reliable

## 15.5 Molotov
- should shape movement/positioning
- fire zones must be easy to read visually
- fire should stun by exposure, not by mystery burst logic

## 15.6 TarThrower
- should reshape information, not raw damage
- tar should clearly communicate which side is blinded
- cleanup interaction should be simple and consistent

---

## 16. Testing plan

## 16.1 Pure logic tests to add
Recommended tests:
- `EnemyMovementController` acceleration and braking
- train-drift application
- arrival behavior near anchors/slots
- shield projectile block angle logic
- budget cost calculation
- procedural selection staying inside budget

## 16.2 Manual gameplay checks
### Movement
- enemy accelerates into slot
- enemy does not snap to holding position
- enemy falls behind naturally at high train speed

### Stun / revive
- rifle projectile can stun player
- player can revive another player
- all stunned causes fail

### Equipment damage
- cannon can break and be repaired
- speed lever can break and be repaired
- anvil can break and be repaired
- `TrainNose` cannot be broken

### Anchor
- anchor slows/stops progress
- player can cut it
- level does not finish while anchor remains active

### Molotov
- fire zone appears
- fire zone expires correctly
- player can be stunned by prolonged fire exposure

### Tar
- tar obscures the correct side
- tar is cleanable
- tar does not cause cleanup leaks across transitions

### Content
- authored levels use the new enemy strings
- procedural budget produces reasonable mixes
- procedural generation can spawn every new enemy type for testing

## 16.3 Debug aids worth adding
Recommended debug tools:
- draw slot anchors
- draw enemy state labels
- draw current movement velocity vectors
- show enemy type above enemy in debug mode
- show active hazard counts

These will help enemy tuning a lot.

---

## 17. Open questions with recommended defaults

### Q1. Do we add vehicle serialization now?
**Recommended default:** No.
Build `EnemyMovementProfile` and factory seams so vehicles are easy later, but do not serialize or implement vehicles in this branch.

### Q2. Should fire or tar block level completion?
**Recommended default:**
- anchors: yes, they count as unresolved threats
- fire/tar: no permanent completion block; they should clean up safely or expire

### Q3. Should mounters pathfind onto the train interior?
**Recommended default:** No.
Mount and steal from the shared coal system from the train side.

### Q4. Which stations are breakable first?
**Recommended default:** cannon, speed lever, anvil.

### Q5. Does `TrainNose` participate in equipment damage?
**Recommended default:** No. It stays non-damageable.

---

## 18. Definition of done

Enemy work is done for this branch when:
- old enemy assumptions are removed
- the game uses the six GDD enemy types
- enemy movement uses acceleration/braking/max speed
- player stun/revive exists
- all-players-stunned fail state exists
- cannon, speed lever, and anvil are repairable/breakable
- `TrainNose` remains non-damageable
- anchor, fire, and tar hazards work and clean up correctly
- authored levels are updated to the new enemy type names
- procedural enemy budget uses the new roster
- no legacy compatibility code remains in the runtime path

---

## 19. Recommended PR order

1. enemy type/content model rewrite
2. `EnemyMovementController` + movement refactor
3. player stun/revive + repairable equipment
4. `MounterEnemy` + `RifleEnemy`
5. `ShieldEnemy`
6. `AnchorEnemy` + anchor hazard
7. `MolotovEnemy` + fire zone
8. `TarThrowerEnemy` + tar patch
9. authored/procedural content rollout
10. delete old enemy files and clean dead code

---

## 20. Step-by-step file change sequence

This is the recommended exact file-change order.

The goal of this sequence is:
- keep changes understandable
- keep compile break windows short
- avoid rewriting the same files multiple times
- switch cleanly from the old enemy system to the new one

### Step 1 — define the new enemy identity layer
Create these files first:
- `Src/Enemies/EnemyType.cs`
- `Src/Enemies/EnemyIds.cs`
- `Src/Enemies/EnemyDefinition.cs`
- `Src/Enemies/EnemyCatalog.cs`

Change these files in the same step:
- `Src/Levels/SpawnEvent.cs`

What to do:
- define the six valid enemy types
- centralize their ids / strings
- define per-type metadata in the catalog
- change `SpawnEvent` comments/docs so `Type` means only the new enemy names

Do **not** delete old enemies yet in this step.

### Step 2 — replace slot naming and slot groups
Change these files:
- `Src/Enemies/EnemyTrainSlot.cs`
- `Src/Enemies/EnemySlotManager.cs`

What to do:
- keep only `Top` / `Bottom` side semantics
- replace old mental model with explicit slot groups:
  - side attack slots
  - mount slots
  - anchor deploy slots
- update slot reservation APIs to reflect those groups

Recommended naming direction in code:
- `TryReserveSideAttackSlot...`
- `TryReserveMountSlot...`
- `TryReserveAnchorDeploySlot...`

### Step 3 — add shared enemy movement foundation
Create these files:
- `Src/Enemies/EnemyMovementProfile.cs`
- `Src/Enemies/EnemyMovementController.cs`

Refactor this file:
- `Src/Enemies/AbstractEnemy.cs`

What to do:
- add the train-drift based movement controller
- move shared acceleration/braking/max-speed behavior into it
- make `AbstractEnemy` own enemy type metadata and movement helper access
- remove the old position-step movement as the default runtime path

### Step 4 — switch the manager/factory architecture
Create these files:
- `Src/Enemies/EnemyFactory.cs`
- `Src/Enemies/EnemyHazardManager.cs`

Refactor these files:
- `Src/Enemies/EnemyManager.cs`
- `Src/Levels/LevelManager.cs`

What to do:
- make `EnemyManager` spawn by `EnemyType`
- centralize concrete enemy creation in `EnemyFactory`
- add hazard ownership/update/draw/cleanup
- update level-completion logic so active anchors still count as unresolved threats

After this step, the runtime shape of the new enemy system exists even if all enemy types are not implemented yet.

### Step 5 — add player stun / revive support
Create this file:
- `Src/Players/PlayerCondition.cs`

Refactor these files:
- `Src/Players/Player.cs`
- `Src/Screens/GameplayScreen.cs`

What to do:
- add stunned player state
- prevent stunned players from moving/interacting
- drop held items on stun
- add revive-by-hold-interact
- add all-players-stunned fail logic with the chosen grace period

### Step 6 — add repairable equipment support
Create these files:
- `Src/PhysicalEntities/IRepairable.cs`
- `Src/PhysicalEntities/RepairState.cs`

Refactor these files:
- `Src/PhysicalEntities/Stations/Cannon/Cannon.cs`
- `Src/PhysicalEntities/Stations/Workbenches/AbstractWorkbench.cs`
- `Src/PhysicalEntities/Stations/SpeedLever.cs`

What to do:
- add broken/usable state for equipment
- make cannon, anvil, and speed lever breakable and repairable
- keep `TrainNose` untouched and non-damageable

### Step 7 — implement `MounterEnemy`
Create this file:
- `Src/Enemies/MounterEnemy.cs`

Refactor these files:
- `Src/Enemies/EnemyManager.cs`
- `Src/Enemies/EnemyFactory.cs`

What to do:
- implement top/bottom mount-slot approach
- implement mount/steal/escape flow
- use `EnemyMovementController`
- steal from shared train coal amount

At the end of this step, the old thief role is functionally replaced.

### Step 8 — implement `RifleEnemy`
Create this file:
- `Src/Enemies/RifleEnemy.cs`

Refactor these files:
- `Src/Enemies/EnemyManager.cs`
- `Src/Enemies/EnemyFactory.cs`
- any targeting helper file if introduced

What to do:
- implement side-attack-slot behavior
- implement wall-first, breach-aware targeting
- implement equipment damage and player stun shots
- keep wall pressure as fallback

At the end of this step, the old shooter role is functionally replaced.

### Step 9 — implement `ShieldEnemy`
Create this file:
- `Src/Enemies/ShieldEnemy.cs`

Refactor these files:
- `Src/Enemies/EnemyFactory.cs`
- `Src/Enemies/EnemyManager.cs`
- `Src/Enemies/AbstractEnemy.cs` only if shield-hit helpers belong there

What to do:
- implement shield windows
- block frontal cannon shots while shielding
- reuse the rifle wall/breach targeting rule when exposed

### Step 10 — implement `AnchorEnemy` and anchor hazard
Create these files:
- `Src/Enemies/AnchorEnemy.cs`
- `Src/PhysicalEntities/Hazards/AnchorCable.cs`

Refactor these files:
- `Src/Enemies/EnemyHazardManager.cs`
- `Src/Enemies/EnemyFactory.cs`
- `Src/Map/Train/State/TrainState.cs`
- `Src/Screens/GameplayScreen.cs` if needed for failure/threat plumbing

What to do:
- implement top/bottom anchor deploy behavior
- add active-anchor effect on train movement
- add player cut/clear interaction
- make anchors block level completion until cleared

### Step 11 — implement `MolotovEnemy` and fire hazard
Create these files:
- `Src/Enemies/MolotovEnemy.cs`
- `Src/PhysicalEntities/Projectiles/MolotovProjectile.cs`
- `Src/PhysicalEntities/Hazards/FireZone.cs`

Refactor these files:
- `Src/Enemies/EnemyHazardManager.cs`
- `Src/Enemies/EnemyFactory.cs`
- `Src/Players/Player.cs` if fire exposure bookkeeping lives there

What to do:
- implement throw behavior from side attack slots
- spawn temporary fire zones
- stun players after prolonged exposure
- make fire temporary and safe to clean up

### Step 12 — implement `TarThrowerEnemy` and tar hazard
Create these files:
- `Src/Enemies/TarThrowerEnemy.cs`
- `Src/PhysicalEntities/Projectiles/TarProjectile.cs`
- `Src/PhysicalEntities/Hazards/TarPatch.cs`

Refactor these files:
- `Src/Enemies/EnemyHazardManager.cs`
- `Src/Enemies/EnemyFactory.cs`
- `Src/Screens/GameplayScreen.cs` or rendering-support files if side occlusion is drawn there

What to do:
- implement throw behavior from side attack slots
- attach tar patches to top/bottom sides
- add clean-by-interact behavior
- make tar affect visibility, not direct damage

### Step 13 — replace procedural generation and authored content
Refactor these files:
- `Src/Levels/RunDifficultyConfig.cs`
- `Src/Levels/ProceduralLevelGenerator.cs`
- `Src/Content/Data/levels/*`

What to do:
- remove the old shooter/thief-only budget logic
- add cost fields for the six new enemy types
- make procedural generation select from affordable new enemy types
- ensure procedural generation can spawn every new enemy type for testing
- rewrite authored level definitions to the new enemy names

### Step 14 — update gameplay config and tuning surface
Refactor these files:
- `Src/Config/GameplayConfig.cs`
- `Src/Content/Data/gameplay.json`

What to do:
- remove old shooter/thief-specific tuning that is no longer used
- add movement, stun, shield, anchor, fire, tar, and equipment-damage tuning values
- keep defaults sane even before content is fully tuned

### Step 15 — add tests for the new foundations
Refactor/add files under:
- `Tests/*`

Recommended first tests:
- `EnemyMovementController` acceleration / braking / clamp behavior
- train-drift application
- shield frontal block logic
- procedural budget selection staying within budget
- parsing/selection for the new enemy types

### Step 16 — delete dead old enemy code
Delete these files once the new system is fully wired:
- `Src/Enemies/ShooterEnemy.cs`
- `Src/Enemies/ThiefEnemy.cs`

Then do final cleanup in:
- `Src/Enemies/EnemyManager.cs`
- `Src/Config/GameplayConfig.cs`
- `Src/Content/Data/gameplay.json`
- any authored levels still referencing removed enemy names

This deletion step should come last so you do not create unnecessary intermediate compile breaks.

---

## 21. Final recommendation

The order of work matters.

### First solve `EnemyMovement`, then add the roster.
If movement remains ad-hoc, every new enemy will feel wrong.
If movement is solved first, all six enemy types become much easier to build and tune.

So the actual recommended implementation order is:
- replace enemy foundations
- build `EnemyMovement`
- add stun/revive and repairable equipment
- build the six GDD enemy types
- update level content and budget

That is the cleanest branch plan given your new constraints.
