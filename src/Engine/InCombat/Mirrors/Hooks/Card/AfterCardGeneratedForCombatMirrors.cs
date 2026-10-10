using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Models.Monsters;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Models.Relics;
using MegaCrit.Sts2.Core.ValueProps;
using CombatSolver.Engine.Common;
using CombatSolver.Engine.Common.Mirrors;

namespace CombatSolver.Engine.InCombat.Mirrors.Hooks.Card;

using Registry = MethodMirrorRegistry<AbstractModel, AfterCardGeneratedForCombatMirrorContext>;

// Mirrors the prediction-relevant parts of Hook.AfterCardGeneratedForCombat.
internal static class AfterCardGeneratedForCombatMirrors
{
    private static readonly MirrorMethodSpec AfterCardGeneratedForCombat = MirrorMethodSpec.Hook(
        nameof(AbstractModel.AfterCardGeneratedForCombat),
        [typeof(CardModel), typeof(Player)]);

    private static readonly Registry Registry = CreateRegistry();

    public static void Invoke(AbstractModel listener, AfterCardGeneratedForCombatMirrorContext context)
    {
        using var dispatch = context.Simulator.BeginExecutionDispatch();
        Registry.Invoke(listener, context);
    }

    private static Registry CreateRegistry()
    {
        var registry = new Registry(AfterCardGeneratedForCombat);

        registry.Register<Aeonglass>(HandleAeonglass);
        registry.Register<ArsenalPower>(HandleArsenalPower);
        registry.Register<Regalite>(HandleRegalite);
#if !STS2_LEGACY_107
        registry.Register<SoulboundPower>(HandleSoulboundPower);
#endif // STS2_LEGACY_107
        registry.Register<PillarOfCreationPower>(HandlePillarOfCreationPower);
        registry.Register<SmokestackPower>(HandleSmokestackPower);
        registry.Register<TrashToTreasurePower>(HandleTrashToTreasurePower);
        registry.Register<RocketPunch>(HandleRocketPunch);

        return registry;
    }

    private static void HandleAeonglass(Aeonglass monster, AfterCardGeneratedForCombatMirrorContext context)
    {
        if (context.PreviewCard is not Wither)
            return;
        var wither = (Wither)context.MutablePreviewCard;
        if (context.CombatState is not ICombatPredictionMonsterStateSink monsterState)
            throw new InvalidOperationException("永世沙漏生成凋零缺少预测怪物状态。");
        int upgradeCount = monsterState.GetAeonglassWitherUpgradeCount(monster.Creature);
        for (int index = 0; index < upgradeCount; index++)
            wither.FakeUpgrade();
    }

    private static void HandleArsenalPower(ArsenalPower power, AfterCardGeneratedForCombatMirrorContext context)
    {
        if (context.Creator?.Creature == power.Owner)
        {
            if (context.CombatState is not ICombatPredictionEffectSink effects)
                throw new InvalidOperationException("军械库效果缺少可写的预测状态。");
            // 原版军械库（ArsenalPower.AfterCardGeneratedForCombat）传 cardSource: null。
            effects.ApplyPowerFromSource(typeof(StrengthPower), power.Owner, power.Amount, power.Owner, cardSource: null);
        }
    }

    private static void HandleRegalite(Regalite relic, AfterCardGeneratedForCombatMirrorContext context)
    {
        if (context.Creator != relic.Owner)
        {
            return;
        }

#if !STS2_LEGACY_107
        var state = context.StateStore.Get(relic, () => new RegalitePredictionState(relic));
        if (state.UsedThisTurn)
        {
            return;
        }

        state.UsedThisTurn = true;
#endif // Legacy Regalite triggers each generated card (confirmed against v0.107.1 IL).
        context.Simulator.GainBlock(relic.Owner.Creature, relic.DynamicVars.Block);
    }

#if !STS2_LEGACY_107
    private static void HandleSoulboundPower(SoulboundPower power, AfterCardGeneratedForCombatMirrorContext context)
    {
        if (context.Creator?.Creature != power.Applier ||
            context.PreviewCard is not Soul ||
            power.Owner.Player is not { } player)
        {
            return;
        }

        var state = context.StateStore.Get(power, () => new SoulboundPredictionState(power));
        if (state.IsAddingSoul)
        {
            return;
        }

        state.IsAddingSoul = true;
        try
        {
            context.Simulator.CreateAndAddGeneratedCardsToCombat<Soul>(
                player,
                PileType.Draw,
                power.Amount,
                player,
                CardPilePosition.Random);
        }
        finally
        {
            state.IsAddingSoul = false;
        }
    }
#endif // STS2_LEGACY_107

    private static void HandlePillarOfCreationPower(PillarOfCreationPower power, AfterCardGeneratedForCombatMirrorContext context)
    {
        if (context.Creator?.Creature == power.Owner)
        {
            context.Simulator.GainBlock(power.Owner, power.Amount, ValueProp.Unpowered);
        }
    }

    private static void HandleSmokestackPower(SmokestackPower power, AfterCardGeneratedForCombatMirrorContext context)
    {
        if (context.PreviewCard.Type == CardType.Status &&
            context.Creator?.Creature == power.Owner)
        {
            context.Simulator.Damage(context.State.HittableEnemies, power.Amount, ValueProp.Unpowered, power.Owner);
        }
    }

    private static void HandleTrashToTreasurePower(TrashToTreasurePower power, AfterCardGeneratedForCombatMirrorContext context)
    {
        if (context.PreviewCard.Type != CardType.Status ||
            context.Creator?.Creature != power.Owner ||
            power.Owner.Player is not { } player)
        {
            return;
        }

        for (var i = 0; i < power.Amount; i++)
        {
            var orb = OrbModel.GetRandomOrb(context.Rng.CombatOrbGeneration).ToMutable();
            context.Simulator.OrbChannel(player, orb);
            if (context.Simulator.HasPendingChoice)
                return;
        }
    }

    private static void HandleRocketPunch(RocketPunch card, AfterCardGeneratedForCombatMirrorContext context)
    {
        if (context.Creator == card.Owner &&
            context.PreviewCard.Owner == card.Owner &&
            context.PreviewCard.Type == CardType.Status)
        {
            context.State.FindCard(card)?.MutablePreview.EnergyCost.AddUntilPlayed(-1);
        }
    }
}

internal sealed class AfterCardGeneratedForCombatMirrorContext : CombatCardMirrorContext
{
    public required Player? Creator { get; init; }
}

#if !STS2_LEGACY_107
internal sealed class SoulboundPredictionState(SoulboundPower power) : IPredictionStateForkable
{
    public bool IsAddingSoul { get; set; } = power._isAddingSoul;

    public object Fork(PredictionForkContext context) => MemberwiseClone();
}
#endif // STS2_LEGACY_107

internal sealed class RegalitePredictionState(Regalite relic) : IPredictionStateForkable
{
#if STS2_LEGACY_107
    // This state is constant in 0.107.1; Regalite has no once-per-turn flag.
    public bool UsedThisTurn { get; set; }
#else
    public bool UsedThisTurn { get; set; } = relic._usedThisTurn;
#endif

    public object Fork(PredictionForkContext context) => MemberwiseClone();
}
