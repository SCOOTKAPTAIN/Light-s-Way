using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

public class variabletest : MonoBehaviour
{

   
    protected GameManager GameManager => GameManager.Instance;


   [SerializeField] private int proficiencyChanger;
   [SerializeField] private int lightChanger;
   [SerializeField] private int wisdomChanger;
   [SerializeField] private int potencyChanger;
   [SerializeField] private int affinityChanger;
   [SerializeField] private int metabolismChanger;
   [SerializeField] private int vigorChanger;
   [SerializeField] private int insightChanger;
   [SerializeField] private int abundanceChanger;
   [SerializeField] private int capacityChanger;
   [SerializeField] private int radianceChanger;


   public void ChangeVariables()
   {
   var data = GameManager.PersistentGameplayData;
   var previousVigor = data.Vigor;
   data.Proficiency = proficiencyChanger;
   data.Light = lightChanger;
   data.Wisdom = wisdomChanger;
   data.Potency = potencyChanger;
   data.Affinity = affinityChanger;
   data.Metabolism = metabolismChanger;
   data.Vigor = vigorChanger;
   data.Insight = insightChanger;
   data.Abundance = abundanceChanger;
   data.Capacity = capacityChanger;
   data.Radiance = radianceChanger;

   ApplyVigorDelta(data.Vigor - previousVigor);

   if (UIManager.Instance != null && UIManager.Instance.InformationCanvas != null)
   {
      UIManager.Instance.InformationCanvas.SetLightText(data.Light);
      RefreshHealthText(data);
      UIManager.Instance.InformationCanvas.RefreshStatsText();
      UIManager.Instance.CombatCanvas?.LightCardSelectionPanel?.RefreshCostText();
   }
   }

   private static void ApplyVigorDelta(int delta)
   {
      var combatManager = CombatManager.Instance;
      var ally = combatManager != null ? combatManager.CurrentMainAlly : null;

      if (ally != null && ally.CharacterStats != null)
      {
         if (delta > 0)
            ally.CharacterStats.IncreaseMaxHealth(delta);
         else if (delta < 0)
            ally.CharacterStats.ApplyPermanentMaxHealthReduction(-delta);

         GameManager.Instance.PersistentGameplayData.SetAllyHealthData(
            ally.AllyCharacterData.CharacterID,
            ally.CharacterStats.CurrentHealth,
            ally.CharacterStats.MaxHealth);
      }
      else if (GameManager.Instance.PersistentGameplayData.AllyHealthDataList.Count > 0)
      {
         var healthData = GameManager.Instance.PersistentGameplayData.AllyHealthDataList[0];
         healthData.MaxHealth += delta;
         healthData.CurrentHealth = Mathf.Clamp(healthData.CurrentHealth + delta, 1, healthData.MaxHealth);
      }
   }

   private static void RefreshHealthText(NueGames.NueDeck.Scripts.Data.Settings.PersistentGameplayData data)
   {
      var combatManager = CombatManager.Instance;
      var ally = combatManager != null ? combatManager.CurrentMainAlly : null;
      if (ally != null && ally.CharacterStats != null)
      {
         UIManager.Instance.InformationCanvas.SetHealthText(
            ally.CharacterStats.CurrentHealth,
            ally.CharacterStats.MaxHealth);
         return;
      }

      if (data.AllyHealthDataList.Count > 0)
      {
         var healthData = data.AllyHealthDataList[0];
         UIManager.Instance.InformationCanvas.SetHealthText(healthData.CurrentHealth, healthData.MaxHealth);
         return;
      }

      var baseHealth = data.AllyList[0].AllyCharacterData.MaxHealth + data.Vigor;
      UIManager.Instance.InformationCanvas.SetHealthText(baseHealth, baseHealth);
   }
}
