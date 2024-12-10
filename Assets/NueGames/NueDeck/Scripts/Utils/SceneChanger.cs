using System;
using System.Collections;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.ThirdParty.NueTooltip.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


namespace NueGames.NueDeck.Scripts.Utils
{
    public class SceneChanger : MonoBehaviour
    {
        private GameManager GameManager => GameManager.Instance;
        private UIManager UIManager => UIManager.Instance;

       

        
        
        

        private enum SceneType
        {
            MainMenu,
            Map,
            Combat,
            Ending
        }

       
        public void OpenMainMenuScene()
        {
            
            StartCoroutine(DelaySceneChange(SceneType.MainMenu));
        }

        public void OpenDialogueScene()
        {
             StartCoroutine(DelaySceneChange(SceneType.Ending));
        }

        private IEnumerator DelaySceneChange(SceneType type)
        {
            // Save the current map if transitioning from the map scene
            if (type == SceneType.Combat)
           {
               var mapManager = UnityEngine.Object.FindFirstObjectByType<Map.MapManager>();
               if (mapManager != null)
               {
                   mapManager.SaveMap();
               }
               else
               {
                   Debug.LogWarning("MapManager not found. Unable to save the map.");
               }
           }

           // Proceed with existing UI and scene management logic
           UIManager.SetCanvas(UIManager.Instance.InventoryCanvas, false, true);
           yield return StartCoroutine(UIManager.Instance.Fade(true));

           switch (type)
           {
               case SceneType.MainMenu:
                   UIManager.ChangeScene(GameManager.SceneData.mainMenuSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, false, true);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   GameManager.InitGameplayData();
                   GameManager.SetInitalHand();
                   break;
               case SceneType.Map:
                   DialogueAudioManager.instance.DynamicMusic("map");
                   UIManager.ChangeScene(GameManager.SceneData.mapSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, true, false);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   break;
               case SceneType.Combat:
                   DialogueAudioManager.instance.DynamicMusic("battle");
                   UIManager.ChangeScene(GameManager.SceneData.combatSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, true, false);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   break;
               case SceneType.Ending:
                   UIManager.ChangeScene(GameManager.SceneData.endingSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, false, true);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   GameManager.InitGameplayData();
                   GameManager.SetInitalHand();
                   break;
               default:
                   throw new ArgumentOutOfRangeException(nameof(type), type, null);
           }
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
}

