using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class PerfectHarmony: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.PerfectHarmony;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            // Apply permanent PerfectHarmony status for this combat
            selfCharacter.CharacterStats.ApplyStatus(StatusType.PerfectHarmony, 1);
            // Make it permanent for combat
            selfCharacter.CharacterStats.StatusDict[StatusType.PerfectHarmony].IsPermanent = true;

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}