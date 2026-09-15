using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class PiquedInterest : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.PiquedInterest;

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
            selectionCanvas.BeginSelection("Select up to 3 cards", 3, cards => selectedCards = cards,
                IsAttackOrBuffCard);

            while (selectedCards == null)
                yield return null;

            if (selectedCards.Count == 0)
                yield break;

            CollectionManager.ExchangeCardsForOppositeCategory(selectedCards);

            if (FxManager != null && actionParameters.SelfCharacter != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.PiquedInterest);

            if (AudioManager != null && actionParameters.CardData != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }

        private static bool IsAttackOrBuffCard(CardBase card) =>
            card != null && card.CardData != null &&
            (card.CardData.Category == CardCategoryType.Attack || card.CardData.Category == CardCategoryType.Skill);
    }
}
