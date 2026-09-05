using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Collection;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Managers
{
    public class CollectionManager : MonoBehaviour
    {
        public CollectionManager(){}
      
        public static CollectionManager Instance { get; private set; }

        [Header("Controllers")] 
        [SerializeField] private HandController handController;


        #region Cache

        public List<CardData> DrawPile { get; private set; } = new List<CardData>();
        public List<CardData> HandPile { get; private set; } = new List<CardData>();
        public List<CardData> DiscardPile { get; private set; } = new List<CardData>();
        
        public List<CardData> ExhaustPile { get; private set; } = new List<CardData>();
        public HandController HandController => handController;
        public int CardsPlayedThisTurn { get; private set; }
        protected FxManager FxManager => FxManager.Instance;
        protected AudioManager AudioManager => AudioManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        protected CombatManager CombatManager => CombatManager.Instance;

        protected UIManager UIManager => UIManager.Instance;

        #endregion
       
        #region Setup
        private void Awake()
        {
            if (Instance)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
            }
        }

        private void Start()
        {
            if (CombatManager != null)
                CombatManager.OnAllyTurnStarted += ResetCardsPlayedThisTurn;
        }

        private void OnDisable()
        {
            if (CombatManager != null)
                CombatManager.OnAllyTurnStarted -= ResetCardsPlayedThisTurn;
        }

        public void RegisterCardPlayed()
        {
            CardsPlayedThisTurn++;
        }

        private void ResetCardsPlayedThisTurn()
        {
            CardsPlayedThisTurn = 0;
        }

        #endregion

        #region Public Methods
        public void DrawCards(int targetDrawCount)
        {
            // If the current main ally has a NoDraw debuff, prevent drawing.
            if (CombatManager != null && CombatManager.CurrentMainAlly != null)
            {
                var stats = CombatManager.CurrentMainAlly.CharacterStats;
                if (stats.StatusDict.ContainsKey(NueGames.NueDeck.Scripts.Enums.StatusType.NoDraw) && stats.StatusDict[NueGames.NueDeck.Scripts.Enums.StatusType.NoDraw].IsActive)
                {
                    // Provide player feedback: small floating text
                    if (FxManager != null)
                    {
                        FxManager.SpawnStaticText(CombatManager.CurrentMainAlly.transform, "Can't Draw", 0, 1);
                    }
                    return;
                }
            }

            var currentDrawCount = 0;

            for (var i = 0; i < targetDrawCount; i++)
            {
                if (GameManager.GameplayData.MaxCardOnHand<=HandPile.Count)
                    return;
                
                if (DrawPile.Count <= 0)
                {
                    var nDrawCount = targetDrawCount - currentDrawCount;
                    
                    if (nDrawCount >= DiscardPile.Count) 
                        nDrawCount = DiscardPile.Count;
                    
                    ReshuffleDiscardPile();
                    DrawCards(nDrawCount);
                    break;
                }

                var randomCard = DrawPile[Random.Range(0, DrawPile.Count)];
                var clone = GameManager.BuildAndGetCard(randomCard, HandController.drawTransform);
                HandController.AddCardToHand(clone);
                HandPile.Add(randomCard);
                DrawPile.Remove(randomCard);
                currentDrawCount++;
                UIManager.CombatCanvas.SetPileTexts();
            }
            
            foreach (var cardObject in HandController.hand)
                cardObject.UpdateCardText();
        }

        public void AddEndlessChambersCards(CharacterBase ally)
        {
            if (ally == null || GameManager == null || GameManager.GameplayData == null ||
                GameManager.GameplayData.AllCardsList == null || HandController == null)
                return;

            var endlessChambers = ally.CharacterStats.StatusDict[StatusType.EndlessChambers];
            if (!endlessChambers.IsActive || endlessChambers.StatusValue <= 0)
                return;

            CardData quickDrawCard = null;
            foreach (var card in GameManager.GameplayData.AllCardsList)
            {
                if (card == null) continue;

                if (string.Equals(card.Id, "11_2_QuickDraw", System.StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(card.CardName, "Quick Draw", System.StringComparison.OrdinalIgnoreCase))
                {
                    quickDrawCard = card;
                    break;
                }
            }

            if (quickDrawCard == null)
            {
                Debug.LogWarning("[EndlessChambers] Quick Draw is not included in Gameplay Settings > All Cards List.");
                return;
            }

            for (var index = 0; index < endlessChambers.StatusValue; index++)
            {
                if (GameManager.GameplayData.MaxCardOnHand <= HandPile.Count)
                    break;

                var cardClone = GameManager.BuildAndGetCard(quickDrawCard, HandController.transform);
                HandController.AddCardToHand(cardClone);
                HandPile.Add(quickDrawCard);
            }

            foreach (var cardObject in HandController.hand)
                cardObject.UpdateCardText();

            if (UIManager != null && UIManager.CombatCanvas != null)
                UIManager.CombatCanvas.SetPileTexts();
        }

        public void DiscardHand()
        {
            var cardsToDiscard = new List<CardBase>();

            foreach (var cardBase in new List<CardBase>(HandController.hand))
            {
                if (cardBase == null || cardBase.CardData == null || cardBase.CardData.Retain)
                    continue;

                cardBase.Discard();
                cardsToDiscard.Add(cardBase);
            }

            foreach (var cardBase in cardsToDiscard)
                HandController.hand.Remove(cardBase);

            HandController.ClampSelectionState();
        }
        
        public void OnCardDiscarded(CardBase targetCard)
        {
            HandPile.Remove(targetCard.CardData);
            DiscardPile.Add(targetCard.CardData);
            UIManager.CombatCanvas.SetPileTexts();
        }
        
        public void OnCardExhausted(CardBase targetCard)
        {
            HandPile.Remove(targetCard.CardData);
            ExhaustPile.Add(targetCard.CardData);
            UIManager.CombatCanvas.SetPileTexts();
        }
        public void OnCardPlayed(CardBase targetCard)
        {
            // If the card requested to be returned to hand after play, add it back instead of discarding/exhausting
            if (targetCard.ReturnToHandAfterPlay)
            {
                if (targetCard.ResetPlayCountWhenReturnedToHand)
                {
                    // Reset any per-turn play-cost scaling when the card returns to hand.
                    targetCard.ResetPlayCountThisTurn();
                }
                // Visual: add the card GameObject back to hand controller
                HandController.AddCardToHand(targetCard);
                // Update UI
                UIManager.CombatCanvas.SetPileTexts();
            }
            else if (targetCard.CardData.ExhaustAfterPlay)
            {
                targetCard.Exhaust();
            }
            else
            {
                targetCard.Discard();
            }

            // Reset the flags after the card has been processed so they do not persist on the same instance.
            targetCard.ReturnToHandAfterPlay = false;
            targetCard.ResetPlayCountWhenReturnedToHand = false;

            foreach (var cardObject in HandController.hand)
                cardObject.UpdateCardText();
        }
        public void SetGameDeck()
        {
            foreach (var i in GameManager.PersistentGameplayData.CurrentCardsList) 
                DrawPile.Add(i);
        }

        public void ClearPiles()
        {
            DiscardPile.Clear();
            DrawPile.Clear();
            HandPile.Clear();
            ExhaustPile.Clear();
            HandController.hand.Clear();
        }
        #endregion

        #region Private Methods
        private void ReshuffleDiscardPile()
        {
            foreach (var i in DiscardPile) 
                DrawPile.Add(i);
            
            DiscardPile.Clear();
        }
        private void ReshuffleDrawPile()
        {
            foreach (var i in DrawPile) 
                DiscardPile.Add(i);
            
            DrawPile.Clear();
        }
        #endregion

    }
}