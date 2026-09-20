using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class GooSpit: EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.GooSpit;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateDamageValue(baseValue, selfCharacter, targetCharacter, actionData);
        }
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            
            var value = CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, actionParameters.TargetCharacter, actionParameters.ActionData);
                                 
            actionParameters.TargetCharacter.CharacterStats.Damage(value, false, "red", actionParameters.SelfCharacter);
            actionParameters.TargetCharacter.CharacterStats.ApplyStatus(StatusType.Slimed, 1, actionParameters.SelfCharacter);

                        PlayActionFxAtPosition(actionParameters, actionParameters.TargetCharacter.transform.position, FxType.GooSpit);
                        PlayActionAudio(actionParameters, AudioActionType.GooSpit);
           //jj//
        }
    }
}