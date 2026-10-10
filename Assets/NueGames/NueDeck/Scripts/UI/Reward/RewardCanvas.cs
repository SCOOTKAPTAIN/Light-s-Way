using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NueGames.NueDeck.Scripts.Card;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Data.Collection.RewardData;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.NueExtentions;
using NueGames.NueDeck.ThirdParty.NueTooltip.Core;
using UnityEngine;
using UnityEngine.UI;

namespace NueGames.NueDeck.Scripts.UI.Reward
{
    public class RewardCanvas : CanvasBase
    {
        protected override bool BlocksBackgroundInput => true;
        private const int CardChoiceCount = 3;
        private const string GoldColor = "#FFD700";
        private const string HealColor = "#62E26F";

        [Header("References")]
        [SerializeField] private RewardContainerData rewardContainerData;
        [SerializeField] private Transform rewardRoot;
        [SerializeField] private RewardContainer rewardContainerPrefab;
        [SerializeField] private Transform rewardPanelRoot;
        [SerializeField] private Sprite metabolismRewardIcon;
        [SerializeField] private Sprite proficiencyRewardIcon;
        [SerializeField] private Sprite insightRewardIcon;
        [Header("Choice")]
        [SerializeField] private Transform choice2DCardSpawnRoot;
        [SerializeField] private ChoiceCard choiceCardUIPrefab;
        [SerializeField] private ChoicePanel choicePanel;
        
        private readonly List<RewardContainer> _currentRewardsList = new List<RewardContainer>();
        private readonly List<ChoiceCard> _spawnedChoiceList = new List<ChoiceCard>();
        private readonly List<CardData> _cardRewardList = new List<CardData>();
        private Button _choiceNextButton;

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
                BuildEliteBossProficiencyReward();
                BuildEliteBossInsightReward();
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
            BuildEliteBossProficiencyReward();
            BuildEliteBossInsightReward();
        }

        public bool OpenCardReward(string poolName, Action onSelected)
        {
            if (rewardContainerData == null || rewardContainerData.CardRewardDataList == null)
            {
                Debug.LogWarning("Cannot open a card reward because RewardContainerData is missing.", this);
                return false;
            }

            var cardRewardData = rewardContainerData.CardRewardDataList.Find(
                reward => reward != null && string.Equals(reward.name, poolName, StringComparison.OrdinalIgnoreCase));
            if (cardRewardData == null)
            {
                Debug.LogWarning($"No card reward pool named '{poolName}' was found.", this);
                return false;
            }

            return OpenCardReward(cardRewardData, onSelected);
        }

        public bool OpenDefaultCardReward(Action onSelected)
        {
            if (rewardContainerData == null || rewardContainerData.CardRewardDataList == null || rewardContainerData.CardRewardDataList.Count == 0)
            {
                Debug.LogWarning("Cannot open the default card reward because RewardContainerData is missing or empty.", this);
                return false;
            }

            var cardRewardList = rewardContainerData.GetRandomCardRewardList(out var cardRewardData);
            if (cardRewardData == null || cardRewardList == null || cardRewardList.Count == 0)
            {
                Debug.LogWarning("Cannot open the default card reward because no cards were generated.", this);
                return false;
            }

            return OpenCardReward(cardRewardData, onSelected);
        }

        public bool OpenCardReward(CardRewardData cardRewardData, Action onSelected)
        {
            if (cardRewardData == null)
            {
                Debug.LogWarning("Cannot open a card reward because the card reward pool is missing.", this);
                return false;
            }

            _cardRewardList.Clear();
            _cardRewardList.AddRange(cardRewardData.GetWeightedRandomCards(CardChoiceCount));
            while (_cardRewardList.Count < CardChoiceCount)
            {
                var fallbackCard = cardRewardData.GetWeightedRandomCard();
                if (fallbackCard == null)
                    break;

                _cardRewardList.Add(fallbackCard);
            }
            if (_cardRewardList.Count == 0)
            {
                Debug.LogWarning($"Card reward pool '{cardRewardData.name}' does not contain any cards.", this);
                return false;
            }

            OpenCanvas();
            ShowCardChoices(_cardRewardList.Count, () =>
            {
                CloseCanvas();
                onSelected?.Invoke();
            });
            return true;
        }

        public bool OpenCardReward(CardRewardData cardRewardData, int rounds, int choicesPerRound, Action onSelected)
        {
            if (cardRewardData == null || rounds <= 0 || choicesPerRound <= 0)
            {
                Debug.LogWarning("Cannot open a multi-round card reward because its pool or choice settings are invalid.", this);
                return false;
            }

            var allCards = cardRewardData.GetWeightedRandomCards(rounds * choicesPerRound);
            while (allCards.Count < rounds * choicesPerRound)
            {
                var fallbackCard = cardRewardData.GetWeightedRandomCard();
                if (fallbackCard == null)
                    break;

                allCards.Add(fallbackCard);
            }

            if (allCards.Count == 0)
            {
                Debug.LogWarning($"Card reward pool '{cardRewardData.name}' does not contain any cards.", this);
                return false;
            }

            OpenCanvas();
            ShowCardRewardRound(allCards, 0, rounds, choicesPerRound, onSelected);
            return true;
        }

        private void ShowCardRewardRound(
            List<CardData> allCards,
            int roundIndex,
            int rounds,
            int choicesPerRound,
            Action onSelected)
        {
            _cardRewardList.Clear();
            int startIndex = roundIndex * choicesPerRound;
            int availableChoices = Mathf.Min(choicesPerRound, allCards.Count - startIndex);
            for (int i = 0; i < availableChoices; i++)
                _cardRewardList.Add(allCards[startIndex + i]);

            ShowCardChoices(_cardRewardList.Count, () =>
            {
                if (roundIndex + 1 < rounds && (roundIndex + 1) * choicesPerRound < allCards.Count)
                {
                    ShowCardRewardRound(allCards, roundIndex + 1, rounds, choicesPerRound, onSelected);
                    return;
                }

                CloseCanvas();
                onSelected?.Invoke();
            });
        }

        public override void CloseCanvas()
        {
            TooltipManager.Instance?.HideTooltip();
            SetChoiceNextButtonVisible(true);
            base.CloseCanvas();
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
                case RewardType.Insight:
                    BuildInsightReward(rewardClone);
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

        private void BuildEliteBossProficiencyReward()
        {
            var gameplayData = GameManager.PersistentGameplayData;
            if (!gameplayData.EliteBossProficiencyRewardArmed)
                return;

            gameplayData.EliteBossProficiencyRewardArmed = false;

            var rewardClone = Instantiate(rewardContainerPrefab, rewardRoot);
            _currentRewardsList.Add(rewardClone);
            rewardClone.BuildReward(proficiencyRewardIcon, "Gain <color=#FFD700>1 Proficiency</color>");
            rewardClone.RewardButton.onClick.AddListener(() => GetEliteBossProficiencyReward(rewardClone));
        }

        private void BuildEliteBossInsightReward()
        {
            int encounterTypeIndex = GameManager.PersistentGameplayData.CurrentEncounterTypeIndex;
            if (encounterTypeIndex != 1 && encounterTypeIndex != 2)
                return;

            BuildReward(RewardType.Insight);
        }

        private void BuildInsightReward(RewardContainer rewardContainer)
        {
            rewardContainer.BuildReward(insightRewardIcon, "Gain <color=#62E2FF>1 Insight</color>");
            rewardContainer.RewardButton.onClick.AddListener(() => GetInsightReward(rewardContainer));
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
            ShowCardChoices(Mathf.Min(amount, _cardRewardList.Count), null);
            _currentRewardsList.Remove(rewardContainer);
            Destroy(rewardContainer.gameObject);
        }

        private void ShowCardChoices(int amount, Action onSelected)
        {
            ChoicePanel.gameObject.SetActive(true);
            SetChoiceNextButtonVisible(false);

            for (int i = 0; i < amount; i++)
            {
                Transform spawnTransform = choice2DCardSpawnRoot;
                var choice = Instantiate(choiceCardUIPrefab, spawnTransform);
                var reward = _cardRewardList.RandomItem();
                choice.BuildReward(reward);
                choice.OnCardChose += () =>
                {
                    ResetChoice();
                    onSelected?.Invoke();
                };

                _cardRewardList.Remove(reward);
                _spawnedChoiceList.Add(choice);
            }
        }

        private void SetChoiceNextButtonVisible(bool isVisible)
        {
            if (_choiceNextButton == null && ChoicePanel != null)
            {
                var nextButtonTransform = ChoicePanel.transform.Find("Midground/NextButton");
                if (nextButtonTransform != null)
                    _choiceNextButton = nextButtonTransform.GetComponent<Button>();
            }

            if (_choiceNextButton != null)
                _choiceNextButton.gameObject.SetActive(isVisible);
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

        private void GetEliteBossProficiencyReward(RewardContainer rewardContainer)
        {
            var gameplayData = GameManager.PersistentGameplayData;
            gameplayData.Proficiency += 1;
            UIManager.InformationCanvas?.SetProficiencyText(gameplayData.Proficiency);
            UIManager.InformationCanvas?.SetStatsProficiencyText(gameplayData.Proficiency);

            _currentRewardsList.Remove(rewardContainer);
            Destroy(rewardContainer.gameObject);
        }

        private void GetInsightReward(RewardContainer rewardContainer)
        {
            var gameplayData = GameManager.PersistentGameplayData;
            gameplayData.Insight += 1;
            UIManager.InformationCanvas?.SetInsightText(gameplayData.Insight);
            UIManager.InformationCanvas?.RefreshStatsText();

            _currentRewardsList.Remove(rewardContainer);
            Destroy(rewardContainer.gameObject);
        }
        #endregion
        
    }
}