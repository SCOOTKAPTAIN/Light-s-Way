using System;
using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.NueExtentions;
using UnityEngine;
using Random = UnityEngine.Random;

namespace NueGames.NueDeck.Scripts.Data.Characters
{
    /// <summary>
    /// Act-specific configuration for an enemy.
    /// Allows one enemy ScriptableObject to scale across multiple acts.
    /// </summary>
    [Serializable]
    public class ActSpecificData
    {
        [Header("Act Configuration")]
        [Tooltip("Which act this configuration applies to (1, 2, 3, 4, etc.)")]
        [SerializeField] private int actNumber;
        
        [Header("Stats")]
        [SerializeField] private int maxHealth;
        
        [Header("Starting Statuses")]
        [SerializeField] private List<StartingStatusData> startingStatuses = new List<StartingStatusData>();
        
        [Header("Abilities")]
        [SerializeField] private List<EnemyAbilityData> enemyAbilityList = new List<EnemyAbilityData>();
        
        public int ActNumber => actNumber;
        public int MaxHealth => maxHealth;
        public List<StartingStatusData> StartingStatuses => startingStatuses;
        public List<EnemyAbilityData> EnemyAbilityList => enemyAbilityList;
    }
    [CreateAssetMenu(fileName = "Enemy Character Data",menuName = "NueDeck/Characters/Enemy",order = 1)]
    public class EnemyCharacterData : CharacterDataBase
    {
        [Header("Enemy Defaults")] 
        [SerializeField] private EnemyBase enemyPrefab;
        
        [Header("Mutation System")]
        [Tooltip("Mutated version of this enemy that can spawn at low Light levels. Leave empty if no mutation exists.")]
        [SerializeField] private EnemyCharacterData mutatedVersion;
        
        public EnemyCharacterData MutatedVersion => mutatedVersion;
        
        [Header("Act-Based Scaling")]
        [Tooltip("Enable to use act-specific configurations. When enabled, parameters below are FALLBACK values if act data is missing.")]
        [SerializeField] private bool useActBasedScaling;
        
        [Tooltip("Act-specific configurations. Add one entry per act with different stats/statuses/abilities.")]
        [SerializeField] private List<ActSpecificData> actConfigurations = new List<ActSpecificData>();
        
        [Header("Ability Selection Mode")]
        [Tooltip("If enabled, abilities execute in sequential order (1→2→3→1...). Overrides weighted selection.")]
        [SerializeField] private bool followAbilityPattern;
        
        [Tooltip("If enabled (and pattern mode is OFF), abilities are selected based on their weight values.")]
        [SerializeField] private bool useWeightedSelection = true;
        
        [Tooltip("If enabled (and weighted selection is ON), prevents the same ability from being used twice in a row.")]
        [SerializeField] private bool preventRepeatAbility = true;
        
        [Header("Starting Statuses")]
        [Tooltip("Statuses that this enemy starts with at the beginning of combat.")]
        [SerializeField] private List<StartingStatusData> startingStatuses = new List<StartingStatusData>();
        
        [SerializeField] private List<EnemyAbilityData> enemyAbilityList;
        public List<EnemyAbilityData> EnemyAbilityList => enemyAbilityList;
        public List<StartingStatusData> StartingStatuses => startingStatuses;

        public EnemyBase EnemyPrefab => enemyPrefab;
        public bool UseActBasedScaling => useActBasedScaling;
        
        /// <summary>
        /// Gets act-specific data for the current act.
        /// Returns null if act-based scaling is disabled or no matching act configuration found.
        /// </summary>
        public ActSpecificData GetActData(int currentAct)
        {
            if (!useActBasedScaling || actConfigurations == null || actConfigurations.Count == 0)
                return null;
            
            return actConfigurations.FirstOrDefault(config => config.ActNumber == currentAct);
        }
        
        /// <summary>
        /// Gets the max health for a specific act.
        /// Falls back to base maxHealth if no act-specific data exists.
        /// </summary>
        public int GetMaxHealth(int currentAct)
        {
            var actData = GetActData(currentAct);
            return actData != null ? actData.MaxHealth : maxHealth;
        }
        
        /// <summary>
        /// Gets the starting statuses for a specific act.
        /// Falls back to base startingStatuses if no act-specific data exists.
        /// </summary>
        public List<StartingStatusData> GetStartingStatuses(int currentAct)
        {
            var actData = GetActData(currentAct);
            return actData != null ? actData.StartingStatuses : startingStatuses;
        }
        
        /// <summary>
        /// Gets the ability list for a specific act.
        /// Falls back to base enemyAbilityList if no act-specific data exists.
        /// </summary>
        public List<EnemyAbilityData> GetEnemyAbilityList(int currentAct)
        {
            var actData = GetActData(currentAct);
            return actData != null ? actData.EnemyAbilityList : enemyAbilityList;
        }

        public EnemyAbilityData GetAbility()
        {
            return GetAbility(null, 0);
        }
        
        public EnemyAbilityData GetAbility(int usedAbilityCount)
        {
            return GetAbility(null, usedAbilityCount);
        }
        
        /// <summary>
        /// Gets the next ability for this enemy.
        /// </summary>
        /// <param name="lastUsedAbility">The last ability this specific enemy used (per-instance tracking)</param>
        /// <param name="usedAbilityCount">Total count of abilities used by this enemy</param>
        public EnemyAbilityData GetAbility(EnemyAbilityData lastUsedAbility, int usedAbilityCount)
        {
            if (followAbilityPattern)
            {
                var index = usedAbilityCount % EnemyAbilityList.Count;
                return EnemyAbilityList[index];
            }

            if (useWeightedSelection)
                return GetWeightedAbility(lastUsedAbility);
            
            return EnemyAbilityList.RandomItem();
        }
        
        /// <summary>
        /// Selects an ability based on weighted probabilities.
        /// Optionally prevents repeating the same ability twice in a row.
        /// </summary>
        private EnemyAbilityData GetWeightedAbility(EnemyAbilityData lastUsedAbility)
        {
            // Filter out the last used ability if repeat prevention is enabled
            var availableAbilities = preventRepeatAbility && lastUsedAbility != null && EnemyAbilityList.Count > 1
                ? EnemyAbilityList.Where(a => a != lastUsedAbility).ToList()
                : EnemyAbilityList;
            
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
            
            // Fallback (should never reach here)
            return availableAbilities[availableAbilities.Count - 1];
        }
    }
    
    [Serializable]
    public class EnemyAbilityData
    {
        [Header("Settings")]
        [SerializeField] private string name;
        [SerializeField] private EnemyIntentionData intention;
        [SerializeField] private bool hideActionValue;
        [SerializeField] private List<EnemyActionData> actionList;
        
        [Header("Repeat Actions")]
        [Tooltip("If > 1, displays intention as 'valueXrepeat' (e.g., 5x3 for 5 damage repeated 3 times). Default is 1 (no multiplier shown).")]
        [SerializeField] private int repeatCount = 1;
        
        [Header("Conditional Activation")]
        [Tooltip("If conditions are set, this ability only becomes available (enters the weighted pool) when ALL conditions are met. Leave empty for always-available abilities.")]
        [SerializeField] private List<AbilityCondition> conditions = new List<AbilityCondition>();
        
        [Header("Weighted Selection")]
        [Tooltip("Higher weight = higher chance to be selected. Default is 1 (equal probability).")]
        [SerializeField] private float weight = 1f;
        
        public string Name => name;
        public EnemyIntentionData Intention => intention;
        public List<EnemyActionData> ActionList => actionList;
        public bool HideActionValue => hideActionValue;
        public int RepeatCount => repeatCount;
        public List<AbilityCondition> Conditions => conditions;
        public float Weight => weight;
    }
    
    [Serializable]
    public class AbilityCondition
    {
        public enum ConditionTarget
        {
            Self,           // This enemy
            Player,         // Main ally (player character)
            AnyEnemy,       // Any enemy (including self)
            AnyAlly,        // Any ally (player side)
            AllEnemies,     // All enemies must meet condition
            AllAllies       // All allies must meet condition
        }
        
        public enum ConditionType
        {
            HasStatus,      // Target has a specific status (any amount)
            LacksStatus,    // Target does NOT have a specific status
            HasDebuff,      // Target has any debuff
            HasBuff,        // Target has any buff
            HealthBelow,    // Target health below threshold (%)
            HealthAbove,    // Target health above threshold (%)
            StatusAbove,    // Specific status stacks above threshold
            StatusBelow     // Specific status stacks below threshold
        }
        
        [Tooltip("Who to check the condition on")]
        public ConditionTarget target = ConditionTarget.Self;
        
        [Tooltip("What type of condition to check")]
        public ConditionType conditionType = ConditionType.HasStatus;
        
        [Tooltip("For HasStatus/LacksStatus/StatusAbove/StatusBelow: which status to check")]
        public StatusType specificStatus = StatusType.None;
        
        [Tooltip("For HealthBelow/HealthAbove: percentage threshold (0-100). For StatusAbove/StatusBelow: stack count threshold.")]
        public int threshold = 50;
    }
    
    [Serializable]
    public class EnemyActionData
    {
        [SerializeField] private EnemyActionType actionType;
        [SerializeField] private int minActionValue;
        [SerializeField] private int maxActionValue;
        
        [Header("Target Restrictions")]
        [Tooltip("NoRestriction = Random single target (self or ally). SelfOnly = Only self. AlliesOnly = Random ally (not self). AllAllies = All allies including self (AOE).")]
        [SerializeField] private EnemyActionTargetType targetRestriction = EnemyActionTargetType.NoRestriction;
        
        [Header("Light Scaling")]
        [Tooltip("If true, this action's value scales with Light level (darker = stronger). Use for combat values like damage/block/heal. Set false for status applications like poison stacks.")]
        [SerializeField] private bool applyLightMultiplier = true;
        
        // Cache the rolled value so it stays consistent
        private int _cachedActionValue = -1;
        
        public EnemyActionType ActionType => actionType;
        public EnemyActionTargetType TargetRestriction => targetRestriction;
        public bool ApplyLightMultiplier => applyLightMultiplier;
        public int ActionValue
        {
            get
            {
                // Roll the value once and cache it for the entire ability execution
                if (_cachedActionValue == -1)
                {
                    _cachedActionValue = Random.Range(minActionValue, maxActionValue + 1);
                }
                return _cachedActionValue;
            }
        }
        
        /// <summary>
        /// Resets the cached action value for the next ability cycle.
        /// Call this when a new ability is queued.
        /// </summary>
        public void ResetCachedValue()
        {
            _cachedActionValue = -1;
        }
    }
    
    [Serializable]
    public class StartingStatusData
    {
        [SerializeField] private StatusType statusType;
        [SerializeField] private int statusValue;
        
        public StatusType StatusType => statusType;
        public int StatusValue => statusValue;
    }
    
    
    
}