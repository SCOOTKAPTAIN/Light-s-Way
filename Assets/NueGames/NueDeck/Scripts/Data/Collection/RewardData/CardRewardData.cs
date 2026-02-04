using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Data.Collection.RewardData
{
    [CreateAssetMenu(fileName = "Card Reward Data",menuName = "NueDeck/Collection/Rewards/CardRW",order = 0)]
    public class CardRewardData : RewardDataBase
    {
        [Header("Card Pools by Rarity")]
        [SerializeField] private List<CardData> commonCards;
        [SerializeField] private List<CardData> uncommonCards;
        [SerializeField] private List<CardData> rareCards;
        [SerializeField] private List<CardData> mysticCards;
        
        [Header("Rarity Weights (0 = disabled)")]
        [Tooltip("If all weights are 0, uses equal chance for all cards")]
        [SerializeField] private float commonWeight = 60f;
        [SerializeField] private float uncommonWeight = 30f;
        [SerializeField] private float rareWeight = 9f;
        [SerializeField] private float mysticWeight = 1f;
        
        // Property to combine all cards (for backwards compatibility if needed)
        public List<CardData> RewardCardList
        {
            get
            {
                var allCards = new List<CardData>();
                if (commonCards != null) allCards.AddRange(commonCards);
                if (uncommonCards != null) allCards.AddRange(uncommonCards);
                if (rareCards != null) allCards.AddRange(rareCards);
                if (mysticCards != null) allCards.AddRange(mysticCards);
                return allCards;
            }
        }
        
        /// <summary>
        /// Gets a random card from the pool, weighted by rarity.
        /// </summary>
        public CardData GetWeightedRandomCard()
        {
            // Calculate total weight based on available cards in each rarity pool
            float totalWeight = 0f;
            if (commonCards != null && commonCards.Count > 0) totalWeight += commonWeight;
            if (uncommonCards != null && uncommonCards.Count > 0) totalWeight += uncommonWeight;
            if (rareCards != null && rareCards.Count > 0) totalWeight += rareWeight;
            if (mysticCards != null && mysticCards.Count > 0) totalWeight += mysticWeight;
            
            // If no weights or no cards, use equal chance from all available cards
            if (totalWeight <= 0f)
            {
                var allCards = RewardCardList;
                if (allCards.Count == 0) return null;
                return allCards[Random.Range(0, allCards.Count)];
            }
            
            // Select a rarity based on weights
            float randomValue = Random.Range(0f, totalWeight);
            float cumulative = 0f;
            
            if (commonCards != null && commonCards.Count > 0)
            {
                cumulative += commonWeight;
                if (randomValue <= cumulative)
                {
                    return commonCards[Random.Range(0, commonCards.Count)];
                }
            }
            
            if (uncommonCards != null && uncommonCards.Count > 0)
            {
                cumulative += uncommonWeight;
                if (randomValue <= cumulative)
                {
                    return uncommonCards[Random.Range(0, uncommonCards.Count)];
                }
            }
            
            if (rareCards != null && rareCards.Count > 0)
            {
                cumulative += rareWeight;
                if (randomValue <= cumulative)
                {
                    return rareCards[Random.Range(0, rareCards.Count)];
                }
            }
            
            if (mysticCards != null && mysticCards.Count > 0)
            {
                return mysticCards[Random.Range(0, mysticCards.Count)];
            }
            
            // Fallback to any available card
            var fallbackCards = RewardCardList;
            return fallbackCards.Count > 0 ? fallbackCards[Random.Range(0, fallbackCards.Count)] : null;
        }
        
        /// <summary>
        /// Gets multiple unique weighted random cards.
        /// </summary>
        public List<CardData> GetWeightedRandomCards(int count)
        {
            var selectedCards = new List<CardData>();
            
            // Create separate pools that can be modified
            var availableCommon = commonCards != null ? new List<CardData>(commonCards) : new List<CardData>();
            var availableUncommon = uncommonCards != null ? new List<CardData>(uncommonCards) : new List<CardData>();
            var availableRare = rareCards != null ? new List<CardData>(rareCards) : new List<CardData>();
            var availableMystic = mysticCards != null ? new List<CardData>(mysticCards) : new List<CardData>();
            
            for (int i = 0; i < count; i++)
            {
                // Calculate current available weight
                float totalWeight = 0f;
                if (availableCommon.Count > 0) totalWeight += commonWeight;
                if (availableUncommon.Count > 0) totalWeight += uncommonWeight;
                if (availableRare.Count > 0) totalWeight += rareWeight;
                if (availableMystic.Count > 0) totalWeight += mysticWeight;
                
                // If no more cards available, stop
                if (totalWeight <= 0f && availableCommon.Count == 0 && availableUncommon.Count == 0 && 
                    availableRare.Count == 0 && availableMystic.Count == 0)
                {
                    break;
                }
                
                // If no weights, use equal chance
                if (totalWeight <= 0f)
                {
                    var allAvailable = new List<CardData>();
                    allAvailable.AddRange(availableCommon);
                    allAvailable.AddRange(availableUncommon);
                    allAvailable.AddRange(availableRare);
                    allAvailable.AddRange(availableMystic);
                    
                    if (allAvailable.Count > 0)
                    {
                        var card = allAvailable[Random.Range(0, allAvailable.Count)];
                        selectedCards.Add(card);
                        
                        // Remove from appropriate pool
                        if (availableCommon.Contains(card)) availableCommon.Remove(card);
                        else if (availableUncommon.Contains(card)) availableUncommon.Remove(card);
                        else if (availableRare.Contains(card)) availableRare.Remove(card);
                        else if (availableMystic.Contains(card)) availableMystic.Remove(card);
                    }
                    continue;
                }
                
                // Select rarity based on weight
                float randomValue = Random.Range(0f, totalWeight);
                float cumulative = 0f;
                CardData selectedCard = null;
                
                if (availableCommon.Count > 0)
                {
                    cumulative += commonWeight;
                    if (randomValue <= cumulative)
                    {
                        selectedCard = availableCommon[Random.Range(0, availableCommon.Count)];
                        availableCommon.Remove(selectedCard);
                    }
                }
                
                if (selectedCard == null && availableUncommon.Count > 0)
                {
                    cumulative += uncommonWeight;
                    if (randomValue <= cumulative)
                    {
                        selectedCard = availableUncommon[Random.Range(0, availableUncommon.Count)];
                        availableUncommon.Remove(selectedCard);
                    }
                }
                
                if (selectedCard == null && availableRare.Count > 0)
                {
                    cumulative += rareWeight;
                    if (randomValue <= cumulative)
                    {
                        selectedCard = availableRare[Random.Range(0, availableRare.Count)];
                        availableRare.Remove(selectedCard);
                    }
                }
                
                if (selectedCard == null && availableMystic.Count > 0)
                {
                    selectedCard = availableMystic[Random.Range(0, availableMystic.Count)];
                    availableMystic.Remove(selectedCard);
                }
                
                if (selectedCard != null)
                {
                    selectedCards.Add(selectedCard);
                }
            }
            
            return selectedCards;
        }
    }
}