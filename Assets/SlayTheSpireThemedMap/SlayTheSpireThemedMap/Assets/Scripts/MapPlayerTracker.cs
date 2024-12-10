using System;
using System.Linq;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.ThirdParty.NueTooltip.Core;
using NueGames.NueDeck.ThirdParty.NueTooltip.CursorSystem;
using NueGames.NueDeck.ThirdParty.NueTooltip.Interfaces;
using NueGames.NueDeck.Scripts.Utils;

namespace Map
{
    public class MapPlayerTracker : MonoBehaviour
    {
        public bool lockAfterSelecting = false;
        public float enterNodeDelay = 1f;
        public MapManager mapManager;
        public MapView view;

        public static MapPlayerTracker Instance;

        public bool Locked { get; set; }

        private GameManager GameManager => GameManager.Instance;
        private UIManager UIManager => UIManager.Instance;
        

        private enum SceneType
        {
            MainMenu,
            Map,
            Combat,
            Dialogue,
            Merchant
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
            if (type == SceneType.Combat || type == SceneType.Dialogue)
           {
               var mapManager = UnityEngine.Object.FindFirstObjectByType<MapManager>();
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
                  // DialogueAudioManager.instance.DynamicMusic("battle");
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
                case SceneType.Merchant:
                   HideTooltipInfo(TooltipManager.Instance);
                   UIManager.ChangeScene(GameManager.SceneData.merchantSceneIndex);
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

         public void OpenMerchantScene()
        {
             StartCoroutine(DelaySceneChange(SceneType.Merchant));
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

        

        














        //-----------------------------------------------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            Instance = this;
        }

        private void Update()
        {
            if (mapManager.CurrentMap.path.Count == 0)
            {
                return;
            }
            else
            {
            Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
            MapNode currentnode = view.GetNode(currentPoint);
            switch(GameManager.Instance.PersistentGameplayData.light)
            {
                case >= 90 and <= 100:
                currentnode.spotlight.pointLightInnerRadius = 16f; 
                currentnode.spotlight.pointLightOuterRadius = 32f;
                break;
                case >= 50 and <= 89:
                currentnode.spotlight.pointLightInnerRadius = 10f; 
                currentnode.spotlight.pointLightOuterRadius = 20f;
                break;
                case >= 10 and <= 49:
                currentnode.spotlight.pointLightInnerRadius = 5f; 
                currentnode.spotlight.pointLightOuterRadius = 10f;
                break;
                case >= 1 and <= 9:
                currentnode.spotlight.pointLightInnerRadius = 2f; 
                currentnode.spotlight.pointLightOuterRadius = 4f;
                break;
                case 0:
                currentnode.spotlight.pointLightInnerRadius = 0.6f; 
                currentnode.spotlight.pointLightOuterRadius = 1.3f;
                break;

            }

            currentnode.spotlight.enabled = true;

            }
            


        }

        public void SelectNode(MapNode mapNode)
        {
            if (Locked) return;

            // Debug.Log("Selected node: " + mapNode.Node.point);

            if (mapManager.CurrentMap.path.Count == 0)
            {
                // player has not selected the node yet, he can select any of the nodes with y = 0
                if (mapNode.Node.point.y == 0)
                    SendPlayerToNode(mapNode);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
            else
            {
                Vector2Int currentPoint = mapManager.CurrentMap.path[mapManager.CurrentMap.path.Count - 1];
                Node currentNode = mapManager.CurrentMap.GetNode(currentPoint);

                if (currentNode != null && CanTravelToNode(mapNode.Node, currentNode))
                    SendPlayerToNode(mapNode);
                else
                    PlayWarningThatNodeCannotBeAccessed();
            }
        }

        private bool CanTravelToNode(Node to, Node from)
        {
            if (mapManager.CurrentMap.movedOnSameLayer)
            {
                return from.outgoing.Any(point => point.Equals(to.point));
            }
            
            return from.outgoing.Any(point => point.Equals(to.point)) ||
                   from.point.y == to.point.y; 
            // (optionally use this instead of (from.point.y == to.point.y), but it can prevent movement to the side if the neighbor node is absent)
            // from.point.y == to.point.y && Mathf.Abs(from.point.x - to.point.x) == 1;
        }

        private void SendPlayerToNode(MapNode mapNode)
        {
            Locked = lockAfterSelecting;
            if (mapManager.CurrentMap.path.Count > 0 && mapManager.CurrentMap.path[^1].y == mapNode.Node.point.y)
            {
                mapManager.CurrentMap.path[^1] = mapNode.Node.point;
                mapManager.CurrentMap.movedOnSameLayer = true;
            }
            else
            {
                mapManager.CurrentMap.path.Add(mapNode.Node.point);
                mapManager.CurrentMap.movedOnSameLayer = false;
            }
            
            mapManager.SaveMap();
            view.SetAttainableNodes();
            view.SetLineColors();
            mapNode.ShowSwirlAnimation();

            DOTween.Sequence().AppendInterval(enterNodeDelay).OnComplete(() => EnterNode(mapNode));
        }

        public void NaturalLightLoss()
        {
            GameManager.Instance.PersistentGameplayData.light -= GameManager.Instance.PersistentGameplayData.LightLoss;
            if(GameManager.Instance.PersistentGameplayData.light < 0)
            {
                GameManager.Instance.PersistentGameplayData.light = 0;
            }
            UIManager.InformationCanvas.SetLightText(GameManager.Instance.PersistentGameplayData.light);
        }

        private static void EnterNode(MapNode mapNode)
        {
            // we have access to blueprint name here as well
            Debug.Log("Entering node: " + mapNode.Node.blueprintName + " of type: " + mapNode.Node.nodeType);
            // load appropriate scene with context based on nodeType:
            // or show appropriate GUI over the map: 
            // if you choose to show GUI in some of these cases, do not forget to set "Locked" in MapPlayerTracker back to false
            switch (mapNode.Node.nodeType)
            {
                case NodeType.MinorEnemy:
                Debug.Log("Go to a normal battle!");
                EncounterManager.instance.EncounterSelector();

                DialogueAudioManager.instance.PlaySFX("enterbattle");
                DialogueAudioManager.instance.DynamicMusic("battle");

                MapPlayerTracker.Instance.NaturalLightLoss();
                MapPlayerTracker.Instance.OpenCombatScene();
                    break;
                case NodeType.EliteEnemy:
                Debug.Log("Go to a dangerous battle!");
                EncounterManager.instance.EliteEncounterSelector();

                DialogueAudioManager.instance.PlaySFX("enterbattle");
                DialogueAudioManager.instance.DynamicMusic("battle");

                MapPlayerTracker.Instance.NaturalLightLoss();
                MapPlayerTracker.Instance.OpenCombatScene();
                    break;
                case NodeType.RestSite:
                Debug.Log("Go to a resting place.");
                DialogueAudioManager.instance.PlaySFX("enterevent");
                MapPlayerTracker.Instance.NaturalLightLoss();
                MapPlayerTracker.Instance.OpenDialogueScene();
                GameManager.Instance.PersistentGameplayData.restevent = true;
                    break;
                case NodeType.Treasure:
                    break;
                case NodeType.Store:
                Debug.Log("hmm shady merchant");
                DialogueAudioManager.instance.PlaySFX("enterevent");
                MapPlayerTracker.Instance.NaturalLightLoss();
                MapPlayerTracker.Instance.OpenMerchantScene();
                    break;
                case NodeType.Boss:
                Debug.Log("Go to a boss battle!");
                GameManager.Instance.PersistentGameplayData.ActNumber++;
                EncounterManager.instance.BossSelector();
                GameManager.Instance.PersistentGameplayData.actalreadyplayed = false;
                
                DialogueAudioManager.instance.PlaySFX("enterbattle");
                DialogueAudioManager.instance.BossMusic();
                MapPlayerTracker.Instance.NaturalLightLoss();
                MapPlayerTracker.Instance.OpenCombatScene();
                    break;
                case NodeType.Mystery:
                Debug.Log("Events happening!");
                DialogueAudioManager.instance.PlaySFX("enterevent");
                MapPlayerTracker.Instance.NaturalLightLoss();
                MapPlayerTracker.Instance.OpenDialogueScene();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void PlayWarningThatNodeCannotBeAccessed()
        {
            Debug.Log("Selected node cannot be accessed");
        }
    }
}