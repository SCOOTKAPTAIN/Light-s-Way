using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BulkUp : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BulkUp;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

            newTarget.CharacterStats.ApplyStatus(StatusType.Block,
                Mathf.RoundToInt(actionParameters.Value + GameManager.PersistentGameplayData.proficiency + actionParameters.SelfCharacter.CharacterStats
                    .StatusDict[StatusType.Fortitude].StatusValue + (actionParameters.SelfCharacter.CharacterStats
                    .StatusDict[StatusType.Strength].StatusValue * 2)));


            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.BulkUp);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}