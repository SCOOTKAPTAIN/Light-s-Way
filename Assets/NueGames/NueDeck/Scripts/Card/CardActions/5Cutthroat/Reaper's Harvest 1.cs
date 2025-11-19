using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class ReapersHarvest1: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.ReapersHarvest;
        public override void DoAction(CardActionParameters actionParameters)
        {
            

            FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.ReapersHarvest, new Vector3(0.2f,0.2f, 0));
                       
   

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}