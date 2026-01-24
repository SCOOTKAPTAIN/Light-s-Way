using System.Collections;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class UnfairPlay: CardActionBase
    {
        private static readonly StatusType[] DebuffTypes = new[]
        {
            StatusType.Poison,
            StatusType.Stun,
            StatusType.Frozen,
            StatusType.Fragile,
            StatusType.Weak,
            StatusType.Bleeding,
            StatusType.Frostbite,
            StatusType.Burning,
            StatusType.NoDraw,
            StatusType.NoGainMana,
            StatusType.Judged,
            StatusType.Obscured
        };

        public override CardActionType ActionType => CardActionType.UnfairPlay;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            // Count debuffs on target
            int debuffCount = 0;
            foreach (var debuffType in DebuffTypes)
            {
                if (targetCharacter.CharacterStats.StatusDict[debuffType].StatusValue > 0)
                    debuffCount++;
            }

            // Repeat for each debuff + 1 (at least once)
            int repeatCount = debuffCount + 1;

            for (int i = 0; i < repeatCount; i++)
            {
                var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.UnfairPlay);

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);

                targetCharacter.CharacterStats.ApplyStatus(StatusType.Weak, 1);
                
            }

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}