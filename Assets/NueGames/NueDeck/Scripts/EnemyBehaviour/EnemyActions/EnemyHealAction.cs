using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class EnemyHealAction : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Heal;
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;

            if (!newTarget) return;
            
            float healValue = actionParameters.Value;
            
            // Apply Light-based multiplier if action has flag enabled (uses cached value from combat start)
            if (actionParameters.ActionData != null && actionParameters.ActionData.ApplyLightMultiplier)
            {
                healValue *= CombatManager.Instance.CombatLightMultiplier;
            }
            
            newTarget.CharacterStats.Heal(Mathf.RoundToInt(healValue));

            if (FxManager != null) 
                FxManager.PlayFx(newTarget.transform, FxType.Heal);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(AudioActionType.Heal);
        }
    }
}