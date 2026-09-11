using NueGames.NueDeck.Scripts.Card;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Collection
{
    // Implemented by UI panels that accept a card dragged out of the hand (e.g. discard-selection canvases).
    public interface ICardDropTarget
    {
        RectTransform DropZone { get; }
        bool TryAcceptCard(CardBase card);
    }
}
