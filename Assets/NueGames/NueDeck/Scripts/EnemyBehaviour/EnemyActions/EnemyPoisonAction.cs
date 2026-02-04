using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class EnemyPoisonAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Poison;
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            if (!newTarget) return;
            
            newTarget.CharacterStats.ApplyStatus(StatusType.Slimed,Mathf.RoundToInt(actionParameters.Value));

           
            // Apply Sabotaged effect (deals damage to self, then reduces Sabotaged by 1)
            NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplySabotaged(selfCharacter);
            
            if (FxManager != null) 
                FxManager.PlayFx(newTarget.transform, FxType.Poison);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(AudioActionType.Poison);
        }
    }
}