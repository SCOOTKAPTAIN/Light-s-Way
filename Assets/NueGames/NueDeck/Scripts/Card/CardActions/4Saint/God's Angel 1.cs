using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class GodsAngel1 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.GodsAngel;
        public override void DoAction(CardActionParameters actionParameters)
        {

            if (FxManager != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.GodsAngel);

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.GodsAngel1);
        }
    }
}