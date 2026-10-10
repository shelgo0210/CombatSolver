// The older game constructs AttackCommand with a card source but does not preserve
// CardPlay. Preserve the prediction trace's original CardPlay as sidecar state,
// without altering game-global command classes or actual combat.
using System.Runtime.CompilerServices;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Entities.Creatures;

namespace MegaCrit.Sts2.Core.Commands.Builders;

internal static class AttackCommandCompatibility
{
    private static readonly ConditionalWeakTable<AttackCommand, CardPlay> Contexts = new();

    public static AttackCommand FromCard(this AttackCommand command, CardModel card, CardPlay play)
    {
        command.FromCard(card);
        Contexts.Remove(command);
        Contexts.Add(command, play);
        return command;
    }

    public static AttackCommand FromOsty(this AttackCommand command, Creature osty, CardModel card, CardPlay play)
    {
        command.FromOsty(osty, card);
        Contexts.Remove(command);
        Contexts.Add(command, play);
        return command;
    }

    internal static CardPlay? GetLegacyCardPlay(this AttackCommand command)
        => Contexts.TryGetValue(command, out var play) ? play : null;
}
