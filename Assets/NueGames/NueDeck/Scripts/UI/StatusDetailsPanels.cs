using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI
{
    public class StatusDetailsPanels : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private Transform statusRowContainer;
        [SerializeField] private GameObject statusRowPrefab;
        [SerializeField] private Button closeButton;
        
        [Header("Data")]
        [SerializeField] private StatusIconsData statusIconsData;
        [SerializeField] private SpecialKeywordData specialKeywordData;
        
        private CharacterBase _currentCharacter;
        private readonly List<GameObject> _spawnedRows = new List<GameObject>();
        private bool _isInitialized = false;
        
        /// <summary>
        /// Shows the status details panel for a specific character.
        /// </summary>
        public void ShowPanel(CharacterBase character)
        {
            // Initialize once
            if (!_isInitialized)
            {
                if (closeButton != null)
                {
                    closeButton.onClick.AddListener(HidePanel);
                }
                _isInitialized = true;
            }
            
            Debug.Log($"ShowPanel called for character: {character?.name}");
            
            if (character == null)
            {
                Debug.LogWarning("Character is null in ShowPanel");
                return;
            }
            
            if (panelRoot == null)
            {
                Debug.LogError("Panel Root is not assigned in StatusDetailsPanels component!");
                return;
            }
            
            _currentCharacter = character;
            
            // Clear previous rows
            ClearRows();
            
            // Update title
            if (titleText != null)
            {
                titleText.text = "Statuses";
            }
            
            // Populate status rows
            PopulateStatusRows();
            
            // Activate the Canvas parent if it exists and is inactive
            var canvasTransform = panelRoot.transform.parent;
            while (canvasTransform != null)
            {
                if (canvasTransform.GetComponent<Canvas>() != null && !canvasTransform.gameObject.activeSelf)
                {
                    Debug.Log($"Activating inactive Canvas: {canvasTransform.name}");
                    canvasTransform.gameObject.SetActive(true);
                    break;
                }
                canvasTransform = canvasTransform.parent;
            }
            
            // Show panel
            panelRoot.SetActive(true);
        }
        
        /// <summary>
        /// Hides the status details panel.
        /// </summary>
        public void HidePanel()
        {
            panelRoot.SetActive(false);
            
            // Deactivate the Canvas parent to clean up
            var canvasTransform = panelRoot.transform.parent;
            while (canvasTransform != null)
            {
                if (canvasTransform.GetComponent<Canvas>() != null)
                {
                    Debug.Log($"Deactivating Canvas: {canvasTransform.name}");
                    canvasTransform.gameObject.SetActive(false);
                    break;
                }
                canvasTransform = canvasTransform.parent;
            }
            
            _currentCharacter = null;
        }
        
        /// <summary>
        /// Populates the panel with all active statuses.
        /// </summary>
        private void PopulateStatusRows()
        {
            if (_currentCharacter == null || _currentCharacter.CharacterStats == null) return;
            
            var statusDict = _currentCharacter.CharacterStats.StatusDict;
            
            // Get all active statuses sorted by display priority
            var activeStatuses = statusDict
                .Where(kvp => kvp.Value.IsActive && kvp.Value.StatusValue > 0)
                .Select(kvp => new
                {
                    StatusType = kvp.Key,
                    StatusValue = kvp.Value.StatusValue,
                    IconData = statusIconsData.StatusIconList.FirstOrDefault(x => x.IconStatus == kvp.Key)
                })
                .Where(x => x.IconData != null)
                .OrderBy(x => x.IconData.DisplayPriority)
                .ThenBy(x => (int)x.StatusType)
                .ToList();
            
            // Create a row for each active status
            foreach (var status in activeStatuses)
            {
                CreateStatusRow(status.StatusType, status.StatusValue, status.IconData);
            }
        }
        
        /// <summary>
        /// Creates a single status row with name, count, and description.
        /// </summary>
        private void CreateStatusRow(StatusType statusType, int statusValue, StatusIconData iconData)
        {
            if (statusRowPrefab == null || statusRowContainer == null) return;
            
            var rowInstance = Instantiate(statusRowPrefab, statusRowContainer);
            _spawnedRows.Add(rowInstance);
            
            // Get the components - search for exact child names
            var nameText = rowInstance.transform.Find("NameText")?.GetComponent<TextMeshProUGUI>();
            var countText = rowInstance.transform.Find("Count Text")?.GetComponent<TextMeshProUGUI>(); // Note: has space
            var descriptionText = rowInstance.transform.Find("DescriptionText")?.GetComponent<TextMeshProUGUI>();
            
            // Try to find an icon image (might not exist in prefab yet)
            var iconImage = rowInstance.transform.Find("Icon")?.GetComponent<Image>() 
                            ?? rowInstance.transform.Find("StatusIcon")?.GetComponent<Image>()
                            ?? rowInstance.transform.Find("IconImage")?.GetComponent<Image>();
            
            // Set icon (if icon child exists in prefab)
            if (iconImage != null && iconData.IconSprite != null)
            {
                iconImage.sprite = iconData.IconSprite;
                iconImage.enabled = true;
            }
            
            // Set name
            if (nameText != null)
            {
                nameText.text = GetStatusDisplayName(statusType, iconData);
            }
            
            // Set count (only if status shows value)
            if (countText != null)
            {
                countText.text = iconData.ShowValue ? statusValue.ToString() : "";
            }
            
            // Set description
            if (descriptionText != null)
            {
                descriptionText.text = GetStatusDescription(statusType, iconData);
            }
            
            rowInstance.SetActive(true);
        }
        
        /// <summary>
        /// Gets the display name for a status type.
        /// </summary>
        private string GetStatusDisplayName(StatusType statusType, StatusIconData iconData)
        {
            // Try to get name from special keywords
            if (iconData.SpecialKeywords != null && iconData.SpecialKeywords.Count > 0)
            {
                var keyword = iconData.SpecialKeywords[0];
                var keywordData = specialKeywordData?.SpecialKeywordBaseList
                    .FirstOrDefault(x => x.SpecialKeyword == keyword);
                
                if (keywordData != null && !string.IsNullOrEmpty(keywordData.Header))
                {
                    return keywordData.Header;
                }
            }
            
            // Fallback to enum name with spacing
            return AddSpacesToCamelCase(statusType.ToString());
        }
        
        /// <summary>
        /// Gets the description for a status type.
        /// </summary>
        private string GetStatusDescription(StatusType statusType, StatusIconData iconData)
        {
            // Try to get description from special keywords
            if (iconData.SpecialKeywords != null && iconData.SpecialKeywords.Count > 0)
            {
                var keyword = iconData.SpecialKeywords[0];
                var keywordData = specialKeywordData?.SpecialKeywordBaseList
                    .FirstOrDefault(x => x.SpecialKeyword == keyword);
                
                if (keywordData != null)
                {
                    // Get description with dynamic values substituted
                    return keywordData.GetContentWithStatusValues(_currentCharacter.CharacterStats);
                }
            }
            
            // Fallback to basic description
            return $"A status effect of type {statusType}.";
        }
        
        /// <summary>
        /// Adds spaces to camel case strings (e.g., "PerfectHarmony" -> "Perfect Harmony").
        /// </summary>
        private string AddSpacesToCamelCase(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            
            var result = "";
            for (int i = 0; i < text.Length; i++)
            {
                if (i > 0 && char.IsUpper(text[i]))
                {
                    result += " ";
                }
                result += text[i];
            }
            return result;
        }
        
        /// <summary>
        /// Clears all spawned status rows.
        /// </summary>
        private void ClearRows()
        {
            foreach (var row in _spawnedRows)
            {
                if (row != null)
                {
                    Destroy(row);
                }
            }
            _spawnedRows.Clear();
        }
        
        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(HidePanel);
            }
        }
    }
}
