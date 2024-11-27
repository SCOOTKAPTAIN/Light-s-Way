using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;
using Random = UnityEngine.Random;
public class EncounterManager : MonoBehaviour
{
    
    public int randomEncounterId;

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


    public void EncounterSelector()
    {
        // 0 - start
        // 1 - Act 1
        // 2 - Act 1 Boss
        // 3 - Act 2
        // 4 - Act 2 Boss
        // 5 - Act 3
        // 6 - Act 3 Boss
        // 7 - Act 4 
        // 8 - Act 4 Boss
        // 9 - Final Act
        // 10 - Final Boss
        switch(GameManager.PersistentGameplayData.ActNumber)
        {
            case 1: // Act 1 Enemies
            GameManager.PersistentGameplayData.CurrentStageId = 1;
            GameManager.PersistentGameplayData.CurrentEncounterId = Random.Range(0,3);
            break;

            case 2: // Act 1 Bosses
            GameManager.PersistentGameplayData.CurrentStageId = 2;
            GameManager.PersistentGameplayData.CurrentEncounterId = Random.Range(0,3);
            break;

            default:
             throw new System.ArgumentOutOfRangeException();
        }

    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
