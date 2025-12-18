using System;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class EmergencyDodge : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.EmergencyDodge;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

           newTarget.CharacterStats.ApplyStatus(StatusType.Armor,1);

           //newTarget.CharacterStats.ApplyStatus(StatusType.DebuffWard,1);

           newTarget.CharacterStats.ApplyStatus(StatusType.Block,Mathf.RoundToInt(CombatManager.Instance.CurrentMainAlly.CharacterStats.MaxHealth));


            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.EmergencyDodge, new Vector3(0,0.5f,0));
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);

                CombatManager.EndTurn();
        }
    }
}