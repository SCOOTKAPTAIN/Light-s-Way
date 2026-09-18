using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Evolution : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Evolution;
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;
            
            float StatValue = actionParameters.Value;
            
            // Apply Light-based multiplier if action has flag enabled (uses cached value from combat start)
            if (actionParameters.ActionData != null && actionParameters.ActionData.ApplyLightMultiplier)
            {
                StatValue *= CombatManager.Instance.CombatLightMultiplier;
            }
            
            newTarget.CharacterStats.ApplyStatus(StatusType.Strength,
                Mathf.RoundToInt(StatValue));
                    newTarget.CharacterStats.ApplyStatus(StatusType.Fortitude,
                Mathf.RoundToInt(StatValue));
            
            PlayActionFx(actionParameters, newTarget.transform, FxType.Evolution);
            PlayActionAudio(actionParameters, AudioActionType.Evolution);
        }
    }
}