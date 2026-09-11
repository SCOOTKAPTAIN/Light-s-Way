using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class PolishShield : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.PolishShield;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

            // Gain 1 Strength
            newTarget.CharacterStats.ApplyStatus(StatusType.Fortitude, 1);

            // Return this card to the player's hand after play.
            // Do not reset per-turn cost scaling, so the cost increases correctly each time it is played.
            if (actionParameters.CardBase != null)
                actionParameters.CardBase.ReturnToHandAfterPlay = true;



            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.DefensePreparation);
                FxManager.PlayFx(newTarget.transform, FxType.PolishShield, new Vector3(0.3f, 0.5f, 0f));

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}