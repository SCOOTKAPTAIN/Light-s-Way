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
        [SerializeField] private TooltipText intentDescriptionTooltip;
        private bool _isPointerOverIntent;
        public Image IntentImage => intentImage;
        public TextMeshProUGUI NextActionValueText => nextActionValueText;

        public void SetIntentDescriptionVisible(bool visible)
        {
            var tooltip = GetIntentDescriptionTooltip();
            if (visible)
            {
                var enemyBase = GetComponentInParent<EnemyBase>();
                if (enemyBase != null)
                    ShowIntentDescription(enemyBase);
            }
            else
            {
                if (tooltip != null)
                    tooltip.gameObject.SetActive(false);
                else if (intentDescriptionPanel != null)
                    intentDescriptionPanel.SetActive(false);
            }
        }

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
            _isPointerOverIntent = IsPointerOverIntent(eventData);
            if (!_isPointerOverIntent)
            {
                base.OnPointerEnter(eventData);
                return;
            }

            var enemyBase = GetComponentInParent<EnemyBase>();
            if (enemyBase == null || intentImage == null)
                return;

            ShowIntentDescription(enemyBase);
            ShowKeywordTooltips(enemyBase);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (!_isPointerOverIntent)
            {
                base.OnPointerExit(eventData);
                return;
            }

            _isPointerOverIntent = false;

            if (TooltipManager.Instance != null)
                TooltipManager.Instance.HideTooltip();

            var tooltip = GetIntentDescriptionTooltip();
            if (tooltip != null)
                tooltip.gameObject.SetActive(false);
            else if (intentDescriptionPanel != null)
                intentDescriptionPanel.SetActive(false);
        }

        private bool IsPointerOverIntent(PointerEventData eventData)
        {
            if (intentImage == null || eventData == null || eventData.pointerEnter == null)
                return false;

            var pointerTransform = eventData.pointerEnter.transform;
            return pointerTransform == intentImage.transform || pointerTransform.IsChildOf(intentImage.transform);
        }

        private void ShowIntentDescription(EnemyBase enemyBase)
        {
            var tooltip = GetIntentDescriptionTooltip();
            if (intentDescriptionPanel == null && tooltip == null)
                return;

            if (tooltip != null)
            {
                tooltip.SetText(
                    enemyBase.GetNextAbilityTooltipContent(),
                    enemyBase.GetNextAbilityTooltipHeader());
                tooltip.gameObject.SetActive(true);
            }
            else
            {
                intentDescriptionPanel.SetActive(true);
            }
        }

        private TooltipText GetIntentDescriptionTooltip()
        {
            if (intentDescriptionTooltip != null)
                return intentDescriptionTooltip;

            return GetComponentInChildren<TooltipText>(true);
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