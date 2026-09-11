using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI
{
    public class CanvasBase : MonoBehaviour
    {
        private GameObject _inputBlocker;

        protected virtual bool BlocksBackgroundInput => false;

        protected CombatManager CombatManager => CombatManager.Instance;
        protected CollectionManager CollectionManager => CollectionManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        protected UIManager UIManager => UIManager.Instance;
        public virtual void OpenCanvas()
        {
            gameObject.SetActive(true);
            if (BlocksBackgroundInput)
                EnsureInputBlocker();
            else if (_inputBlocker != null)
                _inputBlocker.SetActive(false);
        }


        public virtual void CloseCanvas()
        {
            if (_inputBlocker != null)
                _inputBlocker.SetActive(false);
            gameObject.SetActive(false);
        }

        public virtual void ResetCanvas()
        {
            
        }

        private void EnsureInputBlocker()
        {
            if (_inputBlocker != null)
                return;

            _inputBlocker = new GameObject("ModalInputBlocker", typeof(RectTransform), typeof(Image));
            _inputBlocker.transform.SetParent(transform, false);
            _inputBlocker.transform.SetAsFirstSibling();

            var rectTransform = (RectTransform)_inputBlocker.transform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = Vector2.zero;

            var image = _inputBlocker.GetComponent<Image>();
            image.color = Color.clear;
            image.raycastTarget = true;
        }
    }
}