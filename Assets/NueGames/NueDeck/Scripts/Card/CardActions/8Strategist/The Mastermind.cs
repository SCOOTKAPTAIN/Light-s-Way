using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class TheMastermind : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.TheMastermind;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var self = actionParameters.SelfCharacter;
            if (!self) return;

            var amount = Mathf.RoundToInt(actionParameters.Value);
            if (amount <= 0) amount = 3; // default +3 draw if not specified

            self.CharacterStats.ApplyStatus(StatusType.Mastermind, amount);

            // Optional FX to indicate draw increase
            if (FxManager != null)
            {
                FxManager.PlayFx(self.transform, FxType.TheMastermind);
            }

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}
