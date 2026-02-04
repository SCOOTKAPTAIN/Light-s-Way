using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI
{
    /// <summary>
    /// Canvas that shows 3 light-based cards (Rekindle, Shine, Flash) for player to select from.
    /// Costs 10 Light to draw one card into hand.
    /// </summary>
    public class LightCardSelectionPanel : CanvasBase
    {
        [Header("References")]
        [SerializeField] private Transform cardContainer;
        [SerializeField] private ChoiceCard choiceCardPrefab;
        [SerializeField] private Button closeButton;
        
        [Header("Card Data")]
        [SerializeField] private CardData rekindleCardData;
        [SerializeField] private CardData shineCardData;
        [SerializeField] private CardData flashCardData;
        
        [Header("Settings")]
        [SerializeField] private int lightCost = 10;
        [SerializeField] private float cardScale = 1f;
        [SerializeField] private float cardWidth = 200f;
        [SerializeField] private float cardHeight = 280f;
        
        private List<ChoiceCard> _displayedCards = new List<ChoiceCard>();
        
        private void Awake()
        {
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseCanvas);
            }
        }
        
        public override void OpenCanvas()
        {
            base.OpenCanvas(); // Activates the entire Canvas GameObject
            
            // Clear previous cards
            ClearDisplayedCards();
            
            // Create the 3 cards
            CreateLightCards();
            
            // Pause card selection during panel
            if (GameManager != null && GameManager.PersistentGameplayData != null)
            {
                GameManager.PersistentGameplayData.CanSelectCards = false;
            }
        }
        
        public override void CloseCanvas()
        {
            ClearDisplayedCards();
            
            // Re-enable card selection
            if (GameManager != null && GameManager.PersistentGameplayData != null)
            {
                GameManager.PersistentGameplayData.CanSelectCards = true;
            }
            
            base.CloseCanvas(); // Deactivates the entire Canvas GameObject
        }
        
        /// <summary>
        /// Called when player clicks a card. Adds it to hand and costs Light.
        /// </summary>
        public void OnCardSelected(CardData selectedCardData)
        {
            if (selectedCardData == null) return;
            
            // Check if player has enough Light
            if (GameManager == null || GameManager.PersistentGameplayData == null)
            {
                Debug.LogWarning("GameManager or PersistentGameplayData is null!");
                return;
            }
            
            if (GameManager.PersistentGameplayData.light < lightCost)
            {
                Debug.Log($"Not enough Light! Need {lightCost}, have {GameManager.PersistentGameplayData.light}");
                // Could add visual feedback here (shake, sound, etc.)
                return;
            }
            
            // Deduct Light
            GameManager.PersistentGameplayData.ChangeLight(-lightCost);
            
            // Create card instance and add to hand using GameManager's method
            if (GameManager != null && CollectionManager != null && CollectionManager.HandController != null)
            {
                var cardClone = GameManager.BuildAndGetCard(selectedCardData, CollectionManager.HandController.transform);
                CollectionManager.HandController.AddCardToHand(cardClone);
                
                Debug.Log($"Added {selectedCardData.CardName} to hand. Light remaining: {GameManager.PersistentGameplayData.light}");
            }
            
            // Close panel after selection
            CloseCanvas();
        }
        
        private void CreateLightCards()
        {
            if (cardContainer == null || choiceCardPrefab == null)
            {
                Debug.LogError("Card container or prefab not assigned!");
                return;
            }
            
            // Create Rekindle
            if (rekindleCardData != null)
            {
                CreateCardDisplay(rekindleCardData);
            }
            
            // Create Shine
            if (shineCardData != null)
            {
                CreateCardDisplay(shineCardData);
            }
            
            // Create Flash
            if (flashCardData != null)
            {
                CreateCardDisplay(flashCardData);
            }
        }
        
        private void CreateCardDisplay(CardData cardData)
        {
            var choiceCard = Instantiate(choiceCardPrefab, cardContainer);
            choiceCard.BuildReward(cardData);
            choiceCard.OnCardChose += () => OnCardSelected(cardData);
            
            // Apply custom scale
            choiceCard.transform.localScale = Vector3.one * cardScale;
            
            // Set size via RectTransform
            var rectTransform = choiceCard.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.sizeDelta = new Vector2(cardWidth, cardHeight);
            }
            
            // Add LayoutElement to control size in layout group
            var layoutElement = choiceCard.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = choiceCard.gameObject.AddComponent<LayoutElement>();
            }
            layoutElement.preferredWidth = cardWidth;
            layoutElement.preferredHeight = cardHeight;
            layoutElement.flexibleWidth = 0;
            layoutElement.flexibleHeight = 0;
            
            _displayedCards.Add(choiceCard);
        }
        
        private void ClearDisplayedCards()
        {
            foreach (var card in _displayedCards)
            {
                if (card != null)
                {
                    Destroy(card.gameObject);
                }
            }
            
            _displayedCards.Clear();
        }
        
        private void OnDestroy()
        {
            if (closeButton != null)
            {
                closeButton.onClick.RemoveListener(CloseCanvas);
            }
        }
    }
}
