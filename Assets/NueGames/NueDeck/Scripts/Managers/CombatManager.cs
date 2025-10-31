﻿using System;
using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Characters.Enemies;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Utils.Background;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Managers
{
    public class CombatManager : MonoBehaviour
    {
        private CombatManager(){}
        public static CombatManager Instance { get; private set; }

        [Header("References")] 
        [SerializeField] private BackgroundContainer backgroundContainer;
        [SerializeField] private List<Transform> enemyPosList;
        [SerializeField] private List<Transform> allyPosList;
    [Header("UX Anchors")]
    [Tooltip("Designer: optional transform where played cards should animate to when used. If empty, card will animate to discardTransform.")]
    public Transform playAnchor;
    [Tooltip("How long (seconds) a card takes to move to the play anchor. Set to 0 to disable the move animation.")]
    public float cardPlayMoveDuration = 0.12f;
    [SerializeField] [Tooltip("Optional: assign an invisible Transform in the scene where group FX (eg. AllEnemies) should spawn.")]
    private Transform enemiesFxAnchor;
 
        
        #region Cache
        public List<EnemyBase> CurrentEnemiesList { get; private set; } = new List<EnemyBase>();
        public List<AllyBase> CurrentAlliesList { get; private set; }= new List<AllyBase>();

        public Action OnAllyTurnStarted;
        public Action OnEnemyTurnStarted;
        public List<Transform> EnemyPosList => enemyPosList;

        public List<Transform> AllyPosList => allyPosList;

    /// <summary>
    /// Optional designer-placed anchor used as a spawn point for group FX (AllEnemies/AllAllies).
    /// </summary>
    public Transform EnemiesFxAnchor => enemiesFxAnchor;

        public AllyBase CurrentMainAlly => CurrentAlliesList.Count>0 ? CurrentAlliesList[0] : null;

        public EnemyEncounter CurrentEncounter { get; private set; }
        
        public CombatStateType CurrentCombatStateType
        {
            get => _currentCombatStateType;
            private set
            {
                ExecuteCombatState(value);
                _currentCombatStateType = value;
            }
        }
        
        private CombatStateType _currentCombatStateType;
        protected FxManager FxManager => FxManager.Instance;
        protected AudioManager AudioManager => AudioManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        protected UIManager UIManager => UIManager.Instance;

        protected CollectionManager CollectionManager => CollectionManager.Instance;

        #endregion
        
        // Transient action context shared between card actions within a combat.
        // Use SetActionContext to store a value and TryGetActionContext/TryConsumeActionContext to read it.
        private readonly System.Collections.Generic.Dictionary<string, object> _actionContext = new System.Collections.Generic.Dictionary<string, object>();

        public void SetActionContext(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;
            _actionContext[key] = value;
        }

        public bool TryGetActionContext<T>(string key, out T value)
        {
            value = default;
            if (string.IsNullOrEmpty(key)) return false;
            if (_actionContext.TryGetValue(key, out var o) && o is T t)
            {
                value = t;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Try to get the context value and remove it (consume).
        /// </summary>
        public bool TryConsumeActionContext<T>(string key, out T value)
        {
            if (TryGetActionContext(key, out value))
            {
                _actionContext.Remove(key);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Clear transient action context. Call at end of turn/combat if needed.
        /// </summary>
        public void ClearActionContext()
        {
            _actionContext.Clear();
        }

        
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
                CurrentCombatStateType = CombatStateType.PrepareCombat;
            }
        }

        private void Start()
        {
            StartCombat();
        }

        public void StartCombat()
        {
            // Try to find a scene-placed FxAnchor so group FX can be spawned at a designer-chosen location.
            var fxAnchor = FindAnyObjectByType<NueGames.NueDeck.Scripts.Utils.FxAnchor>();
            if (fxAnchor != null)
                enemiesFxAnchor = fxAnchor.transform;

            BuildEnemies();
            BuildAllies();
            backgroundContainer.OpenSelectedBackground();
          
            CollectionManager.SetGameDeck();
           
            UIManager.CombatCanvas.gameObject.SetActive(true);
            UIManager.InformationCanvas.gameObject.SetActive(true);
            CurrentCombatStateType = CombatStateType.AllyTurn;
          //  CurrentMainAlly.CharacterStats.ApplyStatus(StatusType.Proficiency,GameManager.Instance.PersistentGameplayData.proficiency);
        }
        
        private void ExecuteCombatState(CombatStateType targetStateType)
        {
            switch (targetStateType)
            {
                case CombatStateType.PrepareCombat:
                    break;
                case CombatStateType.AllyTurn:

                    OnAllyTurnStarted?.Invoke();
                    if(!CurrentMainAlly){
                        return;
                    }
                    if (CurrentMainAlly.CharacterStats.IsStunned)
                    {
                        EndTurn();
                        return;
                    }
                    
                    GameManager.PersistentGameplayData.CurrentMana = GameManager.PersistentGameplayData.MaxMana;
                    // If the main ally has a NoGainMana status, do not refill mana at start of turn
                    if (CurrentMainAlly != null && CurrentMainAlly.CharacterStats.StatusDict[StatusType.NoGainMana].IsActive)
                    {
                        if (FxManager != null)
                            FxManager.SpawnStaticText(CurrentMainAlly.transform, "No Mana Gain", 0, 1);
                    }
                    else
                    {
                        GameManager.PersistentGameplayData.CurrentMana = GameManager.PersistentGameplayData.MaxMana;
                    }
                    CollectionManager.DrawCards(GameManager.PersistentGameplayData.DrawCount);
                    
                    GameManager.PersistentGameplayData.CanSelectCards = true;
                    
                    break;
                case CombatStateType.EnemyTurn:

                    OnEnemyTurnStarted?.Invoke();
                    
                    CollectionManager.DiscardHand();
                    
                    StartCoroutine(nameof(EnemyTurnRoutine));
                    
                    GameManager.PersistentGameplayData.CanSelectCards = false;
                    
                    break;
                case CombatStateType.EndCombat:
                    
                    GameManager.PersistentGameplayData.CanSelectCards = false;
                    
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetStateType), targetStateType, null);
            }
        }
        #endregion

        #region Public Methods
        public void EndTurn()
        {
            CurrentCombatStateType = CombatStateType.EnemyTurn;
        }
        public void OnAllyDeath(AllyBase targetAlly)
        {
            var targetAllyData = GameManager.PersistentGameplayData.AllyList.Find(x =>
                x.AllyCharacterData.CharacterID == targetAlly.AllyCharacterData.CharacterID);
            if (GameManager.PersistentGameplayData.AllyList.Count>1)
                GameManager.PersistentGameplayData.AllyList.Remove(targetAllyData);
            CurrentAlliesList.Remove(targetAlly);
            UIManager.InformationCanvas.ResetCanvas();
            if (CurrentAlliesList.Count<=0)
                LoseCombat();
        }
        public void OnEnemyDeath(EnemyBase targetEnemy)
        {
            CurrentEnemiesList.Remove(targetEnemy);
            if (CurrentEnemiesList.Count<=0)
                WinCombat();
        }
        public void DeactivateCardHighlights()
        {
            foreach (var currentEnemy in CurrentEnemiesList)
                currentEnemy.EnemyCanvas.SetHighlight(false);

            foreach (var currentAlly in CurrentAlliesList)
                currentAlly.AllyCanvas.SetHighlight(false);
        }
        public void IncreaseMana(int target)
        {
            // Respect NoGainMana on the current main ally
            if (CurrentMainAlly != null && CurrentMainAlly.CharacterStats.StatusDict[StatusType.NoGainMana].IsActive)
            {
                if (FxManager != null)
                    FxManager.SpawnStaticText(CurrentMainAlly.transform, "No Mana Gain", 0, 1);
                return;
            }

            GameManager.PersistentGameplayData.CurrentMana += target;
            UIManager.CombatCanvas.SetPileTexts();
        }

        public void RefillMana()
        {
            // Respect NoGainMana on the current main ally
            if (CurrentMainAlly != null && CurrentMainAlly.CharacterStats.StatusDict[StatusType.NoGainMana].IsActive)
            {
                if (FxManager != null)
                    FxManager.SpawnStaticText(CurrentMainAlly.transform, "No Mana Gain", 0, 1);
                return;
            }

            GameManager.PersistentGameplayData.CurrentMana = GameManager.PersistentGameplayData.MaxMana;
            UIManager.CombatCanvas.SetPileTexts();
        }
        public void HighlightCardTarget(ActionTargetType targetTypeTargetType)
        {
            switch (targetTypeTargetType)
            {
                case ActionTargetType.Enemy:
                    foreach (var currentEnemy in CurrentEnemiesList)
                        currentEnemy.EnemyCanvas.SetHighlight(true);
                    break;
                case ActionTargetType.Ally:
                    foreach (var currentAlly in CurrentAlliesList)
                        currentAlly.AllyCanvas.SetHighlight(true);
                    break;
                case ActionTargetType.AllEnemies:
                    foreach (var currentEnemy in CurrentEnemiesList)
                        currentEnemy.EnemyCanvas.SetHighlight(true);
                    break;
                case ActionTargetType.AllAllies:
                    foreach (var currentAlly in CurrentAlliesList)
                        currentAlly.AllyCanvas.SetHighlight(true);
                    break;
                case ActionTargetType.RandomEnemy:
                    foreach (var currentEnemy in CurrentEnemiesList)
                        currentEnemy.EnemyCanvas.SetHighlight(true);
                    break;
                case ActionTargetType.RandomAlly:
                    foreach (var currentAlly in CurrentAlliesList)
                        currentAlly.AllyCanvas.SetHighlight(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(targetTypeTargetType), targetTypeTargetType, null);
            }
        }
        #endregion
        
        #region Private Methods
        private void BuildEnemies()
        {

           // EncounterManager.instance.EncounterSelector();
            CurrentEncounter = GameManager.EncounterData.GetEnemyEncounter(
                GameManager.PersistentGameplayData.CurrentStageId,
                GameManager.PersistentGameplayData.CurrentEncounterId,
                GameManager.PersistentGameplayData.IsFinalEncounter);
            Debug.Log("Stage =" + GameManager.PersistentGameplayData.CurrentStageId + "Encounter ID = " + GameManager.PersistentGameplayData.CurrentEncounterId);
            
            var enemyList = CurrentEncounter.EnemyList;
            for (var i = 0; i < enemyList.Count; i++)
            {
                var clone = Instantiate(enemyList[i].EnemyPrefab, EnemyPosList.Count >= i ? EnemyPosList[i] : EnemyPosList[0]);
                clone.BuildCharacter();
                CurrentEnemiesList.Add(clone);
            }
        }
        private void BuildAllies()
        {
            for (var i = 0; i < GameManager.PersistentGameplayData.AllyList.Count; i++)
            {
                var clone = Instantiate(GameManager.PersistentGameplayData.AllyList[i], AllyPosList.Count >= i ? AllyPosList[i] : AllyPosList[0]);
                clone.BuildCharacter();
                CurrentAlliesList.Add(clone);
            }
        }
        private void LoseCombat()
        {
            if (CurrentCombatStateType == CombatStateType.EndCombat) return;
            
            CurrentCombatStateType = CombatStateType.EndCombat;
            
            CollectionManager.DiscardHand();
            CollectionManager.DiscardPile.Clear();
            CollectionManager.DrawPile.Clear();
            CollectionManager.HandPile.Clear();
            CollectionManager.HandController.hand.Clear();
            UIManager.CombatCanvas.gameObject.SetActive(true);
            UIManager.CombatCanvas.CombatLosePanel.SetActive(true);
        }
        private void WinCombat()
        {
            if (CurrentCombatStateType == CombatStateType.EndCombat) return;

            CurrentCombatStateType = CombatStateType.EndCombat;

            foreach (var allyBase in CurrentAlliesList)
            {
                GameManager.PersistentGameplayData.SetAllyHealthData(allyBase.AllyCharacterData.CharacterID,
                    allyBase.CharacterStats.CurrentHealth, allyBase.CharacterStats.MaxHealth);
            }

            CollectionManager.ClearPiles();


            if (GameManager.PersistentGameplayData.ActNumber == 11)
            {
                UIManager.CombatCanvas.CombatWinPanel.SetActive(true);
            }
            else
            {
                CurrentMainAlly.CharacterStats.ClearAllStatus();
                //GameManager.PersistentGameplayData.CurrentEncounterId++;
                UIManager.CombatCanvas.gameObject.SetActive(false);
                UIManager.RewardCanvas.gameObject.SetActive(true);
                UIManager.RewardCanvas.PrepareCanvas();
                UIManager.RewardCanvas.BuildReward(RewardType.Gold);
                UIManager.RewardCanvas.BuildReward(RewardType.Card);
            }

            GameManager.PersistentGameplayData.CurrentCardsList.RemoveAll(card => card.RemoveAfterBattle);

           
        }
        #endregion
        
        #region Routines
        private IEnumerator EnemyTurnRoutine()
        {
            var waitDelay = new WaitForSeconds(0.1f);

            foreach (var currentEnemy in CurrentEnemiesList)
            {
                yield return currentEnemy.StartCoroutine(nameof(EnemyExample.ActionRoutine));
                yield return waitDelay;
            }

            if (CurrentCombatStateType != CombatStateType.EndCombat)
                CurrentCombatStateType = CombatStateType.AllyTurn;
        }
        #endregion
    }
}