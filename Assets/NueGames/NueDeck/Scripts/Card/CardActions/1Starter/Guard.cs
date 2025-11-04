using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class Guard : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.Guard;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

            newTarget.CharacterStats.ApplyStatus(StatusType.Block,
                Mathf.RoundToInt(actionParameters.Value + GameManager.PersistentGameplayData.proficiency + actionParameters.SelfCharacter.CharacterStats
                    .StatusDict[StatusType.Fortitude].StatusValue));






            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.Guard);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}