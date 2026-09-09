using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class NewPlates : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.NewPlates;

        public override IEnumerator DoActionRoutine(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            if (!selfCharacter || CollectionManager == null || CollectionManager.HandController == null)
                yield break;

            var inventoryCanvas = UIManager.Instance != null ? UIManager.Instance.InventoryCanvas : null;
            if (inventoryCanvas == null)
            {
                DoAction(actionParameters);
                yield break;
            }

            var selectedCards = new List<CardBase>();
            var selectionComplete = false;
            var uiManager = UIManager.Instance;
            uiManager.SetCanvas(inventoryCanvas, true, true);
            inventoryCanvas.ChangeTitle("Discard up to 4 cards");
            inventoryCanvas.BeginHandMultiSelection(
                new List<CardBase>(CollectionManager.HandController.hand),
                4,
                cards =>
                {
                    selectedCards = cards;
                    selectionComplete = true;
                    GameManager.PersistentGameplayData.CanSelectCards = false;
                });

            while (!selectionComplete)
                yield return null;

            foreach (var card in selectedCards.OrderByDescending(card => CollectionManager.HandController.hand.IndexOf(card)))
            {
                var cardIndex = CollectionManager.HandController.hand.IndexOf(card);
                if (cardIndex < 0)
                    continue;

                CollectionManager.HandController.RemoveCardFromHand(cardIndex);
                card.Discard();
            }

            if (selectedCards.Count > 0)
                selfCharacter.CharacterStats.ApplyStatus(StatusType.Armor, selectedCards.Count);

            if (FxManager != null)
                FxManager.PlayFx(selfCharacter.transform, FxType.NewPlates);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }

        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

            





            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.NewPlates);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}