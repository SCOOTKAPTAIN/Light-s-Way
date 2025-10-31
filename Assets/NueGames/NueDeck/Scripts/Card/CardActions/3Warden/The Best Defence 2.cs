using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class TheBestDefence2 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.TheBestDefense2;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;

            if (!newTarget) return;

            int convertedBlock = 0;
            if (CombatManager.Instance.TryConsumeActionContext<int>("ShieldToStrength", out var storedConverted))
            {
                convertedBlock = storedConverted;
            }
            else
            {
                convertedBlock = Mathf.RoundToInt(actionParameters.Value);
            }

            var strengthToGrant = Mathf.RoundToInt(convertedBlock * 0.3f);
            if (strengthToGrant > 0)
            {
                newTarget.CharacterStats.ApplyStatus(StatusType.Strength, strengthToGrant);
            }

            



            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.TheBestDefense2, new Vector3(0f, 0.4f, 0f));
            
          //  if (AudioManager != null) 
               // AudioManager.PlayOneShot(AudioActionType.TheBestDefense2);
        }
    }
}