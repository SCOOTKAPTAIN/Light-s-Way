using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class WindUp : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.WindUp;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

            newTarget.CharacterStats.ApplyStatus(StatusType.Strength, Mathf.RoundToInt(actionParameters.Value));
            CollectionManager.DrawCards(Mathf.RoundToInt(1));



            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.WindUp, new Vector3(0f, 0.5f, 0f));

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}