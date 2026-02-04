using System;
using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Data.Collection.RewardData;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.NueExtentions;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Data.Containers
{
    [CreateAssetMenu(fileName = "Encounter Data", menuName = "NueDeck/Containers/EncounterData", order = 4)]
    public class EncounterData : ScriptableObject
    {
        [Header("Settings")] 
        [SerializeField] private bool encounterRandomlyAtStage;
        [SerializeField] private List<EnemyEncounterStage> enemyEncounterList;

        public bool EncounterRandomlyAtStage => encounterRandomlyAtStage;
        public List<EnemyEncounterStage> EnemyEncounterList => enemyEncounterList;

        public EnemyEncounter GetEnemyEncounter(int stageId, Map.EncounterType encounterType, int specificIndex = -1)
        {
            var selectedStage = EnemyEncounterList.FirstOrDefault(x => x.StageId == stageId);
            if (selectedStage == null)
            {
                Debug.LogError($"No encounter stage found for StageId {stageId}");
                return null;
            }

            List<EnemyEncounter> targetList = encounterType switch
            {
                Map.EncounterType.Normal => selectedStage.NormalEncounterList,
                Map.EncounterType.Elite => selectedStage.EliteEncounterList,
                Map.EncounterType.Boss => selectedStage.BossEncounterList,
                Map.EncounterType.Special => selectedStage.SpecialEncounterList,
                _ => selectedStage.NormalEncounterList
            };

            if (targetList == null || targetList.Count == 0)
            {
                Debug.LogWarning($"No {encounterType} encounters available for Stage {stageId}");
                return null;
            }

            // Filter out defeated bosses (only for boss encounters)
            if (encounterType == Map.EncounterType.Boss)
            {
                var gameManager = GameManager.Instance;
                if (gameManager != null && gameManager.PersistentGameplayData != null)
                {
                    targetList = targetList.Where(encounter => 
                        !gameManager.PersistentGameplayData.IsBossDefeated(encounter.EncounterId)
                    ).ToList();
                    
                    if (targetList.Count == 0)
                    {
                        Debug.LogWarning($"All bosses for Stage {stageId} have been defeated this run!");
                        return null;
                    }
                    
                    Debug.Log($"Filtered boss pool: {targetList.Count} available (defeated bosses excluded)");
                }
            }

            // Return specific encounter if index is valid
            if (specificIndex >= 0 && specificIndex < targetList.Count)
            {
                return targetList[specificIndex];
            }

            // Otherwise return random
            return targetList.RandomItem();
        }
        
        // Legacy method for backwards compatibility
        [System.Obsolete("Use GetEnemyEncounter(int stageId, EncounterType encounterType) instead")]
        public EnemyEncounter GetEnemyEncounter(int stageId = 0, int encounterId = 0, bool isFinal = false)
        {
            var encounterType = isFinal ? Map.EncounterType.Boss : Map.EncounterType.Normal;
            return GetEnemyEncounter(stageId, encounterType);
        }
        
    }


    [Serializable]
    public class EnemyEncounterStage
    {
        [SerializeField] private string name;
        [SerializeField] private int stageId;
        
        [Header("Encounter Lists")]
        [SerializeField] private List<EnemyEncounter> normalEncounterList;
        [SerializeField] private List<EnemyEncounter> eliteEncounterList;
        [SerializeField] private List<EnemyEncounter> bossEncounterList;
        [SerializeField] private List<EnemyEncounter> specialEncounterList;
        
        public string Name => name;
        public int StageId => stageId;
        public List<EnemyEncounter> NormalEncounterList => normalEncounterList;
        public List<EnemyEncounter> EliteEncounterList => eliteEncounterList;
        public List<EnemyEncounter> BossEncounterList => bossEncounterList;
        public List<EnemyEncounter> SpecialEncounterList => specialEncounterList;
        
        // Legacy properties for backwards compatibility
        [System.Obsolete("Use NormalEncounterList instead")]
        public List<EnemyEncounter> EnemyEncounterList => normalEncounterList;
    }
    
    
    [Serializable]
    public class EnemyEncounter : EncounterBase
    {
        [SerializeField] private string encounterId; // Unique ID for boss tracking
        [SerializeField] private List<EnemyCharacterData> enemyList;
        
        [Header("Custom Rewards (Optional)")]
        [Tooltip("If empty, uses default rewards (1 gold, 1 card)")]
        [SerializeField] private List<GoldRewardData> customGoldRewards;
        [SerializeField] private List<CardRewardData> customCardRewards;
        
        public string EncounterId => encounterId;
        public List<EnemyCharacterData> EnemyList => enemyList;
        
        public bool HasCustomRewards => (customGoldRewards != null && customGoldRewards.Count > 0) || 
                                         (customCardRewards != null && customCardRewards.Count > 0);
        public List<GoldRewardData> CustomGoldRewards => customGoldRewards;
        public List<CardRewardData> CustomCardRewards => customCardRewards;
    }
    
    [Serializable]
    public abstract class EncounterBase
    {
        [SerializeField] private BackgroundTypes targetBackgroundType;

        public BackgroundTypes TargetBackgroundType => targetBackgroundType;
    }
}