using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class OverpoweringDifference: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.OverpoweringDifference;

        public override void DoAction(CardActionParameters actionParameters)
        {
            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            if (!targetCharacter || !selfCharacter) return;

            // Get bleeding value from target, capped at 10
            var weakvalue = targetCharacter.CharacterStats.StatusDict[StatusType.Weak].StatusValue;
          //  var bonusAmount = Mathf.Min(bleedingValue, 10);

            if (weakvalue > 0)
            {
                
                selfCharacter.CharacterStats.ApplyStatus(StatusType.Strength, weakvalue);
                selfCharacter.CharacterStats.ApplyStatus(StatusType.Fortitude, weakvalue);
            }

            if (FxManager != null) 
                FxManager.PlayFx(targetCharacter.transform, FxType.OverpoweringDifference);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}