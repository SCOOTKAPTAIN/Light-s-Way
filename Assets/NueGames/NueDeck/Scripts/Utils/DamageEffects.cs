using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Utils
{
    public static class DamageEffects
    {
        /// <summary>
        /// Applies fragile multiplier to the base damage and applies the attacker's Pursuit effect
        /// (spawn FX/static text and play audio) using the same logic as current per-action code.
        /// Returns the adjusted damage value (after fragile multiplier) so callers can continue their flow.
        /// </summary>
        public static float ApplyFragileAndPursuit(CharacterBase target, CharacterBase attacker, float baseValue)
        {
            if (target == null) return baseValue;

            var fragileStacks = target.CharacterStats.StatusDict[StatusType.Fragile].StatusValue;
            float multiplier = 1f + (0.05f * fragileStacks);

            float adjustedValue = baseValue;
            if (fragileStacks > 0)
            {
                adjustedValue = Mathf.RoundToInt(baseValue * multiplier);
            }

            if (attacker != null && attacker.CharacterStats.StatusDict[StatusType.Pursuit].StatusValue > 0)
            {
                int pursuitStacks = attacker.CharacterStats.StatusDict[StatusType.Pursuit].StatusValue;
                int pursuitValue = Mathf.RoundToInt(pursuitStacks * multiplier);
                if (pursuitValue > 0)
                {
                    // Apply pursuit damage with yellow text (passed via damageTextColor parameter)
                    target.CharacterStats.Damage(Mathf.RoundToInt(pursuitValue), false, "yellow");

                    // FX / audio
                    if (FxManager.Instance != null)
                    {
                        FxManager.Instance.PlayFx(target.transform, FxType.Pursuit);
                    }
                    if (AudioManager.Instance != null)
                    {
                        AudioManager.Instance.PlayOneShot(AudioActionType.Pursuit);
                    }
                }
            }

            return adjustedValue;
        }
    }
}
