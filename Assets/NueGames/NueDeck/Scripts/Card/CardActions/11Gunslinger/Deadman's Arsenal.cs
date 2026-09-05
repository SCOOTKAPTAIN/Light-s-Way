using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class DeadmansArsenal: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.DeadmansArsenal;

        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            if (selfCharacter == null)
                return;

            selfCharacter.CharacterStats.ApplyStatus(StatusType.FiringLine, 1);

            FxManager.PlayFx(selfCharacter.transform, FxType.DeadmansArsenal, new Vector3(0f, 0.4f, 0f));
            FxManager.PlayFx(selfCharacter.transform, FxType.DeadmansArsenal2, new Vector3(0f, 0.4f, 0f));

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}