using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Map
{
    public enum EncounterType
    {
        Normal,
        Elite,
        Boss,
        Special  // For event-based encounters
    }
}

public class EncounterManager : MonoBehaviour
{
    public static EncounterManager instance;
    protected GameManager GameManager => GameManager.Instance;

    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;              
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    /// <summary>
    /// Unified encounter selector - handles normal, elite, boss, and special encounters
    /// </summary>
    public void SelectEncounter(Map.EncounterType encounterType)
    {
        int actNumber = GameManager.PersistentGameplayData.ActNumber;
        
        // Use ActNumber directly as StageId
        GameManager.PersistentGameplayData.CurrentStageId = actNumber;
        GameManager.PersistentGameplayData.CurrentEncounterId = -1; // -1 = random selection
        GameManager.PersistentGameplayData.CurrentEncounterTypeIndex = (int)encounterType; // Store encounter type
        
        // Set encounter type flag (for legacy GetEnemyEncounter calls)
        GameManager.PersistentGameplayData.IsFinalEncounter = (encounterType == Map.EncounterType.Boss);
        
        Debug.Log($"Selected random {encounterType} encounter for Act {actNumber}, Stage {GameManager.PersistentGameplayData.CurrentStageId}");
    }
    
    /// <summary>
    /// Select a specific encounter by index (useful for scripted events/specific battles)
    /// </summary>
    public void SelectSpecificEncounter(Map.EncounterType encounterType, int encounterIndex)
    {
        int actNumber = GameManager.PersistentGameplayData.ActNumber;
        
        GameManager.PersistentGameplayData.CurrentStageId = actNumber;
        GameManager.PersistentGameplayData.CurrentEncounterId = encounterIndex;
        GameManager.PersistentGameplayData.CurrentEncounterTypeIndex = (int)encounterType;
        GameManager.PersistentGameplayData.IsFinalEncounter = (encounterType == Map.EncounterType.Boss);
        
        Debug.Log($"Selected specific {encounterType} encounter #{encounterIndex} for Act {actNumber}");
    }
    
    /// <summary>
    /// Gets the number of encounters available for an act (optional, for debugging/UI)
    /// </summary>
    public int GetEncounterCount(int actNumber, Map.EncounterType encounterType)
    {
        var encounterStage = GameManager.EncounterData.EnemyEncounterList.Find(x => x.StageId == actNumber);
        if (encounterStage == null) return 0;
        
        return encounterType switch
        {
            Map.EncounterType.Normal => encounterStage.NormalEncounterList?.Count ?? 0,
            Map.EncounterType.Elite => encounterStage.EliteEncounterList?.Count ?? 0,
            Map.EncounterType.Boss => encounterStage.BossEncounterList?.Count ?? 0,
            Map.EncounterType.Special => encounterStage.SpecialEncounterList?.Count ?? 0,
            _ => 0
        };
    }

    #region Legacy Methods (for backwards compatibility)
    [System.Obsolete("Use SelectEncounter(EncounterType.Normal) instead")]
    public void EncounterSelector()
    {
        SelectEncounter(Map.EncounterType.Normal);
    }

    [System.Obsolete("Use SelectEncounter(EncounterType.Elite) instead")]
    public void EliteEncounterSelector()
    {
        SelectEncounter(Map.EncounterType.Elite);
    }

    [System.Obsolete("Use SelectEncounter(EncounterType.Boss) instead")]
    public void BossSelector()
    {
        SelectEncounter(Map.EncounterType.Boss);
    }
    #endregion
}
