using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.ThirdParty.NueTooltip.Core;
using System;
//DialogueManager.GetInstance().EnterDialogueMode(scene1, backgroundAnimator);
//StartCoroutine(Flow());
//yield return new WaitForSeconds(5);
//FadeBox.Play("FadeOut");
//FadeBox.Play("FadeIn");
public class DialogueRun : MonoBehaviour
{

    [Header("Background Animator")]
    [SerializeField] private Animator backgroundAnimator;

    [SerializeField] private Animator effectAnimator;

    [SerializeField] private Animator FadeBox;

    [SerializeField] private List<TextAsset> DialogueList;

    private int DialogueId = 0;

    private bool EndDialogue = false;

  
  
   

    private void Start()
    {
        EndDialogue = false;
       // DialogueProcessing();
        StartCoroutine(Flow());
    }


    private void Update()
    {
        if(EndDialogue == true && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            StartCoroutine(BacktoMap());
        }
    }

    public void DialogueProcessing()
    {
       
    }



    IEnumerator Flow()
   {
      //FadeBox.Play("FadeIn");

       yield return new WaitForSeconds(1);
       DialogueManager.GetInstance().EnterDialogueMode(DialogueList[DialogueId], backgroundAnimator, effectAnimator);
       EndDialogue = true;


       

    }

     IEnumerator BacktoMap()
    {
        
     yield return new WaitForSeconds(1);
     OpenMapScene();
     
     
    }
















//---------------------------------------------------------------------------------------------------------------------------------------------------


        private GameManager GameManager => GameManager.Instance;
        private UIManager UIManager => UIManager.Instance;
        

        private enum SceneType
        {
            MainMenu,
            Map,
            Combat,
            Dialogue
        }

        public virtual void HideTooltipInfo(TooltipManager tooltipManager)
        {
            tooltipManager.HideTooltip();
        }

        public void OpenMainMenuScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.MainMenu));
        }

        private IEnumerator DelaySceneChange(SceneType type)
        {
            // Save the current map if transitioning from the map scene
        //     if (type == SceneType.Combat || type == SceneType.Dialogue)
        //    {
        //        var mapManager = UnityEngine.Object.FindFirstObjectByType<Map.MapManager>();
        //        if (mapManager != null)
        //        {
        //            mapManager.SaveMap();
        //        }
        //        else
        //        {
        //            Debug.LogWarning("MapManager not found. Unable to save the map.");
        //        }
        //    }

           // Proceed with existing UI and scene management logic
           UIManager.SetCanvas(UIManager.Instance.InventoryCanvas, false, true);
           yield return StartCoroutine(UIManager.Instance.Fade(true));

           switch (type)
           {
               case SceneType.MainMenu:
                   HideTooltipInfo(TooltipManager.Instance);
                   UIManager.ChangeScene(GameManager.SceneData.mainMenuSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, false, true);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   GameManager.InitGameplayData();
                   GameManager.SetInitalHand();
                   break;
               case SceneType.Map:
                   HideTooltipInfo(TooltipManager.Instance);
                   DialogueAudioManager.instance.DynamicMusic("map");
                   UIManager.ChangeScene(GameManager.SceneData.mapSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, true, false);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   break;
               case SceneType.Combat:
                   HideTooltipInfo(TooltipManager.Instance);
                   DialogueAudioManager.instance.DynamicMusic("battle");
                   UIManager.ChangeScene(GameManager.SceneData.combatSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, true, false);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   break;
                case SceneType.Dialogue:
                   HideTooltipInfo(TooltipManager.Instance);
                   UIManager.ChangeScene(GameManager.SceneData.dialogueSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, true, false);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   break;
                
               default:
                   throw new ArgumentOutOfRangeException(nameof(type), type, null);
           }
        }

        public void OpenDialogueScene()
        {
             StartCoroutine(DelaySceneChange(SceneType.Dialogue));
        }

        public void OpenMapScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.Map));
        }

        public void OpenCombatScene()
        {
            StartCoroutine(DelaySceneChange(SceneType.Combat));
        }

        public void ChangeScene(int sceneId)
        {
            if (sceneId == GameManager.SceneData.mainMenuSceneIndex)
                OpenMainMenuScene();
            else if (sceneId == GameManager.SceneData.mapSceneIndex)
                OpenMapScene();
            else if (sceneId == GameManager.SceneData.combatSceneIndex)
                OpenCombatScene();
            else
                SceneManager.LoadScene(sceneId);

            TooltipManager.Instance.HideTooltip();
        }

        public void ExitApp()
        {
            GameManager.OnExitApp();
            Application.Quit();
        }

        

        












    



}






