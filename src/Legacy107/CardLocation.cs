// Compatibility DTO for game v0.107.1, which exposes only PileType during card-play resolution.
// This type belongs to the mod assembly, not the game's sts2 assembly.
using MegaCrit.Sts2.Core.Entities.Players;

namespace MegaCrit.Sts2.Core.Entities.Cards;

internal record struct CardLocation
{
    public Player player;
    public PileType pileType;
    public CardPilePosition position;

    public CardLocation(Player player, PileType pileType, CardPilePosition position)
    {
        this.player = player;
        this.pileType = pileType;
        this.position = position;
    }
}
