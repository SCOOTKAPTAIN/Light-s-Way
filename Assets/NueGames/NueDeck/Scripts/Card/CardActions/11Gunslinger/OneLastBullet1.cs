using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class OneLastBullet2 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.OneLastBullet2;
        public override void DoAction(CardActionParameters actionParameters)
        {

           FxManager.PlayFx( actionParameters.SelfCharacter.transform, FxType.OneLastBullet2, new Vector3(0, 0.4f, 0));


            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.OneLastBullet2);

        }
    }
}