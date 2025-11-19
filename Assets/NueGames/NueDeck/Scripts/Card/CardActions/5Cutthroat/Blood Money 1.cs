using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BloodMoney: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BloodMoney;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.BloodMoney);
                       
   

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}