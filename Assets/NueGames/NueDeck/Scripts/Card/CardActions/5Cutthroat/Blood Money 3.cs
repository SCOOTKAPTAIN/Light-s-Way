using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BloodMoney3: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BloodMoney3;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;



            FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.BloodMoney3);

            // Gold reward is now handled by Blood Money 2 to prevent combat-end issues
            // This action now only plays the final FX and audio


                      

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.BloodMoney3);

        }
    }
}