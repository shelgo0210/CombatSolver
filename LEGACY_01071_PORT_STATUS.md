# Experimental Slay the Spire 2 v0.107.1 backport

> **NOT A WORKING RELEASE. DO NOT INSTALL OR DISTRIBUTE AS A COMPATIBLE MOD.**
>
> 此分支仅保存面向游戏 v0.107.1 的移植实验。尚未编译通过，不可用。

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

## Current result (2026-10-10)

The latest Release compilation against the **actual v0.107.1 game assembly** fails with **236 compiler errors**. Therefore:

- No compatible DLL or installable ZIP exists.
- No gameplay, search-correctness, smoke, or headless tests have passed.
- `CombatSolver.json` intentionally retains upstream `min_game_version: 0.111.0`. **Do not lower it to install this unfinished port.**
- Do not cherry-pick this branch into main or publish it as a release.

Main outstanding migration areas: changed `CardPlay` ownership API and attack builders; removed/new card/power types; `ModelDb` model enumeration; RNG serialization; changed damage/turn Hooks, card result-pile hooks and method signatures; numerous old-game test harness incompatibilities. Build errors are often repeated references to shared missing APIs; their count does not measure effort precisely.

## Reproduce the current build failure

PowerShell, from the repository root (requires .NET 9 and local game/RitsuLib installation):

```powershell
dotnet build CombatSolver.csproj -c Release `
  -p:SteamRoot="E:\SteamLibrary" `
  -p:Legacy107Port=true `
  -p:CopyModOnBuild=false `
  -p:RunAnalyzers=false
```

Replace `SteamRoot` with your own Steam library root. The `CopyModOnBuild=false` setting prevents deployment into an installed game.

## Licensing

Derived from the original MIT-licensed CombatSolver; retain `LICENSE` and `THIRD_PARTY_NOTICES.md`. This experimental branch is not endorsed as a working backport by the upstream author.
