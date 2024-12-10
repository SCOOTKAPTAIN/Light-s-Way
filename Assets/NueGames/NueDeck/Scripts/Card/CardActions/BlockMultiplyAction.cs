using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class BlockMultiplyAction : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.BlockMultiply;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;
            
            newTarget.CharacterStats.ApplyStatus(StatusType.Block,
                Mathf.RoundToInt(actionParameters.SelfCharacter.CharacterStats
                    .StatusDict[StatusType.Block].StatusValue));

            if (FxManager != null) 
                FxManager.PlayFx(newTarget.transform, FxType.Block);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}