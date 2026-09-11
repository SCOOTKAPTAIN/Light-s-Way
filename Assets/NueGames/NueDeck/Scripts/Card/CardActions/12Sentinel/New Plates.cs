using System.Collections;
using System.Collections.Generic;
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

            var selectionCanvas = UIManager.Instance != null ? UIManager.Instance.CardSelectionCanvas : null;
            if (selectionCanvas == null)
            {
                DoAction(actionParameters);
                yield break;
            }

            List<CardBase> discardedCards = null;
            selectionCanvas.BeginSelection("Discard up to 4 cards", 4, cards => discardedCards = cards);

            while (discardedCards == null)
                yield return null;

            foreach (var card in discardedCards)
                card.Discard();

            if (discardedCards.Count > 0)
                selfCharacter.CharacterStats.ApplyStatus(StatusType.Armor, discardedCards.Count);

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