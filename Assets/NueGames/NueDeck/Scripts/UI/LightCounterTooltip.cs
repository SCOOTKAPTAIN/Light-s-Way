using System;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace NueGames.NueDeck.Scripts.UI
{
    /// <summary>
    /// Simple tooltip for Light counter - shows/hides a panel with light effects info.
    /// </summary>
    public class LightCounterTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [Header("References")]
        [SerializeField] private GameObject tooltipPanel;
        [SerializeField] private TextMeshProUGUI tooltipText;
        
        [Header("Light Thresholds")]
        [SerializeField] private List<LightThreshold> lightThresholds = new List<LightThreshold>();
        
        private GameManager GameManager => GameManager.Instance;
        
        private void Start()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }
        
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltip();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void ShowTooltip()
        {
            if (tooltipPanel == null || tooltipText == null || GameManager == null)
                return;

            int currentLight = GameManager.PersistentGameplayData.light;
            
            // Find the current threshold
            LightThreshold currentThreshold = GetCurrentThreshold(currentLight);
            
            // Calculate current penalties
            float eliteSpawnIncrease = CalculateEliteSpawnIncrease(currentLight);
            float ominousIncrease = CalculateOminousIncrease(currentLight);
            float enemyStatsIncrease = CalculateEnemyStatsIncrease(currentLight);
            
            // Build tooltip text
            string content = "";
            
            // Add flavor text
            if (currentThreshold != null && !string.IsNullOrEmpty(currentThreshold.FlavorText))
            {
                content += $"<i>{currentThreshold.FlavorText}</i>\n";
            }
            
            // Add custom effect descriptions or use calculated values
            if (currentThreshold != null && currentThreshold.CustomEffectTexts.Count > 0)
            {
                foreach (var effectText in currentThreshold.CustomEffectTexts)
                {
                    // Replace placeholders with actual values
                    string processedText = effectText
                        .Replace("{EliteSpawn}", $"{eliteSpawnIncrease:F0}")
                        .Replace("{Ominous}", $"{ominousIncrease:F0}")
                        .Replace("{EnemyStats}", $"{enemyStatsIncrease:F0}");
                    content += processedText + "\n";
                }
            }
            else
            {
                // Fallback to default
                content += $"• Elite Spawn Chance: +{eliteSpawnIncrease:F0}%\n";
                content += $"• Ominous Encounters: +{ominousIncrease:F0}%\n";
                content += GetEnemyStatsInfo(currentLight);
            }
            
            tooltipText.text = content;
            tooltipPanel.SetActive(true);
        }
        
        /// <summary>
        /// Gets the current light threshold based on light value.
        /// </summary>
        private LightThreshold GetCurrentThreshold(int light)
        {
            foreach (var threshold in lightThresholds)
            {
                if (light >= threshold.MinLight && light <= threshold.MaxLight)
                {
                    return threshold;
                }
            }
            return null;
        }

        private void HideTooltip()
        {
            if (tooltipPanel != null)
                tooltipPanel.SetActive(false);
        }

        /// <summary>
        /// Calculates elite spawn chance increase (0% at 100 light, 50% at 0 light).
        /// </summary>
        private float CalculateEliteSpawnIncrease(int light)
        {
            // Linear interpolation: 100 light = 0%, 0 light = 50%
            return (100 - light) * 0.5f;
        }
    
    [Serializable]
    public class LightThreshold
    {
        [Header("Light Range")]
        [SerializeField] private int minLight = 0;
        [SerializeField] private int maxLight = 100;
        
        [Header("Display Text")]
        [SerializeField] [TextArea(2, 3)] private string flavorText;
        [SerializeField] [TextArea(2, 4)] private List<string> customEffectTexts = new List<string>();
        
        public int MinLight => minLight;
        public int MaxLight => maxLight;
        public string FlavorText => flavorText;
        public List<string> CustomEffectTexts => customEffectTexts;
    }

        /// <summary>
        /// Calculates ominous encounter increase (0% at 100 light, 50% at 0 light).
        /// </summary>
        private float CalculateOminousIncrease(int light)
        {
            // Same as elite spawn
            return (100 - light) * 0.5f;
        }

        /// <summary>
        /// Calculates enemy stats increase percentage based on light thresholds.
        /// Matches the exact scaling from DamageEffects.cs
        /// </summary>
        private float CalculateEnemyStatsIncrease(int light)
        {
            return light switch
            {
                >= 80 and <= 100 => 0f,     // No buff
                >= 50 and <= 79 => 10f,     // +10%
                >= 25 and <= 49 => 15f,     // +15%
                >= 10 and <= 24 => 25f,     // +25%
                >= 1 and <= 9 => 40f,       // +40%
                0 => 50f,                    // +50%
                _ => 0f
            };
        }

        /// <summary>
        /// Gets enemy stats increase info based on light thresholds.
        /// </summary>
        private string GetEnemyStatsInfo(int light)
        {
            string info = "• Enemy Stats: ";
            
            if (light >= 75)
            {
                info += "Normal";
            }
            else if (light >= 50)
            {
                info += "+10% Health/Damage";
            }
            else if (light >= 25)
            {
                info += "+25% Health/Damage";
            }
            else
            {
                info += "+50% Health/Damage";
            }
            
            return info + "\n";
        }
    }
}
