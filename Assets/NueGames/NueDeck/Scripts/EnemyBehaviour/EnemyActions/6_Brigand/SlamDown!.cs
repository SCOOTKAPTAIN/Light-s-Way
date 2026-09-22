using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class SlamDown: EnemyActionBase
    {
        public override EnemyActionType ActionType => EnemyActionType.SlamDown;

        public override int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            var blockValue = selfCharacter.CharacterStats.StatusDict[StatusType.Block].StatusValue;
            return CalculateDamageValue(blockValue, selfCharacter, targetCharacter, actionData);
        }
        
        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            
            var value = CalculateValue(actionParameters.Value, actionParameters.SelfCharacter, actionParameters.TargetCharacter, actionParameters.ActionData);
                                 
            actionParameters.TargetCharacter.CharacterStats.Damage(value, false, "red", actionParameters.SelfCharacter);

                        PlayActionFxAtPosition(actionParameters, actionParameters.TargetCharacter.transform.position, FxType.SlamDown);
                        PlayActionAudio(actionParameters, AudioActionType.SlamDown);
           //jj//
        }
    }
}