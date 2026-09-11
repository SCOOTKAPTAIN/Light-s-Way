using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class ReflectiveAegis : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.ReflectiveAegis;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            if (!selfCharacter) return;

            selfCharacter.CharacterStats.ApplyStatus(
                StatusType.Reverberation,
                Mathf.RoundToInt(actionParameters.Value));

            if (FxManager != null)
                FxManager.PlayFx(selfCharacter.transform, FxType.ReflectiveAegis);
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}