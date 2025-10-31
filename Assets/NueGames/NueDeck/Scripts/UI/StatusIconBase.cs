using System;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI
{
    public class StatusIconBase : MonoBehaviour
    {
        [SerializeField] private Image statusImage;
        [SerializeField] private TextMeshProUGUI statusValueText;

        public StatusIconData MyStatusIconData { get; private set; } = null;

        public Image StatusImage => statusImage;

        public TextMeshProUGUI StatusValueText => statusValueText;

        public void SetStatus(StatusIconData statusIconData)
        {
            MyStatusIconData = statusIconData;
            StatusImage.sprite = statusIconData.IconSprite;
            // Decide whether to show the numeric value. Default to the ScriptableObject setting.
            var show = statusIconData.ShowValue;

            // Some statuses are purely symbolic (no numeric value) - treat them as icon-only by default.
            // Add new statuses here as needed.
            var iconOnlyStatuses = new[] { StatusType.GodsAngelBuff };
            if (Array.Exists(iconOnlyStatuses, s => s == statusIconData.IconStatus))
                show = false;

            statusValueText.gameObject.SetActive(show);
        }

        public void SetStatusValue(int statusValue)
        {
            // If the associated status data says not to show a value, hide the text.
            if (MyStatusIconData != null && !MyStatusIconData.ShowValue)
            {
                StatusValueText.gameObject.SetActive(false);
                return;
            }

            // If the status is designated icon-only by code, hide the numeric text.
            if (MyStatusIconData != null && Array.Exists(new[] { StatusType.GodsAngelBuff }, s => s == MyStatusIconData.IconStatus))
            {
                StatusValueText.gameObject.SetActive(false);
                return;
            }

            // Hide zero values to avoid showing '0' on the icon
            if (statusValue <= 0)
            {
                StatusValueText.gameObject.SetActive(false);
                return;
            }

            StatusValueText.gameObject.SetActive(true);
            StatusValueText.text = statusValue.ToString();
        }
    }
}