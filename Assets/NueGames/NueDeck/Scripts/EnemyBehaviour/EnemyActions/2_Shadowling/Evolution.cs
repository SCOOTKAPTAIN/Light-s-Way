using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Evolution : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Evolution;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return Mathf.RoundToInt(ApplyLightMultiplier(baseValue, actionData));
        }
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;
            
            var statValue = CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, newTarget, actionParameters.ActionData);
            
            newTarget.CharacterStats.ApplyStatus(StatusType.Strength,
                statValue);
                    newTarget.CharacterStats.ApplyStatus(StatusType.Fortitude,
                statValue);
            
            PlayActionFx(actionParameters, newTarget.transform, FxType.Evolution);
            PlayActionAudio(actionParameters, AudioActionType.Evolution);
        }
    }
}