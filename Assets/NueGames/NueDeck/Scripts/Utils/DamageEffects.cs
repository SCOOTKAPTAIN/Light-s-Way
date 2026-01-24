using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Utils
{
    public static class DamageEffects
    {
        /// <summary>
        /// Applies all damage modifiers including Fragile, Weak, Pursuit, and The Best Defence.
        /// Flow:
        /// 1. Add The Best Defence bonus (20% of attacker's block) to base damage
        /// 2. Apply Fragile multiplier (+10% per stack on target)
        /// 3. Apply Weak reduction (-30% base + -2% per stack on attacker)
        /// 4. Apply Pursuit bonus damage (stacks * combinedMultiplier)
        /// 5. Reduce attacker's block if The Best Defence triggered
        /// Returns the adjusted damage value so callers can continue their flow.
        /// </summary>
        public static float ApplyFragileAndPursuit(CharacterBase target, CharacterBase attacker, float baseValue)
        {
            if (target == null) return baseValue;

            float adjustedBaseValue = baseValue;

            // The Best Defence: Add 20% of attacker's current block to damage per stack
            // With 2 stacks: 40% bonus. With 3 stacks: 60% bonus, etc.
            if (attacker != null && attacker.CharacterStats.StatusDict.ContainsKey(StatusType.TheBestDefense) &&
                attacker.CharacterStats.StatusDict[StatusType.TheBestDefense].StatusValue > 0)
            {
                int currentBlock = attacker.CharacterStats.StatusDict[StatusType.Block].StatusValue;
                if (currentBlock > 0)
                {
                    int statusStacks = attacker.CharacterStats.StatusDict[StatusType.TheBestDefense].StatusValue;
                    int theBestDefenceBonus = Mathf.RoundToInt(currentBlock * (0.2f * statusStacks));
                    adjustedBaseValue += theBestDefenceBonus;
                    
                    Debug.Log($"[The Best Defence] Stacks: {statusStacks}, Base: {baseValue} + Bonus: {theBestDefenceBonus} (20% × {statusStacks} of {currentBlock}) = {adjustedBaseValue}");

                    // Reduce block by 10% for each stack of The Best Defence
                    int blockReduction = Mathf.RoundToInt(currentBlock * (0.1f * statusStacks));
                    if (blockReduction > 0)
                    {
                        attacker.CharacterStats.ApplyStatus(StatusType.Block, -blockReduction);
                        Debug.Log($"[The Best Defence] Reduced block by {blockReduction}. New block: {attacker.CharacterStats.StatusDict[StatusType.Block].StatusValue}");
                    }
                }
                else
                {
                    Debug.Log($"[The Best Defence] Status active but block is {currentBlock}");
                }
            }

            // Fragile: +10% damage per stack on target
            var fragileStacks = target.CharacterStats.StatusDict[StatusType.Fragile].StatusValue;
            float fragileMultiplier = 1f + (0.1f * fragileStacks);
            
            Debug.Log($"[DamageEffects] Target Fragile stacks: {fragileStacks}, multiplier: {fragileMultiplier}");

            // Weak: 30% base reduction + 2% per stack on attacker
            float weaknessMultiplier = 1f;
            if (attacker != null)
            {
                var weaknessStacks = attacker.CharacterStats.StatusDict[StatusType.Weak].StatusValue;
                // Only apply Weakness reduction if attacker actually has Weakness status active
                if (weaknessStacks > 0)
                {
                    weaknessMultiplier = 1f - (0.30f + (0.02f * weaknessStacks));
                    if (weaknessMultiplier < 0f) weaknessMultiplier = 0f; // Floor at 0 damage
                }
                
                Debug.Log($"[DamageEffects] Attacker Weakness stacks: {weaknessStacks}, multiplier: {weaknessMultiplier}");
            }

            // Combine both multipliers and apply to adjusted base value (now including The Best Defence bonus)
            float combinedMultiplier = fragileMultiplier * weaknessMultiplier;
            float adjustedValue = Mathf.RoundToInt(adjustedBaseValue * combinedMultiplier);
            
            Debug.Log($"[DamageEffects] Fragile×Weak: {fragileMultiplier} × {weaknessMultiplier} = {combinedMultiplier}, Adjusted: {adjustedBaseValue} × {combinedMultiplier} = {adjustedValue}");

            // Pursuit: Deal additional damage based on stacks and multipliers
            if (attacker != null && attacker.CharacterStats.StatusDict[StatusType.Pursuit].StatusValue > 0)
            {
                int pursuitStacks = attacker.CharacterStats.StatusDict[StatusType.Pursuit].StatusValue;
                int pursuitValue = Mathf.RoundToInt(pursuitStacks * combinedMultiplier);
                if (pursuitValue > 0)
                {
                    // Apply pursuit damage with yellow text (passed via damageTextColor parameter)
                    target.CharacterStats.Damage(Mathf.RoundToInt(pursuitValue), false, "yellow", attacker);

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
