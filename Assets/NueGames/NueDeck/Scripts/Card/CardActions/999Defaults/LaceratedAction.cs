using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class LaceratedAction : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.Lacerated;

        public override void DoAction(CardActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null)
                return;

            var damage = Mathf.RoundToInt(actionParameters.Value);
            var player = actionParameters.SelfCharacter;
            player.CharacterStats.Damage(damage, true, "red", null);

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.Bleed);
        }
    }
}