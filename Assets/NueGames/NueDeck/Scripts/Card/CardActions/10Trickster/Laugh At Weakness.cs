using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class LaughAtWeakness: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.LaughAtWeakness;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var anchor = CombatManager.Instance.EnemiesFxAnchor;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            FxManager.PlayFxAtPosition(anchor.position, FxType.LaughAtWeakness, new Vector3(0f, 0.4f, 0f));

            targetCharacter.CharacterStats.ApplyStatus(StatusType.Weak, 1);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
              
        }
    }
}