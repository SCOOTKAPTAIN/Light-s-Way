using System;
using System.Collections.Generic;
using System.Linq;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Characters
{
    public class StatusStats
    { 
        public StatusType StatusType { get; set; }
        public int StatusValue { get; set; }
        public bool DecreaseOverTurn { get; set; } // If true, decrease on turn end
        public bool TriggerAtTurnEnd { get; set; } // If true, trigger and decrease at turn end instead of turn start
        public bool IsPermanent { get; set; } // If true, status can not be cleared during combat
        public bool IsActive { get; set; }
        public bool CanNegativeStack { get; set; }
        public bool ClearAtNextTurn { get; set; }
        
        public Action OnTriggerAction;
        public StatusStats(StatusType statusType,int statusValue,bool decreaseOverTurn = false, bool isPermanent = false,bool isActive = false,bool canNegativeStack = false,bool clearAtNextTurn = false, bool triggerAtTurnEnd = false)
        {
            StatusType = statusType;
            StatusValue = statusValue;
            DecreaseOverTurn = decreaseOverTurn;
            TriggerAtTurnEnd = triggerAtTurnEnd;
            IsPermanent = isPermanent;
            IsActive = isActive;
            CanNegativeStack = canNegativeStack;
            ClearAtNextTurn = clearAtNextTurn;
        }
    }
    public class CharacterStats
    { 
        // Shared list of status types considered debuffs.
        // Keep in one place so new logic (ClearDebuffs and debuff blocking) can reuse it.
        private static readonly StatusType[] DebuffTypes = new[]
        {
            StatusType.Poison,
            StatusType.Stun,
            StatusType.Frozen,
            StatusType.Fragile,
            StatusType.Weak,
            StatusType.Bleeding,
            StatusType.Frostbite,
            StatusType.Burning,
            StatusType.NoDraw,
            StatusType.NoGainMana,
            StatusType.Judged,
            StatusType.Obscured
        };
        private const float FrostbitePercentPerStack = 0.25f; // 25% proficiency per stack
        private const float BurningPercentPerStack = 0.25f; // 25% proficiency per stack
        public int MaxHealth { get; set; }
        public int CurrentHealth { get; set; }
        public bool IsStunned { get;  set; }
        public bool IsDeath { get; private set; }
       
        public Action OnDeath;
        public Action<int, int> OnHealthChanged;
        private readonly Action<StatusType,int> OnStatusChanged;
        private readonly Action<StatusType, int> OnStatusApplied;
        private readonly Action<StatusType> OnStatusCleared;
        public Action OnHealAction;
        public Action OnTakeDamageAction;
        // Public event for status changes that external systems can subscribe to
        public Action<StatusType, int> OnStatusChangedPublic;
        // Invoked when shield (Block) is gained. Passes the positive delta amount.
        public Action<int> OnShieldGained;
        
    public readonly Dictionary<StatusType, StatusStats> StatusDict = new Dictionary<StatusType, StatusStats>();

    // Reference to the canvas for spawning FX at the character's visual root.
    private readonly CharacterCanvas _characterCanvas;
        
        #region Setup
        public CharacterStats(int maxHealth, CharacterCanvas characterCanvas)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
            SetAllStatus();
            
            OnHealthChanged += characterCanvas.UpdateHealthText;
            OnStatusChanged += characterCanvas.UpdateStatusText;
            OnStatusApplied += characterCanvas.ApplyStatus;
            OnStatusCleared += characterCanvas.ClearStatus;
            OnShieldGained += characterCanvas.SpawnShieldGainedText;
            _characterCanvas = characterCanvas;
        }
        
        private void SetAllStatus()
        {
            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
                StatusDict.Add((StatusType) i, new StatusStats((StatusType) i, 0));

            StatusDict[StatusType.Poison].DecreaseOverTurn = true;
            StatusDict[StatusType.Poison].OnTriggerAction += DamagePoison;
            // Poison should trigger at turn end so it can be blocked by Block gained during the turn
            StatusDict[StatusType.Poison].TriggerAtTurnEnd = true;
           

            StatusDict[StatusType.Block].ClearAtNextTurn = true;

            // Armor: consumable counts that nullify one incoming non-piercing attack per stack
            StatusDict[StatusType.Armor].IsActive = false;
            StatusDict[StatusType.Armor].StatusValue = 0;

            StatusDict[StatusType.NoDraw].DecreaseOverTurn = true;

            StatusDict[StatusType.NoGainMana].DecreaseOverTurn = true;

            // Steady Barricade: persists for the combat; each stack retains +10 Block at turn start
            StatusDict[StatusType.SteadyBarricade].IsPermanent = true;

            // Mastermind: increases draw count for the combat while active; keep as combat-permanent
            StatusDict[StatusType.Mastermind].IsPermanent = true;

            StatusDict[StatusType.Strength].CanNegativeStack = true;
            StatusDict[StatusType.Fortitude].CanNegativeStack = true;
            
            StatusDict[StatusType.Stun].DecreaseOverTurn = true;
            StatusDict[StatusType.Stun].OnTriggerAction += CheckStunStatus;

            StatusDict[StatusType.Bleeding].OnTriggerAction += DamageBleeding;
            StatusDict[StatusType.Bleeding].CanNegativeStack = false;
            // Bleeding should trigger at turn end so it can be blocked by Block gained during the turn
            StatusDict[StatusType.Bleeding].TriggerAtTurnEnd = true;
           
            StatusDict[StatusType.Judged].DecreaseOverTurn = true;

            StatusDict[StatusType.Pursuit].DecreaseOverTurn = true;

            // Reactive 'if attacked this turn' statuses
            StatusDict[StatusType.FrozenMirror].DecreaseOverTurn = true;
            StatusDict[StatusType.BlazingSurge].DecreaseOverTurn = true;


            // Frostbite: deals percent of proficiency per stack each turn (unblockable)
            StatusDict[StatusType.Frostbite].OnTriggerAction += DamageFrostbite;
            StatusDict[StatusType.Frostbite].CanNegativeStack = false;

            // Burning: deals percent of proficiency per stack each turn (unblockable)
            StatusDict[StatusType.Burning].OnTriggerAction += DamageBurning;
            StatusDict[StatusType.Burning].CanNegativeStack = false;

            // Frozen: a separate stun-like status for 1 turn; will be checked at turn triggers
            StatusDict[StatusType.Frozen].DecreaseOverTurn = true;
            StatusDict[StatusType.Frozen].OnTriggerAction += CheckFrozenStatus;

            StatusDict[StatusType.Fragile].DecreaseOverTurn = true;
            // Fragile should trigger/clear at turn end so it persists through an enemy's attack
            StatusDict[StatusType.Fragile].TriggerAtTurnEnd = true;

            StatusDict[StatusType.Weak].DecreaseOverTurn = true;
            // Weakness should trigger/clear at turn end so it reduces an enemy's attack before decrementing
            StatusDict[StatusType.Weak].TriggerAtTurnEnd = true;
            
            // Obscured: debuff that should trigger/clear at turn end, not turn start
            StatusDict[StatusType.Obscured].TriggerAtTurnEnd = true;
            StatusDict[StatusType.Obscured].DecreaseOverTurn = true;

            
        }
        #endregion
        
        #region Public Methods
    public void ApplyStatus(StatusType targetStatus,int value, CharacterBase source = null)
        {
            // If this status being applied is a debuff and the character has a DebuffWard active,
            // consume one DebuffWard stack and block the incoming debuff instead of applying it.
            if (DebuffTypes.Contains(targetStatus))
            {
                if (StatusDict.ContainsKey(StatusType.DebuffWard) && StatusDict[StatusType.DebuffWard].IsActive && StatusDict[StatusType.DebuffWard].StatusValue > 0)
                {
                    // Consume one ward stack
                    StatusDict[StatusType.DebuffWard].StatusValue--;
                    // If it reached zero, clear it so UI updates correctly
                    if (StatusDict[StatusType.DebuffWard].StatusValue <= 0)
                        ClearStatus(StatusType.DebuffWard);
                    else
                        OnStatusChanged?.Invoke(StatusType.DebuffWard, StatusDict[StatusType.DebuffWard].StatusValue);

                    // Do not apply the incoming debuff
                    return;
                }
            }

            // compute previous value so we can detect positive deltas (useful for block floating text)
            var previousValue = StatusDict[targetStatus].IsActive ? StatusDict[targetStatus].StatusValue : 0;

            if (StatusDict[targetStatus].IsActive)
            {
                StatusDict[targetStatus].StatusValue += value;
                OnStatusChanged?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
                OnStatusChangedPublic?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
                
            }
            else
            {
                StatusDict[targetStatus].StatusValue = value;
                StatusDict[targetStatus].IsActive = true;
                OnStatusApplied?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
                OnStatusChangedPublic?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
            }

            // If this was Block, notify listeners about positive net gains
            if (targetStatus == StatusType.Block)
            {
                var delta = StatusDict[targetStatus].StatusValue - previousValue;
                if (delta > 0)
                    OnShieldGained?.Invoke(delta);
            }

            // Special handling for Frostbite/Burning thresholds (trigger when reaching 5 stacks)
            if (targetStatus == StatusType.Frostbite || targetStatus == StatusType.Burning)
            {
                while (StatusDict[targetStatus].StatusValue >= 5)
                {
                    // reduce stacks by 5 and trigger the associated effect
                    StatusDict[targetStatus].StatusValue -= 5;

                    // If status cleared to zero, remove it so UI and behavior update correctly
                    if (StatusDict[targetStatus].StatusValue <= 0)
                    {
                        ClearStatus(targetStatus);
                    }
                    else
                    {
                        OnStatusChanged?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
                    }

                    if (targetStatus == StatusType.Frostbite)
                    {
                        // Apply Frozen: 1 turn stun-like status
                        ApplyStatus(StatusType.Frozen, 1, source);
                        // Optional FX feedback
                        if (_characterCanvas != null && FxManager.Instance != null)
                        {
                            var charBase = _characterCanvas.GetComponentInParent<CharacterBase>();
                            var spawnRoot = (charBase != null && charBase.TextSpawnRoot != null) ? charBase.TextSpawnRoot : _characterCanvas.transform;
                            var fxPos = spawnRoot.position;
                            FxManager.Instance.PlayFx(spawnRoot, FxType.Frozen, new Vector3(0f,0f,0f));
                            Debug.Log($"PlayFx Frozen on '{charBase?.name ?? "unknown"}' at position {fxPos}, source {(source!=null?source.name:"null")}");
                            Debug.Log($"PlayFxAtPosition Frozen at {fxPos} for '{charBase?.name ?? "unknown"}', source {(source!=null?source.name:"null")}");
                        }
                        if (AudioManager.Instance != null)
                            AudioManager.Instance.PlayOneShotDebounced(AudioActionType.Frozen, 0.2f);
                    }
                    else
                    {
                        // Combustion: deal 5x Proficiency to all enemies and apply 1 Burning to each (can chain)
                        var proficiency = GameManager.Instance.PersistentGameplayData.proficiency;
                        // Use the status 'source' (owner) for Strength; fallback to this character if none
                        var ownerChar = source ?? (_characterCanvas != null ? _characterCanvas.GetComponentInParent<CharacterBase>() : null);
                        var strengthAmount = 0;
                        if (ownerChar != null)
                            strengthAmount = ownerChar.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

                        var damage = Mathf.RoundToInt(strengthAmount + proficiency * 5);
                        var combatManager = CombatManager.Instance;
                        if (combatManager != null)
                        {
                            // Iterate a snapshot to avoid collection-modified exceptions when enemies die during the loop
                            foreach (var enemy in combatManager.CurrentEnemiesList.ToList())
                            {
                                if (enemy == null || enemy.CharacterStats.IsDeath) continue;
                                // Apply 1 Burning to each enemy (this may chain combustions via ApplyStatus)
                                enemy.CharacterStats.ApplyStatus(StatusType.Burning, 1, ownerChar);
                                // Deal damage (unblockable) using owner's strength as part of the formula
                                enemy.CharacterStats.Damage(damage, true, "red", ownerChar);
                                Debug.Log($"Combustion dealing {damage} (Strength={strengthAmount}, Prof={proficiency}) to {enemy.name} from owner {(ownerChar!=null?ownerChar.name:"null")}");
                                // Play per-enemy combustion FX
                                if (FxManager.Instance != null)
                                {
                                    var enemyBase = enemy.GetComponent<CharacterBase>();
                                    var fxTarget = (enemyBase != null && enemyBase.TextSpawnRoot != null) ? enemyBase.TextSpawnRoot : enemy.transform;
                                    var fxPos = fxTarget.position;
                                    FxManager.Instance.PlayFx(fxTarget, FxType.Combustion, new Vector3(0f,0f,0f));
                                    Debug.Log($"PlayFx Combustion on '{enemy?.name ?? "unknown"}' at {fxPos} source {(ownerChar!=null?ownerChar.name:"null")}");
                                }
                            }
                        }
                        // Play global combustion audio once
                        if (AudioManager.Instance != null)
                            AudioManager.Instance.PlayOneShotDebounced(AudioActionType.Combustion, 0.25f);
                    }
                }
            }

            // Special handling: Mastermind increases draw count for the combat while active
            if (targetStatus == StatusType.Mastermind && value > 0)
            {
                var pgd = GameManager.Instance?.PersistentGameplayData;
                if (pgd != null)
                {
                    pgd.DrawCount += value;
                    Debug.Log($"Mastermind applied: increased draw count by {value} to {pgd.DrawCount}.");
                }
            }
            
            // Special handling: Obscured status obscures all cards in hand
            if (targetStatus == StatusType.Obscured)
            {
                var collectionManager = CollectionManager.Instance;
                if (collectionManager != null && collectionManager.HandController != null && collectionManager.HandController.hand != null)
                {
                    bool isObscured = StatusDict[StatusType.Obscured].StatusValue > 0;
                    foreach (var card in collectionManager.HandController.hand)
                    {
                        if (card != null)
                            card.SetObscuredState(isObscured);
                    }
                }
            }
        }
        
        public void OnObscuredStatusChanged()
        {
            // Called when Obscured status changes to update card visibility
            var collectionManager = CollectionManager.Instance;
            if (collectionManager != null && collectionManager.HandController != null && collectionManager.HandController.hand != null)
            {
                bool isObscured = StatusDict[StatusType.Obscured].StatusValue > 0;
                foreach (var card in collectionManager.HandController.hand)
                {
                    if (card != null)
                        card.SetObscuredState(isObscured);
                }
            }
        }
        
        /// <summary>
        /// Updates all cards currently in hand with the Obscured status state.
        /// Call this when cards are drawn to ensure they respect any active Obscured status.
        /// </summary>
        public void UpdateHandCardsObscuredState()
        {
            var collectionManager = CollectionManager.Instance;
            if (collectionManager != null && collectionManager.HandController != null && collectionManager.HandController.hand != null)
            {
                bool isObscured = StatusDict[StatusType.Obscured].StatusValue > 0;
                foreach (var card in collectionManager.HandController.hand)
                {
                    if (card != null)
                        card.SetObscuredState(isObscured);
                }
            }
        }
        public void TriggerAllStatus()
        {
            // Evaluate stun state for this turn BEFORE any decrement/clear happens, so stacks map to full turns.
            var willStunThisTurn =
                (StatusDict.ContainsKey(StatusType.Stun) && StatusDict[StatusType.Stun].StatusValue > 0) ||
                (StatusDict.ContainsKey(StatusType.Frozen) && StatusDict[StatusType.Frozen].StatusValue > 0);

            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
            {
                var statusType = (StatusType)i;
                // Skip turn-end statuses - they'll be handled in TriggerEndOfTurnStatuses()
                if (StatusDict[statusType].TriggerAtTurnEnd)
                    continue;
                    
                TriggerStatus(statusType);
            }

            // After processing all statuses (including decrement/clear), lock in the stun state for this turn
            // based on the pre-decrement snapshot so Stun stacks translate to full skipped turns.
            IsStunned = willStunThisTurn;
        }
        
        /// <summary>
        /// Triggers statuses that should activate at the END of a turn (e.g., Obscured).
        /// Call this at the end of a turn before transitioning to the next character's turn.
        /// </summary>
        public void TriggerEndOfTurnStatuses()
        {
            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
            {
                var statusType = (StatusType)i;
                if (StatusDict[statusType].TriggerAtTurnEnd)
                    TriggerStatus(statusType);
            }
        }
        
        public void SetCurrentHealth(int targetCurrentHealth)
        {
            CurrentHealth = targetCurrentHealth <=0 ? 1 : targetCurrentHealth;
            OnHealthChanged?.Invoke(CurrentHealth,MaxHealth);
        } 
        
        public void Heal(int value)
        {
            CurrentHealth += value;
            if (CurrentHealth>MaxHealth)  CurrentHealth = MaxHealth;
            OnHealthChanged?.Invoke(CurrentHealth,MaxHealth);
        }
        
        public void Damage(int value, bool canPierceArmor = false, string damageTextColor = "red", NueGames.NueDeck.Scripts.Characters.CharacterBase attacker = null)
        {
            if (IsDeath) return;
            OnTakeDamageAction?.Invoke();
            
            var healthBefore = CurrentHealth;
            var remainingDamage = value;
            var originalDamage = value;
            var wasBlockedCompletely = false;
            var wasNullifiedByArmor = false;
            var blockBefore = 0;
    
            // Check if target has Judged status - if so, bypass Block and Armor entirely
            bool hasJudged = StatusDict.ContainsKey(StatusType.Judged) && StatusDict[StatusType.Judged].IsActive && StatusDict[StatusType.Judged].StatusValue > 0;
            
            if (!hasJudged)
            {
                // Armor consumes a single attack instance entirely (if not piercing).
                if (!canPierceArmor)
                {
                    if (StatusDict.ContainsKey(StatusType.Armor) && StatusDict[StatusType.Armor].IsActive && StatusDict[StatusType.Armor].StatusValue > 0)
                    {
                        // Consume one armor stack and cancel damage
                        StatusDict[StatusType.Armor].StatusValue--;
                        if (StatusDict[StatusType.Armor].StatusValue <= 0)
                            ClearStatus(StatusType.Armor);
                        else
                            OnStatusChanged?.Invoke(StatusType.Armor, StatusDict[StatusType.Armor].StatusValue);

                        // Damage instance is nullified by armor
                        remainingDamage = 0;
                        wasNullifiedByArmor = true;

                        // Play small visual/audio feedback: spawn blue "Nulified!" text and a guard FX
                        if (_characterCanvas != null && FxManager.Instance != null)
                        {
                            var charBase = _characterCanvas.GetComponentInParent<CharacterBase>();
                            var spawnRoot = (charBase != null && charBase.TextSpawnRoot != null) ? charBase.TextSpawnRoot : _characterCanvas.transform;
                            FxManager.Instance.SpawnFloatingTextGrey(spawnRoot, "Nulified!");
                            // Play guard FX
                        }

                        // Play a debounced guard audio cue to avoid overlapping sounds
                        if (AudioManager.Instance != null)
                        {
                            AudioManager.Instance.PlayOneShotDebounced(AudioActionType.Armor, 0f);
                        }
                    }
                    else
                    {
                        if (StatusDict[StatusType.Block].IsActive)
                        {
                            blockBefore = StatusDict[StatusType.Block].StatusValue;
                            ApplyStatus(StatusType.Block, -value);

                            remainingDamage = 0;
                            if (StatusDict[StatusType.Block].StatusValue <= 0)
                            {
                                remainingDamage = StatusDict[StatusType.Block].StatusValue * -1;
                                ClearStatus(StatusType.Block);
                            }
                            
                            // Check if all damage was absorbed by block
                            if (blockBefore >= value)
                            {
                                wasBlockedCompletely = true;
                            }
                        }
                    }
                }
            }
            
            // If this target is Frozen and was hit by an attacker (not DOT/status), consume Frozen and apply Shatter effects.
            if (attacker != null && StatusDict.ContainsKey(StatusType.Frozen) && StatusDict[StatusType.Frozen].IsActive && StatusDict[StatusType.Frozen].StatusValue > 0)
            {
                // If armor nullified the attack, consume Frozen and trigger Shatter FX/audio but do not apply extra damage
                if (wasNullifiedByArmor)
                {
                    ClearStatus(StatusType.Frozen);
                    if (_characterCanvas != null && FxManager.Instance != null)
                    {
                        var charBase = _characterCanvas.GetComponentInParent<CharacterBase>();
                        var spawnRoot = (charBase != null && charBase.TextSpawnRoot != null) ? charBase.TextSpawnRoot : _characterCanvas.transform;
                        FxManager.Instance.PlayFx(_characterCanvas.transform, FxType.Shatter);
                        FxManager.Instance.SpawnFloatingTextOrange(spawnRoot, "Shatter Bonus!");
                    }
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlayOneShotDebounced(AudioActionType.Shatter, 0.2f);
                }
                else
                {
                    // If the target had Block, Shatter increases the attack's total damage (before block), potentially causing overflow.
                    if (blockBefore > 0)
                    {
                        var shatterTotal = Mathf.RoundToInt(originalDamage * 1.5f);
                        var newRemaining = shatterTotal - blockBefore;
                        if (newRemaining < 0) newRemaining = 0;
                        remainingDamage = newRemaining;

                        // Also consume any additional block caused by the shatter multiplier so block state is consistent
                        var extraToConsume = Mathf.Max(0, shatterTotal - originalDamage);
                        if (extraToConsume > 0)
                        {
                            ApplyStatus(StatusType.Block, -extraToConsume);
                        }

                        // Update wasBlockedCompletely to reflect whether the shatter total was fully absorbed
                        wasBlockedCompletely = blockBefore >= shatterTotal;
                    }
                    else
                    {
                        // No block: multiply whatever damage remains
                        remainingDamage = Mathf.RoundToInt(remainingDamage * 1.5f);
                    }

                    ClearStatus(StatusType.Frozen);
                    if (_characterCanvas != null && FxManager.Instance != null)
                    {
                        var charBase = _characterCanvas.GetComponentInParent<CharacterBase>();
                        var spawnRoot = (charBase != null && charBase.TextSpawnRoot != null) ? charBase.TextSpawnRoot : _characterCanvas.transform;
                        FxManager.Instance.PlayFx(_characterCanvas.transform, FxType.Shatter);
                        FxManager.Instance.SpawnFloatingTextOrange(spawnRoot, "Shatter Bonus!");
                    }
                    if (AudioManager.Instance != null)
                        AudioManager.Instance.PlayOneShotDebounced(AudioActionType.Shatter, 0.2f);
                }
            }

            CurrentHealth -= remainingDamage;
            
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                OnDeath?.Invoke();
                IsDeath = true;
            }
            
            // Spawn damage text based on actual health change (damage taken = health_before - health_after).
            // Exception: if the target dies, show the full remaining damage value instead of clamping to health.
            // Armor-negated damage shows no text; Block-negated + health loss still shows text.
            var actualDamageTaken = healthBefore - CurrentHealth;
            var displayDamage = actualDamageTaken;
            
            // If the target died and we have remaining damage that exceeded health, show the full remaining damage
            if (CurrentHealth == 0 && IsDeath && remainingDamage > healthBefore)
            {
                displayDamage = remainingDamage;
            }
            
            if (displayDamage > 0 && _characterCanvas != null && FxManager.Instance != null)
            {
                var charBase = _characterCanvas.GetComponentInParent<CharacterBase>();
                var spawnRoot = (charBase != null && charBase.TextSpawnRoot != null) ? charBase.TextSpawnRoot : _characterCanvas.transform;
                
                // Only spawn damage text if damageTextColor is specified (not empty/null)
                if (!string.IsNullOrEmpty(damageTextColor))
                {
                    if (damageTextColor == "yellow")
                        FxManager.Instance.SpawnFloatingTextYellow(spawnRoot, displayDamage.ToString());
                    else
                        FxManager.Instance.SpawnFloatingText(spawnRoot, displayDamage.ToString());
                }
            }
            
            // Show grey blocked number and 'Blocked!' together as one text when damage is fully absorbed by Block
            if (wasBlockedCompletely && _characterCanvas != null && FxManager.Instance != null)
            {
                var charBase = _characterCanvas.GetComponentInParent<CharacterBase>();
                var spawnRoot = (charBase != null && charBase.TextSpawnRoot != null) ? charBase.TextSpawnRoot : _characterCanvas.transform;
                // Calculate how much Block was consumed during this attack (including shatter extra consumption)
                var finalBlockValue = StatusDict.ContainsKey(StatusType.Block) && StatusDict[StatusType.Block].IsActive ? StatusDict[StatusType.Block].StatusValue : 0;
                var blockedAmount = blockBefore - finalBlockValue;
                if (blockedAmount < 0) blockedAmount = 0;
                FxManager.Instance.SpawnFloatingTextGrey(spawnRoot, blockedAmount.ToString() + "\nBlocked!");
                AudioManager.Instance.PlayOneShotDebounced(AudioActionType.BlockedHit, 0f);
            }
            
            OnHealthChanged?.Invoke(CurrentHealth,MaxHealth);

            // Reactive statuses: if this character had a FrozenMirror or BlazingSurge reflect active, apply the effects to attacker
            if (attacker != null)
            {
                // // If attacker has Sabotaged status, deal damage to self equal to Sabotaged value, then reduce Sabotaged by 1
                // if (attacker.CharacterStats.StatusDict.ContainsKey(StatusType.Sabotaged) && attacker.CharacterStats.StatusDict[StatusType.Sabotaged].IsActive && attacker.CharacterStats.StatusDict[StatusType.Sabotaged].StatusValue > 0)
                // {
                //     var sabotageValue = attacker.CharacterStats.StatusDict[StatusType.Sabotaged].StatusValue;
                //     attacker.CharacterStats.Damage(sabotageValue, false, "red", null);
                //     // Reduce Sabotaged by 1
                //     attacker.CharacterStats.ApplyStatus(StatusType.Sabotaged, -1);
                // }


                // Apply 1 Frostbite to the attacker if FrozenMirror is active
                if (StatusDict.ContainsKey(StatusType.FrozenMirror) && StatusDict[StatusType.FrozenMirror].IsActive && StatusDict[StatusType.FrozenMirror].StatusValue > 0)
                {
                    var ownerChar = _characterCanvas != null ? _characterCanvas.GetComponentInParent<CharacterBase>() : null;
                    attacker.CharacterStats.ApplyStatus(StatusType.Frostbite, 1, ownerChar);
                    // Play the mirrored effect on attacker
                    if (FxManager.Instance != null)
                    {
                        var attackerBase = attacker.GetComponent<CharacterBase>();
                        var attackerSpawn = (attackerBase != null && attackerBase.TextSpawnRoot != null) ? attackerBase.TextSpawnRoot : attacker.transform;
                        var pos = attackerSpawn.position;
                        FxManager.Instance.PlayFx(attackerSpawn, FxType.FrozenMirror2, new Vector3(0f,0f,0f));
                        Debug.Log($"PlayFx FrozenMirror2 on attacker '{attacker.name}' at position {pos}");
                      
                    }
                    AudioManager.Instance.PlayOneShotDebounced(AudioActionType.FrozenMirror2, 0f);
                }

                // Apply 1 Burning to the attacker if BlazingSurge is active
                if (StatusDict.ContainsKey(StatusType.BlazingSurge) && StatusDict[StatusType.BlazingSurge].IsActive && StatusDict[StatusType.BlazingSurge].StatusValue > 0)
                {
                    var ownerChar = _characterCanvas != null ? _characterCanvas.GetComponentInParent<CharacterBase>() : null;
                    attacker.CharacterStats.ApplyStatus(StatusType.Burning, 1, ownerChar);
                    // Play the mirrored effect on attacker
                    if (FxManager.Instance != null)
                    {
                        var attackerBase = attacker.GetComponent<CharacterBase>();
                        var attackerSpawn = (attackerBase != null && attackerBase.TextSpawnRoot != null) ? attackerBase.TextSpawnRoot : attacker.transform;
                        var pos2 = attackerSpawn.position;
                        FxManager.Instance.PlayFx(attackerSpawn, FxType.BlazingSurge2, new Vector3(0f,0f,0f));
                        Debug.Log($"PlayFx BlazingSurge2 on attacker '{attacker.name}' at position {pos2}");
                        
                    }
                    AudioManager.Instance.PlayOneShotDebounced(AudioActionType.BlazingSurge2, 0f);
                }
            }

        }
        
        public void IncreaseMaxHealth(int value)
        {
            MaxHealth += value;
            OnHealthChanged?.Invoke(CurrentHealth,MaxHealth);
        }

        public void ClearAllStatus()
        {
            foreach (var status in StatusDict)
                ClearStatus(status.Key);
        }

        /// <summary>
        /// Clear debuff statuses from this character. Add new debuffs to the DebuffTypes set to make them
        /// removable by this method.
        /// </summary>
    // Clears debuffs and returns how many distinct debuff types were removed.
    // Do not perform healing here; callers can decide whether to heal based on removedCount.
    public int ClearDebuffs()
        {
            var removedCount = 0;
            foreach (var debuff in DebuffTypes)
            {
                if (StatusDict.ContainsKey(debuff) && StatusDict[debuff].IsActive && !StatusDict[debuff].IsPermanent)
                {
                    ClearStatus(debuff);
                    removedCount++;
                }
            }

            return removedCount;
        }
           
        public void ClearStatus(StatusType targetStatus)
        {
            // If clearing Mastermind, revert the draw bonus it provided
            if (targetStatus == StatusType.Mastermind && StatusDict[targetStatus].IsActive && StatusDict[targetStatus].StatusValue > 0)
            {
                var pgd = GameManager.Instance?.PersistentGameplayData;
                if (pgd != null)
                {
                    pgd.DrawCount -= StatusDict[targetStatus].StatusValue;
                    if (pgd.DrawCount < 0) pgd.DrawCount = 0;
                    Debug.Log($"Mastermind cleared: decreased draw count by {StatusDict[targetStatus].StatusValue} to {pgd.DrawCount}.");
                }
            }
            
            // If clearing Obscured, remove the overlay from all cards
            if (targetStatus == StatusType.Obscured)
            {
                var collectionManager = CollectionManager.Instance;
                if (collectionManager != null && collectionManager.HandController != null && collectionManager.HandController.hand != null)
                {
                    foreach (var card in collectionManager.HandController.hand)
                    {
                        if (card != null)
                            card.SetObscuredState(false);
                    }
                }
            }

            StatusDict[targetStatus].IsActive = false;
            StatusDict[targetStatus].StatusValue = 0;
            OnStatusCleared?.Invoke(targetStatus);
        }

        #endregion

        #region Private Methods
        private void TriggerStatus(StatusType targetStatus)
        {
            StatusDict[targetStatus].OnTriggerAction?.Invoke();
            
            //One turn only statuses
            if (StatusDict[targetStatus].ClearAtNextTurn)
            {
                // Special behavior for Block: if SteadyBarricade stacks exist, preserve up to 10 * stacks of Block instead of clearing fully
                if (targetStatus == StatusType.Block && StatusDict.ContainsKey(StatusType.SteadyBarricade) && StatusDict[StatusType.SteadyBarricade].IsActive && StatusDict[StatusType.SteadyBarricade].StatusValue > 0)
                {
                    var stacks = StatusDict[StatusType.SteadyBarricade].StatusValue;
                    var retainCap = stacks * 10;
                    var currentBlock = StatusDict[StatusType.Block].StatusValue;
                    var newBlock = Math.Min(currentBlock, retainCap);

                    if (newBlock <= 0)
                    {
                        ClearStatus(StatusType.Block);
                        OnStatusChanged?.Invoke(StatusType.Block, 0);
                    }
                    else
                    {
                        StatusDict[StatusType.Block].StatusValue = newBlock;
                        OnStatusChanged?.Invoke(StatusType.Block, newBlock);
                    }

                    return;
                }
                else
                {
                    ClearStatus(targetStatus);
                    OnStatusChanged?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
                    return;
                }
            }
            
            //Check status
            if (StatusDict[targetStatus].StatusValue <= 0)
            {
                if (StatusDict[targetStatus].CanNegativeStack)
                {
                    if (StatusDict[targetStatus].StatusValue == 0 && !StatusDict[targetStatus].IsPermanent)
                        ClearStatus(targetStatus);
                }
                else
                {
                    if (!StatusDict[targetStatus].IsPermanent)
                        ClearStatus(targetStatus);
                }
            }
            
            // Only decrease status values for active statuses to prevent inactive ones from going negative
            if (StatusDict[targetStatus].DecreaseOverTurn && StatusDict[targetStatus].IsActive) 
                StatusDict[targetStatus].StatusValue--;
            
            if (StatusDict[targetStatus].StatusValue == 0)
                if (!StatusDict[targetStatus].IsPermanent)
                    ClearStatus(targetStatus);
            
            OnStatusChanged?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
        }


        private void DamagePoison()
        {
            if (StatusDict[StatusType.Poison].StatusValue <= 0) return;
            Damage(StatusDict[StatusType.Poison].StatusValue, true);
        }

        private void DamageBleeding()
        {
            if (StatusDict[StatusType.Bleeding].StatusValue <= 0) return;
            var bleedAmount = StatusDict[StatusType.Bleeding].StatusValue;

            // Play bleed FX at this character's canvas (if available)
            if (_characterCanvas != null && FxManager.Instance != null)
            {
                FxManager.Instance.PlayFx(_characterCanvas.transform, FxType.Bleed);

                // Also spawn red floating text showing the bleed damage amount at the character's text spawn root
                var charBase = _characterCanvas.GetComponentInParent<CharacterBase>();
                var spawnRoot = (charBase != null && charBase.TextSpawnRoot != null) ? charBase.TextSpawnRoot : _characterCanvas.transform;
              //  FxManager.Instance.SpawnFloatingText(spawnRoot, bleedAmount.ToString());
            }

            // Play bleed sound, debounced to avoid overlapping rapid ticks
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShotDebounced(AudioActionType.Bleed, 0.25f);
            }

            // Bleeding damage is blockable (canPierceArmor = false)
            Damage(bleedAmount, false);

             // Increase Bleeding by 1 stack before dealing damage
            StatusDict[StatusType.Bleeding].StatusValue++;
            OnStatusChanged?.Invoke(StatusType.Bleeding, StatusDict[StatusType.Bleeding].StatusValue);
            
        }

        private void DamageFrostbite()
        {
            if (StatusDict[StatusType.Frostbite].StatusValue <= 0) return;
            var stacks = StatusDict[StatusType.Frostbite].StatusValue;
            var proficiency = GameManager.Instance.PersistentGameplayData.proficiency;
            var damageFloat = proficiency * FrostbitePercentPerStack * stacks;
            var damage = Mathf.RoundToInt(damageFloat);
            // Ensure at least 1 damage per tick if stacks are present
            if (damage <= 0) damage = 1;

            // Spawn ice/poison FX and text
            if (_characterCanvas != null && FxManager.Instance != null)
            {
                FxManager.Instance.PlayFx(_characterCanvas.transform, FxType.Frostbite);
                var charBase = _characterCanvas.GetComponentInParent<CharacterBase>();
                var spawnRoot = (charBase != null && charBase.TextSpawnRoot != null) ? charBase.TextSpawnRoot : _characterCanvas.transform;
                //FxManager.Instance.SpawnFloatingText(spawnRoot, damage.ToString());
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayOneShotDebounced(AudioActionType.Frostbite, 0.25f);

            Damage(damage, true);
        }

        private void DamageBurning()
        {
            if (StatusDict[StatusType.Burning].StatusValue <= 0) return;
            var stacks = StatusDict[StatusType.Burning].StatusValue;
            var proficiency = GameManager.Instance.PersistentGameplayData.proficiency;
            var damageFloat = proficiency * BurningPercentPerStack * stacks;
            var damage = Mathf.RoundToInt(damageFloat);
            // Ensure at least 1 damage per tick if stacks are present
            if (damage <= 0) damage = 1;

            if (_characterCanvas != null && FxManager.Instance != null)
            {
                FxManager.Instance.PlayFx(_characterCanvas.transform, FxType.Burning);
                var charBase = _characterCanvas.GetComponentInParent<CharacterBase>();
                var spawnRoot = (charBase != null && charBase.TextSpawnRoot != null) ? charBase.TextSpawnRoot : _characterCanvas.transform;
                //FxManager.Instance.SpawnFloatingText(spawnRoot, damage.ToString());
            }

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayOneShotDebounced(AudioActionType.Burning, 0.25f);

            Damage(damage, true);
        }

        private void CheckFrozenStatus()
        {
            if (StatusDict[StatusType.Frozen].StatusValue <= 0)
            {
                IsStunned = false;
                return;
            }

            IsStunned = true;
        }
        
        public void CheckStunStatus()
        {
            if (StatusDict[StatusType.Stun].StatusValue <= 0)
            {
                IsStunned = false;
                return;
            }
            
            IsStunned = true;
        }
        
        #endregion
    }
}