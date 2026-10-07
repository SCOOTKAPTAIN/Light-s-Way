using System;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Data.Settings
{
    [Serializable]
    public class PersistentGameplayData
    {
        private readonly GameplayData _gameplayData;
        
        [SerializeField] private int currentGold;
        [SerializeField] private int drawCount;
        [SerializeField] private int maxMana;
        [SerializeField] private int maxCardOnHand;
        [SerializeField] private int currentMana;
        [SerializeField] private int lightLoss;
        [SerializeField] private bool canUseCards;
        [SerializeField] private bool canSelectCards;
        [SerializeField] private bool isRandomHand;
        
        [SerializeField] private List<AllyBase> allyList;
        [SerializeField] private int currentStageId;
        [SerializeField] private int currentEncounterId;
        [SerializeField] private bool isFinalEncounter;
        [SerializeField] private int currentEncounterTypeIndex; // 0=Normal, 1=Elite, 2=Boss, 3=Special
        [SerializeField] private List<CardData> currentCardsList;
        [SerializeField] private List<AllyHealthData> allyHealthDataDataList;
        private EnemyEncounter selectedEncounter;

        // My Variables
        [SerializeField] private int actnumber;
        [SerializeField] public int light;
        [SerializeField] public int proficiency;
        [SerializeField] private int wisdom;
        [SerializeField] private int potency;
        [SerializeField] private int affinity;
        [SerializeField] private int metabolism;
        [SerializeField] private int vigor;
        [SerializeField] private int insight;
        [SerializeField] private int abundance;
        [SerializeField] private int capacity;
        [SerializeField] private int radiance;
        [SerializeField] private bool ActAlreadyPlayed;
        [SerializeField] private bool gainProficiencyAfterEliteOrBoss;
        [SerializeField] private bool eliteBossProficiencyRewardArmed;
        [SerializeField] private bool gainInsightAtActStart;
        [SerializeField] private bool gainLightAtActStart;

        [SerializeField] private int BonusMaxHealth;

       [SerializeField] public bool Restevent;
       
       // Boss tracking
       [SerializeField] private List<string> defeatedBossIds; // Tracks bosses defeated this run
    [SerializeField] private List<string> initialCardsToRemove;

        public PersistentGameplayData(GameplayData gameplayData)
        {
            _gameplayData = gameplayData;

            InitData();
        }
        
        public void SetAllyHealthData(string id,int newCurrentHealth, int newMaxHealth)
        {
            var data = allyHealthDataDataList.Find(x => x.CharacterId == id);
            var newData = new AllyHealthData();
            newData.CharacterId = id;
            newData.CurrentHealth = newCurrentHealth;
            newData.MaxHealth = newMaxHealth;
            if (data != null)
            {
                allyHealthDataDataList.Remove(data);
                allyHealthDataDataList.Add(newData);
            }
            else
            {
                allyHealthDataDataList.Add(newData);
            }
        }

        public void ChangeLight(int value)
        {
            
            light += value;
            if (light > 100)
            {
                light = 100;
            }
            if (light < 0)
            {
                light = 0;
            }
            UIManager.Instance.InformationCanvas.SetLightText(light);
            
            if (PostFXManager.Instance != null)
                PostFXManager.Instance.UpdateEffects(light);
        }
        public void InitData()
        {
            DrawCount = Mathf.Max(5, _gameplayData.DrawCount);
            MaxMana = _gameplayData.MaxMana;
            MaxCardOnHand = Mathf.Max(12, _gameplayData.MaxCardOnHand);
            CurrentMana = MaxMana;
            CanUseCards = true;
            CanSelectCards = true;
            IsRandomHand = _gameplayData.IsRandomHand;
            AllyList = new List<AllyBase>(_gameplayData.InitalAllyList);
            CurrentEncounterId = 0;
            SelectedEncounter = null;
            CurrentStageId = 0;
            CurrentGold = 0;
            CurrentCardsList = new List<CardData>();
            IsFinalEncounter = false;
            allyHealthDataDataList = new List<AllyHealthData>();
            proficiency = 1;
            light = 100;
            actnumber = 0;
            lightLoss = 2;
            wisdom = 0;
            potency = 0;
            affinity = 0;
            metabolism = 0;
            vigor = 0;
            insight = 0;
            abundance = 0;
            capacity = 0;
            radiance = 0;
            ActAlreadyPlayed = false;
            gainProficiencyAfterEliteOrBoss = false;
            eliteBossProficiencyRewardArmed = false;
            gainInsightAtActStart = false;
            gainLightAtActStart = false;
            BonusMaxHealth = 0;
            defeatedBossIds = new List<string>(); // Initialize defeated boss list
            initialCardsToRemove = new List<string>();
            if (PostFXManager.Instance != null)
                PostFXManager.Instance.UpdateEffects(light);
        }

       

        #region Encapsulation

        public int DrawCount
        {
            get => drawCount;
            set => drawCount = value;
        }

        public int MaxMana
        {
            get => maxMana;
            set => maxMana = value;
        }

        public int MaxCardOnHand
        {
            get => maxCardOnHand;
            set => maxCardOnHand = Mathf.Max(0, value);
        }

        public int Arcana
        {
            get => MaxMana;
            set => MaxMana = Mathf.Max(0, value);
        }

        public int CurrentMana
        {
            get => currentMana;
            set => currentMana = value;
        }

        public bool CanUseCards
        {
            get => canUseCards;
            set => canUseCards = value;
        }

        public bool CanSelectCards
        {
            get => canSelectCards;
            set => canSelectCards = value;
        }

        public bool IsRandomHand
        {
            get => isRandomHand;
            set => isRandomHand = value;
        }

        public List<AllyBase> AllyList
        {
            get => allyList;
            set => allyList = value;
        }

        public int CurrentStageId
        {
            get => currentStageId;
            set => currentStageId = value;
        }

        public int CurrentEncounterId
        {
            get => currentEncounterId;
            set => currentEncounterId = value;
        }

        public bool IsFinalEncounter
        {
            get => isFinalEncounter;
            set => isFinalEncounter = value;
        }

        public int CurrentEncounterTypeIndex
        {
            get => currentEncounterTypeIndex;
            set => currentEncounterTypeIndex = value;
        }

        public EnemyEncounter SelectedEncounter
        {
            get => selectedEncounter;
            set => selectedEncounter = value;
        }

        public List<CardData> CurrentCardsList
        {
            get => currentCardsList;
            set => currentCardsList = value;
        }

        public List<AllyHealthData> AllyHealthDataList
        {
            get => allyHealthDataDataList;
            set => allyHealthDataDataList = value;
        }
        public int CurrentGold
        {
            get => currentGold;
            set => currentGold = value;
        }

        public int ActNumber
        {
            get => actnumber;
            set => actnumber = value;

        }

        public int Light
        {
            get => light;
            set => light = value;

        }
         public int Proficiency
        {
            get => proficiency;
            set => proficiency = value;

        }

        public bool GainProficiencyAfterEliteOrBoss
        {
            get => gainProficiencyAfterEliteOrBoss;
            set => gainProficiencyAfterEliteOrBoss = value;
        }

        public bool EliteBossProficiencyRewardArmed
        {
            get => eliteBossProficiencyRewardArmed;
            set => eliteBossProficiencyRewardArmed = value;
        }

        public bool GainInsightAtActStart
        {
            get => gainInsightAtActStart;
            set => gainInsightAtActStart = value;
        }

        public bool GainLightAtActStart
        {
            get => gainLightAtActStart;
            set => gainLightAtActStart = value;
        }

        public void ApplyActStartBonuses()
        {
            if (GainInsightAtActStart)
                Insight += 1;

            if (GainLightAtActStart)
                ChangeLight(15);

            UIManager.Instance?.InformationCanvas?.RefreshStatsText();
        }

        public int Wisdom
        {
            get => wisdom;
            set => wisdom = value;
        }

        public int Potency
        {
            get => potency;
            set => potency = value;
        }

        public int Affinity
        {
            get => affinity;
            set => affinity = value;
        }

        public int Metabolism
        {
            get => metabolism;
            set => metabolism = value;
        }

        public int Vigor
        {
            get => vigor;
            set => vigor = value;
        }

        public int Insight
        {
            get => insight;
            set => insight = value;
        }

        public int Abundance
        {
            get => DrawCount;
            set => DrawCount = Mathf.Max(0, value);
        }

        public int Capacity
        {
            get => MaxCardOnHand;
            set => MaxCardOnHand = value;
        }

        public int Radiance
        {
            get => radiance;
            set => radiance = value;
        }

        public int LightLoss
        {
            get => lightLoss;
            set => lightLoss = value;

        }

        public bool actalreadyplayed
        {
            get => ActAlreadyPlayed;
            set => ActAlreadyPlayed = value;
        }
        public int bonusMaxHealth
        {
            get => BonusMaxHealth;
            set => BonusMaxHealth = value;
        }

         public bool restevent
        {
            get => Restevent;
            set => Restevent = value;
        }
        public List<string> DefeatedBossIds
        {
            get => defeatedBossIds;
            set => defeatedBossIds = value;
        }

        public List<string> InitialCardsToRemove => initialCardsToRemove;

        public void QueueInitialCardRemoval(string cardName)
        {
            if (!string.IsNullOrWhiteSpace(cardName) && !initialCardsToRemove.Contains(cardName))
                initialCardsToRemove.Add(cardName);
        }

        public void ResetIntroBonuses()
        {
            DrawCount = Mathf.Max(5, _gameplayData.DrawCount);
            MaxCardOnHand = Mathf.Max(12, _gameplayData.MaxCardOnHand);
            CurrentGold = 0;
            Light = 100;
            Proficiency = 1;
            Wisdom = 0;
            Potency = 0;
            Affinity = 0;
            Metabolism = 0;
            Vigor = 0;
            Insight = 0;
            Radiance = 0;
            LightLoss = 2;
            GainProficiencyAfterEliteOrBoss = false;
            EliteBossProficiencyRewardArmed = false;
            GainInsightAtActStart = false;
            GainLightAtActStart = false;
            InitialCardsToRemove.Clear();
        }
        
        /// <summary>
        /// Marks a boss as defeated so it won't appear in future acts
        /// </summary>
        public void MarkBossAsDefeated(string bossId)
        {
            if (!defeatedBossIds.Contains(bossId))
            {
                defeatedBossIds.Add(bossId);
                Debug.Log($"Boss '{bossId}' marked as defeated. Won't appear again this run.");
            }
        }
        
        /// <summary>
        /// Checks if a boss has been defeated this run
        /// </summary>
        public bool IsBossDefeated(string bossId)
        {
            return defeatedBossIds.Contains(bossId);
        }
        
        
        
        #endregion
    }
}