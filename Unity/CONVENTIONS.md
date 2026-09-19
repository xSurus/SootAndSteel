# Unity port conventions

Established by Phase 0 (docs/superpowers/plans/2026-09-18-unity-port-plan.md).
Every subsystem agent follows these so work merges without renegotiating basics.

## Assemblies

- `Gamelab.Core` (`Assets/Scripts/Core/`): engine-agnostic logic, `noEngineReferences: true`.
  Anything that can be tested without a scene goes here: services, catalogs,
  domain interfaces, pure data/calculation types.
- `Gamelab.Runtime` (`Assets/Scripts/Runtime/`): MonoBehaviours and anything
  touching `UnityEngine` directly (physics, input, rendering).
- `Gamelab.Tests.EditMode` (`Assets/Tests/EditMode/`): NUnit tests for `Core`.
  Add a matching `Gamelab.Tests.PlayMode` (references `Runtime`) the first
  time a subsystem needs a PlayMode test.

## Namespaces

Keep the original `Gamelab.*` namespace roots from `Src/` unchanged where the
type ports directly (e.g. `Gamelab.Services.Random`). This keeps side-by-side
diffing against the MonoGame source easy.

New test classes go under `Gamelab.Tests.*` (e.g. `Gamelab.Tests.Services`),
mirroring the type under test. Several Wave A agents are adding EditMode
tests in parallel; the shared namespace prefix keeps test class names from
colliding.

## C# language version constraints

This project's Unity assemblies are pinned to C# 9.0 targeting netstandard2.1
(confirmed via the generated `Gamelab.Core.csproj`/`Gamelab.Tests.EditMode.csproj`
and their `.rsp` files). This differs from the MonoGame source in `Src/`, which
targets net9.0 and uses modern C# features. When porting code, adapt for these
constraints:

- **No file-scoped namespace declarations** (`namespace Foo;`) — C# 10+ only.
  Use the traditional braced form (`namespace Foo { ... }`).
- **No `System.Random.Shared`** — .NET 6+ only. Use `new System.Random()` instead.
  Trade-off: `Random.Shared` is thread-safe and avoids correlated sequences when
  two instances are created in the same clock tick; `new Random()` has no such
  guarantee. Fine for a single long-lived instance (the common pattern here),
  worth a one-line comment if a ported type is instantiated more than once.
- **Check for other C# 10+/.NET 6+ syntax and APIs** before porting any file from `Src/`.
  Examples: certain record type features, raw string literals, generic math.
  This list isn't exhaustive; these two are the ones already hit.

### Found by A2 (domain/catalog layer)

Porting the domain/catalog layer (bullets, enemies, stations, physical-entity
interfaces) hit several more C# 10-12/.NET 6+ idioms in the MonoGame source that
don't work in C# 9.0/netstandard2.1. Add these to your mental model before
porting any file in this subsystem:

- **No primary constructors on non-record classes/structs** (C# 12).
  Examples: `class EnemySlotManager()`, `class Counter(Vector2 position) : AbstractStation(...)`,
  `class EnemyMovementController(Body physicsBody, EnemyMovementProfile profile)`.
  Rewrite as explicit constructor bodies instead.

- **No `record struct` / `readonly record struct`** (C# 10).
  Use a plain `readonly struct` with hand-written properties and constructor.
  Value equality isn't needed by anything this subsystem ports; if it becomes
  needed later, hand-write `Equals`/`GetHashCode`.

- **No collection expressions** (C# 12).
  Examples: `[1, 2, 3]`, `[]`, `[..existing]`.
  Use `new[] { ... }`, `new List<T> { ... }`, `new HashSet<T>()` instead.

- **No `ArgumentException.ThrowIfNullOrWhiteSpace` / `ArgumentNullException.ThrowIfNull`** (.NET 6/7 static helpers).
  Use explicit `if` guards:
  `if (string.IsNullOrWhiteSpace(x)) throw new ArgumentException(...)`
  `if (x == null) throw new ArgumentNullException(...)`

- **Verify `System.Linq.Enumerable.MinBy`/`MaxBy` before using them** (.NET 6).
  These aren't guaranteed present in Unity's netstandard2.1 BCL.
  Prefer a manual loop or `OrderBy(...).FirstOrDefault()` when in doubt.

## Sprite import

Every sprite sheet import: Filter Mode = Point (no filter), Compression = None,
Pixels Per Unit = the sheet's native tile size in pixels (check the source
tile dimensions in `Src/Content/` before setting this — they are not all the
same size).

## Catalog-driven data (bullets, enemies, stations)

Port hand-registered catalogs (`EnemyCatalog`, `StationRegistry`, bullet
casing/propellant/projectile combinators) to `ScriptableObject` assets rather
than JSON + factory registration. Keep the same interface names
(`IInteractable`, `IDamageable`, etc.) from `Src/PhysicalEntities/Interfaces/`.
See "Core vs. Runtime: where catalogs and interfaces live" below for which
assembly each piece belongs in.

## Core vs. Runtime: where catalogs and interfaces live

`Gamelab.Core` has `noEngineReferences: true`, so nothing that references
`UnityEngine` (including `ScriptableObject`) can live there. Split ported
catalogs and interfaces like this:

- **`Gamelab.Core`**: the pure data, calculation logic, and engine-free
  interfaces. Use `System.Numerics.Vector2` (available on netstandard2.1)
  anywhere the original MonoGame code used `Microsoft.Xna.Framework.Vector2`.
  Reason: `System.Numerics.Vector2` is engine-agnostic and testable without a
  scene, same as the rest of Core, whereas `UnityEngine.Vector2` drags a
  `UnityEngine` reference into an assembly whose whole point is not having
  one. Don't swap this back to `UnityEngine.Vector2` even though it's the more
  common Unity idiom.
- **`Gamelab.Runtime`**: the `ScriptableObject` catalog/wrapper assets
  themselves (`EnemyCatalog`, `StationRegistry`, bullet combinators), plus any
  interface whose signature needs a `UnityEngine` type. These reference the
  plain data types that live in Core.

## Headless Unity verification (no GUI needed)

You do not need Editor GUI access to verify compilation or run tests — use the
Unity CLI in batch mode. The binary on this machine (macOS) is at:

```
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity
```

(This is this machine's path, not a universal one. On Windows it's typically
`C:\Program Files\Unity\Hub\Editor\6000.3.24f1\Editor\Unity.exe`; on Linux,
`~/Unity/Hub/Editor/6000.3.24f1/Editor/Unity`. Adjust for your platform.)

**Compile-only check** (fast, use `-quit` freely here):

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath <project> -logFile <path>
```

Check exit code 0, the log ends with `Exiting batchmode successfully now!`, and
`grep -i "error CS"` on the log is empty.

**Running tests — do NOT combine `-quit` with `-runTests`.** They race in this
Unity version: batchmode exits before the test runner actually executes,
silently producing no results file and no test-runner trace in the log, while
still exiting 0. This looks like a passing run and isn't one. Use:

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath <project> -runTests -testPlatform EditMode \
  -testResults <path>.xml -logFile <path>.log
```

This takes noticeably longer (Unity quits on its own once tests finish). Always
confirm your test actually ran by checking the results XML contains a `<test-case>`
entry for your specific test with `result="Passed"`. Do not rely solely on the
log or exit code — both can be misleading.

Ignore any `[Licensing::Module] Error: Access token is unavailable` line in any
log — harmless, unrelated to compilation or tests.

## Verification

Before merging any subsystem worktree: `Gamelab.Tests.EditMode` (and
`PlayMode` if present) must be green, and the relevant `GameInstructions.md`
checklist items for that subsystem must be manually re-checked against the
integrated build.

When staging new files, use `git add Unity/Assets` (or run `git status`
afterward and check for stray untracked `.meta` files) instead of narrow
per-file/per-folder `git add` paths. Unity generates a `.meta` file for a
folder one level above the folder's own contents, so a `git add <folder>`
that only targets the folder's contents misses the parent folder's `.meta`
file, and it's easy for the omission to go unnoticed across four parallel
Wave A branches.
