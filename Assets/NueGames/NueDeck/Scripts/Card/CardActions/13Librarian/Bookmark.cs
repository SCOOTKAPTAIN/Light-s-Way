using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class Bookmark : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.Bookmark;

        public override void DoAction(CardActionParameters actionParameters)
        {
            if (AudioManager != null && actionParameters.CardData != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }

        public override IEnumerator DoActionRoutine(CardActionParameters actionParameters)
        {
            if (CollectionManager == null || CollectionManager.HandController == null)
                yield break;

            var selectionCanvas = UIManager.Instance != null ? UIManager.Instance.CardSelectionCanvas : null;
            if (selectionCanvas == null)
            {
                DoAction(actionParameters);
                yield break;
            }

            List<CardBase> selectedCards = null;
            selectionCanvas.BeginSelection("Retain a card", 1, cards => selectedCards = cards);

            while (selectedCards == null)
                yield return null;

            if (selectedCards.Count == 0 || selectedCards[0] == null)
                yield break;

            var selectedCard = selectedCards[0];
            if (selectedCard.CardData != null && selectedCard.CardData.Retain)
            {
                CollectionManager.HandController.AddCardToHand(selectedCard);
                yield break;
            }

            selectedCard.SetTemporaryRetain(true);
            CollectionManager.HandController.AddCardToHand(selectedCard);

            if (FxManager != null && actionParameters.SelfCharacter != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.Bookmark);

            if (AudioManager != null && actionParameters.CardData != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}