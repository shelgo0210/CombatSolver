using MegaCrit.Sts2.Core.Combat;

namespace CombatSolver.Engine.InCombat.Extensions;

internal static class CombatManagerExtensions
{
    public static CombatState? GetLiveCombatState(this CombatManager combatManager)
        => combatManager is { IsInProgress: true, IsStarting: false, IsEnding: false }
            ? combatManager.DebugOnlyGetState()
            : null;
}
