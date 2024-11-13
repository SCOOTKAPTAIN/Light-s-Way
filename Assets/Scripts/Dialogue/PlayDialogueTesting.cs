using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDialogueTesting : MonoBehaviour
{

    [Header("Ink JSON")]
    [SerializeField] private TextAsset inkJSON;


  
     private void Start()
     {
       DialogueManager.GetInstance().EnterDialogueMode(inkJSON);
     }

     





}