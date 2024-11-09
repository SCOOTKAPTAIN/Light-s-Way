using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDialogueTesting : MonoBehaviour
{

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;
    [SerializeField] private TextAsset inkJSON2;

  
     private void Start()
     {
       DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
     }

     private void Update()
     {
         string choice = ((Ink.Runtime.StringValue) DialogueManager.GetInstance().GetVariableState("start_choice")).value;

         if(choice == "one")
         {
          DialogueManager.GetInstance().EnterDialogueMode(inkJSON2);
         }
      
     }





}