using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class IllusionofVictory1 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.IllusionOfVictory;
        public override void DoAction(CardActionParameters actionParameters)
        {
           var selfCharacter = actionParameters.SelfCharacter;
           var anchor = CombatManager.Instance.EnemiesFxAnchor;
           
            // Only process once - check if we've already saved the strength value
            if (!CombatManager.Instance.TryGetActionContext<int>("LostStrength", out _))
            {
                // Get current Strength value (or 0 if not present)
                var strengthAmount = 0;
                if (selfCharacter.CharacterStats.StatusDict.ContainsKey(StatusType.Strength))
                {
                    strengthAmount = Mathf.RoundToInt(selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue);
                }
                
                // Save the strength value for subsequent actions
                CombatManager.Instance.SetActionContext("LostStrength", strengthAmount);    

                // Clear the Strength status
                selfCharacter.CharacterStats.ClearStatus(StatusType.Strength);
            }        

            if (FxManager != null)
                FxManager.PlayFxAtPosition(anchor.position, FxType.IllusionOfVictory, new Vector3(0f, 0.4f, 0f));
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}