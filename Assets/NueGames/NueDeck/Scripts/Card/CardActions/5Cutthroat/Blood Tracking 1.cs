using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BloodTracking1: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BloodTracking;

        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.BloodTracking, new Vector3(0,-0.3f,0));
            FxManager.PlayFxAtPosition(actionParameters.TargetCharacter.transform.position, FxType.SlowBleed);
                       
   

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}