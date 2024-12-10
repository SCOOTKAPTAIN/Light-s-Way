﻿using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.UI
{
    public class InformationCanvas : CanvasBase
    {
        [Header("Settings")] 
        [SerializeField] private GameObject randomizedDeckObject;
        [SerializeField] private TextMeshProUGUI roomTextField;
        [SerializeField] private TextMeshProUGUI goldTextField;
        [SerializeField] private TextMeshProUGUI nameTextField;
        [SerializeField] private TextMeshProUGUI healthTextField;
        [SerializeField] private TextMeshProUGUI actionPointField;
        [SerializeField] private TextMeshProUGUI proficiencyField;
        [SerializeField] private TextMeshProUGUI lightField;

        public GameObject RandomizedDeckObject => randomizedDeckObject;
        public TextMeshProUGUI RoomTextField => roomTextField;
        public TextMeshProUGUI GoldTextField => goldTextField;
        public TextMeshProUGUI NameTextField => nameTextField;
        public TextMeshProUGUI HealthTextField => healthTextField;
        public TextMeshProUGUI ActionPointField => actionPointField;
        public TextMeshProUGUI ProficiencyField => proficiencyField;
        public TextMeshProUGUI LightField => lightField;


        
        
        #region Setup
        private void Awake()
        {
            ResetCanvas();
        }
        #endregion
        
        #region Public Methods
        public void SetRoomText(int roomNumber,bool useStage = false, int stageNumber = -1) => 
            RoomTextField.text = useStage ? $"Room {stageNumber}/{roomNumber}" : $"Room {roomNumber}";

        public void SetGoldText(int value)=>GoldTextField.text = $"{value}";

        public void SetProficiencyText(int value)=>ProficiencyField.text = $"{value}";

        public void SetActionPointText(int value)=>ActionPointField.text = $"{value}";
        public void SetLightText(int value) => lightField.text = $"{value}";

        public void SetNameText(string name) => NameTextField.text = $"{name}";

        

        

        public void SetHealthText(int currentHealth,int maxHealth) => HealthTextField.text = $"{currentHealth}/{maxHealth}";

        public override void ResetCanvas()
        {
            RandomizedDeckObject.SetActive(GameManager.PersistentGameplayData.IsRandomHand);
            SetHealthText(GameManager.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth,GameManager.PersistentGameplayData.AllyList[0].AllyCharacterData.MaxHealth);
            SetNameText(GameManager.GameplayData.DefaultName);
            SetRoomText(GameManager.PersistentGameplayData.CurrentEncounterId+1,GameManager.GameplayData.UseStageSystem,GameManager.PersistentGameplayData.CurrentStageId+1);
            UIManager.InformationCanvas.SetGoldText(GameManager.PersistentGameplayData.CurrentGold);
            UIManager.InformationCanvas.SetProficiencyText(GameManager.PersistentGameplayData.proficiency);
            UIManager.InformationCanvas.SetActionPointText(GameManager.PersistentGameplayData.MaxMana);
            UIManager.InformationCanvas.SetLightText(GameManager.PersistentGameplayData.Light);

        }
        #endregion
        
    }
}