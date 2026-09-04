using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class EtherealSlinger: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.EtherealSlinger;

        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            if (selfCharacter == null)
                return;

            var stacks = Mathf.Max(1, Mathf.RoundToInt(actionParameters.Value));
            selfCharacter.CharacterStats.ApplyStatus(StatusType.EndlessChambers, stacks);

            FxManager.PlayFx(selfCharacter.transform, FxType.AmmoPouch, new Vector3(0f, 0.4f, 0f));

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}