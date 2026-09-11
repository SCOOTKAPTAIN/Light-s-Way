using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class StalwartBuffer : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.StalwartBuffer;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            if (!selfCharacter) return;

            selfCharacter.CharacterStats.ApplyStatus(
                StatusType.Bastion,
                Mathf.RoundToInt(actionParameters.Value));

            if (FxManager != null)
                FxManager.PlayFx(selfCharacter.transform, FxType.StalwartBuffer);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}