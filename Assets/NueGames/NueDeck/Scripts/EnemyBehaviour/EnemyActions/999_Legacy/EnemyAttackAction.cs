using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class EnemyAttackAction: EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Attack;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateDamageValue(baseValue, selfCharacter, targetCharacter, actionData);
        }
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            
            var value = CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, actionParameters.TargetCharacter, actionParameters.ActionData);
                                 
            actionParameters.TargetCharacter.CharacterStats.Damage(value, false, "red", actionParameters.SelfCharacter);

                        PlayActionFx(actionParameters, actionParameters.TargetCharacter.transform, FxType.Attack);
                        PlayActionAudio(actionParameters, AudioActionType.Attack);
           
        }
    }
}