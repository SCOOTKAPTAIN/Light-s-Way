using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class LightStunAction : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.LightStun;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var value = actionParameters.Value;
            actionParameters.TargetCharacter.CharacterStats.ApplyStatus(StatusType.Stun,Mathf.RoundToInt(value));

            GameManager.PersistentGameplayData.ChangeLight(-10);


            if (FxManager != null)
            {
                FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position,FxType.Stun);
            }
           
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}