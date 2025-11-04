using System;
using System.Collections;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Collection;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.NueExtentions;
using NueGames.NueDeck.Scripts.Utils;
using NueGames.NueDeck.ThirdParty.NueTooltip.Core;
using NueGames.NueDeck.ThirdParty.NueTooltip.CursorSystem;
using NueGames.NueDeck.ThirdParty.NueTooltip.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace NueGames.NueDeck.Scripts.Card
{
    public class CardBase : MonoBehaviour,I2DTooltipTarget, IPointerDownHandler, IPointerUpHandler
    {
        [Header("Base References")]
        [SerializeField] protected Transform descriptionRoot;
        [SerializeField] protected Image cardImage;
        [SerializeField] protected Image passiveImage;
        [SerializeField] protected TextMeshProUGUI nameTextField;
        [SerializeField] protected TextMeshProUGUI descTextField;
        [SerializeField] protected TextMeshProUGUI manaTextField;
        [SerializeField] protected List<RarityRoot> rarityRootList;
        

        #region Cache
        public CardData CardData { get; private set; }
        public bool IsInactive { get; protected set; }
        protected Transform CachedTransform { get; set; }
        protected WaitForEndOfFrame CachedWaitFrame { get; set; }
        public bool IsPlayable { get; protected set; } = true;

        public List<RarityRoot> RarityRootList => rarityRootList;
        protected FxManager FxManager => FxManager.Instance;
        protected AudioManager AudioManager => AudioManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        protected CombatManager CombatManager => CombatManager.Instance;
        protected CollectionManager CollectionManager => CollectionManager.Instance;
        
        public bool IsExhausted { get; private set; }

        #endregion
        
        #region Setup
        protected virtual void Awake()
        {
            CachedTransform = transform;
            CachedWaitFrame = new WaitForEndOfFrame();
        }

        public virtual void SetCard(CardData targetProfile,bool isPlayable = true)
        {
            CardData = targetProfile;
            IsPlayable = isPlayable;
            nameTextField.text = CardData.CardName;
            descTextField.text = CardData.MyDescription;
            // Show 0 cost when the player currently has a FreeNextCard status active (QoL overlay)
            var displayCost = CardData.ManaCost;
            var mainAlly = CombatManager?.CurrentMainAlly;
            if (mainAlly != null && mainAlly.CharacterStats.StatusDict.ContainsKey(StatusType.FreeNextCard) && mainAlly.CharacterStats.StatusDict[StatusType.FreeNextCard].IsActive && mainAlly.CharacterStats.StatusDict[StatusType.FreeNextCard].StatusValue > 0)
                displayCost = 0;

            manaTextField.text = displayCost.ToString();
            cardImage.sprite = CardData.CardSprite;
            foreach (var rarityRoot in RarityRootList)
                rarityRoot.gameObject.SetActive(rarityRoot.Rarity == CardData.Rarity);
        }
        
        #endregion
        
        #region Card Methods
        public virtual void Use(CharacterBase self,CharacterBase targetCharacter, List<EnemyBase> allEnemies, List<AllyBase> allAllies)
        {
            if (!IsPlayable) return;
         
            StartCoroutine(CardUseRoutine(self, targetCharacter, allEnemies, allAllies));
        }

        private IEnumerator CardUseRoutine(CharacterBase self,CharacterBase targetCharacter, List<EnemyBase> allEnemies, List<AllyBase> allAllies)
        {
            // Prevent the player from selecting or dragging other cards while this card's actions run.
            var prevCanSelect = GameManager.PersistentGameplayData.CanSelectCards;
            GameManager.PersistentGameplayData.CanSelectCards = false;

            SpendMana(CardData.ManaCost);
            // Animate the card to the play anchor (for visual feedback) before running actions.
            Transform playAnchor = CombatManager.playAnchor;
            if (playAnchor == null && CollectionManager != null && CollectionManager.HandController != null)
            {
                // Fallback to discard transform so card animates off-hand regardless.
                playAnchor = CollectionManager.HandController.discardTransform;
            }

            if (playAnchor != null)
            {
                // Start the play animation but don't wait for it — actions should start immediately when the player releases the card.
                StartCoroutine(AnimateToTransform(playAnchor, 0.18f));
            }
            
            foreach (var playerAction in CardData.CardActionDataList)
            {
                yield return new WaitForSeconds(playerAction.ActionDelay);
                var targetList = DetermineTargets(targetCharacter, allEnemies, allAllies, playerAction);

                foreach (var target in targetList)
                    CardActionProcessor.GetAction(playerAction.CardActionType)
                        .DoAction(new CardActionParameters(playerAction.ActionValue,
                            target,self,CardData,this));
            }
            CollectionManager.OnCardPlayed(this);

            // Restore previous selection state (usually true during player's turn).
            // If the combat state changed to EnemyTurn (e.g. a card ended the turn), do not re-enable selection.
            if (CombatManager != null && CombatManager.CurrentCombatStateType == CombatStateType.AllyTurn)
            {
                GameManager.PersistentGameplayData.CanSelectCards = prevCanSelect;
            }
        }

        private IEnumerator AnimateToTransform(Transform target, float duration)
        {
            if (target == null) yield break;
            var startPos = CachedTransform.position;
            var startRot = CachedTransform.rotation;
            var startScale = CachedTransform.localScale;

            var endPos = target.position;
            var endRot = target.rotation;
            var endScale = target.localScale;

            var t = 0f;
            while (t < duration)
            {
                t += Time.deltaTime;
                var p = Mathf.SmoothStep(0f, 1f, t / duration);
                CachedTransform.position = Vector3.Lerp(startPos, endPos, p);
                CachedTransform.rotation = Quaternion.Slerp(startRot, endRot, p);
                CachedTransform.localScale = Vector3.Lerp(startScale, endScale, p);
                yield return null;
            }

            CachedTransform.position = endPos;
            CachedTransform.rotation = endRot;
            CachedTransform.localScale = endScale;
        }

        private static List<CharacterBase> DetermineTargets(CharacterBase targetCharacter, List<EnemyBase> allEnemies, List<AllyBase> allAllies,
            CardActionData playerAction)
        {
            List<CharacterBase> targetList = new List<CharacterBase>();
            switch (playerAction.ActionTargetType)
            {
                case ActionTargetType.Enemy:
                    targetList.Add(targetCharacter);
                    break;
                case ActionTargetType.Ally:
                    targetList.Add(targetCharacter);
                    break;
                case ActionTargetType.AllEnemies:
                    foreach (var enemyBase in allEnemies)
                        targetList.Add(enemyBase);
                    break;
                case ActionTargetType.AllAllies:
                    foreach (var allyBase in allAllies)
                        targetList.Add(allyBase);
                    break;
                case ActionTargetType.RandomEnemy:
                    if (allEnemies.Count>0)
                        targetList.Add(allEnemies.RandomItem());
                    
                    break;
                case ActionTargetType.RandomAlly:
                    if (allAllies.Count>0)
                        targetList.Add(allAllies.RandomItem());
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return targetList;
        }
        
        public virtual void Discard()
        {
            if (IsExhausted) return;
            if (!IsPlayable) return;
            CollectionManager.OnCardDiscarded(this);
            StartCoroutine(DiscardRoutine());
        }
        
        public virtual void Exhaust(bool destroy = true)
        {
            if (IsExhausted) return;
            if (!IsPlayable) return;
            IsExhausted = true;
            CollectionManager.OnCardExhausted(this);
            StartCoroutine(ExhaustRoutine(destroy));
        }

        protected virtual void SpendMana(int value)
        {
            if (!IsPlayable) return;
            // Check if the main ally has a FreeNextCard status active. If so, consume one and skip spending mana.
            var mainAlly = CombatManager?.CurrentMainAlly;
            if (mainAlly != null)
            {
                if (mainAlly.CharacterStats.StatusDict.ContainsKey(StatusType.FreeNextCard) && mainAlly.CharacterStats.StatusDict[StatusType.FreeNextCard].IsActive && mainAlly.CharacterStats.StatusDict[StatusType.FreeNextCard].StatusValue > 0)
                {
                    // decrement the counter via ApplyStatus so events update, then clear if <= 0
                    mainAlly.CharacterStats.ApplyStatus(StatusType.FreeNextCard, -1);
                    if (mainAlly.CharacterStats.StatusDict[StatusType.FreeNextCard].StatusValue <= 0)
                        mainAlly.CharacterStats.ClearStatus(StatusType.FreeNextCard);

                    // skip spending mana
                    return;
                }
            }

            GameManager.PersistentGameplayData.CurrentMana -= value;

            // If the main ally has the God's Angel combat buff active, and the card cost was 2 or more,
            // refund 1 mana and heal 1 HP.
            if (mainAlly != null && mainAlly.CharacterStats.StatusDict.ContainsKey(StatusType.GodsAngelBuff) && mainAlly.CharacterStats.StatusDict[StatusType.GodsAngelBuff].IsActive && value >= 2)
            {
                GameManager.PersistentGameplayData.CurrentMana += 1;
                mainAlly.CharacterStats.Heal(1);
                // Optional feedback
                if (FxManager != null)
                {
                    FxManager.SpawnFloatingTextGreen(mainAlly.transform, "1");
                    FxManager.PlayFx(mainAlly.transform, FxType.GodsAngel2);
                    AudioManager.PlayOneShot(AudioActionType.GodsAngel2);
                }
            }
        }
        
        public virtual void SetInactiveMaterialState(bool isInactive) 
        {
            if (!IsPlayable) return;
            if (isInactive == this.IsInactive) return; 
            
            IsInactive = isInactive;
            passiveImage.gameObject.SetActive(isInactive);
        }
        
        public virtual void UpdateCardText()
        {
            CardData.UpdateDescription();
            nameTextField.text = CardData.CardName;
            descTextField.text = CardData.MyDescription;
            var displayCost = CardData.ManaCost;
            var mainAlly = CombatManager?.CurrentMainAlly;
            if (mainAlly != null && mainAlly.CharacterStats.StatusDict.ContainsKey(StatusType.FreeNextCard) && mainAlly.CharacterStats.StatusDict[StatusType.FreeNextCard].IsActive && mainAlly.CharacterStats.StatusDict[StatusType.FreeNextCard].StatusValue > 0)
                displayCost = 0;
            manaTextField.text = displayCost.ToString();
        }
        
        #endregion
        
        #region Routines
        protected virtual IEnumerator DiscardRoutine(bool destroy = true)
        {
            var timer = 0f;
            transform.SetParent(CollectionManager.HandController.discardTransform);
            
            var startPos = CachedTransform.localPosition;
            var endPos = Vector3.zero;

            var startScale = CachedTransform.localScale;
            var endScale = Vector3.zero;

            var startRot = CachedTransform.localRotation;
            var endRot = Quaternion.Euler(Random.value * 360, Random.value * 360, Random.value * 360);
            
            while (true)
            {
                timer += Time.deltaTime*5;

                CachedTransform.localPosition = Vector3.Lerp(startPos, endPos, timer);
                CachedTransform.localRotation = Quaternion.Lerp(startRot,endRot,timer);
                CachedTransform.localScale = Vector3.Lerp(startScale, endScale, timer);
                
                if (timer>=1f)  break;
                
                yield return CachedWaitFrame;
            }

            if (destroy)
                Destroy(gameObject);
           
        }
        
        protected virtual IEnumerator ExhaustRoutine(bool destroy = true)
        {
            var timer = 0f;
            transform.SetParent(CollectionManager.HandController.exhaustTransform);
            
            var startPos = CachedTransform.localPosition;
            var endPos = Vector3.zero;

            var startScale = CachedTransform.localScale;
            var endScale = Vector3.zero;

            var startRot = CachedTransform.localRotation;
            var endRot = Quaternion.Euler(Random.value * 360, Random.value * 360, Random.value * 360);
            
            while (true)
            {
                timer += Time.deltaTime*5;

                CachedTransform.localPosition = Vector3.Lerp(startPos, endPos, timer);
                CachedTransform.localRotation = Quaternion.Lerp(startRot,endRot,timer);
                CachedTransform.localScale = Vector3.Lerp(startScale, endScale, timer);
                
                if (timer>=1f)  break;
                
                yield return CachedWaitFrame;
            }

            if (destroy)
                Destroy(gameObject);
           
        }

        #endregion

        #region Pointer Events
        public virtual void OnPointerEnter(PointerEventData eventData)
        {
            ShowTooltipInfo();
        }

        public virtual void OnPointerExit(PointerEventData eventData)
        {
            HideTooltipInfo(TooltipManager.Instance);
        }

        public virtual void OnPointerDown(PointerEventData eventData)
        {
            HideTooltipInfo(TooltipManager.Instance);
        }

        public virtual void OnPointerUp(PointerEventData eventData)
        {
            ShowTooltipInfo();
        }
        #endregion

        #region Tooltip
        protected virtual void ShowTooltipInfo()
        {
            if (!descriptionRoot) return;
            if (CardData.KeywordsList.Count<=0) return;
           
            var tooltipManager = TooltipManager.Instance;
            foreach (var cardDataSpecialKeyword in CardData.KeywordsList)
            {
                var specialKeyword = tooltipManager.SpecialKeywordData.SpecialKeywordBaseList.Find(x=>x.SpecialKeyword == cardDataSpecialKeyword);
                if (specialKeyword != null)
                    ShowTooltipInfo(tooltipManager,specialKeyword.GetContent(),specialKeyword.GetHeader(),descriptionRoot,CursorType.Default,CollectionManager ? CollectionManager.HandController.cam : Camera.main);
            }
        }
        public virtual void ShowTooltipInfo(TooltipManager tooltipManager, string content, string header = "", Transform tooltipStaticTransform = null, CursorType targetCursor = CursorType.Default,Camera cam = null, float delayShow =0)
        {
            tooltipManager.ShowTooltip(content,header,tooltipStaticTransform,targetCursor,cam,delayShow);
        }

        public virtual void HideTooltipInfo(TooltipManager tooltipManager)
        {
            tooltipManager.HideTooltip();
        }
        #endregion
       
    }
}