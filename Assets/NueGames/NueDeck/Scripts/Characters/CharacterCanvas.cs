using System;
using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.UI;
using NueGames.NueDeck.ThirdParty.NueTooltip.Core;
using NueGames.NueDeck.ThirdParty.NueTooltip.CursorSystem;
using NueGames.NueDeck.ThirdParty.NueTooltip.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.Characters
{
    [RequireComponent(typeof(Canvas))]
    public abstract class CharacterCanvas : MonoBehaviour,I2DTooltipTarget
    {
        [Header("References")]
        [SerializeField] protected Transform statusIconRoot;
        [SerializeField] protected Transform highlightRoot;
        [SerializeField] protected Transform descriptionRoot;
        [SerializeField] protected StatusIconsData statusIconsData;
        [SerializeField] protected TextMeshProUGUI currentHealthText;
        
        private Button _statusContainerButton;
        
        #region Cache

        protected Dictionary<StatusType, StatusIconBase> StatusDict = new Dictionary<StatusType, StatusIconBase>();

        protected Canvas TargetCanvas;
        
        protected FxManager FxManager => FxManager.Instance;
        protected AudioManager AudioManager => AudioManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        protected CombatManager CombatManager => CombatManager.Instance;
        protected CollectionManager CollectionManager => CollectionManager.Instance;
        protected UIManager UIManager => UIManager.Instance;

        #endregion
        
        #region Setup

        public void InitCanvas()
        {
            highlightRoot.gameObject.SetActive(false);
            
            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
                StatusDict.Add((StatusType) i, null);

            TargetCanvas = GetComponent<Canvas>();

            if (TargetCanvas)
                TargetCanvas.worldCamera = Camera.main;
                
            // Setup status container as clickable button
            SetupStatusContainerButton();
        }
        
        /// <summary>
        /// Sets up the status icon root as a clickable button to open status details panel.
        /// </summary>
        private void SetupStatusContainerButton()
        {
            if (statusIconRoot == null) return;
            
            // Add Button component if it doesn't exist
            _statusContainerButton = statusIconRoot.GetComponent<Button>();
            if (_statusContainerButton == null)
            {
                _statusContainerButton = statusIconRoot.gameObject.AddComponent<Button>();
            }
            
            // Make button transparent (only used for clicking, not visual)
            _statusContainerButton.transition = Selectable.Transition.None;
            
            // Add click listener
            _statusContainerButton.onClick.AddListener(OnStatusContainerClicked);
            
            // Ensure there's an Image component for raycasting (can be invisible)
            var image = statusIconRoot.GetComponent<Image>();
            if (image == null)
            {
                image = statusIconRoot.gameObject.AddComponent<Image>();
                image.color = new Color(0, 0, 0, 0); // Fully transparent
            }
            
            // Add EventTrigger for cursor change on hover
            var eventTrigger = statusIconRoot.GetComponent<UnityEngine.EventSystems.EventTrigger>();
            if (eventTrigger == null)
            {
                eventTrigger = statusIconRoot.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();
            }
            
            // On pointer enter - change to hand cursor (uses default system cursor)
            var pointerEnter = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
            };
            pointerEnter.callback.AddListener((data) => { Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); });
            eventTrigger.triggers.Add(pointerEnter);
            
            // On pointer exit - restore default cursor
            var pointerExit = new UnityEngine.EventSystems.EventTrigger.Entry
            {
                eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
            };
            pointerExit.callback.AddListener((data) => { Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto); });
            eventTrigger.triggers.Add(pointerExit);
        }
        
        /// <summary>
        /// Called when the status container is clicked - opens the status details panel.
        /// </summary>
        private void OnStatusContainerClicked()
        {
            var characterBase = GetComponentInParent<CharacterBase>();
            if (characterBase == null) return;
            
            var statusPanel = FindFirstObjectByType<StatusDetailsPanels>(FindObjectsInactive.Include);
            if (statusPanel != null)
            {
                statusPanel.ShowPanel(characterBase);
            }
            else
            {
                Debug.LogWarning("StatusDetailsPanel not found in scene. Make sure it exists in the UI.");
            }
        }

        #endregion
        
        #region Public Methods
        public void ApplyStatus(StatusType targetStatus, int value)
        {
            // Do not create or show icons for zero or negative values.
            if (value <= 0)
            {
                ClearStatus(targetStatus);
                return;
            }

            if (StatusDict[targetStatus] == null)
            {
                var targetData = statusIconsData.StatusIconList.FirstOrDefault(x => x.IconStatus == targetStatus);
                if (targetData == null) return;

                var clone = Instantiate(statusIconsData.StatusIconBasePrefab, statusIconRoot);
                clone.SetStatus(targetData);
                StatusDict[targetStatus] = clone;
                
                // Sort icons by display priority after adding a new one
                SortStatusIcons();
            }

            StatusDict[targetStatus].SetStatusValue(value);
        }
        
        /// <summary>
        /// Sorts all active status icons by their display priority.
        /// Lower priority values appear first (leftmost).
        /// </summary>
        private void SortStatusIcons()
        {
            // Get all active status icons with their priority
            var activeIcons = StatusDict
                .Where(kvp => kvp.Value != null)
                .Select(kvp => new { Icon = kvp.Value, Priority = kvp.Value.MyStatusIconData.DisplayPriority, StatusType = kvp.Key })
                .OrderBy(x => x.Priority)
                .ThenBy(x => (int)x.StatusType) // Secondary sort by enum order if priority is the same
                .ToList();
            
            // Set sibling index to match sorted order
            for (int i = 0; i < activeIcons.Count; i++)
            {
                activeIcons[i].Icon.transform.SetSiblingIndex(i);
            }
        }

        public void ClearStatus(StatusType targetStatus)
        {
            if (StatusDict[targetStatus])
            {
                Destroy(StatusDict[targetStatus].gameObject);
            }
           
            StatusDict[targetStatus] = null;
        }
        
        public void UpdateStatusText(StatusType targetStatus, int value)
        {
            if (StatusDict[targetStatus] == null) return;
            // If the status value is 0 or less, remove the icon instead of showing '0'
            if (value <= 0)
            {
                ClearStatus(targetStatus);
                return;
            }

            StatusDict[targetStatus].StatusValueText.text = $"{value}";
        }
        
        public void UpdateHealthText(int currentHealth,int maxHealth) =>  currentHealthText.text = $"{currentHealth}/{maxHealth}";
        public void SetHighlight(bool open) => highlightRoot.gameObject.SetActive(open);
       
        // Handler for shield (Block) gain notifications. Spawn blue floating text at the
        // character's text spawn root.
        public void SpawnShieldGainedText(int amount)
        {
            if (amount <= 0) return;
            if (FxManager == null) return;

            var charBase = GetComponentInParent<CharacterBase>();
            var spawnRoot = charBase != null ? charBase.TextSpawnRoot : transform;
            FxManager.SpawnFloatingTextBlue(spawnRoot, amount.ToString());
        }
       
        #endregion

        #region Pointer Events
        public void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltipInfo();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltipInfo(TooltipManager.Instance);
        }

        #endregion

        #region Tooltip
        public void ShowTooltipInfo()
        {
            var tooltipManager = TooltipManager.Instance;
            var specialKeywords = new List<SpecialKeywords>();
            var characterBase = GetComponentInParent<CharacterBase>();
            
            foreach (var statusIcon in StatusDict)
            {
                if (statusIcon.Value == null) continue;
               
                var statusData = statusIcon.Value.MyStatusIconData;
                foreach (var statusDataSpecialKeyword in statusData.SpecialKeywords)
                {
                    if (specialKeywords.Contains(statusDataSpecialKeyword)) continue;
                    specialKeywords.Add(statusDataSpecialKeyword);
                }
            }
            
            foreach (var specialKeyword in specialKeywords)
            {
                var specialKeywordData = tooltipManager.SpecialKeywordData.SpecialKeywordBaseList.Find(x => x.SpecialKeyword == specialKeyword);
                if (specialKeywordData != null)
                {
                    // Get content with dynamic status values if character context is available
                    string content = characterBase != null 
                        ? specialKeywordData.GetContentWithStatusValues(characterBase.CharacterStats)
                        : specialKeywordData.GetContent();
                    
                    // Get header with status value if character context is available
                    string header = characterBase != null
                        ? specialKeywordData.GetHeaderWithStatusValue(characterBase.CharacterStats)
                        : specialKeywordData.GetHeader();
                    
                    ShowTooltipInfo(tooltipManager, content, header, descriptionRoot);
                }
            }
            
        }
        public void ShowTooltipInfo(TooltipManager tooltipManager, string content, string header = "", Transform tooltipStaticTransform = null, CursorType targetCursor = CursorType.Default,Camera cam = null, float delayShow =0)
        {
            tooltipManager.ShowTooltip(content,header,tooltipStaticTransform,targetCursor,cam,delayShow);
        }

        public void HideTooltipInfo(TooltipManager tooltipManager)
        {
            tooltipManager.HideTooltip();
        }
        

        #endregion
       
    }
}