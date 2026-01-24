using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class EnemyAttackAction: EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Attack;
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            var value = Mathf.RoundToInt(actionParameters.Value +
                                         actionParameters.SelfCharacter.CharacterStats.StatusDict[StatusType.Strength]
                                             .StatusValue);

            var selfCharacter = actionParameters.SelfCharacter;

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(actionParameters.TargetCharacter, selfCharacter, value));
                                 
            actionParameters.TargetCharacter.CharacterStats.Damage(value, false, "red", actionParameters.SelfCharacter);
            if (FxManager != null)
            {
                FxManager.PlayFx(actionParameters.TargetCharacter.transform,FxType.Attack);
              //  FxManager.SpawnFloatingText(actionParameters.TargetCharacter.TextSpawnRoot,value.ToString());
            }

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.Attack);
           
        }
    }
}