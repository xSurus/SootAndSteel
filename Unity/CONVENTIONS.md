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

## FMOD native libraries

The Editor and PlayMode tests run real FMOD. Provenance and caveats:

- The natives are the same binaries the MonoGame build uses (repo-root
  `libfmodL.dylib`, `fmodL.dll`, `fmodstudioL.dll`, 2.02.19), copied under
  `Assets/Plugins/FMOD/platforms/{mac,win}/lib` and tracked by Git LFS. The
  vendored managed wrapper is 2.02.35. FMOD only enforces major.minor, so this
  runs today, but a header mismatch error means one side moved.
- `platforms/mac/lib/fmodstudioL.dylib` is patched: `install_name_tool
  -add_rpath @loader_path/.` plus an ad hoc `codesign`, so it finds the sibling
  `libfmodL.dylib`. Refreshing it from an FMOD download loses that patch.
- Editor only. The plugin metas enable Editor and disable Standalone. FMOD's
  own macOS build step expects `fmodstudio{L}.bundle`, so a macOS player build
  has no FMOD natives yet. Whoever does the build wave must install the
  official FMOD Unity plugin binaries or extend this setup.
- Windows files are untested (no Windows machine was available).
- `SourceBankPath` is `../Src/Content/soundbanks` (authoring-time read
  dependency on the MonoGame tree). Copy the banks into Unity before `Src/` is
  removed.
- No automated test can prove audible output. Play a scene with
  `SoundServiceRunner` in the Editor with speakers on to confirm.
