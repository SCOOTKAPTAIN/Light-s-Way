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

        public int DrawAllCopies(CardData targetCard)
        {
            if (targetCard == null || HandController == null || GameManager == null || GameManager.GameplayData == null)
                return 0;

            var drawnCount = DrawMatchingCopies(DrawPile, targetCard);
            drawnCount += DrawMatchingCopies(DiscardPile, targetCard);
            drawnCount += DrawMatchingCopies(ExhaustPile, targetCard);

            foreach (var cardObject in HandController.hand)
                cardObject.UpdateCardText();

            if (UIManager != null && UIManager.CombatCanvas != null)
                UIManager.CombatCanvas.SetPileTexts();

            return drawnCount;
        }

        public bool MoveDrawCardToHand(CardData targetCard)
        {
            if (targetCard == null || HandController == null || GameManager == null ||
                GameManager.GameplayData == null || HandPile.Count >= GameManager.GameplayData.MaxCardOnHand)
                return false;

            var drawIndex = DrawPile.IndexOf(targetCard);
            if (drawIndex < 0)
                return false;

            DrawPile.RemoveAt(drawIndex);
            var cardClone = GameManager.BuildAndGetCard(targetCard, HandController.drawTransform);
            HandController.AddCardToHand(cardClone);
            HandPile.Add(targetCard);

            foreach (var cardObject in HandController.hand)
                cardObject.UpdateCardText();

            if (UIManager != null && UIManager.CombatCanvas != null)
                UIManager.CombatCanvas.SetPileTexts();

            return true;
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
                if (cardBase == null || cardBase.CardData == null)
                    continue;

                cardBase.ResolveEndTurnEffects(CombatManager != null ? CombatManager.CurrentMainAlly : null);
                if (cardBase.CardData.ExhaustAfterEndTurn)
                {
                    cardBase.Exhaust();
                    cardsToDiscard.Add(cardBase);
                }
                else if (!cardBase.CardData.Retain && !cardBase.TemporaryRetain)
                {
                    cardBase.Discard();
                    cardsToDiscard.Add(cardBase);
                }
            }

            foreach (var cardBase in cardsToDiscard)
                HandController.hand.Remove(cardBase);

            HandController.ClampSelectionState();
        }

        public int ExhaustHandAndDraw()
        {
            if (HandController == null)
                return 0;

            var cardsToExhaust = new List<CardBase>();
            foreach (var cardBase in new List<CardBase>(HandController.hand))
            {
                if (cardBase != null && cardBase.CardData != null)
                    cardsToExhaust.Add(cardBase);
            }

            foreach (var cardBase in cardsToExhaust)
                cardBase.Exhaust();

            foreach (var cardBase in cardsToExhaust)
                HandController.hand.Remove(cardBase);

            HandController.ClampSelectionState();
            DrawCards(cardsToExhaust.Count);
            return cardsToExhaust.Count;
        }

        public bool ExhaustRandomCard()
        {
            if (HandController == null)
                return false;

            var handCards = new List<CardBase>();
            foreach (var card in HandController.hand)
            {
                if (card != null && card.CardData != null && !card.IsExhausted)
                    handCards.Add(card);
            }

            var totalCards = handCards.Count + DrawPile.Count + DiscardPile.Count;
            if (totalCards == 0)
                return false;

            var selectedIndex = Random.Range(0, totalCards);
            if (selectedIndex < handCards.Count)
            {
                var card = handCards[selectedIndex];
                card.Exhaust(false);
                HandController.hand.Remove(card);
                HandController.ClampSelectionState();
                return true;
            }

            selectedIndex -= handCards.Count;
            var sourcePile = selectedIndex < DrawPile.Count ? DrawPile : DiscardPile;
            var pileIndex = selectedIndex < DrawPile.Count ? selectedIndex : selectedIndex - DrawPile.Count;
            var cardData = sourcePile[pileIndex];
            sourcePile.RemoveAt(pileIndex);
            ExhaustPile.Add(cardData);
            RevertVanguardStanceEntry(ExhaustPile);

            if (UIManager != null && UIManager.CombatCanvas != null)
                UIManager.CombatCanvas.SetPileTexts();

            return true;
        }

        // Piqued Interest: discards the selected Attack/Buff cards, then draws the same number of
        // opposite-category cards from the draw pile (falling back to a random card if the draw pile lacks one).
        public int ExchangeCardsForOppositeCategory(List<CardBase> selectedCards)
        {
            if (selectedCards == null || selectedCards.Count == 0 || HandController == null ||
                GameManager == null || GameManager.GameplayData == null)
                return 0;

            var neededCategories = new List<CardCategoryType>();
            foreach (var cardBase in selectedCards)
            {
                if (cardBase == null || cardBase.CardData == null)
                    continue;

                neededCategories.Add(GetOppositeCategory(cardBase.CardData.Category));
                cardBase.Discard();
            }

            var drawnCount = 0;
            var maxHandSize = GameManager.GameplayData.MaxCardOnHand;

            foreach (var category in neededCategories)
            {
                if (HandPile.Count >= maxHandSize)
                    break;

                var cardData = PopRandomCardOfCategory(DrawPile, category) ?? PopRandomCard(DrawPile);
                if (cardData == null)
                    continue;

                var clone = GameManager.BuildAndGetCard(cardData, HandController.drawTransform);
                HandController.AddCardToHand(clone);
                HandPile.Add(cardData);
                drawnCount++;
            }

            foreach (var cardObject in HandController.hand)
                cardObject.UpdateCardText();

            if (UIManager != null && UIManager.CombatCanvas != null)
                UIManager.CombatCanvas.SetPileTexts();

            return drawnCount;
        }
        
        public void OnCardDiscarded(CardBase targetCard)
        {
            HandPile.Remove(targetCard.CardData);
            DiscardPile.Add(targetCard.CardData);
            RevertVanguardStanceEntry(DiscardPile);
            UIManager.CombatCanvas.SetPileTexts();
        }
        
        public void OnCardExhausted(CardBase targetCard)
        {
            HandPile.Remove(targetCard.CardData);
            ExhaustPile.Add(targetCard.CardData);
            RevertVanguardStanceEntry(ExhaustPile);
            UIManager.CombatCanvas.SetPileTexts();
        }

        // Knight's Pride: converts every "Guard" card into "Vanguard Stance" across the deck/hand/discard piles.
        public void ConvertGuardCardsToVanguardStance()
        {
            var guardCard = FindCardDataByName(GuardCardName);
            var vanguardCard = FindCardDataByName(VanguardStanceCardName);
            if (guardCard == null || vanguardCard == null)
                return;

            ReplaceCardDataInPile(DrawPile, guardCard, vanguardCard);
            ReplaceCardDataInPile(HandPile, guardCard, vanguardCard);
            ReplaceCardDataInPile(DiscardPile, guardCard, vanguardCard);

            if (HandController != null && HandController.hand != null)
            {
                foreach (var cardBase in HandController.hand)
                {
                    if (cardBase != null && cardBase.CardData == guardCard)
                    {
                        cardBase.SetCard(vanguardCard, cardBase.IsPlayable);
                        cardBase.UpdateCardText();
                    }
                }
            }
        }

        public void RestoreVanguardStanceCardsToGuard()
        {
            var guardCard = FindCardDataByName(GuardCardName);
            var vanguardCard = FindCardDataByName(VanguardStanceCardName);
            if (guardCard == null || vanguardCard == null)
                return;

            ReplaceCardDataInPile(DrawPile, vanguardCard, guardCard);
            ReplaceCardDataInPile(HandPile, vanguardCard, guardCard);
            ReplaceCardDataInPile(DiscardPile, vanguardCard, guardCard);
            ReplaceCardDataInPile(ExhaustPile, vanguardCard, guardCard);

            var currentCards = GameManager?.PersistentGameplayData?.CurrentCardsList;
            ReplaceCardDataInPile(currentCards, vanguardCard, guardCard);

            if (HandController != null && HandController.hand != null)
            {
                foreach (var cardBase in HandController.hand)
                {
                    if (cardBase != null && cardBase.CardData == vanguardCard)
                    {
                        cardBase.SetCard(guardCard, cardBase.IsPlayable);
                        cardBase.UpdateCardText();
                    }
                }
            }
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
        private static CardCategoryType GetOppositeCategory(CardCategoryType category) =>
            category == CardCategoryType.Attack ? CardCategoryType.Skill : CardCategoryType.Attack;

        private static CardData PopRandomCardOfCategory(List<CardData> pile, CardCategoryType category)
        {
            List<int> matchIndexes = null;
            for (var i = 0; i < pile.Count; i++)
            {
                if (pile[i] != null && pile[i].Category == category)
                {
                    matchIndexes ??= new List<int>();
                    matchIndexes.Add(i);
                }
            }

            if (matchIndexes == null || matchIndexes.Count == 0)
                return null;

            var index = matchIndexes[Random.Range(0, matchIndexes.Count)];
            var cardData = pile[index];
            pile.RemoveAt(index);
            return cardData;
        }

        private static CardData PopRandomCard(List<CardData> pile)
        {
            if (pile.Count == 0)
                return null;

            var index = Random.Range(0, pile.Count);
            var cardData = pile[index];
            pile.RemoveAt(index);
            return cardData;
        }

        private int DrawMatchingCopies(List<CardData> sourcePile, CardData targetCard)
        {
            var drawnCount = 0;
            var maxHandSize = GameManager.GameplayData.MaxCardOnHand;

            for (var index = sourcePile.Count - 1; index >= 0; index--)
            {
                if (HandPile.Count >= maxHandSize)
                    break;

                if (sourcePile[index] != targetCard)
                    continue;

                sourcePile.RemoveAt(index);
                var cardClone = GameManager.BuildAndGetCard(targetCard, HandController.drawTransform);
                HandController.AddCardToHand(cardClone);
                HandPile.Add(targetCard);
                drawnCount++;
            }

            return drawnCount;
        }

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

        // Vanguard Stance is a single-use enhancement: once played, the copy that goes to the
        // discard/exhaust pile reverts back into a plain Guard card.
        private const string GuardCardName = "Guard";
        private const string VanguardStanceCardName = "Vanguard Stance";

        private void RevertVanguardStanceEntry(List<CardData> pile)
        {
            if (pile == null || pile.Count == 0)
                return;

            var lastIndex = pile.Count - 1;
            var lastEntry = pile[lastIndex];
            if (lastEntry == null || !string.Equals(lastEntry.CardName, VanguardStanceCardName, System.StringComparison.OrdinalIgnoreCase))
                return;

            var guardCard = FindCardDataByName(GuardCardName);
            if (guardCard != null)
                pile[lastIndex] = guardCard;
        }

        private static void ReplaceCardDataInPile(List<CardData> pile, CardData from, CardData to)
        {
            if (pile == null) return;
            for (var i = 0; i < pile.Count; i++)
            {
                if (pile[i] == from)
                    pile[i] = to;
            }
        }

        private CardData FindCardDataByName(string cardName)
        {
            if (GameManager == null || GameManager.GameplayData == null || GameManager.GameplayData.AllCardsList == null)
                return null;

            foreach (var card in GameManager.GameplayData.AllCardsList)
            {
                if (card != null && string.Equals(card.CardName, cardName, System.StringComparison.OrdinalIgnoreCase))
                    return card;
            }

            return null;
        }
        #endregion

    }
}