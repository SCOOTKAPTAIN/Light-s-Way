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
    [CreateAssetMenu(fileName = "Enemy Character Data",menuName = "NueDeck/Characters/Enemy",order = 1)]
    public class EnemyCharacterData : CharacterDataBase
    {
        [Header("Enemy Defaults")] 
        [SerializeField] private EnemyBase enemyPrefab;
        
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
        
        [Header("Weighted Selection")]
        [Tooltip("Higher weight = higher chance to be selected. Default is 1 (equal probability).")]
        [SerializeField] private float weight = 1f;
        
        public string Name => name;
        public EnemyIntentionData Intention => intention;
        public List<EnemyActionData> ActionList => actionList;
        public bool HideActionValue => hideActionValue;
        public float Weight => weight;
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
        
        // Cache the rolled value so it stays consistent
        private int _cachedActionValue = -1;
        
        public EnemyActionType ActionType => actionType;
        public EnemyActionTargetType TargetRestriction => targetRestriction;
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