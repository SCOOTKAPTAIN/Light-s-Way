using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BloodTracking2: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BloodTracking2;

        public override void DoAction(CardActionParameters actionParameters)
        {
            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            if (!targetCharacter || !selfCharacter) return;

            // Get bleeding value from target, capped at 10
            var bleedingValue = targetCharacter.CharacterStats.StatusDict[StatusType.Bleeding].StatusValue;
            var bonusAmount = Mathf.Min(bleedingValue, 10);

            if (bonusAmount > 0)
            {
                // Give player health equal to bleeding value (up to 10)
                selfCharacter.CharacterStats.Heal(bonusAmount);
                
                // Give player strength equal to bleeding value (up to 10)
                selfCharacter.CharacterStats.ApplyStatus(StatusType.Strength, bonusAmount);

                // Spawn floating text to show the bonuses
                if (FxManager != null)
                {
                    FxManager.SpawnFloatingTextGreen(selfCharacter.TextSpawnRoot, bonusAmount.ToString());
                }
            }

            if (FxManager != null) 
                FxManager.PlayFx(selfCharacter.transform, FxType.BloodTracking2);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(AudioActionType.BloodTracking2);
        }
    }
}