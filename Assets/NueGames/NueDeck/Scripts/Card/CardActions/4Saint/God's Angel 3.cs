using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class GodsAngel3 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.GodsAngel3;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (actionParameters.SelfCharacter != null)
            {
                actionParameters.SelfCharacter.CharacterStats.ApplyStatus(StatusType.GodsAngelBuff, 1);
            }

            if (FxManager != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.GodsAngel3, new Vector3(0f, 0.3f, 0f));
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.GodsAngel2);

           // if (AudioManager != null)
             //   AudioManager.PlayOneShot(AudioActionType.GodsAngel3);
        }
    }
}