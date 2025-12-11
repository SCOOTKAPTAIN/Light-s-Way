using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class LeaveNoOpening1: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.LeaveNoOpening;
        public override void DoAction(CardActionParameters actionParameters)
        {
                var anchor = CombatManager.Instance.EnemiesFxAnchor;
            
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            FxManager.Instance.PlayFxAtPosition(anchor.position, FxType.LeaveNoOpening, 0.2f);

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value));

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);

                
              
        }
    }
}