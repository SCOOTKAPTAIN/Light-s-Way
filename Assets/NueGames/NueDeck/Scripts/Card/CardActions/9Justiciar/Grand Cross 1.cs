using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class GrandCross1: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.GrandCross;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;

            targetCharacter.CharacterStats.ApplyStatus(StatusType.Judged, 1);
            
            targetCharacter.CharacterStats.ApplyStatus(StatusType.Fragile, 2);

            if (FxManager != null)
                FxManager.PlayFx(targetCharacter.transform, FxType.GrandCross);
            
            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}