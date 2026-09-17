using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Claw: EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Claw;
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            
            float baseValue = actionParameters.Value;
            
            // Apply Light-based damage multiplier if action has flag enabled (uses cached value from combat start)
            if (actionParameters.ActionData != null && actionParameters.ActionData.ApplyLightMultiplier)
            {
                baseValue *= CombatManager.Instance.CombatLightMultiplier;
            }
            
            var value = Mathf.RoundToInt(baseValue +
                                         actionParameters.SelfCharacter.CharacterStats.StatusDict[StatusType.Strength]
                                             .StatusValue);

            var selfCharacter = actionParameters.SelfCharacter;

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(actionParameters.TargetCharacter, selfCharacter, value));
                                 
            actionParameters.TargetCharacter.CharacterStats.Damage(value, false, "red", actionParameters.SelfCharacter);

                        PlayActionFxAtPosition(actionParameters, actionParameters.TargetCharacter.transform.position, FxType.Claw);
                        PlayActionAudio(actionParameters, AudioActionType.Claw);
           //jj//
        }
    }
}