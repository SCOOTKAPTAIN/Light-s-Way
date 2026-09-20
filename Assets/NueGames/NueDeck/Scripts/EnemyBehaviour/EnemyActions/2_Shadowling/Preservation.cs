using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Preservation : EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Preservation;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBlockValue(baseValue, selfCharacter, actionData);
        }
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;
            
            newTarget.CharacterStats.ApplyStatus(StatusType.Block,
                CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, newTarget, actionParameters.ActionData));
            
            PlayActionFx(actionParameters, newTarget.transform, FxType.Preservation);
            PlayActionAudio(actionParameters, AudioActionType.Preservation);
        }
    }
}