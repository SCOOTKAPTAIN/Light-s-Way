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
using System.Text.RegularExpressions;
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
        private bool _isChaosEnemy;
        
        // Track which act this enemy was spawned in for act-based scaling
        private int _currentAct;
        
        public EnemyCharacterData EnemyCharacterData => enemyCharacterData;
        public EnemyCanvas EnemyCanvas => enemyCanvas;
        public SoundProfileData DeathSoundProfileData => deathSoundProfileData;

        public void SetEnemyCharacterData(EnemyCharacterData characterData, bool isChaosEnemy = false)
        {
            if (characterData != null)
            {
                enemyCharacterData = characterData;
                _isChaosEnemy = isChaosEnemy;
            }
        }

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
            EnemyCanvas.SetChaosificationStatus(EnemyCharacterData.ChaosificationStatus);
            
            // Use act-specific max health if act-based scaling is enabled
            int maxHealth = EnemyCharacterData.GetMaxHealth(_currentAct);
            
            // Apply Light-based health multiplier (cached at combat start)
            float lightMultiplier = CombatManager.CombatLightMultiplier;
            maxHealth = Mathf.RoundToInt(maxHealth * lightMultiplier);
            Debug.Log($"[Light Health Buff] Base: {EnemyCharacterData.GetMaxHealth(_currentAct)}, Multiplier: {lightMultiplier}x, Final: {maxHealth}");
            
            CharacterStats = new CharacterStats(maxHealth, EnemyCanvas);
            CharacterStats.OnDeath += OnDeath;
            CharacterStats.SetCurrentHealth(CharacterStats.CurrentHealth);

            if (_isChaosEnemy || EnemyCharacterData.IsChaosEnemy)
                CharacterStats.ApplyStatus(StatusType.Chaotic, 1);
            
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
        private int _lastAbilityConsecutiveUses;
        private readonly Dictionary<EnemyActionData, int> _cachedActionValues = new Dictionary<EnemyActionData, int>();
        
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
            if (NextAbility == _lastUsedAbility)
                _lastAbilityConsecutiveUses++;
            else
                _lastAbilityConsecutiveUses = 1;

            _lastUsedAbility = NextAbility; // Update last used ability for this instance
            
            // Action values are cached per enemy instance, not on shared ScriptableObject data.
            _cachedActionValues.Clear();
            
            EnemyCanvas.IntentImage.sprite = NextAbility.Intention.IntentionSprite;
            
            if (NextAbility.HideActionValue)
            {
                EnemyCanvas.NextActionValueText.gameObject.SetActive(false);
            }
            else
            {
                EnemyCanvas.NextActionValueText.gameObject.SetActive(true);
                // Calculate displayed value using action data (checks ApplyLightMultiplier flag)
                int displayedValue = CalculateActionValue(NextAbility.ActionList[0], CombatManager.CurrentMainAlly);
                
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
                
                var followPattern = characterData.FollowAbilityPattern;
                var useWeighted = characterData.UseWeightedSelection;
                var preventRepeat = characterData.PreventRepeatAbility;
                
                // Pattern mode: cycle through abilities sequentially (filtered by conditions)
                if (followPattern)
                {
                    // Filter abilities by conditions
                    var patternAbilities = abilityList
                        .Where(a => AreConditionsMet(a) && IsAbilityTargetAvailable(a) && !IsAtConsecutiveLimit(a, lastUsedAbility))
                        .ToList();
                    
                    // If no abilities meet conditions, use all abilities as fallback
                    if (patternAbilities.Count == 0)
                        patternAbilities = abilityList.Where(IsAbilityTargetAvailable).ToList();
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
                var randomAbilities = abilityList
                    .Where(a => AreConditionsMet(a) && IsAbilityTargetAvailable(a) && !IsAtConsecutiveLimit(a, lastUsedAbility))
                    .ToList();
                
                // If no abilities meet conditions, use all abilities as fallback
                if (randomAbilities.Count == 0)
                    randomAbilities = abilityList.Where(IsAbilityTargetAvailable).ToList();
                if (randomAbilities.Count == 0)
                    randomAbilities = abilityList;
                
                return randomAbilities[Random.Range(0, randomAbilities.Count)];
            }
            
            // Fallback to original method if not using act-based scaling
            if (EnemyCharacterData.FollowAbilityPattern)
            {
                var patternAbilities = abilityList
                    .Where(a => AreConditionsMet(a) && IsAbilityTargetAvailable(a) && !IsAtConsecutiveLimit(a, lastUsedAbility))
                    .ToList();

                if (patternAbilities.Count == 0)
                    patternAbilities = abilityList.Where(IsAbilityTargetAvailable).ToList();
                if (patternAbilities.Count == 0)
                    patternAbilities = abilityList;

                return patternAbilities[usedAbilityCount % patternAbilities.Count];
            }

            if (EnemyCharacterData.UseWeightedSelection)
                return GetWeightedAbilityFromList(abilityList, lastUsedAbility, EnemyCharacterData.PreventRepeatAbility);

            var randomAbilitiesWithoutWeights = abilityList
                .Where(a => AreConditionsMet(a) && IsAbilityTargetAvailable(a) && !IsAtConsecutiveLimit(a, lastUsedAbility))
                .ToList();

            if (randomAbilitiesWithoutWeights.Count == 0)
                randomAbilitiesWithoutWeights = abilityList.Where(IsAbilityTargetAvailable).ToList();
            if (randomAbilitiesWithoutWeights.Count == 0)
                randomAbilitiesWithoutWeights = abilityList;

            return randomAbilitiesWithoutWeights[Random.Range(0, randomAbilitiesWithoutWeights.Count)];
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
                .Where(ability => AreConditionsMet(ability) && IsAbilityTargetAvailable(ability) && !IsAtConsecutiveLimit(ability, lastUsedAbility))
                .ToList();
            
            // If no abilities meet their conditions, fallback to all available (ignoring conditions)
            if (availableAbilities.Count == 0)
            {
                availableAbilities = preventRepeat && lastUsedAbility != null && abilityList.Count > 1
                    ? abilityList.Where(a => a != lastUsedAbility).ToList()
                    : new List<EnemyAbilityData>(abilityList);

                availableAbilities = availableAbilities
                    .Where(ability => IsAbilityTargetAvailable(ability) && !IsAtConsecutiveLimit(ability, lastUsedAbility))
                    .ToList();
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

        private bool IsAtConsecutiveLimit(EnemyAbilityData ability, EnemyAbilityData lastUsedAbility)
        {
            return ability == lastUsedAbility &&
                   ability.MaxConsecutiveUses > 0 &&
                   _lastAbilityConsecutiveUses >= ability.MaxConsecutiveUses;
        }

        private bool IsAbilityTargetAvailable(EnemyAbilityData ability)
        {
            if (ability == null || ability.ActionList == null)
                return false;

            if (!ability.ActionList.Any(action => action != null && action.TargetRestriction == EnemyActionTargetType.AlliesOnly))
                return true;

            if (CombatManager == null || CombatManager.CurrentEnemiesList == null)
                return true;

            return CombatManager.CurrentEnemiesList.Any(enemy =>
                enemy != null && enemy != this && !enemy.CharacterStats.IsDeath);
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

                case AbilityCondition.ConditionType.HealthAtOrBelow:
                    float healthPercentAtOrBelow = (stats.CurrentHealth / (float)stats.MaxHealth) * 100f;
                    return healthPercentAtOrBelow <= condition.threshold;
                
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
        private int CalculateActionValue(EnemyActionData actionData, CharacterBase targetCharacter)
        {
            var action = EnemyActionProcessor.GetAction(actionData.ActionType);
            return action.CalculateValue(GetActionValue(actionData), this, targetCharacter, actionData);
        }

        private int GetActionValue(EnemyActionData action)
        {
            if (!_cachedActionValues.TryGetValue(action, out var value))
            {
                value = action.RollActionValue();
                _cachedActionValues[action] = value;
            }

            return value;
        }

        public string GetNextAbilityTooltipHeader()
        {
            if (NextAbility == null || NextAbility.ActionList == null || NextAbility.ActionList.Count == 0)
                return "Enemy action";

            return string.IsNullOrWhiteSpace(NextAbility.Name)
                ? NextAbility.ActionList[0].ActionType.ToString()
                : NextAbility.Name;
        }

        public string GetNextAbilityTooltipContent()
        {
            if (NextAbility == null || NextAbility.ActionList == null || NextAbility.ActionList.Count == 0)
                return string.Empty;

            var action = NextAbility.ActionList[0];
            var displayedValue = CalculateActionValue(action, CombatManager.CurrentMainAlly);
            var actionName = action.ActionType == EnemyActionType.ApplyDebuff || action.ActionType == EnemyActionType.ApplyBuff
                ? action.StatusType.ToString()
                : action.ActionType.ToString();
            var repeatText = NextAbility.RepeatCount > 1 ? $"{NextAbility.RepeatCount} times" : "once";
            var description = NextAbility.Description;

            if (string.IsNullOrWhiteSpace(description))
            {
                description = action.ActionType switch
                {
                    EnemyActionType.Attack => "Deal {value} damage.",
                    EnemyActionType.MultiHitAttack => "Deal {value} damage {repeat}.",
                    EnemyActionType.VeilAndVerdict => "Inflict Blind and gain Ambush.",
                    EnemyActionType.Heal => "Heal {value} health.",
                    EnemyActionType.Poison => "Apply {value} Poison.",
                    EnemyActionType.ApplyDebuff => "Apply {value} {action}.",
                    EnemyActionType.ApplyBuff => "Gain {value} {action}.",
                    EnemyActionType.Block => "Gain {value} Block.",
                    _ => "Use {action} with a value of {value}."
                };
            }

            description = description
                .Replace("{value}", displayedValue.ToString())
                .Replace("{action}", actionName)
                .Replace("{repeat}", repeatText);

            for (var actionIndex = 0; actionIndex < NextAbility.ActionList.Count; actionIndex++)
            {
                var actionData = NextAbility.ActionList[actionIndex];
                if (actionData == null)
                    continue;

                var valuePlaceholder = "{value" + (actionIndex + 1) + "}";
                if (description.Contains(valuePlaceholder))
                {
                    var actionValue = CalculateActionValue(actionData, CombatManager.CurrentMainAlly);
                    description = description.Replace(valuePlaceholder, actionValue.ToString());
                }

                var actionPlaceholder = "{" + actionData.ActionType.ToString().ToLowerInvariant() + "}";
                if (description.Contains(actionPlaceholder))
                {
                    var actionValue = CalculateActionValue(actionData, CombatManager.CurrentMainAlly);
                    description = description.Replace(actionPlaceholder, actionValue.ToString());
                }
            }

            if (CombatManager.CurrentMainAlly != null)
            {
                foreach (StatusType statusType in System.Enum.GetValues(typeof(StatusType)))
                {
                    if (statusType == StatusType.None || !CombatManager.CurrentMainAlly.CharacterStats.StatusDict.ContainsKey(statusType))
                        continue;

                    var spacedStatusName = Regex.Replace(statusType.ToString(), "(?<!^)([A-Z])", " $1");
                    var statusValue = CombatManager.CurrentMainAlly.CharacterStats.StatusDict[statusType].StatusValue;
                    description = Regex.Replace(
                        description,
                        $@"\{{\s*{Regex.Escape(spacedStatusName)}\s*\}}",
                        statusValue.ToString(),
                        RegexOptions.IgnoreCase);
                }
            }

            return description;
        }

            public List<SpecialKeywords> GetNextAbilityKeywords()
            {
                return NextAbility?.Keywords ?? new List<SpecialKeywords>();
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
            UpdateIntentionValue();
        }
        
        /// <summary>
        /// Updates the displayed intention damage value in real-time.
        /// </summary>
        private void UpdateIntentionValue()
        {
            if (NextAbility == null || NextAbility.HideActionValue)
                return;
            
            int displayedValue = CalculateActionValue(NextAbility.ActionList[0], CombatManager.CurrentMainAlly);
            
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
            if (NextAbility.Intention.EnemyIntentionType == EnemyIntentionType.Attack ||
                NextAbility.Intention.EnemyIntentionType == EnemyIntentionType.Debuff ||
                NextAbility.Intention.EnemyIntentionType == EnemyIntentionType.AttackMultiHit ||
                NextAbility.Intention.EnemyIntentionType == EnemyIntentionType.AttackPierce ||
                NextAbility.Intention.EnemyIntentionType == EnemyIntentionType.KillingBlow)
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
            targetAbility.ActionList.ForEach(x => EnemyActionProcessor.GetAction(x.ActionType).DoAction(new EnemyActionParameters(GetActionValue(x), target, this, x, targetAbility.RepeatCount)));
            
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
            if (targetAbility.ActionList.Any(action => action.TargetSlot != EnemyTargetSlot.None))
            {
                yield return StartCoroutine(SlottedBuffRoutine(targetAbility, aliveEnemies, waitFrame));
                yield break;
            }

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

            targetAbility.ActionList.ForEach(x => EnemyActionProcessor.GetAction(x.ActionType).DoAction(new EnemyActionParameters(GetActionValue(x), target, this, x, targetAbility.RepeatCount)));
            
            yield return MoveToTargetRoutine(waitFrame, endPos, startPos, endRot, startRot, 5);
            Debug.Log($"BuffRoutine END for '{name}'");
        }

        private IEnumerator SlottedBuffRoutine(EnemyAbilityData targetAbility, List<EnemyBase> aliveEnemies, WaitForEndOfFrame waitFrame)
        {
            var startPos = transform.position;
            var endPos = startPos + new Vector3(0, 0.2f, 0);
            var startRot = transform.localRotation;
            var endRot = transform.localRotation;

            yield return MoveToTargetRoutine(waitFrame, startPos, endPos, startRot, endRot, 5);

            foreach (var actionData in targetAbility.ActionList)
            {
                var target = actionData.TargetSlot == EnemyTargetSlot.None
                    ? GetValidBuffTarget(targetAbility, aliveEnemies)
                    : GetTargetForSlot(aliveEnemies, actionData.TargetSlot);

                if (target == null)
                    continue;

                EnemyActionProcessor.GetAction(actionData.ActionType).DoAction(
                    new EnemyActionParameters(GetActionValue(actionData), target, this, actionData, targetAbility.RepeatCount));
            }

            yield return MoveToTargetRoutine(waitFrame, endPos, startPos, endRot, startRot, 5);
        }

        private static CharacterBase GetTargetForSlot(List<EnemyBase> aliveEnemies, EnemyTargetSlot targetSlot)
        {
            var targetStatus = targetSlot switch
            {
                EnemyTargetSlot.TargetA => StatusType.TargetA,
                EnemyTargetSlot.TargetB => StatusType.TargetB,
                EnemyTargetSlot.TargetC => StatusType.TargetC,
                _ => StatusType.None
            };

            return aliveEnemies.FirstOrDefault(enemy =>
                enemy != null && enemy.CharacterStats.StatusDict[targetStatus].IsActive);
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
                    EnemyActionProcessor.GetAction(action.ActionType).DoAction(new EnemyActionParameters(GetActionValue(action), ally, this, action, targetAbility.RepeatCount));
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