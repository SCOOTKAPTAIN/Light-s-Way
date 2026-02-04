using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class EnemyBlockAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Block;
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;
            
            float blockValue = actionParameters.Value;
            
            // Apply Light-based multiplier if action has flag enabled (uses cached value from combat start)
            if (actionParameters.ActionData != null && actionParameters.ActionData.ApplyLightMultiplier)
            {
                blockValue *= CombatManager.Instance.CombatLightMultiplier;
            }
            
            newTarget.CharacterStats.ApplyStatus(StatusType.Block,
                Mathf.RoundToInt(blockValue + actionParameters.SelfCharacter.CharacterStats
                    .StatusDict[StatusType.Fortitude].StatusValue));
            
            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform,FxType.Block);

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.Block);
        }
    }
}