using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class TheBestDefence1 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.TheBestDefense;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;

            if (!newTarget) return;

            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.TheBestDefense, new Vector3(0f, 0.1f, 0f));
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}