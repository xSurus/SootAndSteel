# Wave A3 (Audio) Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace the MonoGame `FmodForFoxes` sound layer (`Src/Services/Sound/`) with a Unity port of `ISoundService` built on the official Unity FMOD integration, reusing the existing `FmodProject/`/`.bank` content unchanged.

**Architecture:** Vendor the official `fmod/fmod-for-unity` C# integration source (branch `2.02`, matching the FMOD Studio project's `Studio.02.02.00` serialization model) into `Unity/Assets/Plugins/FMOD/`. Point its `Settings.SourceBankPath` at the existing `Src/Content/soundbanks/` instead of rebuilding banks inside Unity. Port `Sounds.cs`/`SoundSettings.cs` (genuinely engine-free) to `Gamelab.Core`; port `ISoundService`/`ParameterBinding`/`SoundService` (coupled to `FMOD.Studio`/`FMODUnity` types) to `Gamelab.Runtime`, plus a small `SoundServiceRunner` MonoBehaviour that replaces the MonoGame `IGameSystem.Update(GameTime)` hook the original used to drive parameter bindings each frame. `DesktopAndMacNativeFmodLibrary.cs` is excluded: it exists only to redirect `FmodForFoxes`'s P/Invoke resolver to platform-named native libs; the official Unity integration loads its own native plugins through Unity's normal native-plugin pipeline, so there is nothing for it to do here.

**Tech Stack:** Unity 6 LTS, C# 9.0/netstandard2.1 (per `Unity/CONVENTIONS.md`), official FMOD Unity integration (`fmod/fmod-for-unity`, MIT-licensed C# source on GitHub), Newtonsoft.Json (`com.unity.nuget.newtonsoft-json`), NUnit/Unity Test Framework.

**Spec:** `docs/superpowers/specs/2026-09-18-unity-port-design.md` (see "Target", "What ports directly vs what gets rebuilt" > Audio row, "Risk areas" #3) and the work order in `docs/superpowers/plans/2026-09-18-unity-port-plan.md` (Wave A table, row A3, and Wave A verification gate).

## Global Constraints

- Unity 6 LTS, C#, URP 2D renderer (spec "Target")
- Desktop build targets only: Windows, macOS, Linux (spec "Target")
- FMOD stays the audio middleware; use the official Unity FMOD integration, reuse `FmodProject/` and `.bank` files unchanged (spec "Target")
- The MonoGame build (`Src/Gamelab.csproj`) stays runnable throughout; do not modify anything under `Src/`
- `Gamelab.Core` (`Assets/Scripts/Core/`) is engine-free (`noEngineReferences: true`); `Gamelab.Runtime` (`Assets/Scripts/Runtime/`) holds anything coupled to `UnityEngine`/`FMODUnity` types (`Unity/CONVENTIONS.md`)
- C# 9.0/netstandard2.1: no file-scoped namespaces, no `System.Random.Shared`, watch for other C# 10+/.NET 6+ syntax (`Unity/CONVENTIONS.md`)
- Namespaces stay `Gamelab.*` where a type ports directly, e.g. `Gamelab.Services.Sound` (`Unity/CONVENTIONS.md`)
- New test classes go under `Gamelab.Tests.*` (`Unity/CONVENTIONS.md`)
- Verify via headless Unity batchmode; never combine `-quit` with `-runTests` (`Unity/CONVENTIONS.md`)
- `git add Unity/Assets` broadly rather than narrow per-folder paths, to avoid missing parent `.meta` files (`Unity/CONVENTIONS.md`)

## Known blocker (read before Task 1)

> Resolved during Task 6: the repo-root FMOD natives from the MonoGame build were reused and work in the Editor. See "FMOD native libraries" in Unity/CONVENTIONS.md. The text below is the original assumption.

The public `fmod/fmod-for-unity` GitHub source (what Task 1 vendors) explicitly
ships **without** the native FMOD engine binaries (`fmod`/`fmodstudio`
core libraries per platform). Those are only distributed through an
authenticated download (an FMOD account at fmod.com/download, or a Unity
ID via the Asset Store) — this sandbox has network access but no such
account credentials, and creating/logging into one is a real-world action
outside what an agent should do unprompted (same category as Phase 0
Task 1's "needs a human at a GUI once"). Confirmed via `gh api
repos/fmod/fmod-for-unity` (MIT, public, no binaries) and OpenUPM (no FMOD
package there either) — this is a real gap, not a missed lookup.

Consequence: everything that only needs the FMOD **managed C# API surface**
(which ships in full as source — `fmod.cs`, `fmod_studio.cs`, `RuntimeManager`,
`Settings`, etc.) compiles and works today. Anything that needs the native
engine to actually *run* (creating a real `FMOD.Studio.System`, loading a
bank, starting an event) will throw `DllNotFoundException` at the P/Invoke
call site until a human supplies the native libraries. Task 6's two
native-dependent PlayMode tests are written correctly and will pass once
those libraries are added to `Unity/Assets/Plugins/FMOD/` — but cannot be
proven green inside this sandbox. Every other task's tests are unaffected
and must pass for real.

---

### Task 1: Vendor the FMOD Unity integration and point it at the existing soundbanks

**Files:**
- Create: `Unity/Assets/Plugins/FMOD/` (vendored from `fmod/fmod-for-unity`, branch `2.02`, minus `addons/`)
- Create: `Unity/Assets/Editor/FmodSoundbankSetup.cs`
- Modify: `Unity/Packages/manifest.json`

**Interfaces:**
- Produces: the `FMODUnity` assembly (types `FMODUnity.RuntimeManager`, `FMODUnity.Settings`, `FMODUnity.ImportType`, `FMODUnity.BankLoadType`) and `FMOD.Studio` (`FMOD.Studio.System`, `EventInstance`, `EventDescription`, `Bus`, `RESULT`, `PLAYBACK_STATE`) that every later task in this plan depends on.

- [ ] **Step 1: Download the integration source**

```bash
cd /tmp
rm -rf fmod-vendor && mkdir fmod-vendor && cd fmod-vendor
curl -sSL -o fmod-for-unity.tar.gz https://github.com/fmod/fmod-for-unity/archive/refs/heads/2.02.tar.gz
tar xzf fmod-for-unity.tar.gz
ls fmod-for-unity-2.02/Assets/Plugins/FMOD
```

Expected: a directory containing `FMODUnity.asmdef`, `src/`, `images/`, `platforms/`, `LICENSE.TXT`, `README.txt` (no `lib/` folder with compiled binaries — that absence is expected, see "Known blocker" above).

- [ ] **Step 2: Copy it into the Unity project, excluding the unused Resonance Audio addon**

```bash
WORKTREE="/Users/alexanderschlieper/Documents/ETH/master/fs2026/Game Programming Lab/Gamelab2026-TeamSnow/.worktrees/port-audio"
rm -rf /tmp/fmod-vendor/fmod-for-unity-2.02/Assets/Plugins/FMOD/addons
mkdir -p "$WORKTREE/Unity/Assets/Plugins"
cp -R /tmp/fmod-vendor/fmod-for-unity-2.02/Assets/Plugins/FMOD "$WORKTREE/Unity/Assets/Plugins/FMOD"
ls "$WORKTREE/Unity/Assets/Plugins/FMOD"
```

Team Snow doesn't use FMOD's Resonance Audio ambisonic addon (nothing in
`FmodProject/` references it) — dropping it keeps the vendored tree smaller.
If a later agent needs it, it can be re-added from the same source tag.

- [ ] **Step 3: Add the Newtonsoft.Json package** (needed by Task 3's `SoundSettings`, matching the original's `Newtonsoft.Json` usage)

Open `Unity/Packages/manifest.json`, add this entry to `"dependencies"` (alphabetical among the `com.unity.*` entries):

```json
    "com.unity.nuget.newtonsoft-json": "3.2.1",
```

- [ ] **Step 4: Compile-only verification**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "$WORKTREE/Unity" \
  -logFile /tmp/fmod-vendor-compile.log
echo "exit: $?"
grep -i "error CS" /tmp/fmod-vendor-compile.log
tail -5 /tmp/fmod-vendor-compile.log
```

Expected: exit code 0, log ends with `Exiting batchmode successfully now!`,
and the `grep` finds nothing. If it finds C# 10+/.NET 6+ syntax errors
(file-scoped namespaces etc. — see `Unity/CONVENTIONS.md`'s language
version section), that means the vendored FMOD source itself hits our
project's C# 9 pin; if so, stop and report it rather than patching FMOD's
own vendored files — that would be a real, reportable blocker, not
something to route around silently.

- [ ] **Step 5: Point FMOD's Settings at the existing soundbanks**

```csharp
// Unity/Assets/Editor/FmodSoundbankSetup.cs
using FMODUnity;
using UnityEditor;
using UnityEngine;

namespace Gamelab.Editor.Sound
{
    // One-off configuration: points the FMOD Unity integration at the
    // existing FMOD Studio soundbanks under Src/Content/soundbanks instead
    // of rebuilding them inside the Unity project (the FmodProject/ and
    // .bank files carry over unchanged per the port spec's "Target"
    // section and this work order).
    public static class FmodSoundbankSetup
    {
        [MenuItem("Tools/FMOD/Configure Soundbank Path")]
        public static void ConfigureSoundbankPath()
        {
            Settings settings = Settings.Instance;
            settings.SourceBankPath = "../Src/Content/soundbanks";
            settings.ImportType = ImportType.StreamingAssets;

            if (settings.DefaultPlatform != null)
            {
                settings.DefaultPlatform.BankLoadType = BankLoadType.All;
            }

            if (settings.PlayInEditorPlatform != null)
            {
                settings.PlayInEditorPlatform.BankLoadType = BankLoadType.All;
            }

            EditorUtility.SetDirty(settings);
            AssetDatabase.SaveAssets();
            Debug.Log($"FMOD SourceBankPath set to {settings.SourceBankPath}, ImportType={settings.ImportType}");
        }
    }
}
```

Before relying on `DefaultPlatform`/`PlayInEditorPlatform`/`BankLoadType` exactly as
spelled above, confirm the field names against the vendored source you just
copied (it's the ground truth, not this plan):

```bash
grep -n "DefaultPlatform\|PlayInEditorPlatform\|BankLoadType" \
  "$WORKTREE/Unity/Assets/Plugins/FMOD/src/Settings.cs"
```

Adjust the script to match if the vendored `2.02` source differs from what's
written above.

- [ ] **Step 6: Run it and verify the settings asset was written**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "$WORKTREE/Unity" \
  -executeMethod Gamelab.Editor.Sound.FmodSoundbankSetup.ConfigureSoundbankPath \
  -logFile /tmp/fmod-setup.log
grep "SourceBankPath set to" /tmp/fmod-setup.log
ls "$WORKTREE/Unity/Assets/Plugins/FMOD/Resources/FMODStudioSettings.asset"
```

Expected: the log line confirming the path, and the settings asset file
exists on disk.

- [ ] **Step 7: Commit**

```bash
cd "$WORKTREE"
git add Unity/Assets Unity/Packages/manifest.json
git commit -m "Vendor official FMOD Unity integration, point it at Src/Content/soundbanks"
```

---

### Task 2: Port `Sounds.cs` to `Gamelab.Core`

**Files:**
- Create: `Unity/Assets/Scripts/Core/Services/Sound/Sounds.cs`
- Test: `Unity/Assets/Tests/EditMode/Services/Sound/SoundsTests.cs`

**Interfaces:**
- Consumes: nothing (pure data).
- Produces: `Gamelab.Services.Sound.Sounds` — `const string` event path constants and `static readonly List<KeyValuePair<string, float>> DefaultGlobalParameterValues`. Task 5/6 (`Gamelab.Runtime`) reference these constants.

`Sounds.cs` has zero `UnityEngine`/FMOD dependency in the original (`Src/Services/Sound/Sounds.cs`) — it's pure string constants — so unlike the rest of the sound service it belongs in `Gamelab.Core`, not `Gamelab.Runtime`.

- [ ] **Step 1: Write the failing test**

```csharp
// Unity/Assets/Tests/EditMode/Services/Sound/SoundsTests.cs
using NUnit.Framework;
using Gamelab.Services.Sound;

namespace Gamelab.Tests.Services
{
    public class SoundsTests
    {
        [Test]
        public void AllEventPaths_StartWithEventPrefix()
        {
            string[] paths =
            {
                Sounds.MenuSelect, Sounds.Stamp, Sounds.Train, Sounds.ShovelUp, Sounds.ShovelDown,
                Sounds.CannonLoad, Sounds.CannonFire, Sounds.Craft, Sounds.WallHit, Sounds.WallBreak,
                Sounds.WallFix, Sounds.WallFixed, Sounds.Walk, Sounds.PickupItem, Sounds.DropItem,
                Sounds.OpenDoor, Sounds.CloseDoor, Sounds.SpeedChange, Sounds.Purchase, Sounds.GrabStation,
                Sounds.DropStation, Sounds.Freeze, Sounds.Fall, Sounds.HorseRiding, Sounds.HorseFlee,
                Sounds.EnemyHit, Sounds.EnemyFire, Sounds.GameOver, Sounds.AmbientSong, Sounds.BattleTheme
            };

            foreach (var path in paths)
            {
                StringAssert.StartsWith("event:/", path);
            }
        }

        [Test]
        public void DefaultGlobalParameterValues_ContainsTemperature()
        {
            Assert.AreEqual(1, Sounds.DefaultGlobalParameterValues.Count);
            Assert.AreEqual("Temperature", Sounds.DefaultGlobalParameterValues[0].Key);
            Assert.AreEqual(1f, Sounds.DefaultGlobalParameterValues[0].Value);
        }
    }
}
```

- [ ] **Step 2: Run it, confirm it fails to compile** (`Sounds` doesn't exist yet)

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$WORKTREE/Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/sounds-test-red.xml -logFile /tmp/sounds-test-red.log
grep "error CS" /tmp/sounds-test-red.log
```

Expected: compile errors referencing `Sounds`/`SoundsTests`.

- [ ] **Step 3: Port the implementation**

```csharp
// Unity/Assets/Scripts/Core/Services/Sound/Sounds.cs
using System.Collections.Generic;

namespace Gamelab.Services.Sound
{
    public static class Sounds
    {
        // SFX
        public const string MenuSelect = "event:/UI/Menu Select";
        public const string Stamp = "event:/UI/Stamp";
        public const string Train = "event:/Gameplay/Train";
        public const string ShovelUp = "event:/Gameplay/Shovel Up";
        public const string ShovelDown = "event:/Gameplay/Shovel Down";
        public const string CannonLoad = "event:/Gameplay/Cannon Load";
        public const string CannonFire = "event:/Gameplay/Cannon Fire";
        public const string Craft = "event:/Gameplay/Craft";
        public const string WallHit = "event:/Gameplay/Wall Hit";
        public const string WallBreak = "event:/Gameplay/Wall Break";
        public const string WallFix = "event:/Gameplay/Wall Fix";
        public const string WallFixed = "event:/Gameplay/Wall Fixed";
        public const string Walk = "event:/Gameplay/Walk";
        public const string PickupItem = "event:/Gameplay/Pickup Item";
        public const string DropItem = "event:/Gameplay/Drop Item";
        public const string OpenDoor = "event:/Gameplay/Open Door";
        public const string CloseDoor = "event:/Gameplay/Close Door";
        public const string SpeedChange = "event:/Gameplay/Speed Change";
        public const string Purchase = "event:/Gameplay/Purchase";
        public const string GrabStation = "event:/Gameplay/Grab Station";
        public const string DropStation = "event:/Gameplay/Drop Station";
        public const string Freeze = "event:/Gameplay/Freeze";
        public const string Fall = "event:/Gameplay/Fall";
        public const string HorseRiding = "event:/Gameplay/Enemy/Horse Riding";
        public const string HorseFlee = "event:/Gameplay/Enemy/Horse Flee";
        public const string EnemyHit = "event:/Gameplay/Enemy/Enemy Hit";
        public const string EnemyFire = "event:/Gameplay/Enemy/Enemy Fire";
        public const string GameOver = "event:/Gameplay/Game Over";

        // Music
        public const string AmbientSong = "event:/Music/Ambient";
        public const string BattleTheme = "event:/Music/Battle Theme";

        // Default Global Parameter Values
        public static readonly List<KeyValuePair<string, float>> DefaultGlobalParameterValues =
            new List<KeyValuePair<string, float>>
            {
                new KeyValuePair<string, float>("Temperature", 1f)
            };
    }
}
```

(Braced namespace, not `namespace Foo;` — file-scoped namespaces are C# 10+
and this project is pinned to C# 9, per `Unity/CONVENTIONS.md`. Same for
`new List<...> { ... }` instead of a `[...]` collection expression, which is
C# 12.)

- [ ] **Step 4: Run it, confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$WORKTREE/Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/sounds-test-green.xml -logFile /tmp/sounds-test-green.log
grep -A2 'test-case.*SoundsTests' /tmp/sounds-test-green.xml
```

Expected: both `SoundsTests` test-cases with `result="Passed"`.

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets/Scripts/Core/Services/Sound Unity/Assets/Tests/EditMode/Services/Sound
git commit -m "Port Sounds constants to Gamelab.Core"
```

---

### Task 3: Port `SoundSettings.cs` to `Gamelab.Core`

**Files:**
- Create: `Unity/Assets/Scripts/Core/Services/Sound/SoundSettings.cs`
- Test: `Unity/Assets/Tests/EditMode/Services/Sound/SoundSettingsTests.cs`

**Interfaces:**
- Consumes: nothing beyond `System.IO`/`Newtonsoft.Json` (Task 1 added the Newtonsoft package).
- Produces: `Gamelab.Services.Sound.SoundSettings` with `float MasterVolume/MusicVolume/SfxVolume { get; set; }`, `static SoundSettings Load()`, `void Save()`, and a test-only seam `public static string SettingsFilePathOverride`. Task 6's `SoundService` constructs this via `SoundSettings.Load()`.

The original (`Src/Services/Sound/SoundSettings.cs`) depends on
`Gamelab.Utils.Logger`, which wraps MonoGame's `GamelabGame.Instance` and is
out of this subsystem's scope (not under `Src/Services/Sound/`, and not
something A3 owns). This port drops that dependency and logs failures with
`Console.WriteLine` instead — a deliberate, scoped simplification, not an
oversight.

- [ ] **Step 1: Write the failing tests**

```csharp
// Unity/Assets/Tests/EditMode/Services/Sound/SoundSettingsTests.cs
using System;
using System.IO;
using NUnit.Framework;
using Gamelab.Services.Sound;

namespace Gamelab.Tests.Services
{
    public class SoundSettingsTests
    {
        private string tempFile;

        [SetUp]
        public void SetUp()
        {
            tempFile = Path.Combine(Path.GetTempPath(), $"gamelab_audio_settings_{Guid.NewGuid():N}.json");
            SoundSettings.SettingsFilePathOverride = tempFile;
        }

        [TearDown]
        public void TearDown()
        {
            SoundSettings.SettingsFilePathOverride = null;
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }

        [Test]
        public void Load_WithNoFile_ReturnsDefaults()
        {
            var settings = SoundSettings.Load();

            Assert.AreEqual(0.8f, settings.MasterVolume, 0.0001f);
            Assert.AreEqual(0.6f, settings.MusicVolume, 0.0001f);
            Assert.AreEqual(0.8f, settings.SfxVolume, 0.0001f);
        }

        [Test]
        public void Load_WithOutOfRangeValues_ClampsTo0To1()
        {
            File.WriteAllText(tempFile, "{\"MasterVolume\": 5.0, \"MusicVolume\": -3.0, \"SfxVolume\": 0.5}");

            var settings = SoundSettings.Load();

            Assert.AreEqual(1f, settings.MasterVolume, 0.0001f);
            Assert.AreEqual(0f, settings.MusicVolume, 0.0001f);
            Assert.AreEqual(0.5f, settings.SfxVolume, 0.0001f);
        }

        [Test]
        public void Save_ThenLoad_RoundTripsValues()
        {
            var settings = new SoundSettings { MasterVolume = 0.3f, MusicVolume = 0.4f, SfxVolume = 0.9f };

            settings.Save();
            var loaded = SoundSettings.Load();

            Assert.AreEqual(0.3f, loaded.MasterVolume, 0.0001f);
            Assert.AreEqual(0.4f, loaded.MusicVolume, 0.0001f);
            Assert.AreEqual(0.9f, loaded.SfxVolume, 0.0001f);
        }
    }
}
```

- [ ] **Step 2: Run it, confirm it fails to compile**

Same command pattern as Task 2 Step 2, targeting this test. Expected: `error CS` referencing `SoundSettings`.

- [ ] **Step 3: Port the implementation**

```csharp
// Unity/Assets/Scripts/Core/Services/Sound/SoundSettings.cs
using System;
using System.IO;
using Newtonsoft.Json;

namespace Gamelab.Services.Sound
{
    public class SoundSettings
    {
        private static readonly string SettingsDirectory =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Gamelab");

        // Test-only seam: when set, Load()/Save() use this file path instead
        // of the real per-user AppData settings file. Tests must reset this
        // to null in TearDown.
        public static string SettingsFilePathOverride;

        private static string SettingsFile =>
            SettingsFilePathOverride ?? Path.Combine(SettingsDirectory, "audio_settings.json");

        public float MasterVolume { get; set; } = 0.8f;
        public float MusicVolume { get; set; } = 0.6f;
        public float SfxVolume { get; set; } = 0.8f;

        public static SoundSettings Load()
        {
            if (!File.Exists(SettingsFile)) return new SoundSettings();

            try
            {
                string json = File.ReadAllText(SettingsFile);
                var loaded = JsonConvert.DeserializeObject<SoundSettings>(json) ?? new SoundSettings();
                loaded.ClampAll();
                return loaded;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SoundSettings] Failed to load audio settings, using defaults: {ex.Message}");
                return new SoundSettings();
            }
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFile);
                if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsFile, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[SoundSettings] Failed to save audio settings: {ex.Message}");
            }
        }

        private void ClampAll()
        {
            MasterVolume = Math.Clamp(MasterVolume, 0f, 1f);
            MusicVolume = Math.Clamp(MusicVolume, 0f, 1f);
            SfxVolume = Math.Clamp(SfxVolume, 0f, 1f);
        }
    }
}
```

- [ ] **Step 4: Run it, confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$WORKTREE/Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/soundsettings-test-green.xml -logFile /tmp/soundsettings-test-green.log
grep -A2 'test-case.*SoundSettingsTests' /tmp/soundsettings-test-green.xml
```

Expected: all three `SoundSettingsTests` test-cases with `result="Passed"`.

- [ ] **Step 5: Commit**

```bash
git add Unity/Assets/Scripts/Core/Services/Sound Unity/Assets/Tests/EditMode/Services/Sound
git commit -m "Port SoundSettings to Gamelab.Core"
```

---

### Task 4: Add the `Gamelab.Tests.PlayMode` assembly

**Files:**
- Create: `Unity/Assets/Tests/PlayMode/Gamelab.Tests.PlayMode.asmdef`

**Interfaces:**
- Produces: a `Gamelab.Tests.PlayMode` assembly referencing `Gamelab.Core`, `Gamelab.Runtime`, and `FMODUnity`. Task 5 and Task 6 add test files under `Unity/Assets/Tests/PlayMode/`.

This is the first Wave A subsystem needing a PlayMode test, per
`Unity/CONVENTIONS.md`'s instruction to add this asmdef "the first time a
subsystem needs a PlayMode test" — mirroring `Gamelab.Tests.EditMode`'s
shape but without the `"includePlatforms": ["Editor"]` restriction.

- [ ] **Step 1: Create the asmdef**

```json
{
    "name": "Gamelab.Tests.PlayMode",
    "rootNamespace": "",
    "references": [
        "Gamelab.Core",
        "Gamelab.Runtime",
        "FMODUnity",
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": true,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": []
}
```

- [ ] **Step 2: Compile-only verification** (no test files exist yet, so this just confirms the empty asmdef is valid)

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -quit -projectPath "$WORKTREE/Unity" \
  -logFile /tmp/playmode-asmdef-compile.log
grep -i "error CS" /tmp/playmode-asmdef-compile.log
tail -5 /tmp/playmode-asmdef-compile.log
```

Expected: no `error CS` lines, log ends with `Exiting batchmode successfully now!`.

- [ ] **Step 3: Commit**

```bash
git add Unity/Assets/Tests/PlayMode
git commit -m "Add Gamelab.Tests.PlayMode assembly for the audio subsystem"
```

---

### Task 5: Port `ISoundService` and `ParameterBinding` to `Gamelab.Runtime`

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/Services/Sound/ISoundService.cs`
- Create: `Unity/Assets/Scripts/Runtime/Services/Sound/ParameterBinding.cs`
- Modify: `Unity/Assets/Scripts/Runtime/Gamelab.Runtime.asmdef` (add `"FMODUnity"` to `references`)
- Test: `Unity/Assets/Tests/PlayMode/Services/Sound/ParameterBindingTests.cs`

**Interfaces:**
- Consumes: `Gamelab.Services.Sound.SoundSettings` (Task 3), `FMOD.Studio.EventInstance`, `FMODUnity.RuntimeManager.StudioSystem` (Task 1).
- Produces: `Gamelab.Services.Sound.ISoundService` (full interface, unchanged shape from the original) and `Gamelab.Services.Sound.ParameterBinding` with `bool active`, `void Update()`, `static ParameterBinding Local(EventInstance, string, Func<float>)`, `static ParameterBinding Global(string, Func<float>)`, `void Set(float)`, `float Get(float)`, `void Deactivate()`. Task 6's `SoundService` implements `ISoundService` and constructs `ParameterBinding`s through these factories.

These two types are the ones the work order calls out as "conceptually
carries over" even though the call-site API against FMOD's types differs
from `FmodForFoxes`. `ParameterBinding`'s generic constructor and
`Update()`/`Deactivate()` logic never touch an FMOD native call, so they're
testable in this sandbox even without the native FMOD binaries (see "Known
blocker"); only the `Local`/`Global` factories touch `FMODUnity` types, and
this task's test exercises the plain constructor instead of those factories.

- [ ] **Step 1: Write the failing test**

```csharp
// Unity/Assets/Tests/PlayMode/Services/Sound/ParameterBindingTests.cs
using NUnit.Framework;
using Gamelab.Services.Sound;

namespace Gamelab.Tests.Services
{
    public class ParameterBindingTests
    {
        [Test]
        public void Update_CallsSetterWithCurrentGetterValue()
        {
            float current = 1f;
            float captured = -1f;
            var binding = new ParameterBinding(v => captured = v, () => current);

            binding.Update();
            Assert.AreEqual(1f, captured, 0.0001f);

            current = 2.5f;
            binding.Update();
            Assert.AreEqual(2.5f, captured, 0.0001f);
        }

        [Test]
        public void Deactivate_SetsActiveFalse()
        {
            var binding = new ParameterBinding(v => { }, () => 0f);

            Assert.IsTrue(binding.active);
            binding.Deactivate();
            Assert.IsFalse(binding.active);
        }
    }
}
```

- [ ] **Step 2: Run it, confirm it fails to compile** (neither `ISoundService` nor `ParameterBinding` exist yet, and `Gamelab.Tests.PlayMode` doesn't yet reference anything that defines them)

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$WORKTREE/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/parambinding-test-red.xml -logFile /tmp/parambinding-test-red.log
grep "error CS" /tmp/parambinding-test-red.log
```

Expected: `error CS0246` (type or namespace `ParameterBinding` not found).

- [ ] **Step 3: Add `FMODUnity` to `Gamelab.Runtime`'s references**

In `Unity/Assets/Scripts/Runtime/Gamelab.Runtime.asmdef`, change:

```json
    "references": [
        "Gamelab.Core"
    ],
```

to:

```json
    "references": [
        "Gamelab.Core",
        "FMODUnity"
    ],
```

- [ ] **Step 4: Port `ISoundService`**

```csharp
// Unity/Assets/Scripts/Runtime/Services/Sound/ISoundService.cs
using System;
using FMOD.Studio;

namespace Gamelab.Services.Sound
{
    public interface ISoundService
    {
        void LoadSound(string id);
        void UnloadSound(string id);
        void PlayOnce(string id);
        EventInstance GetSoundInstance(string id);
        ParameterBinding RegisterParameter(EventInstance eventInstance, string parameterName, Func<float> valueGetter);
        ParameterBinding RegisterGlobalParameter(string parameterName, Func<float> valueGetter);
        void SetGlobalParameter(string parameterName, float value);
        void ResetGlobalParameters();
        SoundSettings Settings { get; }
        void SetMasterVolume(float volume);
        void SetMusicVolume(float volume);
        void SetSfxVolume(float volume);
    }
}
```

- [ ] **Step 5: Port `ParameterBinding`**

```csharp
// Unity/Assets/Scripts/Runtime/Services/Sound/ParameterBinding.cs
using System;
using FMOD.Studio;
using FMODUnity;

namespace Gamelab.Services.Sound
{
    public class ParameterBinding
    {
        private readonly Action<float> setter;
        private readonly Func<float> valueGetter;

        public bool active = true;

        public ParameterBinding(Action<float> setter, Func<float> valueGetter)
        {
            this.setter = setter;
            this.valueGetter = valueGetter;
        }

        public void Update() => setter(valueGetter());

        public static ParameterBinding Local(EventInstance eventInstance, string parameterName, Func<float> valueGetter)
            => new ParameterBinding(val => eventInstance.setParameterByName(parameterName, val), valueGetter);

        public static ParameterBinding Global(string parameterName, Func<float> valueGetter)
            => new ParameterBinding(val => RuntimeManager.StudioSystem.setParameterByName(parameterName, val), valueGetter);

        public void Set(float value) => setter(value);

        public float Get(float value) => valueGetter();

        public void Deactivate() => active = false;
    }
}
```

(`Get(float value)` keeps the original's unused parameter verbatim — it's
dead in the source we're porting from too, and changing the signature isn't
this task's job.)

- [ ] **Step 6: Run it, confirm it passes**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$WORKTREE/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/parambinding-test-green.xml -logFile /tmp/parambinding-test-green.log
grep -A2 'test-case.*ParameterBindingTests' /tmp/parambinding-test-green.xml
```

Expected: both `ParameterBindingTests` test-cases with `result="Passed"`.
This confirms `Gamelab.Tests.PlayMode` itself runs correctly in this
sandbox (i.e. the PlayMode test pipeline works, independent of the native
FMOD blocker).

- [ ] **Step 7: Commit**

```bash
git add Unity/Assets/Scripts/Runtime Unity/Assets/Tests/PlayMode
git commit -m "Port ISoundService and ParameterBinding to Gamelab.Runtime"
```

---

### Task 6: Port `SoundService`, add `SoundServiceRunner`, write the native-dependent PlayMode tests

**Files:**
- Create: `Unity/Assets/Scripts/Runtime/Services/Sound/SoundService.cs`
- Create: `Unity/Assets/Scripts/Runtime/Services/Sound/SoundServiceRunner.cs`
- Test: `Unity/Assets/Tests/PlayMode/Services/Sound/SoundServicePlayModeTests.cs`

**Interfaces:**
- Consumes: `Gamelab.Services.Sound.ISoundService`/`ParameterBinding` (Task 5), `Gamelab.Services.Sound.Sounds`/`SoundSettings` (Tasks 2-3), `FMODUnity.RuntimeManager`, `FMOD.Studio.*` (Task 1).
- Produces: `Gamelab.Services.Sound.SoundService : ISoundService` (plain C# class, not a `MonoBehaviour`) with an added `public void Tick()` that advances registered parameter bindings once per frame, and `Gamelab.Services.Sound.SoundServiceRunner : MonoBehaviour` that owns one `SoundService` instance and calls `Tick()` from `Update()`. Later waves (e.g. gameplay code needing to play a sound) get the service via `SoundServiceRunner.Instance.SoundService`.

The original `SoundService` implemented `IGameSystem`, whose
`Initialize`/`Update(GameTime)`/`Shutdown` hooks came from a MonoGame-specific
game-object registry this port doesn't have. `SoundServiceRunner` is the
Unity-idiomatic replacement for exactly that lifecycle hook — a single
scene-persistent `MonoBehaviour`, not a new abstraction layer. It also
replaces the original's explicit `StudioSystem.LoadBank(...)` calls in
`Initialize()`: the official Unity integration loads every bank found at
`Settings.SourceBankPath` automatically (per `BankLoadType.All`, set in
Task 1) the first time any FMOD API is touched, so `SoundService` doesn't
need to load banks itself — `LoadSound`/`UnloadSound` here are about caching
`EventDescription`/instance objects (same as the original), not literal
FMOD bank loading.

This task's PlayMode test is the one blocked by the "Known blocker" above —
write it correctly, run it, and record whatever result actually comes back
(don't paper over a `DllNotFoundException` as a pass).

- [ ] **Step 1: Write the tests**

```csharp
// Unity/Assets/Tests/PlayMode/Services/Sound/SoundServicePlayModeTests.cs
using NUnit.Framework;
using FMOD.Studio;
using FMODUnity;
using Gamelab.Services.Sound;

namespace Gamelab.Tests.Services
{
    // These tests exercise the real FMOD Studio runtime (RuntimeManager, the
    // vendored banks under Src/Content/soundbanks). They need the native FMOD
    // engine libraries for the current platform, which are not included in
    // the vendored Assets/Plugins/FMOD source (see this plan's "Known
    // blocker" section) - they must be added from an authenticated
    // FMOD/Unity Asset Store download before these tests can pass.
    // Verification uses FMOD's own RESULT codes and event/parameter state,
    // never audio playback - there's no way to listen for this in this
    // environment, and that's the point (per the work order).
    public class SoundServicePlayModeTests
    {
        private SoundService soundService;

        [SetUp]
        public void SetUp()
        {
            soundService = new SoundService();
        }

        [Test]
        public void PlayOnce_MenuSelectEvent_StartsSuccessfully()
        {
            EventInstance instance = soundService.GetSoundInstance(Sounds.MenuSelect);

            RESULT result = instance.start();

            Assert.AreEqual(RESULT.OK, result);

            instance.getPlaybackState(out PLAYBACK_STATE state);
            Assert.AreNotEqual(PLAYBACK_STATE.STOPPED, state);
        }

        [Test]
        public void RegisterGlobalParameter_UpdatesTemperatureOnTick()
        {
            float value = 0.25f;
            soundService.RegisterGlobalParameter("Temperature", () => value);

            soundService.Tick();

            RuntimeManager.StudioSystem.getParameterByName("Temperature", out float actual);
            Assert.AreEqual(0.25f, actual, 0.01f);
        }
    }
}
```

- [ ] **Step 2: Run it, confirm it fails to compile** (`SoundService` doesn't exist yet)

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$WORKTREE/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/soundservice-test-red.xml -logFile /tmp/soundservice-test-red.log
grep "error CS" /tmp/soundservice-test-red.log
```

Expected: `error CS0246` referencing `SoundService`.

- [ ] **Step 3: Port `SoundService`**

```csharp
// Unity/Assets/Scripts/Runtime/Services/Sound/SoundService.cs
using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;

namespace Gamelab.Services.Sound
{
    public class SoundService : ISoundService
    {
        private readonly List<ParameterBinding> parameterUpdates = new List<ParameterBinding>();
        private readonly Dictionary<string, EventDescription> eventDescriptions = new Dictionary<string, EventDescription>();
        private readonly Dictionary<string, EventInstance> playOnceInstances = new Dictionary<string, EventInstance>();

        public SoundSettings Settings { get; }

        public SoundService()
        {
            Settings = SoundSettings.Load();
            SetMasterVolume(Settings.MasterVolume);
            SetMusicVolume(Settings.MusicVolume);
            SetSfxVolume(Settings.SfxVolume);
            ResetGlobalParameters();
        }

        public void LoadSound(string id)
        {
            if (eventDescriptions.ContainsKey(id)) return;

            EventDescription eventDescription = RuntimeManager.GetEventDescription(id);
            eventDescriptions.Add(id, eventDescription);
            eventDescription.loadSampleData();
            eventDescription.createInstance(out EventInstance instance);
            playOnceInstances.Add(id, instance);
        }

        public void UnloadSound(string id)
        {
            if (!eventDescriptions.ContainsKey(id)) return;

            eventDescriptions[id].releaseAllInstances();
            eventDescriptions[id].unloadSampleData();
            eventDescriptions.Remove(id);
            playOnceInstances[id].release();
            playOnceInstances.Remove(id);
        }

        public void PlayOnce(string id)
        {
            LoadSound(id);
            EventInstance sound = playOnceInstances[id];
            sound.stop(STOP_MODE.IMMEDIATE);
            sound.start();
        }

        public EventInstance GetSoundInstance(string id)
        {
            LoadSound(id);
            eventDescriptions[id].createInstance(out EventInstance instance);
            return instance;
        }

        public ParameterBinding RegisterParameter(EventInstance eventInstance, string parameterName, Func<float> valueGetter)
        {
            ParameterBinding binding = ParameterBinding.Local(eventInstance, parameterName, valueGetter);
            parameterUpdates.Add(binding);
            return binding;
        }

        public ParameterBinding RegisterGlobalParameter(string parameterName, Func<float> valueGetter)
        {
            ParameterBinding binding = ParameterBinding.Global(parameterName, valueGetter);
            parameterUpdates.Add(binding);
            return binding;
        }

        public void SetGlobalParameter(string parameterName, float value)
        {
            RuntimeManager.StudioSystem.setParameterByName(parameterName, value);
        }

        public void SetMasterVolume(float volume)
        {
            Settings.MasterVolume = Math.Clamp(volume, 0f, 1f);
            Settings.Save();
            RuntimeManager.GetBus("bus:/").setVolume(Settings.MasterVolume);
        }

        public void SetMusicVolume(float volume)
        {
            Settings.MusicVolume = Math.Clamp(volume, 0f, 1f);
            Settings.Save();
            RuntimeManager.GetBus("bus:/Music").setVolume(Settings.MusicVolume);
        }

        public void SetSfxVolume(float volume)
        {
            Settings.SfxVolume = Math.Clamp(volume, 0f, 1f);
            Settings.Save();
            RuntimeManager.GetBus("bus:/Sounds").setVolume(Settings.SfxVolume);
        }

        public void ResetGlobalParameters()
        {
            foreach (KeyValuePair<string, float> pair in Sounds.DefaultGlobalParameterValues)
            {
                SetGlobalParameter(pair.Key, pair.Value);
            }
        }

        // Advances every registered parameter binding. Call once per frame
        // (see SoundServiceRunner) - this replaces the MonoGame
        // SoundService's IGameSystem.Update(GameTime) hook, which has no
        // Unity equivalent.
        public void Tick()
        {
            parameterUpdates.RemoveAll(binding =>
            {
                if (!binding.active) return true;
                try
                {
                    binding.Update();
                    return false;
                }
                catch (Exception)
                {
                    binding.Deactivate();
                    return true;
                }
            });
        }
    }
}
```

- [ ] **Step 4: Add `SoundServiceRunner`**

```csharp
// Unity/Assets/Scripts/Runtime/Services/Sound/SoundServiceRunner.cs
using UnityEngine;

namespace Gamelab.Services.Sound
{
    // Drives SoundService.Tick() once per frame and gives the rest of the
    // game a single place to get the sound service from. The MonoGame
    // original hooked this through IGameSystem.Update(GameTime); Unity has
    // no equivalent global system registry, so a single scene-persistent
    // MonoBehaviour does the same job.
    public class SoundServiceRunner : MonoBehaviour
    {
        public static SoundServiceRunner Instance { get; private set; }
        public SoundService SoundService { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            SoundService = new SoundService();
        }

        private void Update()
        {
            SoundService.Tick();
        }
    }
}
```

- [ ] **Step 5: Run the tests and record the actual result**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$WORKTREE/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/soundservice-test-result.xml -logFile /tmp/soundservice-test-result.log
grep -A3 'test-case.*SoundServicePlayModeTests' /tmp/soundservice-test-result.xml
```

Two outcomes are both acceptable evidence, and the task report must state
which one actually happened (never assume):
- If `DllNotFoundException`/similar shows up in the result XML or log: this
  confirms the "Known blocker" precisely, at the exact call site. Record
  the failure text — that's the evidence for the coordinator's report, not
  a task failure to silently retry.
- If it passes: the native libraries were available after all (e.g. a human
  added them between Task 1 and now) — even better, record that.

Either way, also re-run `Gamelab.Tests.PlayMode` as a whole to confirm
Task 5's `ParameterBindingTests` are still green (they must be, regardless
of the native blocker, since they never call into FMOD's native layer).

- [ ] **Step 6: Commit**

```bash
git add Unity/Assets/Scripts/Runtime Unity/Assets/Tests/PlayMode
git commit -m "Port SoundService and add SoundServiceRunner to Gamelab.Runtime"
```

---

### Task 7: Whole-branch review and Wave A gate check

**Files:** none (verification only)

- [ ] **Step 1: Full EditMode + PlayMode run**

```bash
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$WORKTREE/Unity" -runTests -testPlatform EditMode \
  -testResults /tmp/final-editmode.xml -logFile /tmp/final-editmode.log
/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics -projectPath "$WORKTREE/Unity" -runTests -testPlatform PlayMode \
  -testResults /tmp/final-playmode.xml -logFile /tmp/final-playmode.log
grep -c 'result="Passed"' /tmp/final-editmode.xml /tmp/final-playmode.xml
grep 'result="Failed"' /tmp/final-editmode.xml /tmp/final-playmode.xml
```

Expected: every `SoundsTests`/`SoundSettingsTests`/`ParameterBindingTests`
case passes. `SoundServicePlayModeTests` cases reflect whatever Task 6 Step 5
actually found (native blocker or not) — this step's job is to confirm
nothing regressed since then, not to force a pass.

- [ ] **Step 2: Confirm the MonoGame reference build still runs unmodified**

```bash
cd "$WORKTREE"
git status Src/
dotnet run --project Src/Gamelab.csproj &
sleep 10
kill %1 2>/dev/null
```

Expected: `git status Src/` shows no changes (this subsystem never touched
`Src/`), and the game launches without error.

- [ ] **Step 3: Whole-branch code review**

Use `superpowers:requesting-code-review` against the full `port/audio`
branch diff (`git diff main...port/audio`). Focus areas: does `SoundService`
faithfully match the original's semantics (volume clamping, parameter
binding lifecycle, play-once instance reuse)? Does anything reference
`Src/` (it must not)? Are Core/Runtime assembly boundaries respected per
`Unity/CONVENTIONS.md`? Is the native-binary blocker documented clearly
enough that the coordinator doesn't have to rediscover it?

- [ ] **Step 4: Resolve or park findings**

Fix anything concrete. For anything that depends on the native FMOD
binaries being supplied by a human, park it explicitly (don't invent a
workaround) and carry it into the final report.

---

## Self-review notes

- **Spec coverage:** `ISoundService`/`SoundService`/`Sounds`/`ParameterBinding`/`SoundSettings` from `Src/Services/Sound/` are each ported (Tasks 2, 3, 5, 6). `DesktopAndMacNativeFmodLibrary.cs` is explicitly excluded with its reason stated in "Architecture" above, per the work order. `FmodProject/`/`.bank` files are reused unchanged via `Settings.SourceBankPath` (Task 1), never recreated. Wave A gate items: EditMode/PlayMode tests (Task 7 Step 1), "at least one sound event and one music/parameter binding plays correctly" (Task 6, with the native-binary caveat spelled out rather than hidden), MonoGame reference build still runs (Task 7 Step 2).
- **Placeholder scan:** every step has real code or a real command; no "TBD"/"add error handling" left unstated.
- **Type consistency:** `ISoundService`'s methods (Task 5) match `SoundService`'s implementation (Task 6) exactly; `ParameterBinding`'s constructor/factories (Task 5) match how `SoundService.RegisterParameter`/`RegisterGlobalParameter` call them (Task 6); `Sounds.DefaultGlobalParameterValues` (Task 2) matches the `KeyValuePair<string, float>` iteration in `SoundService.ResetGlobalParameters` (Task 6).
