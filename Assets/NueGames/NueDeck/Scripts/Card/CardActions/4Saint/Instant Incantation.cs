using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class InstantIncantation : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.InstantIncantation;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (actionParameters.SelfCharacter != null)
            {
                actionParameters.SelfCharacter.CharacterStats.ApplyStatus(StatusType.FreeNextCard, 1);
            }

            if (FxManager != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.InstantIncantation);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}