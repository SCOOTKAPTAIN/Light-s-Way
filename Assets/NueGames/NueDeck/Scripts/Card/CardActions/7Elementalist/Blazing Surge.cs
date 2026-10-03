using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using System.Linq;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BlazingSurge: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BlazingSurge;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            var combatManager = CombatManager.Instance;
            if (combatManager == null) return;

            FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.BlazingSurge,new Vector3(0,0.3f,0));

            // Gain Strength
            var affinityDuration = Mathf.RoundToInt(CardScaling.AddAffinity(2f));
            selfCharacter.CharacterStats.ApplyStatus(StatusType.Strength, affinityDuration);
            // Gain Armor
            selfCharacter.CharacterStats.ApplyStatus(
                StatusType.Armor,
                Mathf.RoundToInt(CardScaling.AddAffinity(1f)));

            // Apply the reactive BlazingSurge status
            selfCharacter.CharacterStats.ApplyStatus(StatusType.BlazingSurge, affinityDuration);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}