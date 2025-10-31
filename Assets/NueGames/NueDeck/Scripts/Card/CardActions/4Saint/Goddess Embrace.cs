using NueGames.NueDeck.Scripts.Data.Settings;
using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class GoddessEmbrace : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.GoddessEmbrace;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;

            if (!newTarget) return;

            newTarget.CharacterStats.ApplyStatus(StatusType.Block,
                Mathf.RoundToInt(actionParameters.Value + GameManager.PersistentGameplayData.proficiency + actionParameters.SelfCharacter.CharacterStats
                    .StatusDict[StatusType.Fortitude].StatusValue));

            newTarget.CharacterStats.ApplyStatus(StatusType.DebuffWard, 3);


            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.GoddessEmbrace);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}
