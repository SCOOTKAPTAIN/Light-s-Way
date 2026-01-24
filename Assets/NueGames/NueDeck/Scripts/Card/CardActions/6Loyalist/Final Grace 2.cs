using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class FinalGrace2: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.FinalGrace2;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
           

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            // Final hit: Stun chance is 50% + Pursuit value (as a percentage)
            var pursuitValue = selfCharacter.CharacterStats.StatusDict[StatusType.Pursuit].StatusValue;
            var stunChance = 0.5f + (pursuitValue * 0.01f); // 50% + (Pursuit * 1%)
            stunChance = Mathf.Clamp01(stunChance); // Cap at 100%

            // Play FX at position instead of on transform to prevent FX being destroyed when enemy dies
            FxManager.PlayFxAtPosition(targetCharacter.transform.position, FxType.FinalGrace2);
            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);

            // Roll for stun
            if (Random.value < stunChance)
            {
                targetCharacter.CharacterStats.ApplyStatus(StatusType.Stun, 1);
            }

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.FinalGrace2);
        }
    }
}