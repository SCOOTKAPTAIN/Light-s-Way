using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class LiteratureSelection : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.LiteratureSelection;

        public override void DoAction(CardActionParameters actionParameters)
        {
        }

        public override IEnumerator DoActionRoutine(CardActionParameters actionParameters)
        {
            if (CollectionManager == null || CollectionManager.HandController == null)
                yield break;

            var selectionCanvas = UIManager.Instance != null ? UIManager.Instance.CardSelectionCanvas : null;
            if (selectionCanvas == null)
                yield break;

            List<CardBase> selectedCards = null;
            selectionCanvas.BeginSelection("Select a card", 1, cards => selectedCards = cards);

            while (selectedCards == null)
                yield return null;

            if (selectedCards.Count == 0 || selectedCards[0] == null)
                yield break;

            var selectedCard = selectedCards[0];
            if (!CollectionManager.HandController.hand.Contains(selectedCard))
                CollectionManager.HandController.AddCardToHand(selectedCard);

            CollectionManager.DrawAllCopies(selectedCard.CardData);

            if (FxManager != null && actionParameters.SelfCharacter != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.LiteratureSelection);

            if (AudioManager != null && actionParameters.CardData != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}