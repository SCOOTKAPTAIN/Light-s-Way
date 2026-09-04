using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Utils
{
    public static class DamageEffects
    {
        /// <summary>
        /// Gets the Light-based damage multiplier for enemies based on current Light level.
        /// 100-80: 1.0x (no buff), 79-50: 1.1x, 49-25: 1.15x, 24-10: 1.25x, 9-1: 1.4x, 0: 1.5x
        /// </summary>
        public static float GetLightDamageMultiplier()
        {
            int light = GameManager.Instance?.PersistentGameplayData?.light ?? 100;
            
            return light switch
            {
                >= 80 and <= 100 => 1.0f,   // No buff
                >= 50 and <= 79 => 1.10f,   // +10%
                >= 25 and <= 49 => 1.15f,   // +15%
                >= 10 and <= 24 => 1.25f,   // +25%
                >= 1 and <= 9 => 1.40f,     // +40%
                0 => 1.50f,                  // +50%
                _ => 1.0f
            };
        }
        
        /// <summary>
        /// Gets the Light-based health multiplier for enemies based on current Light level.
        /// Uses same thresholds as damage multiplier.
        /// </summary>
        public static float GetLightHealthMultiplier()
        {
            return GetLightDamageMultiplier(); // Same scaling for now
        }
        
        /// <summary>
        /// Calculates mutation chance based on Light level.
        /// 100 Light = 0% chance, 0 Light = 50% chance (gradual).
        /// </summary>
        public static float GetMutationChance()
        {
            int light = GameManager.Instance?.PersistentGameplayData?.light ?? 100;
            return (100 - light) / 2f; // 0% at 100 Light, 50% at 0 Light
        }
        
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

            // Ammo Pouch: each stack adds 5% damage per card in the exhaust pile.
            if (attacker != null && attacker.CharacterStats.StatusDict.ContainsKey(StatusType.Deadstock))
            {
                var ammoPouchStacks = attacker.CharacterStats.StatusDict[StatusType.Deadstock].StatusValue;
                var exhaustCount = CollectionManager.Instance != null ? CollectionManager.Instance.ExhaustPile.Count : 0;
                var ammoPouchMultiplier = 1f + (0.05f * ammoPouchStacks * exhaustCount);
                adjustedBaseValue *= ammoPouchMultiplier;

                Debug.Log($"[DamageEffects] Ammo Pouch stacks: {ammoPouchStacks}, exhaust cards: {exhaustCount}, multiplier: {ammoPouchMultiplier}");
            }

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
            
            // Slimed: 25% damage reduction per tier (every 3 stacks), up to 100% at 12 stacks
            float slimedMultiplier = 1f;
            if (attacker != null && attacker.CharacterStats.StatusDict.ContainsKey(StatusType.Slimed))
            {
                var slimedStacks = attacker.CharacterStats.StatusDict[StatusType.Slimed].StatusValue;
                if (slimedStacks > 0)
                {
                    // Calculate tier: 3 stacks = tier 1 (25%), 6 = tier 2 (50%), 9 = tier 3 (75%), 12+ = tier 4 (100%)
                    int tier = slimedStacks / 3;
                    if (tier > 4) tier = 4; // Cap at 100% reduction
                    float reductionPercent = tier * 0.25f;
                    slimedMultiplier = 1f - reductionPercent;
                    
                    Debug.Log($"[DamageEffects] Attacker Slimed stacks: {slimedStacks}, tier: {tier}, reduction: {reductionPercent * 100}%, multiplier: {slimedMultiplier}");
                }
            }

            // Combine all multipliers and apply to adjusted base value (now including The Best Defence bonus)
            float combinedMultiplier = fragileMultiplier * weaknessMultiplier * slimedMultiplier;
            float adjustedValue = Mathf.RoundToInt(adjustedBaseValue * combinedMultiplier);
            
            Debug.Log($"[DamageEffects] Fragile×Weak×Slimed: {fragileMultiplier} × {weaknessMultiplier} × {slimedMultiplier} = {combinedMultiplier}, Adjusted: {adjustedBaseValue} × {combinedMultiplier} = {adjustedValue}");

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

        /// <summary>
        /// Applies Sabotaged effect: deals damage to attacker equal to Sabotaged value, then reduces it by 1.
        /// Call this when an enemy is about to attack (before or after dealing damage).
        /// </summary>
        public static void ApplySabotaged(CharacterBase attacker)
        {
            if (attacker == null) return;
            
            // Check if attacker has Sabotaged status
            if (attacker.CharacterStats.StatusDict.ContainsKey(StatusType.Sabotaged) && 
                attacker.CharacterStats.StatusDict[StatusType.Sabotaged].IsActive && 
                attacker.CharacterStats.StatusDict[StatusType.Sabotaged].StatusValue > 0)
            {
                var sabotageValue = attacker.CharacterStats.StatusDict[StatusType.Sabotaged].StatusValue;
                
                // Deal damage to self (no text color to avoid duplicate text)
                attacker.CharacterStats.Damage(sabotageValue, false, "", null);
                
                // Reduce Sabotaged by 1
                attacker.CharacterStats.ApplyStatus(StatusType.Sabotaged, -1);
                
                // Spawn custom floating text with label and value
                if (FxManager.Instance != null)
                    FxManager.Instance.SpawnFloatingTextOrange(attacker.TextSpawnRoot, "Sabotaged!\n" + sabotageValue.ToString());
                
                // Play audio
                if (AudioManager.Instance != null)
                    AudioManager.Instance.PlayOneShot(AudioActionType.Sabotaged);
            }
        }
    }
}
