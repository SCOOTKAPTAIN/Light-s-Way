using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BlockAttackAction: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BlockDamage;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            
            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;
            
            var value = selfCharacter.CharacterStats.StatusDict[StatusType.Block].StatusValue; 
            
            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value));

            if (FxManager != null)
            {
                FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position,FxType.Attack);
                FxManager.SpawnFloatingText(actionParameters.TargetCharacter.TextSpawnRoot,value.ToString());
            }
           
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}