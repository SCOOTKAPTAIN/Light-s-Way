using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Data.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour
{
    
    public abstract class EnemyActionBase
    {
        protected EnemyActionBase(){}
        public abstract EnemyActionType ActionType { get;}
        public abstract void DoAction(EnemyActionParameters actionParameters);

        public virtual int CalculateValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            return CalculateBaseValue(baseValue, actionData);
        }

        protected int CalculateBaseValue(float baseValue, EnemyActionData actionData)
        {
            return Mathf.RoundToInt(ApplyLightMultiplier(baseValue, actionData));
        }

        protected float ApplyLightMultiplier(float baseValue, EnemyActionData actionData)
        {
            if (actionData != null && actionData.ApplyLightMultiplier && CombatManager != null)
                return baseValue * CombatManager.CombatLightMultiplier;

            return baseValue;
        }

        protected int CalculateDamageValue(float baseValue, CharacterBase selfCharacter, CharacterBase targetCharacter, EnemyActionData actionData)
        {
            var value = CalculateBaseValue(baseValue, actionData);
            value = Mathf.RoundToInt(value + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue);
            if (targetCharacter == null)
                return value;

            return Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));
        }

        protected int CalculateBlockValue(float baseValue, CharacterBase selfCharacter, EnemyActionData actionData)
        {
            var value = CalculateBaseValue(baseValue, actionData);
            return value + selfCharacter.CharacterStats.StatusDict[StatusType.Fortitude].StatusValue;
        }
        
        protected FxManager FxManager => FxManager.Instance;
        protected AudioManager AudioManager => AudioManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        protected CombatManager CombatManager => CombatManager.Instance;
        protected CollectionManager CollectionManager => CollectionManager.Instance;

        protected void PlayActionFx(EnemyActionParameters actionParameters, Transform target, FxType fallback)
        {
            if (FxManager == null || target == null || ShouldSuppressPresentation(actionParameters))
                return;

            var fxType = actionParameters.ActionData != null && actionParameters.ActionData.OverridePresentation
                ? actionParameters.ActionData.CustomFxType
                : fallback;
            FxManager.PlayFx(target, fxType);
        }

        protected void PlayActionFx(EnemyActionParameters actionParameters, Transform target, FxType fallback, Vector3 offset)
        {
            if (FxManager == null || target == null || ShouldSuppressPresentation(actionParameters))
                return;

            var fxType = actionParameters.ActionData != null && actionParameters.ActionData.OverridePresentation
                ? actionParameters.ActionData.CustomFxType
                : fallback;
            FxManager.PlayFx(target, fxType, offset);
        }

        protected void PlayActionFxAtPosition(EnemyActionParameters actionParameters, Vector3 position, FxType fallback)
        {
            if (FxManager == null || ShouldSuppressPresentation(actionParameters))
                return;

            var fxType = actionParameters.ActionData != null && actionParameters.ActionData.OverridePresentation
                ? actionParameters.ActionData.CustomFxType
                : fallback;
            FxManager.PlayFxAtPosition(position, fxType);
        }

        protected void PlayActionAudio(EnemyActionParameters actionParameters, AudioActionType fallback)
        {
            if (AudioManager == null || ShouldSuppressPresentation(actionParameters))
                return;

            var audioType = actionParameters.ActionData != null && actionParameters.ActionData.OverridePresentation
                ? actionParameters.ActionData.CustomAudioType
                : fallback;
            AudioManager.PlayOneShot(audioType);
        }

        protected static bool ShouldSuppressPresentation(EnemyActionParameters actionParameters)
        {
             return actionParameters != null &&
                 actionParameters.ActionData != null &&
                 actionParameters.ActionData.SuppressPresentation;
        }
        
    }
    
    public class EnemyActionParameters
    {
        public readonly float Value;
        public readonly CharacterBase TargetCharacter;
        public readonly CharacterBase SelfCharacter;
        public readonly Data.Characters.EnemyActionData ActionData;
        public readonly int RepeatCount;

        public EnemyActionParameters(float value, CharacterBase target, CharacterBase self, Data.Characters.EnemyActionData actionData = null, int repeatCount = 1)
        {
            Value = value;
            TargetCharacter = target;
            SelfCharacter = self;
            ActionData = actionData;
            RepeatCount = Mathf.Max(1, repeatCount);
        }
    }
    
    
}