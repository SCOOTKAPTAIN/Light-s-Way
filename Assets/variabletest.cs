using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Settings;
using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine;

public class variabletest : MonoBehaviour
{

   
    protected GameManager GameManager => GameManager.Instance;


    [SerializeField] public int stageIdChanger;

   [SerializeField] public int EncounterIdChanger;

   [SerializeField] public int ActChanger;

      [SerializeField] public int ProficiencyChanger;

      [SerializeField] public int LightChanger;


   public void ChangeVariables()
   {
    //GameManager.PersistentGameplayData.CurrentEncounterId = EncounterIdChanger;

    //GameManager.PersistentGameplayData.CurrentStageId = stageIdChanger;

    GameManager.PersistentGameplayData.ActNumber = ActChanger;

    GameManager.PersistentGameplayData.proficiency = ProficiencyChanger;
    GameManager.PersistentGameplayData.light = LightChanger;
    UIManager.Instance.InformationCanvas.SetLightText(GameManager.PersistentGameplayData.light);
    UIManager.Instance.InformationCanvas.SetProficiencyText(GameManager.PersistentGameplayData.proficiency);

    //CharacterStats. = healthtext;
    
   }
   


    
}
