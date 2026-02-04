using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.EnemyBehaviour;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Interfaces;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.NueExtentions;
using System.Linq;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Characters
{
    public class EnemyBase : CharacterBase, IEnemy
    {
        [Header("Enemy Base References")]
        [SerializeField] protected EnemyCharacterData enemyCharacterData;
        [SerializeField] protected EnemyCanvas enemyCanvas;
        [SerializeField] protected SoundProfileData deathSoundProfileData;
        [SerializeField] protected SpriteRenderer spriteRenderer;
        protected EnemyAbilityData NextAbility;
        
        // Track which act this enemy was spawned in for act-based scaling
        private int _currentAct;
        
        public EnemyCharacterData EnemyCharacterData => enemyCharacterData;
        public EnemyCanvas EnemyCanvas => enemyCanvas;
        public SoundProfileData DeathSoundProfileData => deathSoundProfileData;

        #region Setup
        
        /// <summary>
        /// Sets the current act for this enemy instance.
        /// Must be called BEFORE BuildCharacter() to apply act-based scaling.
        /// </summary>
        public void SetCurrentAct(int actNumber)
        {
            _currentAct = actNumber;
        }
        
        public override void BuildCharacter()
        {
            base.BuildCharacter();
            EnemyCanvas.InitCanvas();
            
            // Use act-specific max health if act-based scaling is enabled
            int maxHealth = EnemyCharacterData.GetMaxHealth(_currentAct);
            
            // Apply Light-based health multiplier (cached at combat start)
            float lightMultiplier = CombatManager.CombatLightMultiplier;
            maxHealth = Mathf.RoundToInt(maxHealth * lightMultiplier);
            Debug.Log($"[Light Health Buff] Base: {EnemyCharacterData.GetMaxHealth(_currentAct)}, Multiplier: {lightMultiplier}x, Final: {maxHealth}");
            
            CharacterStats = new CharacterStats(maxHealth, EnemyCanvas);
            CharacterStats.OnDeath += OnDeath;
            CharacterStats.SetCurrentHealth(CharacterStats.CurrentHealth);
            
            // Apply starting statuses using act-specific data
            var startingStatuses = EnemyCharacterData.GetStartingStatuses(_currentAct);
            if (startingStatuses != null && startingStatuses.Count > 0)
            {
                foreach (var startingStatus in startingStatuses)
                {
                    if (startingStatus.StatusValue > 0)
                    {
                        CharacterStats.ApplyStatus(startingStatus.StatusType, startingStatus.StatusValue);
                    }
                }
            }
            
            CombatManager.OnAllyTurnStarted += ShowNextAbility;
            CombatManager.OnEnemyTurnStarted += CharacterStats.TriggerAllStatus;
            
            // Subscribe to player status changes to update intention value
            if (CombatManager.CurrentMainAlly != null)
            {
                CombatManager.CurrentMainAlly.CharacterStats.OnStatusChangedPublic += OnPlayerStatusChanged;
            }
            // Also subscribe to own status changes that affect damage (Strength, Weakness)
            CharacterStats.OnStatusChangedPublic += OnEnemyStatusChanged;
        }
        protected override void OnDeath()
        {
            base.OnDeath();
            CombatManager.OnAllyTurnStarted -= ShowNextAbility;
            CombatManager.OnEnemyTurnStarted -= CharacterStats.TriggerAllStatus;
            
            // Unsubscribe from status change events
            if (CombatManager.CurrentMainAlly != null)
            {
                CombatManager.CurrentMainAlly.CharacterStats.OnStatusChangedPublic -= OnPlayerStatusChanged;
            }
            CharacterStats.OnStatusChangedPublic -= OnEnemyStatusChanged;
           
            CombatManager.OnEnemyDeath(this);
            AudioManager.PlayOneShot(DeathSoundProfileData.GetRandomClip());
            
            // Start death fade animation
            StartCoroutine(DeathFadeRoutine());
        }
        
        private IEnumerator DeathFadeRoutine()
        {
            // Find sprite renderer if not assigned
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
            
            if (spriteRenderer != null)
            {
                // Turn red
                Color originalColor = spriteRenderer.color;
                spriteRenderer.color = Color.red;
                
                // Fade out over 0.5 seconds
                float fadeTime = 0.5f;
                float timer = 0f;
                
                while (timer < fadeTime)
                {
                    timer += Time.deltaTime;
                    float alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);
                    spriteRenderer.color = new Color(1f, 0f, 0f, alpha);
                    yield return null;
                }
            }
            
            Destroy(gameObject);
        }
        #endregion
        
        #region Private Methods

        private int _usedAbilityCount;
        private EnemyAbilityData _lastUsedAbility; // Per-instance tracking
        
        private void ShowNextAbility()
        {
            // Get act-specific ability list
            var abilityList = EnemyCharacterData.GetEnemyAbilityList(_currentAct);
            
            // Safety check: if no abilities available, don't proceed
            if (abilityList == null || abilityList.Count == 0)
            {
                Debug.LogWarning($"Enemy '{name}' has no abilities configured for Act {_currentAct}!");
                return;
            }
            
            // Pass the last used ability to prevent repeats (per-instance)
            NextAbility = GetActSpecificAbility(abilityList, _lastUsedAbility, _usedAbilityCount);
            _lastUsedAbility = NextAbility; // Update last used ability for this instance
            
            // Reset cached action values for all actions in this ability
            foreach (var action in NextAbility.ActionList)
            {
                action.ResetCachedValue();
            }
            
            EnemyCanvas.IntentImage.sprite = NextAbility.Intention.IntentionSprite;
            
            if (NextAbility.HideActionValue)
            {
                EnemyCanvas.NextActionValueText.gameObject.SetActive(false);
            }
            else
            {
                EnemyCanvas.NextActionValueText.gameObject.SetActive(true);
                // Calculate displayed value using action data (checks ApplyLightMultiplier flag)
                int displayedValue = CalculateDisplayedValue(NextAbility.ActionList[0].ActionValue, NextAbility.ActionList[0]);
                
                // Show repeat multiplier if repeatCount > 1
                if (NextAbility.RepeatCount > 1)
                {
                    EnemyCanvas.NextActionValueText.text = $"{displayedValue}x{NextAbility.RepeatCount}";
                }
                else
                {
                    EnemyCanvas.NextActionValueText.text = displayedValue.ToString();
                }
            }

            _usedAbilityCount++;
            EnemyCanvas.IntentImage.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Gets the next ability using act-specific ability list.
        /// Respects the enemy's ability selection settings (pattern, weighted, etc.).
        /// </summary>
        private EnemyAbilityData GetActSpecificAbility(List<EnemyAbilityData> abilityList, EnemyAbilityData lastUsedAbility, int usedAbilityCount)
        {
            if (EnemyCharacterData.UseActBasedScaling)
            {
                // When using act-based scaling, respect the enemy's ability selection settings
                // Check if pattern mode is enabled (use original GetAbility logic)
                var characterData = EnemyCharacterData as EnemyCharacterData;
                
                // Use reflection to access private fields (followAbilityPattern, useWeightedSelection, preventRepeatAbility)
                var followPattern = (bool)typeof(EnemyCharacterData)
                    .GetField("followAbilityPattern", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(characterData);
                var useWeighted = (bool)typeof(EnemyCharacterData)
                    .GetField("useWeightedSelection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(characterData);
                var preventRepeat = (bool)typeof(EnemyCharacterData)
                    .GetField("preventRepeatAbility", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    ?.GetValue(characterData);
                
                // Pattern mode: cycle through abilities sequentially (filtered by conditions)
                if (followPattern)
                {
                    // Filter abilities by conditions
                    var patternAbilities = abilityList.Where(a => AreConditionsMet(a)).ToList();
                    
                    // If no abilities meet conditions, use all abilities as fallback
                    if (patternAbilities.Count == 0)
                        patternAbilities = abilityList;
                    
                    var index = usedAbilityCount % patternAbilities.Count;
                    return patternAbilities[index];
                }
                
                // Weighted selection mode
                if (useWeighted)
                {
                    return GetWeightedAbilityFromList(abilityList, lastUsedAbility, preventRepeat);
                }
                
                // Random selection (no pattern, no weights) - filtered by conditions
                var randomAbilities = abilityList.Where(a => AreConditionsMet(a)).ToList();
                
                // If no abilities meet conditions, use all abilities as fallback
                if (randomAbilities.Count == 0)
                    randomAbilities = abilityList;
                
                return randomAbilities[Random.Range(0, randomAbilities.Count)];
            }
            
            // Fallback to original method if not using act-based scaling
            return EnemyCharacterData.GetAbility(lastUsedAbility, usedAbilityCount);
        }
        
        /// <summary>
        /// Weighted ability selection from a specific ability list.
        /// </summary>
        private EnemyAbilityData GetWeightedAbilityFromList(List<EnemyAbilityData> abilityList, EnemyAbilityData lastUsedAbility, bool preventRepeat)
        {
            // Filter out the last used ability ONLY if preventRepeat is enabled
            var availableAbilities = preventRepeat && lastUsedAbility != null && abilityList.Count > 1
                ? abilityList.Where(a => a != lastUsedAbility).ToList()
                : new List<EnemyAbilityData>(abilityList);
            
            // Further filter by conditions - only include abilities whose conditions are ALL met
            availableAbilities = availableAbilities
                .Where(ability => AreConditionsMet(ability))
                .ToList();
            
            // If no abilities meet their conditions, fallback to all available (ignoring conditions)
            if (availableAbilities.Count == 0)
            {
                availableAbilities = preventRepeat && lastUsedAbility != null && abilityList.Count > 1
                    ? abilityList.Where(a => a != lastUsedAbility).ToList()
                    : new List<EnemyAbilityData>(abilityList);
            }
            
            // Safety check: if still empty, just return first ability
            if (availableAbilities.Count == 0)
            {
                Debug.LogWarning($"Enemy '{name}' has no available abilities after filtering. Returning first ability from original list.");
                return abilityList[0];
            }
            
            // Calculate total weight
            float totalWeight = availableAbilities.Sum(a => a.Weight);
            
            if (totalWeight <= 0)
            {
                // Fallback to random if all weights are 0
                return availableAbilities[Random.Range(0, availableAbilities.Count)];
            }
            
            // Roll a random value between 0 and total weight
            float roll = Random.Range(0f, totalWeight);
            float currentWeight = 0f;
            
            // Find which ability the roll landed on
            foreach (var ability in availableAbilities)
            {
                currentWeight += ability.Weight;
                if (roll < currentWeight)
                {
                    return ability;
                }
            }
            
            // Fallback
            return availableAbilities[availableAbilities.Count - 1];
        }
        
        /// <summary>
        /// Checks if all conditions for an ability are met.
        /// Returns true if no conditions are set (always available).
        /// </summary>
        private bool AreConditionsMet(EnemyAbilityData ability)
        {
            if (ability.Conditions == null || ability.Conditions.Count == 0)
                return true; // No conditions = always available
            
            // ALL conditions must be met
            foreach (var condition in ability.Conditions)
            {
                if (!IsConditionMet(condition))
                    return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Evaluates a single condition.
        /// </summary>
        private bool IsConditionMet(AbilityCondition condition)
        {
            var combatManager = CombatManager.Instance;
            if (combatManager == null) return false;
            
            List<CharacterBase> targets = GetConditionTargets(condition.target, combatManager);
            if (targets.Count == 0) return false;
            
            // For "All" conditions, ALL targets must meet the condition
            bool requireAll = condition.target == AbilityCondition.ConditionTarget.AllEnemies ||
                              condition.target == AbilityCondition.ConditionTarget.AllAllies;
            
            if (requireAll)
            {
                return targets.All(target => EvaluateConditionOnTarget(condition, target));
            }
            else
            {
                // For "Any" conditions, at least ONE target must meet the condition
                return targets.Any(target => EvaluateConditionOnTarget(condition, target));
            }
        }
        
        /// <summary>
        /// Gets the list of targets based on the condition target type.
        /// </summary>
        private List<CharacterBase> GetConditionTargets(AbilityCondition.ConditionTarget targetType, CombatManager combatManager)
        {
            var targets = new List<CharacterBase>();
            
            switch (targetType)
            {
                case AbilityCondition.ConditionTarget.Self:
                    targets.Add(this);
                    break;
                    
                case AbilityCondition.ConditionTarget.Player:
                    if (combatManager.CurrentMainAlly != null)
                        targets.Add(combatManager.CurrentMainAlly);
                    break;
                    
                case AbilityCondition.ConditionTarget.AnyEnemy:
                case AbilityCondition.ConditionTarget.AllEnemies:
                    targets.AddRange(combatManager.CurrentEnemiesList.Where(e => e != null && !e.CharacterStats.IsDeath));
                    break;
                    
                case AbilityCondition.ConditionTarget.AnyAlly:
                case AbilityCondition.ConditionTarget.AllAllies:
                    targets.AddRange(combatManager.CurrentAlliesList.Where(a => a != null && !a.CharacterStats.IsDeath));
                    break;
            }
            
            return targets;
        }
        
        /// <summary>
        /// Evaluates a condition on a specific target character.
        /// </summary>
        private bool EvaluateConditionOnTarget(AbilityCondition condition, CharacterBase target)
        {
            if (target == null || target.CharacterStats == null) return false;
            
            var stats = target.CharacterStats;
            
            switch (condition.conditionType)
            {
                case AbilityCondition.ConditionType.HasStatus:
                    return stats.StatusDict.ContainsKey(condition.specificStatus) && 
                           stats.StatusDict[condition.specificStatus].StatusValue > 0;
                
                case AbilityCondition.ConditionType.LacksStatus:
                    return !stats.StatusDict.ContainsKey(condition.specificStatus) || 
                           stats.StatusDict[condition.specificStatus].StatusValue <= 0;
                
                case AbilityCondition.ConditionType.HasDebuff:
                    return stats.StatusDict.Any(kvp => 
                        System.Array.Exists(CharacterStats.DebuffTypes, debuff => debuff == kvp.Key) && 
                        kvp.Value.StatusValue > 0);
                
                case AbilityCondition.ConditionType.HasBuff:
                    return stats.StatusDict.Any(kvp => 
                        !System.Array.Exists(CharacterStats.DebuffTypes, debuff => debuff == kvp.Key) && 
                        kvp.Value.StatusValue > 0 && 
                        kvp.Key != StatusType.None);
                
                case AbilityCondition.ConditionType.HealthBelow:
                    float healthPercentBelow = (stats.CurrentHealth / (float)stats.MaxHealth) * 100f;
                    return healthPercentBelow < condition.threshold;
                
                case AbilityCondition.ConditionType.HealthAbove:
                    float healthPercentAbove = (stats.CurrentHealth / (float)stats.MaxHealth) * 100f;
                    return healthPercentAbove > condition.threshold;
                
                case AbilityCondition.ConditionType.StatusAbove:
                    if (!stats.StatusDict.ContainsKey(condition.specificStatus))
                        return false; // Status doesn't exist = not above threshold
                    return stats.StatusDict[condition.specificStatus].StatusValue > condition.threshold;
                
                case AbilityCondition.ConditionType.StatusBelow:
                    if (!stats.StatusDict.ContainsKey(condition.specificStatus))
                        return true; // Status doesn't exist (0 stacks) = below any positive threshold
                    return stats.StatusDict[condition.specificStatus].StatusValue < condition.threshold;
                
                default:
                    return false;
            }
        }
        
        /// <summary>
        /// Calculates the value that will be displayed in the intention text.
        /// Checks the action's ApplyLightMultiplier flag to determine if Light scaling applies.
        /// Attack-type actions also apply Strength + Fragile + Weak + Pursuit modifiers.
        /// </summary>
        private int CalculateDisplayedValue(int baseValue, EnemyActionData actionData)
        {
            var combatManager = CombatManager.Instance;
            if (combatManager == null)
                return baseValue;
            
            float value = baseValue;
            
            // Apply Light multiplier if the action has it enabled (uses cached value from combat start)
            if (actionData.ApplyLightMultiplier)
            {
                value *= CombatManager.CombatLightMultiplier;
            }
            
            // Attack actions get additional modifiers (Strength, Fragile, Weak, Pursuit)
            if (actionData.ActionType == EnemyActionType.Attack)
            {
                if (combatManager.CurrentMainAlly == null)
                    return Mathf.RoundToInt(value);
                    
                var targetCharacter = combatManager.CurrentMainAlly;
                
                // Add enemy's Strength
                value += CharacterStats.StatusDict[StatusType.Strength].StatusValue;
                
                // Apply Fragile, Weak, Pursuit, Slimed modifiers
                value = NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, this, value);
            }
            // Block actions get Fortitude bonus
            else if (actionData.ActionType == EnemyActionType.Block)
            {
                value += CharacterStats.StatusDict[StatusType.Fortitude].StatusValue;
            }
            
            return Mathf.RoundToInt(value);
        }
        
        /// <summary>
        /// Updates the intention damage value when player statuses change (Fragile, Pursuit, etc).
        /// </summary>
        private void OnPlayerStatusChanged(StatusType statusType, int value)
        {
            // Only update if it's a status that affects damage calculation
            if (statusType == StatusType.Fragile || statusType == StatusType.Pursuit)
            {
                UpdateIntentionValue();
            }
        }
        
        /// <summary>
        /// Updates the intention damage value when enemy statuses change (Strength, Weakness, etc).
        /// </summary>
        private void OnEnemyStatusChanged(StatusType statusType, int value)
        {
            // Only update if it's a status that affects damage calculation
            if (statusType == StatusType.Strength || statusType == StatusType.Weak)
            {
                UpdateIntentionValue();
            }
        }
        
        /// <summary>
        /// Updates the displayed intention damage value in real-time.
        /// </summary>
        private void UpdateIntentionValue()
        {
            if (NextAbility == null || NextAbility.HideActionValue)
                return;
            
            int displayedValue = CalculateDisplayedValue(NextAbility.ActionList[0].ActionValue, NextAbility.ActionList[0]);
            
            // Update intention text with repeat multiplier if needed
            if (NextAbility.RepeatCount > 1)
            {
                EnemyCanvas.NextActionValueText.text = $"{displayedValue}x{NextAbility.RepeatCount}";
            }
            else
            {
                EnemyCanvas.NextActionValueText.text = displayedValue.ToString();
            }
        }
        #endregion
        
        #region Action Routines
        public virtual IEnumerator ActionRoutine()
        {
            if (CharacterStats.IsStunned)
                yield break;

            Debug.Log($"ActionRoutine START for '{name}' with intent '{NextAbility?.Intention?.EnemyIntentionType}'");
            EnemyCanvas.IntentImage.gameObject.SetActive(false);
            if (NextAbility.Intention.EnemyIntentionType == EnemyIntentionType.Attack || NextAbility.Intention.EnemyIntentionType == EnemyIntentionType.Debuff)
            {
                yield return StartCoroutine(AttackRoutine(NextAbility));
            }
            else
            {
                yield return StartCoroutine(BuffRoutine(NextAbility));
            }
            Debug.Log($"ActionRoutine END for '{name}'");
        }
        
        protected virtual IEnumerator AttackRoutine(EnemyAbilityData targetAbility)
        {
            var waitFrame = new WaitForEndOfFrame();
            Debug.Log($"AttackRoutine START for '{name}' (ability: '{targetAbility?.Intention?.EnemyIntentionType}')");

            if (CombatManager == null) yield break;
            
            var aliveAllies = CombatManager.CurrentAlliesList.Where(a => a != null && !a.CharacterStats.IsDeath).ToList();
            if (aliveAllies.Count == 0) yield break;
            
            var target = aliveAllies.RandomItem();
            
            var startPos = transform.position;
            var directionToTarget = (target.transform.position - startPos).normalized;
            
            // Windup: catapult back
            var windupPos = startPos - directionToTarget * 0.3f;
            var startRot = transform.localRotation;
            var windupRot = Quaternion.Euler(-15, 0, 0);
            
            // Windup phase
            yield return MoveToTargetRoutine(waitFrame, startPos, windupPos, startRot, windupRot, 3f);
            
            // Fast lunge forward (short distance)
            var lungePos = startPos + directionToTarget * 0.5f;
            var lungeRot = Quaternion.Euler(30, 0, 0);
            yield return MoveToTargetRoutine(waitFrame, windupPos, lungePos, windupRot, lungeRot, 15f);
          
            // Re-evaluate target in case it died while earlier actions ran.
            if (target == null || target.CharacterStats.IsDeath)
            {
                var fallbackAllies = CombatManager.CurrentAlliesList.Where(a => a != null && !a.CharacterStats.IsDeath).ToList();
                if (fallbackAllies.Count == 0)
                {
                    // Nothing to attack; return to start position and end routine.
                    Debug.LogWarning($"{name} had no allies to attack (all dead) — skipping action.");
                    yield return MoveToTargetRoutine(waitFrame, lungePos, startPos, lungeRot, startRot, 2f);
                    yield break;
                }
                target = fallbackAllies.RandomItem();
                Debug.Log($"{name} switched attack target to '{target.name}' because original died.");
            }

            // Execute attack actions
            targetAbility.ActionList.ForEach(x => EnemyActionProcessor.GetAction(x.ActionType).DoAction(new EnemyActionParameters(x.ActionValue, target, this, x)));
            
            // Slow slide back to original position
            yield return MoveToTargetRoutine(waitFrame, lungePos, startPos, lungeRot, startRot, 2f);
            Debug.Log($"AttackRoutine END for '{name}'");
        }
        
        protected virtual IEnumerator BuffRoutine(EnemyAbilityData targetAbility)
        {
            var waitFrame = new WaitForEndOfFrame();
            
            var aliveEnemies = CombatManager.CurrentEnemiesList.Where(e => e != null && !e.CharacterStats.IsDeath).ToList();
            
            // Check if this ability has AOE actions
            bool hasAOE = targetAbility.ActionList.Any(a => a.TargetRestriction == EnemyActionTargetType.AllAllies);
            
            if (hasAOE)
            {
                // AOE ability - target all allies including self
                yield return StartCoroutine(AOEBuffRoutine(targetAbility, aliveEnemies, waitFrame));
                yield break;
            }
            
            // Single-target ability - determine valid target based on action restrictions
            CharacterBase target = GetValidBuffTarget(targetAbility, aliveEnemies);
            
            if (target == null)
            {
                // No valid target found - skip this ability
                Debug.LogWarning($"{name} could not find valid target for buff ability '{targetAbility.Name}' - skipping.");
                yield break;
            }
            
            var startPos = transform.position;
            var endPos = startPos+new Vector3(0,0.2f,0);
            
            var startRot = transform.localRotation;
            var endRot = transform.localRotation;
            
            // Run movement inline so it completes correctly even if the enemy GameObject is destroyed mid-action.
            yield return MoveToTargetRoutine(waitFrame, startPos, endPos, startRot, endRot, 5);
            
            // Re-evaluate target in case it died while earlier actions ran.
            if (target == null || target.CharacterStats.IsDeath)
            {
                target = GetValidBuffTarget(targetAbility, aliveEnemies.Where(e => e != null && !e.CharacterStats.IsDeath).ToList());
                
                if (target == null)
                {
                    Debug.LogWarning($"{name} had no valid targets after target died — skipping ability.");
                    yield return MoveToTargetRoutine(waitFrame, endPos, startPos, endRot, startRot, 5);
                    yield break;
                }
                
                Debug.Log($"{name} switched buff target to '{target.name}' because original died.");
            }

            targetAbility.ActionList.ForEach(x => EnemyActionProcessor.GetAction(x.ActionType).DoAction(new EnemyActionParameters(x.ActionValue, target, this)));
            
            yield return MoveToTargetRoutine(waitFrame, endPos, startPos, endRot, startRot, 5);
            Debug.Log($"BuffRoutine END for '{name}'");
        }
        
        /// <summary>
        /// Handles AOE buff actions that affect all allies.
        /// </summary>
        protected virtual IEnumerator AOEBuffRoutine(EnemyAbilityData targetAbility, List<EnemyBase> aliveEnemies, WaitForEndOfFrame waitFrame)
        {
            var startPos = transform.position;
            var endPos = startPos + new Vector3(0, 0.2f, 0);
            var startRot = transform.localRotation;
            var endRot = transform.localRotation;
            
            yield return MoveToTargetRoutine(waitFrame, startPos, endPos, startRot, endRot, 5);
            
            Debug.Log($"{name} performing AOE buff on {aliveEnemies.Count} allies.");
            
            // Apply actions to all alive enemies
            foreach (var ally in aliveEnemies)
            {
                if (ally == null || ally.CharacterStats.IsDeath) continue;
                
                foreach (var action in targetAbility.ActionList)
                {
                    EnemyActionProcessor.GetAction(action.ActionType).DoAction(new EnemyActionParameters(action.ActionValue, ally, this, action));
                }
            }
            
            yield return MoveToTargetRoutine(waitFrame, endPos, startPos, endRot, startRot, 5);
            Debug.Log($"AOEBuffRoutine END for '{name}'");
        }
        
        /// <summary>
        /// Gets a valid target for buff actions based on target restrictions.
        /// </summary>
        private CharacterBase GetValidBuffTarget(EnemyAbilityData targetAbility, List<EnemyBase> aliveEnemies)
        {
            // Check the most restrictive action in the ability
            bool hasSelfOnly = targetAbility.ActionList.Any(a => a.TargetRestriction == EnemyActionTargetType.SelfOnly);
            bool hasAlliesOnly = targetAbility.ActionList.Any(a => a.TargetRestriction == EnemyActionTargetType.AlliesOnly);
            
            // If has SelfOnly actions, must target self
            if (hasSelfOnly)
            {
                return this;
            }
            
            // If has AlliesOnly actions, must target allies (not self)
            if (hasAlliesOnly)
            {
                var allies = aliveEnemies.Where(e => e != this).ToList();
                if (allies.Count == 0)
                {
                    Debug.LogWarning($"{name} has AlliesOnly action but no allies available.");
                    return null;
                }
                return allies.RandomItem();
            }
            
            // NoRestriction - can target anyone (self or allies)
            if (aliveEnemies.Count == 0)
            {
                return this;
            }
            return aliveEnemies.RandomItem();
        }
        #endregion
        
        #region Other Routines
        private IEnumerator MoveToTargetRoutine(WaitForEndOfFrame waitFrame,Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot, float speed)
        {
            var timer = 0f;
            while (true)
            {
                timer += Time.deltaTime*speed;

                // Guard transform access in case the GameObject is destroyed mid-movement.
                if (this == null)
                {
                    if (timer >= 1f) break;
                    yield return waitFrame;
                    continue;
                }

                transform.position = Vector3.Lerp(startPos, endPos, timer);
                transform.localRotation = Quaternion.Lerp(startRot, endRot, timer);
                if (timer >= 1f)
                {
                    break;
                }

                yield return waitFrame;
            }
        }

        #endregion
    }
}