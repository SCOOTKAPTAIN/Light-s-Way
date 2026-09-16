using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using NueGames.NueDeck.ThirdParty.NueTooltip.Core;

namespace NueGames.NueDeck.Scripts.Characters
{
    public class EnemyCanvas : CharacterCanvas
    {
        [Header("Enemy Canvas Settings")]
        [SerializeField] private Image intentImage;
        [SerializeField] private TextMeshProUGUI nextActionValueText;
        public Image IntentImage => intentImage;
        public TextMeshProUGUI NextActionValueText => nextActionValueText;

        public override void OnPointerEnter(PointerEventData eventData)
        {
            var enemyBase = GetComponentInParent<EnemyBase>();
            if (enemyBase == null || TooltipManager.Instance == null || intentImage == null)
                return;

            TooltipManager.Instance.ShowTooltip(
                enemyBase.GetNextAbilityTooltipContent(),
                enemyBase.GetNextAbilityTooltipHeader(),
                intentImage.transform);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            if (TooltipManager.Instance != null)
                TooltipManager.Instance.HideTooltip();
        }
    }
}