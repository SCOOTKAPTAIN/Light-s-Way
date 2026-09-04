using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class OneLastBullet: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.OneLastBullet;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var proficiency = GameManager.PersistentGameplayData.proficiency;
            var strengthValue = selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;
            var cardsPlayedBonus = CollectionManager.CardsPlayedThisTurn * 5;
            var value = actionParameters.Value + proficiency + strengthValue + cardsPlayedBonus - 5;
            
            Debug.Log($"[OneLastBullet] Base: {actionParameters.Value} + Proficiency: {proficiency} + Strength: {strengthValue} + Cards played bonus: {cardsPlayedBonus} = {value}");

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));
            
            Debug.Log($"[OneLastBullet] After ApplyFragileAndPursuit: {value}");

                FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.OneLastBullet);
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
                targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);
            
        }
    }
}