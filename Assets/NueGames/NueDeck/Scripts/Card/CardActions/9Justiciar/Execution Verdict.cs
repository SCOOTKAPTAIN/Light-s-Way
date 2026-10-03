using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class ExecutionVerdict : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.ExecutionVerdict;
        public override void DoAction(CardActionParameters actionParameters)
        {
           var targetCharacter = actionParameters.TargetCharacter;
           var selfCharacter = actionParameters.SelfCharacter;
           var anchor = CombatManager.Instance.EnemiesFxAnchor;
            
            if (!targetCharacter) return;

            targetCharacter.CharacterStats.ApplyStatus(
                StatusType.Weak,
                Mathf.RoundToInt(CardScaling.AddPotency(2f)),
                selfCharacter);

            targetCharacter.CharacterStats.ApplyStatus(
                StatusType.Fragile,
                Mathf.RoundToInt(CardScaling.AddPotency(2f)),
                selfCharacter);


            if (FxManager != null)
                FxManager.PlayFxAtPosition(anchor.position, FxType.ExecutionVerdict, new Vector3(0f, 0.4f, 0f));
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}