using System.Collections;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class VastLibrary : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.VastLibrary;

        public override void DoAction(CardActionParameters actionParameters)
        {
        }

        public override IEnumerator DoActionRoutine(CardActionParameters actionParameters)
        {
            var inventoryCanvas = UIManager.Instance != null
                ? UIManager.Instance.InventoryCanvas
                : null;
            if (CollectionManager == null || CollectionManager.HandController == null ||
                inventoryCanvas == null || CollectionManager.DrawPile.Count == 0 || GameManager == null ||
                GameManager.GameplayData == null ||
                CollectionManager.HandPile.Count >= GameManager.GameplayData.MaxCardOnHand)
                yield break;

            CardData selectedCard = null;
            var selectionFinished = false;

            inventoryCanvas.OpenCanvas();
            inventoryCanvas.ChangeTitle("Choose a card to draw");
            inventoryCanvas.SetCardsForSelection(CollectionManager.DrawPile, chosenCard =>
            {
                selectedCard = chosenCard;
                selectionFinished = true;
            });

            while (!selectionFinished && inventoryCanvas.gameObject.activeInHierarchy)
                yield return null;

            if (inventoryCanvas.gameObject.activeInHierarchy)
                inventoryCanvas.CloseCanvas();

            if (!selectionFinished || selectedCard == null || !CollectionManager.MoveDrawCardToHand(selectedCard))
                yield break;

            if (FxManager != null && actionParameters.SelfCharacter != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.VastLibrary);

            if (AudioManager != null && actionParameters.CardData != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}