using System.Reflection;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using CombatSolver.Engine.Common;
using CombatSolver.Engine.Common.Mirrors;
using CombatSolver.Engine.InCombat.Simulation;

namespace CombatSolver.Engine.InCombat.Mirrors.Cards;

using Registry = MethodMirrorRegistry<CardModel, CardResultLocationMirrorContext, CardLocation>;

// Mirrors CardModel.GetResultLocationForCardPlay without calling virtual methods on detached
// preview cards. In particular, TheBall's original override would advance the real CombatTargets RNG.
internal static class CardResultLocationMirrors
{
    private static readonly MirrorMethodSpec GetResultLocationForCardPlay = new(
        typeof(CardModel),
#if STS2_LEGACY_107
        "GetResultPileTypeForCardPlay",
#else
        "GetResultLocationForCardPlay",
#endif
        BindingFlags.Instance | BindingFlags.NonPublic,
        []);

    private static readonly Registry Registry = CreateRegistry();

    public static CardLocation GetResultLocation(
        CombatPredictionSimulator simulator,
        PredictedCard card)
    {
        var result = GetBaseResultLocation(simulator, card);

        return Registry.Invoke(card.Preview, new()
        {
            Simulator = simulator,
            Card = card,
            BaseResult = result
        }, result).Value;
    }

    private static Registry CreateRegistry()
    {
        var registry = new Registry(GetResultLocationForCardPlay);

        registry.Register<ParticleWall>(HandleParticleWall);
#if !STS2_LEGACY_107
        registry.Register<ShiningStrike>(HandleShiningStrike);
#endif
#if !STS2_LEGACY_107
        registry.Register<TheBall>(HandleTheBall);
#endif // STS2_LEGACY_107

        return registry;
    }

    private static CardLocation GetBaseResultLocation(
        CombatPredictionSimulator simulator,
        PredictedCard card)
    {
        var preview = card.Preview;
        if (preview.IsDupe || preview.Type is CardType.Power)
        {
            return new(preview.Owner, PileType.None, CardPilePosition.Bottom);
        }

        bool exhaustOnNextPlay = preview.ExhaustOnNextPlay;
        if (exhaustOnNextPlay || card.HasKeyword(simulator.State, CardKeyword.Exhaust))
        {
            if (exhaustOnNextPlay)
                card.MutablePreview.ExhaustOnNextPlay = false;
            return new(preview.Owner, PileType.Exhaust, CardPilePosition.Bottom);
        }

        return new(preview.Owner, PileType.Discard, CardPilePosition.Bottom);
    }

    private static CardLocation HandleParticleWall(
        ParticleWall _,
        CardResultLocationMirrorContext context)
    {
        var result = context.BaseResult;
        if (result.pileType is PileType.Discard)
        {
            result.pileType = PileType.Hand;
        }
        return result;
    }

    private static CardLocation HandleShiningStrike(
        ShiningStrike _,
        CardResultLocationMirrorContext context)
    {
        var result = context.BaseResult;
        if (result.pileType is PileType.Discard)
        {
            result.pileType = PileType.Draw;
            result.position = CardPilePosition.Top;
        }
        return result;
    }

#if !STS2_LEGACY_107
    private static CardLocation HandleTheBall(TheBall _, CardResultLocationMirrorContext context)
    {
        var result = context.BaseResult;
        var owner = context.PreviewCard.Owner;
        var teammates = context.State.GetTeammatesOf(owner.Creature)
            .Where(creature =>
                context.State.GetCreature(creature).IsAlive &&
                creature.IsPlayer &&
                creature.Player != owner)
            .ToList();
        if (teammates.Count == 0)
        {
            return result;
        }

        result.player = context.Rng.CombatTargets.NextItem(teammates)!.Player!;
        if (result.pileType is PileType.Discard)
        {
            result.pileType = PileType.Draw;
            result.position = CardPilePosition.Random;
        }
        return result;
    }
#endif // STS2_LEGACY_107
}

internal sealed class CardResultLocationMirrorContext : CombatCardMirrorContext<CardModel>
{
    public required CardLocation BaseResult { get; init; }

    protected override AbstractModel GetDispatchSource(CardModel _) => OriginalCard;
}
