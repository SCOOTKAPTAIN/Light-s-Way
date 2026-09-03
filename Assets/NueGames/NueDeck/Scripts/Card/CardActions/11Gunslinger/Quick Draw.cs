using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class QuickDraw: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.QuickDraw;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var proficiency = GameManager.PersistentGameplayData.proficiency;
            var cardValue = actionParameters.Value;
            var strengthValue = selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;
            
            var value = proficiency + cardValue + strengthValue;
            
            Debug.Log($"[QuickDraw] Proficiency: {proficiency} + CardValue: {cardValue} + Strength: {strengthValue} = {value}");

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));
            
            Debug.Log($"[QuickDraw] After ApplyFragileAndPursuit: {value}");

                FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.QuickDraw);
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
                targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);
            
        }
    }
}