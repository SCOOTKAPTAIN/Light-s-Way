using System.Collections;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class FullSalvo: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.FullSalvo;

        public override void DoAction(CardActionParameters actionParameters)
        {
            PerformAttacks(actionParameters);
        }

        public override IEnumerator DoActionRoutine(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) yield break;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;
            var proficiency = GameManager.PersistentGameplayData.proficiency;
            var cardValue = actionParameters.Value;
            var strengthValue = selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;
            var baseValue = proficiency + cardValue + strengthValue;
            var attackCount = 1 + (CollectionManager.HandController != null && CollectionManager.HandController.hand != null
                ? CollectionManager.HandController.hand.Count
                : 0);

            Debug.Log($"[FullSalvo] Base attack: {baseValue} ({proficiency} proficiency + {cardValue} card value + {strengthValue} strength). Attacks: {attackCount}");

            for (var attackIndex = 0; attackIndex < attackCount; attackIndex++)
            {
                if (attackIndex > 0)
                    yield return new WaitForSeconds(0.1f);

                if (!targetCharacter)
                    yield break;

                if (!DealAttack(actionParameters, targetCharacter, selfCharacter, baseValue, attackIndex, attackCount))
                    yield break;
            }
        }

        private void PerformAttacks(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var proficiency = GameManager.PersistentGameplayData.proficiency;
            var cardValue = actionParameters.Value;
            var strengthValue = selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            var baseValue = proficiency + cardValue + strengthValue;
            var attackCount = 1 + (CollectionManager.HandController != null && CollectionManager.HandController.hand != null
                ? CollectionManager.HandController.hand.Count
                : 0);

            Debug.Log($"[FullSalvo] Base attack: {baseValue} ({proficiency} proficiency + {cardValue} card value + {strengthValue} strength). Attacks: {attackCount}");

            for (var attackIndex = 0; attackIndex < attackCount; attackIndex++)
            {
                if (!DealAttack(actionParameters, targetCharacter, selfCharacter, baseValue, attackIndex, attackCount))
                    break;
            }
        }

        private bool DealAttack(CardActionParameters actionParameters, NueGames.NueDeck.Scripts.Characters.CharacterBase targetCharacter,
            NueGames.NueDeck.Scripts.Characters.CharacterBase selfCharacter, float baseValue, int attackIndex, int attackCount)
        {
            if (!targetCharacter || !selfCharacter)
                return false;

            var value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(
                targetCharacter, selfCharacter, baseValue));

            Debug.Log($"[FullSalvo] Attack {attackIndex + 1}/{attackCount}: {value} damage after modifiers");
            FxManager.PlayFxAtPosition(targetCharacter.transform.position, FxType.FullSalvo);
            AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
            targetCharacter.CharacterStats.Damage(value, false, "red", selfCharacter);
            return targetCharacter;
        }
    }
}