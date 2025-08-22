using UnityEngine;
using UnityEngine.EventSystems;

namespace NueGames.NueDeck.Scripts.Card
{
    public class CardUI : CardBase
    {
        private Vector3 originalScale;

        protected override void Awake()
        {
            base.Awake();
            originalScale = transform.localScale;
        }

        public override void OnPointerEnter(PointerEventData eventData)
        {
            base.OnPointerEnter(eventData); // keeps tooltip behavior
            transform.localScale = originalScale * 1.1f; // zoom effect
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData); // keeps tooltip hide behavior
            transform.localScale = originalScale;
        }
    }
}
