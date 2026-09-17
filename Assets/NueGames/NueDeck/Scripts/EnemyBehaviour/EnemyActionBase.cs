using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour
{
    
    public abstract class EnemyActionBase
    {
        protected EnemyActionBase(){}
        public abstract EnemyActionType ActionType { get;}
        public abstract void DoAction(EnemyActionParameters actionParameters);
        
        protected FxManager FxManager => FxManager.Instance;
        protected AudioManager AudioManager => AudioManager.Instance;
        protected GameManager GameManager => GameManager.Instance;
        protected CombatManager CombatManager => CombatManager.Instance;
        protected CollectionManager CollectionManager => CollectionManager.Instance;

        protected void PlayActionFx(EnemyActionParameters actionParameters, Transform target, FxType fallback)
        {
            if (FxManager == null || target == null)
                return;

            var fxType = actionParameters.ActionData != null && actionParameters.ActionData.OverridePresentation
                ? actionParameters.ActionData.CustomFxType
                : fallback;
            FxManager.PlayFx(target, fxType);
        }

        protected void PlayActionFxAtPosition(EnemyActionParameters actionParameters, Vector3 position, FxType fallback)
        {
            if (FxManager == null)
                return;

            var fxType = actionParameters.ActionData != null && actionParameters.ActionData.OverridePresentation
                ? actionParameters.ActionData.CustomFxType
                : fallback;
            FxManager.PlayFxAtPosition(position, fxType);
        }

        protected void PlayActionAudio(EnemyActionParameters actionParameters, AudioActionType fallback)
        {
            if (AudioManager == null)
                return;

            var audioType = actionParameters.ActionData != null && actionParameters.ActionData.OverridePresentation
                ? actionParameters.ActionData.CustomAudioType
                : fallback;
            AudioManager.PlayOneShot(audioType);
        }
        
    }
    
    public class EnemyActionParameters
    {
        public readonly float Value;
        public readonly CharacterBase TargetCharacter;
        public readonly CharacterBase SelfCharacter;
        public readonly Data.Characters.EnemyActionData ActionData;

        public EnemyActionParameters(float value, CharacterBase target, CharacterBase self, Data.Characters.EnemyActionData actionData = null)
        {
            Value = value;
            TargetCharacter = target;
            SelfCharacter = self;
            ActionData = actionData;
        }
    }
    
    
}