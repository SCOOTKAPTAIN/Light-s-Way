using System;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Collection;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI
{
    // Reusable panel: drag cards out of hand into the drop zone, then confirm to select them.
    // Cards that are never confirmed are returned to the hand when the panel closes.
    public class CardSelectionCanvas : CanvasBase, ICardDropTarget
    {
        protected override bool BlocksBackgroundInput => true;

        [Header("References")]
        [SerializeField] private TextMeshProUGUI titleTextField;
        [SerializeField] private TextMeshProUGUI counterTextField;
        [SerializeField] private RectTransform dropZone;
        [SerializeField] private Transform stagedCardRoot;
        [SerializeField] private Button confirmButton;
        [SerializeField] [Min(0.01f)] private float stagedCardScale = 100f;
        [SerializeField] [Min(1f)] private float stagedCardSpacing = 220f;

        public RectTransform DropZone => dropZone;

        private readonly List<CardBase> _stagedCards = new List<CardBase>();
        private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();
        private int _maxCards;
        private Action<List<CardBase>> _onSelectionComplete;
        private bool _confirmed;
        private bool _previousCanUseCards;
        private bool _previousCanSelectCards;
        private bool _selectionIsOpen;
        private CardBase _draggedStagedCard;

        private void Awake()
        {
            if (confirmButton != null)
                confirmButton.onClick.AddListener(Confirm);

            if (!_selectionIsOpen)
                gameObject.SetActive(false);
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;

            if (!_selectionIsOpen)
                gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _draggedStagedCard = null;
        }

        private void Update()
        {
            if (!_selectionIsOpen) return;

            if (_draggedStagedCard == null)
            {
                if (Input.GetMouseButtonDown(0))
                    TryBeginStagedCardDrag(Input.mousePosition);
                return;
            }

            UpdateStagedCardDrag(Input.mousePosition);

            if (Input.GetMouseButtonUp(0))
                EndStagedCardDrag(Input.mousePosition);
        }

        /// <summary>
        /// Opens the panel and lets the player drag up to maxCards from hand into the drop zone.
        /// onSelectionComplete receives the confirmed cards, or an empty list if the player cancels.
        /// </summary>
        public void BeginSelection(string title, int maxCards, Action<List<CardBase>> onSelectionComplete)
        {
            _maxCards = Mathf.Max(0, maxCards);
            _onSelectionComplete = onSelectionComplete;
            _confirmed = false;
            _stagedCards.Clear();
            _selectionIsOpen = true;

            if (titleTextField != null)
                titleTextField.text = title;

            UpdateCounterText();
            UIManager.SetCanvas(this, true, true);

            if (CollectionManager != null && CollectionManager.HandController != null)
            {
                _previousCanUseCards = GameManager.PersistentGameplayData.CanUseCards;
                _previousCanSelectCards = GameManager.PersistentGameplayData.CanSelectCards;
                GameManager.PersistentGameplayData.CanUseCards = false;
                GameManager.PersistentGameplayData.CanSelectCards = true;
                CollectionManager.HandController.SetActiveCardDropTarget(this);
            }
        }

        public bool TryAcceptCard(CardBase card)
        {
            if (card == null || _stagedCards.Count >= _maxCards)
                return false;

            _stagedCards.Add(card);

            var cardTransform = card.transform;
            cardTransform.SetParent(stagedCardRoot != null ? stagedCardRoot : dropZone, false);
            cardTransform.localRotation = Quaternion.identity;
            cardTransform.localScale = Vector3.one * stagedCardScale;
            SetCardCanvasSorting(card, true, 100 + _stagedCards.Count);
            RefreshStagedCardLayout();

            UpdateCounterText();
            return true;
        }

        private void Confirm()
        {
            _confirmed = true;
            CloseCanvas();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_selectionIsOpen)
                CloseCanvas();
        }

        public override void CloseCanvas()
        {
            _selectionIsOpen = false;
            _draggedStagedCard = null;

            if (CollectionManager != null && CollectionManager.HandController != null)
            {
                CollectionManager.HandController.SetActiveCardDropTarget(null);
                GameManager.PersistentGameplayData.CanUseCards = _previousCanUseCards;
                GameManager.PersistentGameplayData.CanSelectCards = _previousCanSelectCards;
            }

            var callback = _onSelectionComplete;
            var confirmed = _confirmed;
            var stagedCards = new List<CardBase>(_stagedCards);
            _stagedCards.Clear();
            _onSelectionComplete = null;
            _confirmed = false;

            if (CollectionManager != null && CollectionManager.HandController != null)
            {
                var destination = confirmed && CollectionManager.HandController.discardTransform != null
                    ? CollectionManager.HandController.discardTransform
                    : CollectionManager.HandController.transform;

                foreach (var card in stagedCards)
                {
                    if (card != null)
                    {
                        SetCardCanvasSorting(card, false, 0);
                        card.transform.localScale = Vector3.one;
                        card.transform.SetParent(destination, false);
                        // Old UI-space local position would otherwise be read as a huge world offset
                        card.transform.position = destination.position;
                    }
                }
            }

            base.CloseCanvas();

            if (!confirmed)
            {
                foreach (var card in stagedCards)
                    CollectionManager.HandController.AddCardToHand(card);

                callback?.Invoke(new List<CardBase>());
                return;
            }

            callback?.Invoke(stagedCards);
        }

        private void RefreshStagedCardLayout()
        {
            var centerIndex = (_stagedCards.Count - 1) * 0.5f;

            for (var i = 0; i < _stagedCards.Count; i++)
            {
                var card = _stagedCards[i];
                if (card == null)
                    continue;

                card.transform.localPosition = new Vector3((i - centerIndex) * stagedCardSpacing, 0f, 0f);
                SetCardCanvasSorting(card, true, 100 + i);
            }
        }

        private static void SetCardCanvasSorting(CardBase card, bool overrideSorting, int sortingOrder)
        {
            foreach (var canvas in card.GetComponentsInChildren<Canvas>(true))
            {
                canvas.overrideSorting = overrideSorting;
                if (overrideSorting)
                    canvas.sortingOrder = sortingOrder;
            }
        }

        private Camera GetDropZoneCamera()
        {
            if (dropZone == null) return null;
            var canvas = dropZone.GetComponentInParent<Canvas>();
            return canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
        }

        private bool IsScreenPointInsideDropZone(Vector2 screenPoint)
        {
            return dropZone != null && RectTransformUtility.RectangleContainsScreenPoint(dropZone, screenPoint, GetDropZoneCamera());
        }

        private void TryBeginStagedCardDrag(Vector2 screenPos)
        {
            if (EventSystem.current == null || _stagedCards.Count == 0) return;

            var pointerData = new PointerEventData(EventSystem.current) { position = screenPos };
            _raycastResults.Clear();
            EventSystem.current.RaycastAll(pointerData, _raycastResults);

            foreach (var result in _raycastResults)
            {
                var card = result.gameObject.GetComponentInParent<CardBase>();
                if (card != null && _stagedCards.Contains(card))
                {
                    _draggedStagedCard = card;
                    SetCardCanvasSorting(card, true, 1000);
                    return;
                }
            }
        }

        private void UpdateStagedCardDrag(Vector2 screenPos)
        {
            var rect = (stagedCardRoot as RectTransform) ?? dropZone;
            if (rect == null) return;

            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, screenPos, GetDropZoneCamera(), out var localPoint))
                _draggedStagedCard.transform.localPosition = localPoint;
        }

        private void EndStagedCardDrag(Vector2 screenPos)
        {
            var card = _draggedStagedCard;
            _draggedStagedCard = null;

            if (IsScreenPointInsideDropZone(screenPos))
                RefreshStagedCardLayout();
            else
                ReturnStagedCardToHand(card);
        }

        private void ReturnStagedCardToHand(CardBase card)
        {
            if (card == null) return;

            _stagedCards.Remove(card);
            SetCardCanvasSorting(card, false, 0);
            card.transform.localScale = Vector3.one;

            if (CollectionManager != null && CollectionManager.HandController != null)
            {
                var handTransform = CollectionManager.HandController.transform;
                card.transform.SetParent(handTransform, false);
                // Old UI-space local position would otherwise be read as a huge world offset
                card.transform.position = handTransform.position;
                CollectionManager.HandController.AddCardToHand(card);
            }

            RefreshStagedCardLayout();
            UpdateCounterText();
        }

        private void UpdateCounterText()
        {
            if (counterTextField != null)
                counterTextField.text = $"{_stagedCards.Count}/{_maxCards}";
        }
    }
}
