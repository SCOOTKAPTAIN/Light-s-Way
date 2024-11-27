using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using NueGames.NueDeck.Scripts.Data.Settings;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

public class variabletest : MonoBehaviour
{

   
    protected GameManager GameManager => GameManager.Instance;


    [SerializeField] public int stageIdChanger;

   [SerializeField] public int EncounterIdChanger;

   [SerializeField] public int ActChanger;

   public void ChangeVariables()
   {
    GameManager.PersistentGameplayData.CurrentEncounterId = EncounterIdChanger;

    GameManager.PersistentGameplayData.CurrentStageId = stageIdChanger;

     GameManager.PersistentGameplayData.ActNumber = ActChanger;
    
   }
   


    
}
