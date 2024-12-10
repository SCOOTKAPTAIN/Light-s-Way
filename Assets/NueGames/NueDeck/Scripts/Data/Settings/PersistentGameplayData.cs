using System;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Collection;
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
        [SerializeField] private int currentMana;
        [SerializeField] private int lightLoss;
        [SerializeField] private bool canUseCards;
        [SerializeField] private bool canSelectCards;
        [SerializeField] private bool isRandomHand;
        
        [SerializeField] private List<AllyBase> allyList;
        [SerializeField] private int currentStageId;
        [SerializeField] private int currentEncounterId;
        [SerializeField] private bool isFinalEncounter;
        [SerializeField] private List<CardData> currentCardsList;
        [SerializeField] private List<AllyHealthData> allyHealthDataDataList;

        // My Variables
        [SerializeField] private int actnumber;
        [SerializeField] public int light;
        [SerializeField] public int proficiency;
        [SerializeField] private bool ActAlreadyPlayed;

        [SerializeField] private int BonusMaxHealth;

       [SerializeField] public bool Restevent;

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
            if(light > 100)
            {
                light = 100;
            }
            if(light < 0)
            {
                light = 0;
            }
            UIManager.Instance.InformationCanvas.SetLightText(light);
        }
        public void InitData()
        {
            DrawCount = _gameplayData.DrawCount;
            MaxMana = _gameplayData.MaxMana;
            CurrentMana = MaxMana;
            CanUseCards = true;
            CanSelectCards = true;
            IsRandomHand = _gameplayData.IsRandomHand;
            AllyList = new List<AllyBase>(_gameplayData.InitalAllyList);
            CurrentEncounterId = 0;
            CurrentStageId = 0;
            CurrentGold = 0;
            CurrentCardsList = new List<CardData>();
            IsFinalEncounter = false;
            allyHealthDataDataList = new List<AllyHealthData>();
            proficiency = 1;
            light = 100;
            actnumber = 0;
            lightLoss = 2;
            ActAlreadyPlayed = false;
            BonusMaxHealth = 0;
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
        
        
        #endregion
    }
}