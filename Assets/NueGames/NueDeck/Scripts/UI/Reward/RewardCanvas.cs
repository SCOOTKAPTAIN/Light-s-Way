using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Data.Collection.RewardData;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.NueExtentions;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.UI.Reward
{
    public class RewardCanvas : CanvasBase
    {
        protected override bool BlocksBackgroundInput => true;
        private const string GoldColor = "#FFD700";
        private const string HealColor = "#62E26F";

        [Header("References")]
        [SerializeField] private RewardContainerData rewardContainerData;
        [SerializeField] private Transform rewardRoot;
        [SerializeField] private RewardContainer rewardContainerPrefab;
        [SerializeField] private Transform rewardPanelRoot;
        [SerializeField] private Sprite metabolismRewardIcon;
        [Header("Choice")]
        [SerializeField] private Transform choice2DCardSpawnRoot;
        [SerializeField] private ChoiceCard choiceCardUIPrefab;
        [SerializeField] private ChoicePanel choicePanel;
        
        private readonly List<RewardContainer> _currentRewardsList = new List<RewardContainer>();
        private readonly List<ChoiceCard> _spawnedChoiceList = new List<ChoiceCard>();
        private readonly List<CardData> _cardRewardList = new List<CardData>();

        public ChoicePanel ChoicePanel => choicePanel;
        
        #region Public Methods

        public void PrepareCanvas()
        {
            rewardPanelRoot.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Builds rewards based on encounter configuration, or uses defaults if none specified.
        /// </summary>
        public void BuildRewardsForEncounter(EnemyEncounter encounter)
        {
            if (encounter == null || !encounter.HasCustomRewards)
            {
                // Use default rewards
                BuildReward(RewardType.Gold);
                BuildReward(RewardType.Card);
                BuildMetabolismReward();
                return;
            }
            
            // Build custom gold rewards
            if (encounter.CustomGoldRewards != null)
            {
                foreach (var goldRewardData in encounter.CustomGoldRewards)
                {
                    BuildCustomGoldReward(goldRewardData);
                }
            }
            
            // Build custom card rewards
            if (encounter.CustomCardRewards != null)
            {
                foreach (var cardRewardData in encounter.CustomCardRewards)
                {
                    BuildCustomCardReward(cardRewardData);
                }
            }

            BuildMetabolismReward();
        }
        
        public void BuildReward(RewardType rewardType)
        {
            var rewardClone = Instantiate(rewardContainerPrefab, rewardRoot);
            _currentRewardsList.Add(rewardClone);
            
            switch (rewardType)
            {
                case RewardType.Gold:
                    var rewardGold = rewardContainerData.GetRandomGoldReward(out var goldRewardData);
                    rewardClone.BuildReward(goldRewardData.RewardSprite, ColorGoldWord(goldRewardData.RewardDescription));
                    rewardClone.RewardButton.onClick.AddListener(()=>GetGoldReward(rewardClone,rewardGold));
                    break;
                case RewardType.Card:
                    var rewardCardList = rewardContainerData.GetRandomCardRewardList(out var cardRewardData);
                    _cardRewardList.Clear();
                    foreach (var cardData in rewardCardList)
                        _cardRewardList.Add(cardData);
                    rewardClone.BuildReward(cardRewardData.RewardSprite,cardRewardData.RewardDescription);
                    rewardClone.RewardButton.onClick.AddListener(()=>GetCardReward(rewardClone,3));
                    break;
                case RewardType.Relic:
                    break;
                case RewardType.MetabolismHeal:
                    if (GameManager.PersistentGameplayData.Metabolism <= 0)
                    {
                        _currentRewardsList.Remove(rewardClone);
                        Destroy(rewardClone.gameObject);
                        return;
                    }

                    BuildMetabolismReward(rewardClone);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(rewardType), rewardType, null);
            }
        }
        
        public override void ResetCanvas()
        {
            ResetRewards();

            ResetChoice();
        }

        private void ResetRewards()
        {
            foreach (var rewardContainer in _currentRewardsList)
                Destroy(rewardContainer.gameObject);

            _currentRewardsList?.Clear();
        }

        private void ResetChoice()
        {
            foreach (var choice in _spawnedChoiceList)
            {
                Destroy(choice.gameObject);
            }

            _spawnedChoiceList?.Clear();
            ChoicePanel.DisablePanel();
        }

        #endregion
        
        #region Private Methods
        
        private void BuildCustomGoldReward(GoldRewardData goldRewardData)
        {
            var rewardClone = Instantiate(rewardContainerPrefab, rewardRoot);
            _currentRewardsList.Add(rewardClone);
            
            var goldAmount = UnityEngine.Random.Range(goldRewardData.MinGold, goldRewardData.MaxGold);
            rewardClone.BuildReward(goldRewardData.RewardSprite, ColorGoldWord(goldRewardData.RewardDescription));
            rewardClone.RewardButton.onClick.AddListener(() => GetGoldReward(rewardClone, goldAmount));
        }

        private void BuildMetabolismReward()
        {
            if (GameManager.PersistentGameplayData.Metabolism > 0)
                BuildReward(RewardType.MetabolismHeal);
        }

        private void BuildMetabolismReward(RewardContainer rewardContainer)
        {
            var ally = CombatManager != null ? CombatManager.CurrentMainAlly : null;
            var stats = ally != null ? ally.CharacterStats : null;
            var metabolism = GameManager.PersistentGameplayData.Metabolism;
            var healAmount = stats != null
                ? Mathf.Max(1, Mathf.RoundToInt(stats.MaxHealth * metabolism * 0.02f))
                : 0;

            var recoveryPercent = $"<color={HealColor}>{metabolism * 2}%</color>";
            var recoveryAmount = $"<color={HealColor}>{healAmount}</color>";
            rewardContainer.BuildReward(metabolismRewardIcon, $"Recover {recoveryPercent} Health ({recoveryAmount})");
            rewardContainer.RewardButton.onClick.AddListener(() => GetMetabolismReward(rewardContainer, healAmount));
        }

        private static string ColorGoldWord(string description)
        {
            return Regex.Replace(
                description ?? string.Empty,
                @"\bgold\b",
                $"<color={GoldColor}>Gold</color>",
                RegexOptions.IgnoreCase);
        }
        
        private void BuildCustomCardReward(CardRewardData cardRewardData)
        {
            var rewardClone = Instantiate(rewardContainerPrefab, rewardRoot);
            _currentRewardsList.Add(rewardClone);
            
            // Use weighted random selection for card rewards
            _cardRewardList.Clear();
            var selectedCards = cardRewardData.GetWeightedRandomCards(3);
            foreach (var cardData in selectedCards)
                _cardRewardList.Add(cardData);
            
            rewardClone.BuildReward(cardRewardData.RewardSprite, cardRewardData.RewardDescription);
            rewardClone.RewardButton.onClick.AddListener(() => GetCardReward(rewardClone, 3));
        }
        
        private void GetGoldReward(RewardContainer rewardContainer,int amount)
        {
            GameManager.PersistentGameplayData.CurrentGold += amount;
            _currentRewardsList.Remove(rewardContainer);
            UIManager.InformationCanvas.SetGoldText(GameManager.PersistentGameplayData.CurrentGold);
            Destroy(rewardContainer.gameObject);
        }

        private void GetCardReward(RewardContainer rewardContainer,int amount = 3)
        {
            ChoicePanel.gameObject.SetActive(true);
            
            for (int i = 0; i < amount; i++)
            {
                Transform spawnTransform = choice2DCardSpawnRoot;
              
                var choice = Instantiate(choiceCardUIPrefab, spawnTransform);
                
                var reward = _cardRewardList.RandomItem();
                choice.BuildReward(reward);
                choice.OnCardChose += ResetChoice;
                
                _cardRewardList.Remove(reward);
                _spawnedChoiceList.Add(choice);
                _currentRewardsList.Remove(rewardContainer);
                
            }
            
            Destroy(rewardContainer.gameObject);
        }

        private void GetMetabolismReward(RewardContainer rewardContainer, int amount)
        {
            var ally = CombatManager != null ? CombatManager.CurrentMainAlly : null;
            if (ally != null && ally.CharacterStats != null)
            {
                ally.CharacterStats.HealWithPopup(amount);
                GameManager.PersistentGameplayData.SetAllyHealthData(
                    ally.AllyCharacterData.CharacterID,
                    ally.CharacterStats.CurrentHealth,
                    ally.CharacterStats.MaxHealth);
            }

            _currentRewardsList.Remove(rewardContainer);
            Destroy(rewardContainer.gameObject);
        }
        #endregion
        
    }
}