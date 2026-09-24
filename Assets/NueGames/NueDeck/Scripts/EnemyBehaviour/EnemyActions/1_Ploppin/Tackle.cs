using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Tackle: EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.Tackle;
        public override bool UsesDamageModifiersForPreview => true;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateDamageValue(baseValue, selfCharacter, targetCharacter, actionData);
        }
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            
            var value = CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, actionParameters.TargetCharacter, actionParameters.ActionData);
                                 
            actionParameters.TargetCharacter.CharacterStats.Damage(value, false, "red", actionParameters.SelfCharacter);

                        PlayActionFxAtPosition(actionParameters, actionParameters.TargetCharacter.transform.position, FxType.Tackle);
                        PlayActionAudio(actionParameters, AudioActionType.Tackle);
           //jj//
        }
    }
}