using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.ThirdParty.NueTooltip.Core;

namespace NueGames.NueDeck.Scripts.Characters
{
    public class EnemyCanvas : CharacterCanvas
    {
        [Header("Enemy Canvas Settings")]
        [SerializeField] private Image intentImage;
        [SerializeField] private TextMeshProUGUI nextActionValueText;
        [SerializeField] private GameObject chaosificationStatusRoot;
        [SerializeField] private Image chaosificationStatusImage;
        [SerializeField] private GameObject intentDescriptionPanel;
        [SerializeField] private TextMeshProUGUI intentDescriptionHeaderText;
        [SerializeField] private TextMeshProUGUI intentDescriptionText;
        [Header("Intent Description Layout")]
        [SerializeField, Min(0f)] private float intentPanelHorizontalPadding = 24f;
        [SerializeField, Min(0f)] private float intentPanelVerticalPadding = 20f;
        [SerializeField, Min(1f)] private float intentPanelMinWidth = 140f;
        [SerializeField, Min(1f)] private float intentPanelMaxWidth = 360f;
        public Image IntentImage => intentImage;
        public TextMeshProUGUI NextActionValueText => nextActionValueText;

        private ChaosificationStatusData _chaosificationStatus;

        public void SetChaosificationStatus(ChaosificationStatusData chaosificationStatus)
        {
            _chaosificationStatus = chaosificationStatus;

            if (chaosificationStatusRoot == null && chaosificationStatusImage != null)
                chaosificationStatusRoot = chaosificationStatusImage.gameObject;

            if (chaosificationStatusRoot == null)
                return;

            var showStatus = _chaosificationStatus != null && _chaosificationStatus.IsConfigured;
            chaosificationStatusRoot.SetActive(showStatus);

            if (!showStatus)
                return;

            if (chaosificationStatusImage != null)
                chaosificationStatusImage.sprite = _chaosificationStatus.Icon;

            SetupChaosificationHover(chaosificationStatusRoot);
        }

        private void SetupChaosificationHover(GameObject statusObject)
        {
            var image = statusObject.GetComponent<Image>();
            if (image == null)
                image = statusObject.AddComponent<Image>();
            image.raycastTarget = true;

            var eventTrigger = statusObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = statusObject.AddComponent<EventTrigger>();

            eventTrigger.triggers.Clear();

            var pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
            pointerEnter.callback.AddListener(_ => ShowChaosificationTooltip());
            eventTrigger.triggers.Add(pointerEnter);

            var pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
            pointerExit.callback.AddListener(_ => HideChaosificationTooltip());
            eventTrigger.triggers.Add(pointerExit);
        }

        private void ShowChaosificationTooltip()
        {
            if (_chaosificationStatus == null || TooltipManager.Instance == null)
                return;

            var header = string.IsNullOrWhiteSpace(_chaosificationStatus.ModifierName)
                ? "Chaosification"
                : $"Chaosification: {_chaosificationStatus.ModifierName}";

            TooltipManager.Instance.ShowTooltip(
                _chaosificationStatus.Description,
                header,
                chaosificationStatusRoot != null ? chaosificationStatusRoot.transform : transform);
        }

        private void HideChaosificationTooltip()
        {
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.HideTooltip();
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            var enemyBase = GetComponentInParent<EnemyBase>();
            if (enemyBase == null || intentImage == null)
                return;

            ShowIntentDescription(enemyBase);
            ShowKeywordTooltips(enemyBase);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.HideTooltip();

            if (intentDescriptionPanel != null)
                intentDescriptionPanel.SetActive(false);
        }

        private void ShowIntentDescription(EnemyBase enemyBase)
        {
            if (intentDescriptionPanel == null)
                return;

            if (intentDescriptionHeaderText != null)
                intentDescriptionHeaderText.text = enemyBase.GetNextAbilityTooltipHeader();

            if (intentDescriptionText != null)
                intentDescriptionText.text = enemyBase.GetNextAbilityTooltipContent();

            intentDescriptionPanel.SetActive(true);
            ResizeIntentDescriptionPanel();
        }

        private void ResizeIntentDescriptionPanel()
        {
            var panelRect = intentDescriptionPanel.GetComponent<RectTransform>();
            if (panelRect == null)
                return;

            var headerText = intentDescriptionHeaderText != null ? intentDescriptionHeaderText.text : string.Empty;
            var contentText = intentDescriptionText != null ? intentDescriptionText.text : string.Empty;
            var availableMaxWidth = Mathf.Max(intentPanelMinWidth, intentPanelMaxWidth - intentPanelHorizontalPadding);

            var headerPreferred = intentDescriptionHeaderText != null
                ? intentDescriptionHeaderText.GetPreferredValues(headerText, availableMaxWidth, 0f)
                : Vector2.zero;
            var contentPreferred = intentDescriptionText != null
                ? intentDescriptionText.GetPreferredValues(contentText, availableMaxWidth, 0f)
                : Vector2.zero;

            var textWidth = Mathf.Clamp(
                Mathf.Max(headerPreferred.x, contentPreferred.x),
                intentPanelMinWidth - intentPanelHorizontalPadding,
                availableMaxWidth);
            var panelWidth = textWidth + intentPanelHorizontalPadding;

            ResizeText(intentDescriptionHeaderText, textWidth);
            ResizeText(intentDescriptionText, textWidth);

            if (intentDescriptionHeaderText != null)
                intentDescriptionHeaderText.ForceMeshUpdate();
            if (intentDescriptionText != null)
                intentDescriptionText.ForceMeshUpdate();

            var headerHeight = intentDescriptionHeaderText != null
                ? intentDescriptionHeaderText.GetPreferredValues(headerText, textWidth, 0f).y
                : 0f;
            var contentHeight = intentDescriptionText != null
                ? intentDescriptionText.GetPreferredValues(contentText, textWidth, 0f).y
                : 0f;

            panelRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, panelWidth);
            panelRect.SetSizeWithCurrentAnchors(
                RectTransform.Axis.Vertical,
                headerHeight + contentHeight + intentPanelVerticalPadding);
            LayoutRebuilder.ForceRebuildLayoutImmediate(panelRect);
        }

        private void ResizeText(TextMeshProUGUI text, float width)
        {
            if (text == null)
                return;

            text.rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        }

        private void ShowKeywordTooltips(EnemyBase enemyBase)
        {
            if (TooltipManager.Instance == null)
                return;

            var keywordData = TooltipManager.Instance.SpecialKeywordData;
            if (keywordData == null || keywordData.SpecialKeywordBaseList == null)
                return;

            var characterStats = enemyBase.CharacterStats;
            var player = NueGames.NueDeck.Scripts.Managers.CombatManager.Instance != null
                ? NueGames.NueDeck.Scripts.Managers.CombatManager.Instance.CurrentMainAlly
                : null;
            if (player != null && player.CharacterStats != null)
                characterStats = player.CharacterStats;

            var shownKeywords = new HashSet<SpecialKeywords>();
            foreach (var keyword in enemyBase.GetNextAbilityKeywords())
            {
                if (!shownKeywords.Add(keyword))
                    continue;

                var data = keywordData.SpecialKeywordBaseList.Find(x => x.SpecialKeyword == keyword);
                if (data == null)
                    continue;

                var content = characterStats != null
                    ? data.GetContentWithStatusValues(characterStats)
                    : data.GetContent();
                var header = characterStats != null
                    ? data.GetHeaderWithStatusValue(characterStats)
                    : data.GetHeader();

                TooltipManager.Instance.ShowTooltip(content, header, intentImage.transform);
            }
        }
    }
}