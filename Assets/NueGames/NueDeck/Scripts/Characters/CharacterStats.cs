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
        public bool IsPermanent { get; set; } // If true, status can not be cleared during combat
        public bool IsActive { get; set; }
        public bool CanNegativeStack { get; set; }
        public bool ClearAtNextTurn { get; set; }
        
        public Action OnTriggerAction;
        public StatusStats(StatusType statusType,int statusValue,bool decreaseOverTurn = false, bool isPermanent = false,bool isActive = false,bool canNegativeStack = false,bool clearAtNextTurn = false)
        {
            StatusType = statusType;
            StatusValue = statusValue;
            DecreaseOverTurn = decreaseOverTurn;
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
            StatusType.Fragile,
            StatusType.Bleeding,
            StatusType.NoDraw,
            StatusType.NoGainMana
        };
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

            StatusDict[StatusType.Block].ClearAtNextTurn = true;

            StatusDict[StatusType.NoDraw].DecreaseOverTurn = true;

            StatusDict[StatusType.NoGainMana].DecreaseOverTurn = true;

            StatusDict[StatusType.Strength].CanNegativeStack = true;
            StatusDict[StatusType.Fortitude].CanNegativeStack = true;
            
            StatusDict[StatusType.Stun].DecreaseOverTurn = true;
            StatusDict[StatusType.Stun].OnTriggerAction += CheckStunStatus;

            StatusDict[StatusType.Bleeding].DecreaseOverTurn = true;
            StatusDict[StatusType.Bleeding].OnTriggerAction += DamageBleeding;
            StatusDict[StatusType.Bleeding].CanNegativeStack = false;
            
        }
        #endregion
        
        #region Public Methods
    public void ApplyStatus(StatusType targetStatus,int value)
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
                
            }
            else
            {
                StatusDict[targetStatus].StatusValue = value;
                StatusDict[targetStatus].IsActive = true;
                OnStatusApplied?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
            }

            // If this was Block, notify listeners about positive net gains
            if (targetStatus == StatusType.Block)
            {
                var delta = StatusDict[targetStatus].StatusValue - previousValue;
                if (delta > 0)
                    OnShieldGained?.Invoke(delta);
            }
        }
        public void TriggerAllStatus()
        {
            for (int i = 0; i < Enum.GetNames(typeof(StatusType)).Length; i++)
                TriggerStatus((StatusType) i);
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
        
        public void Damage(int value, bool canPierceArmor = false)
        {
            if (IsDeath) return;
            OnTakeDamageAction?.Invoke();
            
            var remainingDamage = value;
    
            if (!canPierceArmor)
            {
                if (StatusDict[StatusType.Block].IsActive)
                {
                    ApplyStatus(StatusType.Block, -value);

                    remainingDamage = 0;
                    if (StatusDict[StatusType.Block].StatusValue <= 0)
                    {
                        remainingDamage = StatusDict[StatusType.Block].StatusValue * -1;
                        ClearStatus(StatusType.Block);
                    }
                }
            }
            
            CurrentHealth -= remainingDamage;
            
            if (CurrentHealth <= 0)
            {
                CurrentHealth = 0;
                OnDeath?.Invoke();
                IsDeath = true;
            }
            OnHealthChanged?.Invoke(CurrentHealth,MaxHealth);
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
                ClearStatus(targetStatus);
                OnStatusChanged?.Invoke(targetStatus, StatusDict[targetStatus].StatusValue);
                return;
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
                FxManager.Instance.SpawnFloatingText(spawnRoot, bleedAmount.ToString());
            }

            // Play bleed sound, debounced to avoid overlapping rapid ticks
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayOneShotDebounced(AudioActionType.Bleed, 0.25f);
            }

            Damage(bleedAmount, true);
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