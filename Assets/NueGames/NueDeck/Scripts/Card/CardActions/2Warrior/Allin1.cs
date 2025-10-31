using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class Allin1 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.AllIn;
        public override void DoAction(CardActionParameters actionParameters)
        {

           FxManager.PlayFx( actionParameters.SelfCharacter.transform, FxType.AllIn, new Vector3(0, 0.4f, 0));

           actionParameters.SelfCharacter.CharacterStats.ApplyStatus(StatusType.Strength,Mathf.RoundToInt(actionParameters.Value));



            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.AllIn);

        }
    }
}