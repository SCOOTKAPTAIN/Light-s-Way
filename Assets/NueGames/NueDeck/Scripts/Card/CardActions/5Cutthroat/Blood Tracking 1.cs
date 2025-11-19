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

            FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.BloodTracking, new Vector3(0,-0.3f,0));
            FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.SlowBleed);
                       
   

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}