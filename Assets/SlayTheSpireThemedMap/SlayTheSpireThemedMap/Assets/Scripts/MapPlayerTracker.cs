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
                   DialogueAudioManager.instance.PlayMusic("Map_100_Light");
                   UIManager.ChangeScene(GameManager.SceneData.mapSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, true, false);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   break;
               case SceneType.Combat:
                   HideTooltipInfo(TooltipManager.Instance);
                   DialogueAudioManager.instance.PlayMusic("battle");
                   UIManager.ChangeScene(GameManager.SceneData.combatSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, true, false);
                   UIManager.SetCanvas(UIManager.RewardCanvas, false, true);
                   break;
                case SceneType.Dialogue:
                   HideTooltipInfo(TooltipManager.Instance);
                   UIManager.ChangeScene(GameManager.SceneData.dialogueSceneIndex);
                   UIManager.SetCanvas(UIManager.CombatCanvas, false, true);
                   UIManager.SetCanvas(UIManager.InformationCanvas, false, false);
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

        

        














        //-----------------------------------------------------------------------------------------------------------------------------------------------

        private void Awake()
        {
            Instance = this;
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

        private static bool CanTravelToNode(Node to, Node from)
        {
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
            }
            else
            {
                mapManager.CurrentMap.path.Add(mapNode.Node.point);
            }
            
            mapManager.SaveMap();
            view.SetAttainableNodes();
            view.SetLineColors();
            mapNode.ShowSwirlAnimation();

            DOTween.Sequence().AppendInterval(enterNodeDelay).OnComplete(() => EnterNode(mapNode));
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
                MapPlayerTracker.Instance.OpenCombatScene();
                    break;
                case NodeType.EliteEnemy:
                Debug.Log("Go to a dangerous battle!");
                MapPlayerTracker.Instance.OpenCombatScene();
                    break;
                case NodeType.RestSite:
                Debug.Log("Go to a resting place.");
                MapPlayerTracker.Instance.OpenDialogueScene();
                    break;
                case NodeType.Treasure:
                    break;
                case NodeType.Store:
                Debug.Log("hmm shady merchant");
                MapPlayerTracker.Instance.OpenDialogueScene();
                    break;
                case NodeType.Boss:
                Debug.Log("Go to a boss battle!");
                MapPlayerTracker.Instance.OpenCombatScene();
                    break;
                case NodeType.Mystery:
                Debug.Log("Events happening!");
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