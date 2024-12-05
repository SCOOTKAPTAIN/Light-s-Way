using System;
using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.ThirdParty.NueTooltip.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MerchantRun : MonoBehaviour
{

    [SerializeField] private Animator backgroundAnimator;

    [SerializeField] private Animator effectAnimator;

    [SerializeField] private Animator FadeBox;

    [SerializeField] private List<TextAsset> DialogueList;

    public GameObject ShopPanel;

    private int DialogueId = 0;
    private bool EndDialogue = false;

    private void Update()
    {
        if(EndDialogue == true && !DialogueManager.GetInstance().dialogueIsPlaying)
        {
            ShopPanel.SetActive(true);   
        }else if( EndDialogue == false)
        {
            ShopPanel.SetActive(false);   

        }
    }
    

    private void Start()
    {
      
       // DialogueProcessing();
        StartCoroutine(Flow());
    }

    IEnumerator Flow()
   {
      //FadeBox.Play("FadeIn");

       yield return new WaitForSeconds(0.2f);
       DialogueManager.GetInstance().EnterDialogueMode(DialogueList[0], backgroundAnimator, effectAnimator);
       EndDialogue = true;
    }

    IEnumerator BacktoMap()
    {
        OpenMapScene();
        yield return new WaitForSeconds(0.01f);
    }

    public void Buy(string what)
    {
        switch(what)
        {
            case "heal":
            if(GameManager.Instance.PersistentGameplayData.CurrentGold < 200)
            {
                DialogueAudioManager.instance.PlaySFX("broke");
              DialogueManager.GetInstance().EnterDialogueMode(DialogueList[1], backgroundAnimator, effectAnimator);
            }else
            {
                DialogueAudioManager.instance.PlaySFX("buy");
                GameManager.Instance.PersistentGameplayData.CurrentGold -= 200;
                UIManager.Instance.InformationCanvas.SetGoldText(GameManager.Instance.PersistentGameplayData.CurrentGold);
                CombatManager.Instance.CurrentMainAlly.CharacterStats.Heal(Mathf.RoundToInt(CombatManager.Instance.CurrentMainAlly.CharacterStats.MaxHealth));
                GameManager.Instance.PersistentGameplayData.SetAllyHealthData(CombatManager.Instance.CurrentMainAlly.AllyCharacterData.CharacterID,CombatManager.Instance.CurrentMainAlly.CharacterStats.CurrentHealth
                ,CombatManager.Instance.CurrentMainAlly.CharacterStats.MaxHealth);
                DialogueManager.GetInstance().EnterDialogueMode(DialogueList[2], backgroundAnimator, effectAnimator);
            }

            break;

            case "proficiency":
            if(GameManager.Instance.PersistentGameplayData.CurrentGold < 250)
            {
                DialogueAudioManager.instance.PlaySFX("broke");
              DialogueManager.GetInstance().EnterDialogueMode(DialogueList[1], backgroundAnimator, effectAnimator);
            }else
            {
                DialogueAudioManager.instance.PlaySFX("buy");
                GameManager.Instance.PersistentGameplayData.CurrentGold -= 250;
                UIManager.Instance.InformationCanvas.SetGoldText(GameManager.Instance.PersistentGameplayData.CurrentGold);
                GameManager.Instance.PersistentGameplayData.proficiency += 1;
                UIManager.Instance.InformationCanvas.SetProficiencyText(GameManager.Instance.PersistentGameplayData.proficiency);
                DialogueManager.GetInstance().EnterDialogueMode(DialogueList[3], backgroundAnimator, effectAnimator);
            }

            break;

            case "maxHP":
            if(GameManager.Instance.PersistentGameplayData.CurrentGold < 300)
            {
                DialogueAudioManager.instance.PlaySFX("broke");
              DialogueManager.GetInstance().EnterDialogueMode(DialogueList[1], backgroundAnimator, effectAnimator);
            }else
            {
                DialogueAudioManager.instance.PlaySFX("buy");
                GameManager.Instance.PersistentGameplayData.CurrentGold -= 300;
                UIManager.Instance.InformationCanvas.SetGoldText(GameManager.Instance.PersistentGameplayData.CurrentGold);
                CombatManager.Instance.CurrentMainAlly.CharacterStats.IncreaseMaxHealth(Mathf.RoundToInt(25));
                GameManager.Instance.PersistentGameplayData.SetAllyHealthData(CombatManager.Instance.CurrentMainAlly.AllyCharacterData.CharacterID
               ,CombatManager.Instance.CurrentMainAlly.CharacterStats.CurrentHealth + 25,CombatManager.Instance.CurrentMainAlly.CharacterStats.MaxHealth);

                DialogueManager.GetInstance().EnterDialogueMode(DialogueList[4], backgroundAnimator, effectAnimator);
            }

            break;

            case "maxAP":
             if(GameManager.Instance.PersistentGameplayData.CurrentGold < 500)
            {
                DialogueAudioManager.instance.PlaySFX("broke");
              DialogueManager.GetInstance().EnterDialogueMode(DialogueList[1], backgroundAnimator, effectAnimator);
            }else
            {
                DialogueAudioManager.instance.PlaySFX("buy");
                GameManager.Instance.PersistentGameplayData.CurrentGold -= 500;
                UIManager.Instance.InformationCanvas.SetGoldText(GameManager.Instance.PersistentGameplayData.CurrentGold);

                GameManager.Instance.PersistentGameplayData.MaxMana += 1;
                UIManager.Instance.InformationCanvas.SetActionPointText(GameManager.Instance.PersistentGameplayData.MaxMana);
                DialogueManager.GetInstance().EnterDialogueMode(DialogueList[5], backgroundAnimator, effectAnimator);
            }

            break;

            case "Refuel Light":
            if(GameManager.Instance.PersistentGameplayData.CurrentGold < 150)
            {
                DialogueAudioManager.instance.PlaySFX("broke");
              DialogueManager.GetInstance().EnterDialogueMode(DialogueList[1], backgroundAnimator, effectAnimator);
            }else if( GameManager.Instance.PersistentGameplayData.light == 100)
            {
                DialogueAudioManager.instance.PlaySFX("broke");
                DialogueManager.GetInstance().EnterDialogueMode(DialogueList[8], backgroundAnimator, effectAnimator);


            }else
            {
                DialogueAudioManager.instance.PlaySFX("buy");
                GameManager.Instance.PersistentGameplayData.CurrentGold -= 150;
                UIManager.Instance.InformationCanvas.SetGoldText(GameManager.Instance.PersistentGameplayData.CurrentGold);

                GameManager.Instance.PersistentGameplayData.light += 10;
                if(GameManager.Instance.PersistentGameplayData.light > 100)
                {
                    GameManager.Instance.PersistentGameplayData.light = 100;
                }
                UIManager.Instance.InformationCanvas.SetLightText(GameManager.Instance.PersistentGameplayData.light);
                DialogueManager.GetInstance().EnterDialogueMode(DialogueList[6], backgroundAnimator, effectAnimator);
            }


            break;



        }
    }

    public void ExitMerchant()
    {
        StartCoroutine(Exit());
    }

    IEnumerator Exit()
    {
        EndDialogue = false;
        DialogueManager.GetInstance().EnterDialogueMode(DialogueList[7], backgroundAnimator, effectAnimator);
        yield return new WaitForSeconds(2);
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
