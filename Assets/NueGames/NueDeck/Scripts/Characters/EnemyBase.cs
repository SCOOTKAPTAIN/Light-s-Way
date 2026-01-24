using System.Collections;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Data.Containers;
using NueGames.NueDeck.Scripts.EnemyBehaviour;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Interfaces;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.NueExtentions;
using System.Linq;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Characters
{
    public class EnemyBase : CharacterBase, IEnemy
    {
        [Header("Enemy Base References")]
        [SerializeField] protected EnemyCharacterData enemyCharacterData;
        [SerializeField] protected EnemyCanvas enemyCanvas;
        [SerializeField] protected SoundProfileData deathSoundProfileData;
        protected EnemyAbilityData NextAbility;
        
        public EnemyCharacterData EnemyCharacterData => enemyCharacterData;
        public EnemyCanvas EnemyCanvas => enemyCanvas;
        public SoundProfileData DeathSoundProfileData => deathSoundProfileData;

        #region Setup
        public override void BuildCharacter()
        {
            base.BuildCharacter();
            EnemyCanvas.InitCanvas();
            CharacterStats = new CharacterStats(EnemyCharacterData.MaxHealth,EnemyCanvas);
            CharacterStats.OnDeath += OnDeath;
            CharacterStats.SetCurrentHealth(CharacterStats.CurrentHealth);
            CombatManager.OnAllyTurnStarted += ShowNextAbility;
            CombatManager.OnEnemyTurnStarted += CharacterStats.TriggerAllStatus;
            
            // Subscribe to player status changes to update intention value
            if (CombatManager.CurrentMainAlly != null)
            {
                CombatManager.CurrentMainAlly.CharacterStats.OnStatusChangedPublic += OnPlayerStatusChanged;
            }
            // Also subscribe to own status changes that affect damage (Strength, Weakness)
            CharacterStats.OnStatusChangedPublic += OnEnemyStatusChanged;
        }
        protected override void OnDeath()
        {
            base.OnDeath();
            CombatManager.OnAllyTurnStarted -= ShowNextAbility;
            CombatManager.OnEnemyTurnStarted -= CharacterStats.TriggerAllStatus;
            
            // Unsubscribe from status change events
            if (CombatManager.CurrentMainAlly != null)
            {
                CombatManager.CurrentMainAlly.CharacterStats.OnStatusChangedPublic -= OnPlayerStatusChanged;
            }
            CharacterStats.OnStatusChangedPublic -= OnEnemyStatusChanged;
           
            CombatManager.OnEnemyDeath(this);
            AudioManager.PlayOneShot(DeathSoundProfileData.GetRandomClip());
            Destroy(gameObject);
        }
        #endregion
        
        #region Private Methods

        private int _usedAbilityCount;
        private void ShowNextAbility()
        {
            NextAbility = EnemyCharacterData.GetAbility(_usedAbilityCount);
            
            // Reset cached action values for all actions in this ability
            foreach (var action in NextAbility.ActionList)
            {
                action.ResetCachedValue();
            }
            
            EnemyCanvas.IntentImage.sprite = NextAbility.Intention.IntentionSprite;
            
            if (NextAbility.HideActionValue)
            {
                EnemyCanvas.NextActionValueText.gameObject.SetActive(false);
            }
            else
            {
                EnemyCanvas.NextActionValueText.gameObject.SetActive(true);
                // Calculate displayed damage including strength, weakness, and target's fragile
                int displayedDamage = CalculateDisplayedDamage(NextAbility.ActionList[0].ActionValue);
                EnemyCanvas.NextActionValueText.text = displayedDamage.ToString();
            }

            _usedAbilityCount++;
            EnemyCanvas.IntentImage.gameObject.SetActive(true);
        }
        
        /// <summary>
        /// Calculates the damage that will be displayed in the intention text,
        /// accounting for enemy Strength, enemy Weakness, and target's Fragile stacks.
        /// </summary>
        private int CalculateDisplayedDamage(int baseValue)
        {
            var combatManager = CombatManager.Instance;
            if (combatManager == null || combatManager.CurrentMainAlly == null)
                return baseValue;
            
            var targetCharacter = combatManager.CurrentMainAlly;
            
            // Add enemy's Strength to base value
            var value = baseValue + CharacterStats.StatusDict[StatusType.Strength].StatusValue;
            
            // Apply Fragile and Pursuit modifiers based on target's status
            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, this, value));
            
            return value;
        }
        
        /// <summary>
        /// Updates the intention damage value when player statuses change (Fragile, Pursuit, etc).
        /// </summary>
        private void OnPlayerStatusChanged(StatusType statusType, int value)
        {
            // Only update if it's a status that affects damage calculation
            if (statusType == StatusType.Fragile || statusType == StatusType.Pursuit)
            {
                UpdateIntentionValue();
            }
        }
        
        /// <summary>
        /// Updates the intention damage value when enemy statuses change (Strength, Weakness, etc).
        /// </summary>
        private void OnEnemyStatusChanged(StatusType statusType, int value)
        {
            // Only update if it's a status that affects damage calculation
            if (statusType == StatusType.Strength || statusType == StatusType.Weak)
            {
                UpdateIntentionValue();
            }
        }
        
        /// <summary>
        /// Updates the displayed intention damage value in real-time.
        /// </summary>
        private void UpdateIntentionValue()
        {
            if (NextAbility == null || NextAbility.HideActionValue)
                return;
            
            int displayedDamage = CalculateDisplayedDamage(NextAbility.ActionList[0].ActionValue);
            EnemyCanvas.NextActionValueText.text = displayedDamage.ToString();
        }
        #endregion
        
        #region Action Routines
        public virtual IEnumerator ActionRoutine()
        {
            if (CharacterStats.IsStunned)
                yield break;

            Debug.Log($"ActionRoutine START for '{name}' with intent '{NextAbility?.Intention?.EnemyIntentionType}'");
            EnemyCanvas.IntentImage.gameObject.SetActive(false);
            if (NextAbility.Intention.EnemyIntentionType == EnemyIntentionType.Attack || NextAbility.Intention.EnemyIntentionType == EnemyIntentionType.Debuff)
            {
                yield return StartCoroutine(AttackRoutine(NextAbility));
            }
            else
            {
                yield return StartCoroutine(BuffRoutine(NextAbility));
            }
            Debug.Log($"ActionRoutine END for '{name}'");
        }
        
        protected virtual IEnumerator AttackRoutine(EnemyAbilityData targetAbility)
        {
            var waitFrame = new WaitForEndOfFrame();
            Debug.Log($"AttackRoutine START for '{name}' (ability: '{targetAbility?.Intention?.EnemyIntentionType}')");

            if (CombatManager == null) yield break;
            
            var aliveAllies = CombatManager.CurrentAlliesList.Where(a => a != null && !a.CharacterStats.IsDeath).ToList();
            if (aliveAllies.Count == 0) yield break;
            
            var target = aliveAllies.RandomItem();
            
            var startPos = transform.position;
            var endPos = target.transform.position;

            var startRot = transform.localRotation;
            var endRot = Quaternion.Euler(60, 0, 60);
            
            // Run movement inline so it completes correctly even if the enemy GameObject is destroyed mid-action.
            yield return MoveToTargetRoutine(waitFrame, startPos, endPos, startRot, endRot, 5);
          
            // Re-evaluate target in case it died while earlier actions ran.
            if (target == null || target.CharacterStats.IsDeath)
            {
                var fallbackAllies = CombatManager.CurrentAlliesList.Where(a => a != null && !a.CharacterStats.IsDeath).ToList();
                if (fallbackAllies.Count == 0)
                {
                    // Nothing to attack; return to start position and end routine.
                    Debug.LogWarning($"{name} had no allies to attack (all dead) — skipping action.");
                    // Return to origin; run inline to ensure completion even if the enemy is destroyed.
                    yield return MoveToTargetRoutine(waitFrame, endPos, startPos, endRot, startRot, 5);
                    yield break;
                }
                target = fallbackAllies.RandomItem();
                Debug.Log($"{name} switched attack target to '{target.name}' because original died.");
            }

            targetAbility.ActionList.ForEach(x => EnemyActionProcessor.GetAction(x.ActionType).DoAction(new EnemyActionParameters(x.ActionValue, target, this)));
            
            yield return StartCoroutine(MoveToTargetRoutine(waitFrame, endPos, startPos, endRot, startRot, 5));
            Debug.Log($"AttackRoutine END for '{name}'");
        }
        
        protected virtual IEnumerator BuffRoutine(EnemyAbilityData targetAbility)
        {
            var waitFrame = new WaitForEndOfFrame();
            
            var aliveEnemies = CombatManager.CurrentEnemiesList.Where(e => e != null && !e.CharacterStats.IsDeath).ToList();
            CharacterBase target;
            if (aliveEnemies.Count == 0)
            {
                // No alive enemies to buff; default to self to avoid invalid targets.
                target = this;
            }
            else
            {
                target = aliveEnemies.RandomItem();
            }
            
            var startPos = transform.position;
            var endPos = startPos+new Vector3(0,0.2f,0);
            
            var startRot = transform.localRotation;
            var endRot = transform.localRotation;
            
            // Run movement inline so it completes correctly even if the enemy GameObject is destroyed mid-action.
            yield return MoveToTargetRoutine(waitFrame, startPos, endPos, startRot, endRot, 5);
            
            // Re-evaluate target in case it died while earlier actions ran.
            if (target == null || target.CharacterStats.IsDeath)
            {
                var fallbackEnemies = CombatManager.CurrentEnemiesList.Where(e => e != null && !e.CharacterStats.IsDeath).ToList();
                if (fallbackEnemies.Count == 0)
                {
                    // No valid enemy targets; fallback to self.
                    Debug.LogWarning($"{name} had no enemy targets to buff — defaulting to self.");
                    target = this;
                }
                else
                {
                    target = fallbackEnemies.RandomItem();
                    Debug.Log($"{name} switched buff target to '{target.name}' because original died.");
                }
            }

            targetAbility.ActionList.ForEach(x => EnemyActionProcessor.GetAction(x.ActionType).DoAction(new EnemyActionParameters(x.ActionValue, target, this)));
            
            yield return MoveToTargetRoutine(waitFrame, endPos, startPos, endRot, startRot, 5);
            Debug.Log($"BuffRoutine END for '{name}'");
        }
        #endregion
        
        #region Other Routines
        private IEnumerator MoveToTargetRoutine(WaitForEndOfFrame waitFrame,Vector3 startPos, Vector3 endPos, Quaternion startRot, Quaternion endRot, float speed)
        {
            var timer = 0f;
            while (true)
            {
                timer += Time.deltaTime*speed;

                // Guard transform access in case the GameObject is destroyed mid-movement.
                if (this == null)
                {
                    if (timer >= 1f) break;
                    yield return waitFrame;
                    continue;
                }

                transform.position = Vector3.Lerp(startPos, endPos, timer);
                transform.localRotation = Quaternion.Lerp(startRot, endRot, timer);
                if (timer >= 1f)
                {
                    break;
                }

                yield return waitFrame;
            }
        }

        #endregion
    }
}