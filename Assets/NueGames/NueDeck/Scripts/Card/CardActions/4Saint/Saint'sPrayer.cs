using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class SaintsPrayer : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.SaintsPrayer;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (CombatManager != null)
                CombatManager.IncreaseMana(Mathf.RoundToInt(CardScaling.AddWisdom(actionParameters.Value)));
            else
                Debug.LogError("There is no CombatManager");

            if (FxManager != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.SaintsPrayer);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}