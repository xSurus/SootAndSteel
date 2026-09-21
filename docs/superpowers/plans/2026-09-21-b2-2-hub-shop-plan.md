# B2.2 hub and shop UI port plan

Branch `port/ui-hub` (from main 537f185). `Src/` is read-only. Follow `Unity/CONVENTIONS.md` and the handoff testing rules (C# 9, netstandard2.1, never `-quit` with `-runTests`, one Unity process at a time, `git add Unity/Assets`, commit per task).
Slice: hub overlay (credits, players ready), crafting help panel, departure hint and decision dialog (DialogBubble), shop tooltip (ToolTip, ShopItemType, ButtonWithIcon), and the shop domain behind them. Out of scope: in-game HUD, post-level screens, join screen, finished menus, cannon seat, player ejection, player spawning, world-space hub view.

## Scope decision: UI versus domain

Src has no shop screen. The shop is world objects (`BuyableStationWrapper`) that show a `ToolTip` above them, a credits counter in `HubOverlay`, and `HubScreen` glue. The split:

Domain, ported engine-free into `Gamelab.Core` with EditMode tests (the UI needs real data to bind to, per the B2 row of the port plan):
- Shop data and rules: `EItemType`, `StationConfig`, `ComponentConfig` (shop and text subset), `StationRegistry`, `ComponentRegistry`, `CatalogItem`, `IShopService` and `ShopManager` (catalog order matters, `HubMapModel.SelectOfferIndices` indexes into it), `RunCredits` (Src `RunSession` credits, upgrade counter, crafting help flag), `ShopItemIconAtlas`, `XboxButtonAtlas`, `StationTooltipInfo` (category, functionality and icon rect of a station id, from `AbstractStation` and `ComponentResourceStation`).
- Offers: `BuyableOffer` is the data shape of `BuyableStationWrapper` (kind id, cost, title, description, category, position, `IsVisible` highlight flag, `ITooltipable`). `HubShopModel` restocks with `HubMapModel.RestockSeed`, `SelectOfferIndices` and `OfferPositions` and runs the purchase rule (spend, count upgrade, raise `Purchased`). Spawning the real station and the buy particles and sound stay with the world wave, which listens to `Purchased`.
- Hub state that drives UI: `HubDepartureModel` (ready set, all ready, pending off-board items, depart hold timer, lever hint delay, depart decision), because HubOverlay, the hint bubble and the decision bubble are pure functions of it.
- Tooltip content and layout: `TooltipModel` (texts, category row, two interaction buttons, affordability) with `TooltipInteractions` (the `ConfigureInteractionButtons` switch) and `TooltipLayout` (ideal placement above or below, overlap separation, clamp), all in 1920x1080 canvas units.

UI, in Runtime with PlayMode tests: UXML/USS and views for `HubOverlay`, `CraftingHelp`, `DialogBubble`, `ToolTip`, and one `HubUiController` that owns them. Screen space only.

Not ours (recorded in CONVENTIONS): station objects and their highlight, the lever `OnInteractOverride`, `CountPendingShopItemsOffBoard` (needs the map), the buy VFX and `Sounds.Purchase`, the departure fade and screen switch, `SaveManager`. Each is a callback or a value the caller passes in.

Player-dependent seams, the narrowest possible:
- `IWorldToScreen` (Runtime): world pixel position (Src Y-down frame) to a screen point in top-left origin pixels. The conversion under the Y-mirrored camera is unverified. `CameraWorldToScreen` is a small `Camera.WorldToScreenPoint` implementation with a PlayMode test against `MirroredCamera`, marked unverified beyond that test.
- `ITooltipable` (already in Core) is what the world wave implements on stations. `TooltipLayer.Set(id, ITooltipable, TooltipKind)` takes it.
- Input: the controller polls all players through `Func<IReadOnlyList<IInputActions>>` like `MenuNavigator`. Back toggles crafting help, Interact and Grab answer the decision bubble, exactly as Src.
- Sound: `Action playSelect` is not needed here (Src hub UI plays no UI sounds). Nothing is added.

## Decisions

- Namespaces follow Src: `Gamelab.Services.Shop`, `Gamelab.Data`, `Gamelab.PhysicalEntities.Configurable`, `Gamelab.Items`, `Gamelab.UI` (Core UI logic), `Gamelab.UI.Runtime` (views).
- JSON is not ported (CONVENTIONS: catalogs are C# or assets). `StationConfig.json` and `ComponentConfig.json` are transcribed into Core tables by a script, in JSON order, and a test pins the catalog ids, prices and order (produced from the Src files, same method as the level goldens).
- Fonts: Src names Bernard MT Condensed, Bahnschrift Light, Berlin Sans FB, Bodoni MT. None is in the repo. Use the B2.1 fonts (Ubuntu Mono, Special Elite, Libre Bodoni) and record the substitution.
- Xbox glyphs: crop the 32 px cells out of `xbox_buttons_spritesheet.png` (`UiSpriteCrop`, `Sprite.Create`). This resolves the B2.1 deferral, so the controls overlay Close glyph switches from the "A" label to the real glyph.
- DialogBubble: only the modes the hub uses are ported (passive hint, two-button decision, hide). Typewriter reveal, `Show(line)` with world anchor and tail belong to the tutorial dialogue slice (`DialogueOverlay`, `DialogueManager`), deferred.
- Category icons: Src picks `Content/Items/Basic{Casing,Projectile,Propellant}.png` when the file exists and only otherwise crops the atlas. The files ship, so the atlas fallback is unreachable for those three categories. The port copies the `Items/Basic*.png` files as `WorldUiManager` does, and keeps the atlas rect in Core for `IconSourceRect` parity.
- Layout numbers come from the Gum `.gucx` files (dumped with a script), design canvas 1920x1080. Deviations are recorded.

## Tasks (TDD, tests, commit, then task review)

1. Shop data and rules (Core). `Core/Items/EItemType.cs`, `Core/Shop/*`, `Core/PhysicalEntities/Stations/{StationConfig,StationRegistry,StationTooltipInfo}.cs`, `Core/Items/Bullets/{ComponentConfig,ComponentRegistry}.cs`, `Core/UI/{ShopItemIconAtlas,XboxButtonAtlas}.cs`, `Core/Run/RunCredits.cs`. Tests: catalog golden (ids, names, prices, order), `GetCatalogItem`, credits spend rules, category and functionality per station id, atlas rects.
2. Offers and tooltip model (Core). `BuyableOffer`, `HubShopModel` (restock determinism vs `HubMapModel`, purchase rules: affordable, not affordable, cost 0 for unknown id, `Purchased` once, offer removed), `TooltipInteractions`, `TooltipModel` (affordability follows credits), `TooltipLayout` (numbers hand-computed from `WorldUiManager`). Tests for each.
3. Hub state models (Core). `HubDepartureModel`, `HubOverlayModel` (credits text, ready state, per joined-slot ready flags), `DialogBubbleModel`, `CraftingHelpModel`, `HubInput` (polls all players: Back toggle, decision Interact and Grab, input block timer). Tests port the `UpdateDepartureLogic`, `UpdateDepartureHints`, `UpdateDepartDecisionInput` behaviour: hint after 5 s and hidden after 8 s, decision opens when all ready with pending items, 0.2 s input block, Grab sets not ready, Interact allows depart, hold timer 0.75 s raises `DepartRequested`, ready set pruned to joined players.
4. UI plumbing and art. Import art into `Assets/Resources/UI/Art/` (coin, tooltip base, badge, category icons, Idle heads, bubble parts, xbox sheet, xbtn, select), `UiSpriteCrop`, USS additions to `Common.uss`, importer test coverage. Controls overlay glyph switch.
5. Views. `HubOverlay`, `CraftingHelp`, `DialogBubble`, `ToolTip` UXML/USS and view classes. PlayMode tests: layout numbers from `resolvedStyle`, texts and visibility follow models, tooltip affordability colour, category row hidden without category, buttons per kind.
6. `HubUiController`, `TooltipLayer`, `IWorldToScreen`, `CameraWorldToScreen`. PlayMode tests: full flow with fake inputs (ready, decision, depart event), tooltip positions from a fake projector via `TooltipLayout`, visible tooltips overlap apart, mirrored camera projection test.
7. CONVENTIONS section and updates to the stale deferred lines, then the lead runs both full suites.

Final: whole-branch review (most capable model), one fix wave, one scoped re-review. Verify EditMode and PlayMode, `git diff 537f185 -- Src/` empty, `dotnet build Src/Gamelab.csproj`, no stray `.meta`.
