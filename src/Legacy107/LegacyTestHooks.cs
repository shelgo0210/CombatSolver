using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;

namespace CombatSolver;

/// <summary>Test-only old-game equivalents for APIs removed/renamed after v0.107.1.</summary>
internal static class LegacyTestHooks
{
    public static async Task AfterSideTurnEndAsync(
        CombatState combat, CombatSide side, IEnumerable<Creature> participants)
    {
        Creature[] ordered = participants.ToArray();
        // Run original old-game AbstractModel hook methods, not simulation mirrors.
        // This is a manually dispatched direct native-effects fixture.
        foreach (AbstractModel listener in combat.IterateHookListeners())
            await listener.AfterSideTurnEnd(new BlockingPlayerChoiceContext(), side, ordered);
    }

    public static async Task WaitForStartupAsync(NGame host)
    {
        long deadline = System.Environment.TickCount64 + 60_000;
        while (host.MainMenu is null || !host.MainMenu.IsInsideTree() ||
               !ModelDb.AllCharacters.Any())
        {
            if (System.Environment.TickCount64 > deadline)
                throw new TimeoutException("v0.107.1 game startup did not reach populated main menu in 60 seconds.");
            await host.ToSignal(host.GetTree(), SceneTree.SignalName.ProcessFrame);
        }
    }
}
