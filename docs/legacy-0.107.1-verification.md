# Legacy v0.107.1 verification

This is a **separate experimental backport** from the MIT-licensed Torch1230/CombatSolver v0.50.1. It is not an upstream release, and it has not completed broad gameplay or parity certification.

## Environment

- Slay the Spire 2: **v0.107.1**, game binary from user's local Steam installation.
- RitsuLib: **0.6.7**, using `compat/0.107.1` (separate dependency).
- Windows x64, .NET SDK 9.0.308, Godot.NET.Sdk 4.5.1.
- Isolated, headless copied-game instances; no writes to the live game installation or save directory.

## Verified

| Test | Result | Scope |
| --- | --- | --- |
| Release compile against actual old game DLL | Passed | 0 compiler errors with `Legacy107Port=true` |
| Mod loader/Harmony | Passed | All 68 CombatSolver patches installed without error |
| `SMOKE-001` | Passed | Auto-solving and native play of an Ironclad route against FUZZY_WURM_CRAWLER_WEAK; first-turn victory at 80/80 HP |
| `LAZY-RNG-FORK` | Passed | Native RNG draws across nine streams, read-only snapshots, parent/sibling/grandchild fork independence and native-state isolation; auto-battle also won |
| `SMOKE-001` with BoneTea and ToastyMittens injected | Passed (game-generated result) | First-turn auto-win with both legacy relics present; **does not prove** multi-turn effect parity. The wrapper exited 1 amid unrelated headless engine shutdown resource warnings, despite game result `Passed` |
| Static type references | Passed | 1,493 referenced game types found, 0 missing names of referenced members |

Log/result evidence recorded under `.local/tool-tasks/legacy107/{aftercard,rng}-evidence/` in the developer worktree; those runtime logs are excluded from the public source branch.

## Important incomplete coverage

This **does not certify** all cards, all enemies, all relics, different seeds, mid-turn choices, damage/potion behavior, long multi-turn searches, multiplayer, saves, performance or equivalence with native game behavior.

Some v0.107.1 changes are not cosmetic: source-sensitive attack builders, card-result-pile hooks, 32-bit RNG seeds, altered hook dispatch, several moved effect stages. Old-only implementations for HandDrill, JugglingPower, RegenPower, BoneTea and ToastyMittens were built based on native IL inspection; full effect-by-effect differential tests are still pending. Unknown or mismatched gameplay semantics should fail rather than fabricate a result.

## Reproduce build

Use PowerShell with .NET 9 SDK and the original Steam library root:

```powershell
dotnet build CombatSolver.csproj -c Release `
  -p:SteamRoot="E:\SteamLibrary" `
  -p:Legacy107Port=true `
  -p:CopyModOnBuild=false `
  -p:SkipMemoryCleaner=true `
  -p:RunAnalyzers=false
```

This compiles **without copying the mod into the installed game**. The independent .NET Framework 4.8 memory cleaner is skipped during development builds because its package restore is unrelated to v0.107.1 compatibility; packaged previews retain the unmodified helper from upstream.

The checked-in `CombatSolver.json` remains unchanged at minimum game version 0.111.0 as a protection against accidental installation. Any **explicitly labeled preview archive** must include its own copied manifest declaring `min_game_version: 0.107.1`, with the original MIT license and third-party notices. Installation should be approached cautiously with a backup of saves.

## Ongoing qualification

More isolated-game tests, especially card choices, block/exhaust, old RNG forward compatibility and native-vs-predicted effects across future turns, are required before removing the experimental label.
