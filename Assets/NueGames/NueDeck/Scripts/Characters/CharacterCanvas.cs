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
            }

            StatusDict[targetStatus].SetStatusValue(value);
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