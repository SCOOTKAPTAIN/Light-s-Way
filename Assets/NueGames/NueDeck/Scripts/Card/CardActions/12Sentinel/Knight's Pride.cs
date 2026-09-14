using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class KnightsPride : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.KnightsPride;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

            if (CollectionManager != null)
                CollectionManager.ConvertGuardCardsToVanguardStance();

            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.KnightsPride, new Vector3(0f, 0f, 0f));

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}