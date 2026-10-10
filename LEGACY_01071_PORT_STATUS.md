# Experimental Slay the Spire 2 v0.107.1 backport

> **EXPERIMENTAL COMPATIBILITY BUILD — limited validation, not certified for general gameplay.**
>
> 此分支对 v0.107.1 已能编译、载入并通过单场战斗和 RNG 分支测试；尚未完成全部卡牌、遗物、跨回合和存档回归。测试/预览版本请备份存档并谨慎使用。

## Provenance and environment

- Upstream: `Torch1230/CombatSolver` at `5c773caaec742bc55588bc9ee21c361326a732f0` (mod 0.50.1), MIT license.
- Target: Slay the Spire 2 `v0.107.1` (`release_info.json`, commit `59260271`).
- Framework: RitsuLib 0.6.7, using its `compat/0.107.1` references.
- Toolchain: .NET SDK 9.0.308, `Godot.NET.Sdk/4.5.1`.

## Implemented experiments

- Added a local `AssemblyInfo` compatibility adapter based on the older `ModManager.Mods` API and assembly identity.
- Replaced the missing `CombatTurnState` live-state accessor with the old combat state API.
- Added a local `CardLocation` transition DTO. **This alone does not port the older game's pile-choice hooks**.
- Reworked part of RNG clone/snapshot/advance for the older 32-bit seed and counter APIs. **RNG forecast semantics remain unverified**.
- Conditionally excluded handlers for cards/powers that did not exist in the target game.
- Added `Legacy107Port=true` build switch; no alteration of Steam game files.

## Verified state (2026-10-10)

- **Release build:** Success, zero C# compiler errors, .NET 9 targeting the actual game v0.107.1 assembly and RitsuLib compat/0.107.1.
- **Static game API audit:** All 1,493 referenced game types present; zero missing member names. Seven potential generic method-signature differences were audit artifacts (generic parameter naming) requiring no compiler changes.
- **Native mod loader:** 68/68 CombatSolver Harmony patches applied to the **isolated** v0.107.1 game, zero failed.
- **SMOKE-001:** Passed. Ironclad in FUZZY_WURM_CRAWLER_WEAK; solver found a route, automatically played it and won on turn 1 with player HP 80/80. Evidence: `.local/tool-tasks/legacy107/aftercard-evidence/result.json`.
- **LAZY-RNG-FORK:** Passed. Nine RNG streams: native draw sequence, fork independence (including parent/sibling/grandchild), cold read-only observation and original state isolation. Battle also completed. Evidence: `.local/tool-tasks/legacy107/rng-evidence/result.json`.
- **SMOKE-001 + BoneTea + ToastyMittens:** Passed in the isolated v0.107.1 game (same first-turn native victory at 80/80 HP), with both relics injected. Evidence: `.local/tool-tasks/legacy107/relic-final-evidence/result.json`. This only exercises their native battle setup and the tested first-turn search; multi-turn prediction parity for their effects remains unverified. The launcher wrapper returned exit code 1 during cleanup despite its authoritative game-generated result reporting `Passed`; the headless Godot shutdown log contains resource-leak warnings.
- **Caveat:** The suite above is insufficient to certify broad compatibility, future-turn simulation, saved games, all relic/card models, multiplayer or performance.

The source repository `CombatSolver.json` intentionally retains upstream `min_game_version: 0.111.0` to prevent accidental installation of this experimental branch. For explicitly labeled test or preview packaging only, construct a copy with `min_game_version: 0.107.1`. The installed original game, Steam Workshop mod and personal save directory were not modified.

The original game changed methods, hook timings, card-result destination, Rng seeding and many item effects. The port contains old-version conditional implementations for turn setup/auto-preplay hooks, card-play owner, RNG snapshot/fork, card result pile, HandDrill damage hook, Juggling third attack, Regen after-side-turn healing, BoneTea after-side-turn upgrades, and ToastyMittens before-hand-draw choice-free exhaustion. Some changes have source/IL support but have **not** undergone focused native-vs-predicted differential tests.

## Reproduce the Release build

PowerShell, from the repository root (requires .NET 9 and local game/RitsuLib installation):

```powershell
dotnet build CombatSolver.csproj -c Release `
  -p:SteamRoot="E:\SteamLibrary" `
  -p:Legacy107Port=true `
  -p:CopyModOnBuild=false `
  -p:SkipMemoryCleaner=true `
  -p:RunAnalyzers=false
```

Replace `SteamRoot` with your own Steam library root. `CopyModOnBuild=false` prevents deployment into your installed game. `SkipMemoryCleaner=true` bypasses the unrelated .NET Framework 4.8 helper restoration; previews use the unchanged MIT-licensed upstream helper binary. RitsuLib 0.6.7 must be installed separately. Use a test backup of saves before trying this preview in ordinary gameplay.

## Licensing

Derived from the original MIT-licensed CombatSolver; retain `LICENSE` and `THIRD_PARTY_NOTICES.md`. This experimental branch is not endorsed as a working backport by the upstream author.
