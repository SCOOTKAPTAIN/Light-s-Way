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
        Boss
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
    /// Unified encounter selector - handles normal, elite, and boss encounters
    /// </summary>
    public void SelectEncounter(Map.EncounterType encounterType)
    {
        int actNumber = GameManager.PersistentGameplayData.ActNumber;
        
        // Use ActNumber directly as StageId (no need for separate tracking)
        GameManager.PersistentGameplayData.CurrentStageId = actNumber;
        
        switch (encounterType)
        {
            case Map.EncounterType.Normal:
                // Random normal encounter for current act
                int normalEncounterCount = GetEncounterCount(actNumber, false);
                GameManager.PersistentGameplayData.CurrentEncounterId = Random.Range(0, normalEncounterCount);
                GameManager.PersistentGameplayData.IsFinalEncounter = false;
                break;
                
            case Map.EncounterType.Elite:
                // Elite encounters are at the end of normal encounter list
                int totalNormalCount = GetEncounterCount(actNumber, false);
                GameManager.PersistentGameplayData.CurrentEncounterId = totalNormalCount; // Elite is after normal encounters
                GameManager.PersistentGameplayData.IsFinalEncounter = false;
                break;
                
            case Map.EncounterType.Boss:
                // Boss encounter
                GameManager.PersistentGameplayData.CurrentEncounterId = 0; // Bosses use separate list
                GameManager.PersistentGameplayData.IsFinalEncounter = true;
                break;
        }
        
        Debug.Log($"Selected {encounterType} encounter for Act {actNumber}, Stage {GameManager.PersistentGameplayData.CurrentStageId}, Encounter {GameManager.PersistentGameplayData.CurrentEncounterId}");
    }
    
    /// <summary>
    /// Gets the number of encounters available for an act
    /// </summary>
    private int GetEncounterCount(int actNumber, bool isBoss)
    {
        var encounterStage = GameManager.EncounterData.EnemyEncounterList.Find(x => x.StageId == actNumber);
        if (encounterStage == null) return 1;
        
        return isBoss 
            ? encounterStage.BossEncounterList.Count 
            : encounterStage.EnemyEncounterList.Count;
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
