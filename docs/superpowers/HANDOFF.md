# Unity port handoff

Written 2026-09-21 at the end of a long session, for whoever picks the port up next (a fresh Claude session or a person).

## What this is

Soot & Steel (course name Team Snow) is a 2D local co-op game built in MonoGame/.NET for ETH's Game Programming Lab. It is being ported to Unity 6.3 LTS (6000.3.24f1, URP 2D) so the team can iterate faster and ship to Steam. Online co-op is planned later and is not part of this port.

- Repo checkout: `/Users/alexanderschlieper/Documents/ETH/master/fs2026/Game Programming Lab/Gamelab2026-TeamSnow`
- Unity project: `Unity/` inside that repo. The MonoGame game in `Src/` is the read-only reference and must stay untouched and runnable (`dotnet run --project Src/Gamelab.csproj`).
- Remotes: `origin` is ETH's GitLab (leave it alone, its `.gitlab-ci.yml` is locked course infrastructure). `github` is `https://github.com/xSurus/SootAndSteel` (public), which is the long-term home.
- Branch `main` at `73dbfa4` is fully pushed to `github`.
- Unity binary on this Mac: `/Applications/Unity/Hub/Editor/6000.3.24f1/Unity.app/Contents/MacOS/Unity`. There is no Editor GUI access for agents, everything is headless.

## Read these first

- `docs/superpowers/specs/2026-09-18-unity-port-design.md`: the design and the risk list.
- `docs/superpowers/plans/2026-09-18-unity-port-plan.md`: the overall plan (Phase 0, Waves A, B, C).
- `Unity/CONVENTIONS.md`: the binding conventions plus the lists of what is ported, what is deferred, and the exact seam for each deferral. Treat it as the source of truth for architecture rules.
- One plan per finished round in `docs/superpowers/plans/` (physics, audio, input, domain catalog, domain follow-up, domain breadth, levels and map).

## Status

Done and merged to `main`:
- Phase 0 foundation (project, asmdefs, CI workflow, conventions).
- Wave A1 physics and movement.
- Wave A2 domain layer: interfaces, bullets (11 parts, per-bullet state, collisions, ticking), enemy catalog and ammo, stations, workbench, conveyors, rifle and tutorial enemy, cannon station logic, speed lever.
- Wave A3 audio: official FMOD Unity integration, native libraries committed, `SoundService`.
- Wave A4 input and local co-op: Input System, `PlayerJoinManager`, two-controller join test.
- Wave B1 levels and map: level generator with Src-derived golden tests, train map, hub, world scroller, seam implementations, tile art import.

Test baseline on `main`: EditMode 242/242, PlayMode 170/170. Any new work must keep these green.

Not started:
- Wave B2, the UI. About 23 Gum/Myra screens and overlays (`Src/Screens`, `Src/UI`, `Src/Components`, including the `*.Generated.cs` Gum codegen) rebuilt in Unity UI Toolkit. This is the highest-risk piece. Suggested slices: menus, pause and options first; then hub and shop; then HUD and tooltips. The UI must bind to the data shapes and seams described in `Unity/CONVENTIONS.md` and `JoinFlowController` (join screen state).
- Wave C. Integration, porting the remaining `Tests/` suite, the full `GameInstructions.md` parity checklist, a team playtest with real controllers, and the final tag.

## Things only a person can do

- Visual side-by-side of the map, hub, world scroller and the physics parity demo scene against the MonoGame build. Nobody has looked at any of it, all checks so far are automated.
- Play audio in the Editor with speakers. Windows and macOS player builds of the FMOD natives are untested.
- Add Unity license secrets (`UNITY_LICENSE`, `UNITY_EMAIL`, `UNITY_PASSWORD`) in the GitHub repo settings so `.github/workflows/unity-tests.yml` can run. Also check game-ci publishes an image for 6000.3.24f1.
- Copy the soundbanks from `Src/Content/soundbanks` into the Unity project before `Src/` is ever removed (the audio setup points at that folder today).
- Decide whether the repo should stay public.

## Known open items

- Mirrored camera: the map wave flips Y once in `Gamelab.Map.MapSpace` with a mirrored projection. That reverses winding (backface culling would hide meshes) and mouse-to-world conversion under it is unverified.
- Still deferred, each with a named seam in `Unity/CONVENTIONS.md`: cannon seat and player ejection, shop and tooltips (`BuyableStationWrapper`, `StationConfig`; the two `Compile Remove` lines in `Src/Gamelab.csproj` are stale no-ops, both files are live), sound and VFX wiring, animation, bullet colour, shop-wave selection, wall repair and door toggle behaviour.
- `IInteractable` uses default interface methods, so a station subclass that forgets to list `: IInteractable` silently does nothing.
- Nothing in a scene consumes `ISoundService` yet, `SoundServiceRunner` has to be added to a scene.
- Leftover worktrees `.worktrees/port-physics` and `.worktrees/port-domain3` hold only agent scratch files and can be removed. `.claude/` and `.vscode/settings.json` at the repo root are untracked and were there before this work, leave them.
- Repo-root `.gitignore` has a bare `*.dll` rule. `Unity/.gitignore` negates it for `Assets/Plugins/**`.

## How the work was run (repeat this)

- One subsystem at a time. Running four coordinator agents in parallel burned the usage limit repeatedly. One lead agent per round worked well.
- Per round: a lead agent writes a plan with the `superpowers:writing-plans` skill, then runs `superpowers:subagent-driven-development` in its own git worktree under `.worktrees/`: a fresh implementer per task, a task review after every task, an SDD ledger, then a final whole-branch review on the most capable model, one consolidated fix wave, one scoped re-review by an agent. The lead does not merge or push.
- After a lead reports, the coordinator merges the branch into `main` itself and re-runs both full test suites before trusting the report. Twice a report needed correction this way (a stale dead-code claim, an integration compile failure).
- Merging branches that each passed alone can still break: A1's `Gamelab.Editor` asmdef swallowed A3's FMOD editor script. Always run the combined suites after a merge. Expect `.meta` add/add conflicts when two branches create the same folder, resolve by keeping one and unioning asmdef references.
- Rate limits interrupt agents often. Work survives if committed after every task. After an interruption, treat uncommitted files as unverified and diff them against the task brief. Resume with a fresh agent that reads the plan and the git log.
- Do not remove a worktree before its ledger and review notes matter: `.superpowers/` lives inside the worktree and is deleted with it.
- Background "completed" notices from long-finished agents keep arriving. Ignore them.
- Ask before every push to `github`. The user has said yes each time so far.

## Headless testing rules

- Never combine `-quit` with `-runTests`. They race, Unity exits 0 without running tests and writes no results file.
- Command shape: `Unity -batchmode -nographics -projectPath <repo>/Unity -runTests -testPlatform EditMode|PlayMode -testResults <file>.xml -logFile <file>.log`. Then read the XML for `result="Passed"` and the counts. Do not trust the exit code.
- Run one Unity process at a time on a project. Two instances lock each other and hang. Run it in the background with a log, poll with bounded waits, kill and investigate if the log stops growing for 5 minutes.
- The project compiles as C# 9.0 on netstandard2.1: no file-scoped namespaces, no `Random.Shared`. `Gamelab.Core` has no engine references and uses `System.Numerics.Vector2`. Use `git add Unity/Assets` (broad), not narrow paths, so folder `.meta` files are not missed.
- `dotnet build Src/Gamelab.csproj` should stay at 0 errors and `git diff <base> -- Src/` should stay empty.
