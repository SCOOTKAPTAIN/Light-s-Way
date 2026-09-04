using System;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class FreshRounds: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.FreshRounds;

        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            if (selfCharacter == null)
                return;

            var quickDrawCardData = FindQuickDrawCard();
            if (quickDrawCardData == null)
            {
                Debug.LogWarning("[FreshRounds] Quick Draw is not included in Gameplay Settings > All Cards List.");
                return;
            }

            int quickDrawCount = Mathf.Max(0, Mathf.RoundToInt(actionParameters.Value));
            if (quickDrawCount <= 0)
                return;

            for (int i = 0; i < quickDrawCount; i++)
            {
                if (CollectionManager == null || CollectionManager.HandController == null)
                    break;

                if (GameManager != null && GameManager.GameplayData != null &&
                    GameManager.GameplayData.MaxCardOnHand <= CollectionManager.HandPile.Count)
                    break;

                var cardClone = GameManager.BuildAndGetCard(quickDrawCardData, CollectionManager.HandController.transform);
                CollectionManager.HandController.AddCardToHand(cardClone);
                CollectionManager.HandPile.Add(quickDrawCardData);
            }

            foreach (var cardObject in CollectionManager.HandController.hand)
                cardObject.UpdateCardText();

            if (UIManager.Instance != null)
                UIManager.Instance.CombatCanvas.SetPileTexts();

            FxManager.PlayFx(selfCharacter.transform, FxType.FreshRounds, new Vector3(0f, 0.4f, 0f));

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }

        private CardData FindQuickDrawCard()
        {
            if (GameManager == null || GameManager.GameplayData == null || GameManager.GameplayData.AllCardsList == null)
                return null;

            foreach (var card in GameManager.GameplayData.AllCardsList)
            {
                if (card == null) continue;

                if (string.Equals(card.Id, "11_2_QuickDraw", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(card.CardName, "Quick Draw", StringComparison.OrdinalIgnoreCase))
                    return card;
            }

            return null;
        }
    }
}