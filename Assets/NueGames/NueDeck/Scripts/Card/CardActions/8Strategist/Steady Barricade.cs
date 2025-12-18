using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class SteadyBarricade : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.SteadyBarricade;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;

            if (!newTarget) return;

            newTarget.CharacterStats.ApplyStatus(StatusType.SteadyBarricade, 1);

            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.SteadyBarricade);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}
