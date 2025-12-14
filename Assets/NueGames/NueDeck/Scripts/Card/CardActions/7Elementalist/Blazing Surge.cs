using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
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
            selfCharacter.CharacterStats.ApplyStatus(StatusType.Strength, Mathf.RoundToInt(2));
            // Gain Armor
            selfCharacter.CharacterStats.ApplyStatus(StatusType.Armor,1);

            // Apply the reactive BlazingSurge status
            selfCharacter.CharacterStats.ApplyStatus(StatusType.BlazingSurge, 2);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}