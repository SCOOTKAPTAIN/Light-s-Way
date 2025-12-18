using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class PerfectHarmony: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.PerfectHarmony;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            // Apply permanent PerfectHarmony status for this combat
            selfCharacter.CharacterStats.ApplyStatus(StatusType.PerfectHarmony, 1);
            FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.PerfectHarmony,new Vector3(0,0.4f,0));

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);

               
        }
    }
}