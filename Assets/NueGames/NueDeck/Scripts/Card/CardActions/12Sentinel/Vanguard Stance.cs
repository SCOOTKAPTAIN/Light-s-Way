using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class VanguardStance : CardActionBase
    {
        private const int HonorGain = 1;

        public override CardActionType ActionType => CardActionType.VanguardStance;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;

            if (!newTarget) return;

            newTarget.CharacterStats.ApplyStatus(StatusType.Block,
                Mathf.RoundToInt(actionParameters.Value + GameManager.PersistentGameplayData.proficiency + actionParameters.SelfCharacter.CharacterStats
                    .StatusDict[StatusType.Fortitude].StatusValue));

            newTarget.CharacterStats.ApplyStatus(StatusType.Honor, HonorGain);

            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.VanguardStance, new Vector3(0.3f,0.2f,0f));

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}
