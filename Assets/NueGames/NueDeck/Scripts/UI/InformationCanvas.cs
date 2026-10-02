using System.Globalization;
using System.Text.RegularExpressions;
using NueGames.NueDeck.Scripts.Managers;
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
        [SerializeField] private GameObject statsPanel;
        [SerializeField] private TextMeshProUGUI statsProficiencyField;
        [SerializeField] private TextMeshProUGUI arcanaField;
        [SerializeField] private TextMeshProUGUI wisdomField;
        [SerializeField] private TextMeshProUGUI potencyField;
        [SerializeField] private TextMeshProUGUI affinityField;
        [SerializeField] private TextMeshProUGUI metabolismField;
        [SerializeField] private TextMeshProUGUI vigorField;
        [SerializeField] private TextMeshProUGUI insightField;
        [SerializeField] private TextMeshProUGUI abundanceField;
        [SerializeField] private TextMeshProUGUI capacityField;
        [SerializeField] private TextMeshProUGUI radianceField;

        private string wisdomTemplate;
        private string statsProficiencyTemplate;
        private string arcanaTemplate;
        private string potencyTemplate;
        private string affinityTemplate;
        private string metabolismTemplate;
        private string vigorTemplate;
        private string insightTemplate;
        private string abundanceTemplate;
        private string capacityTemplate;
        private string radianceTemplate;

        public GameObject RandomizedDeckObject => randomizedDeckObject;
        public TextMeshProUGUI RoomTextField => roomTextField;
        public TextMeshProUGUI GoldTextField => goldTextField;
        public TextMeshProUGUI NameTextField => nameTextField;
        public TextMeshProUGUI HealthTextField => healthTextField;
        public TextMeshProUGUI ActionPointField => actionPointField;
        public TextMeshProUGUI ProficiencyField => proficiencyField;
        public TextMeshProUGUI LightField => lightField;
        public GameObject StatsPanel => statsPanel;
        public TextMeshProUGUI StatsProficiencyField => statsProficiencyField;
        public TextMeshProUGUI ArcanaField => arcanaField;
        public TextMeshProUGUI WisdomField => wisdomField;
        public TextMeshProUGUI PotencyField => potencyField;
        public TextMeshProUGUI AffinityField => affinityField;
        public TextMeshProUGUI MetabolismField => metabolismField;
        public TextMeshProUGUI VigorField => vigorField;
        public TextMeshProUGUI InsightField => insightField;
        public TextMeshProUGUI AbundanceField => abundanceField;
        public TextMeshProUGUI CapacityField => capacityField;
        public TextMeshProUGUI RadianceField => radianceField;


        
        
        #region Setup
        private void Awake()
        {
            statsProficiencyTemplate = StatsProficiencyField != null ? StatsProficiencyField.text : "{Proficiency}";
            arcanaTemplate = ArcanaField != null ? ArcanaField.text : "{Arcana}";
            wisdomTemplate = WisdomField != null ? WisdomField.text : "{Wisdom}";
            potencyTemplate = PotencyField != null ? PotencyField.text : "{Potency}";
            affinityTemplate = AffinityField != null ? AffinityField.text : "{Affinity}";
            metabolismTemplate = MetabolismField != null ? MetabolismField.text : "{Metabolism}";
            vigorTemplate = VigorField != null ? VigorField.text : "{Vigor}";
            insightTemplate = InsightField != null ? InsightField.text : "{Insight}";
            abundanceTemplate = AbundanceField != null ? AbundanceField.text : "{Abundance}";
            capacityTemplate = CapacityField != null ? CapacityField.text : "{Capacity}";
            radianceTemplate = RadianceField != null ? RadianceField.text : "{Radiance}";

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

        public void SetStatsProficiencyText(int value) => SetStatFieldText(StatsProficiencyField, statsProficiencyTemplate, "Proficiency", value);
        public void SetArcanaText(int value) => SetStatFieldText(ArcanaField, arcanaTemplate, "Arcana", value);
        public void SetWisdomText(int value) => SetStatFieldText(WisdomField, wisdomTemplate, "Wisdom", value);
        public void SetPotencyText(int value) => SetStatFieldText(PotencyField, potencyTemplate, "Potency", value);
        public void SetAffinityText(int value) => SetStatFieldText(AffinityField, affinityTemplate, "Affinity", value);
        public void SetMetabolismText(int value) => SetStatFieldText(MetabolismField, metabolismTemplate, "Metabolism", value);
        public void SetVigorText(int value) => SetStatFieldText(VigorField, vigorTemplate, "Vigor", value);
        public void SetInsightText(int value) => SetStatFieldText(InsightField, insightTemplate, "Insight", value);
        public void SetAbundanceText(int value) => SetStatFieldText(AbundanceField, abundanceTemplate, "Abundance", value);
        public void SetCapacityText(int value) => SetStatFieldText(CapacityField, capacityTemplate, "Capacity", value);
        public void SetRadianceText(int value) => SetStatFieldText(RadianceField, radianceTemplate, "Radiance", value);

        public void RefreshStatsText()
        {
            var gameplayData = GameManager.PersistentGameplayData;
            SetStatsProficiencyText(gameplayData.Proficiency);
            SetArcanaText(gameplayData.MaxMana);
            SetWisdomText(gameplayData.Wisdom);
            SetPotencyText(gameplayData.Potency);
            SetAffinityText(gameplayData.Affinity);
            SetMetabolismText(gameplayData.Metabolism);
            SetVigorText(gameplayData.Vigor);
            SetInsightText(gameplayData.Insight);
            SetAbundanceText(gameplayData.Abundance);
            SetCapacityText(gameplayData.Capacity);
            SetRadianceText(gameplayData.Radiance);
        }

        private static void SetStatFieldText(
            TextMeshProUGUI field,
            string template,
            string statName,
            int value)
        {
            if (field != null)
            {
                var pattern = $@"\{{{Regex.Escape(statName)}\s*(?:(?<operator>[+\-*/])\s*(?<operand>-?\d+(?:\.\d+)?))?\}}";
                field.text = Regex.Replace(template, pattern, match =>
                {
                    if (!match.Groups["operator"].Success)
                        return value.ToString();

                    if (!float.TryParse(match.Groups["operand"].Value, NumberStyles.Float,
                            CultureInfo.InvariantCulture, out var operand))
                        return value.ToString();

                    var result = match.Groups["operator"].Value switch
                    {
                        "+" => value + operand,
                        "-" => value - operand,
                        "*" => value * operand,
                        "/" when operand != 0f => value / operand,
                        _ => value
                    };

                    return Mathf.RoundToInt(result).ToString();
                }, RegexOptions.IgnoreCase);
            }
        }

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
            UIManager.InformationCanvas.RefreshStatsText();

        }
        #endregion
        
    }
}