using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using System.Linq;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class FrozenMirror: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.FrozenMirror;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;
            var combatManager = CombatManager.Instance;
            if (combatManager == null) return;

            FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.FrozenMirror,new Vector3(0,0.3f,0));

            // Gain Fortitude
            var affinityDuration = Mathf.RoundToInt(CardScaling.AddAffinity(2f));
            selfCharacter.CharacterStats.ApplyStatus(StatusType.Fortitude, affinityDuration);

            // Gain Block
            selfCharacter.CharacterStats.ApplyStatus(StatusType.Block,
                Mathf.RoundToInt(actionParameters.Value + GameManager.PersistentGameplayData.proficiency + actionParameters.SelfCharacter.CharacterStats
                    .StatusDict[StatusType.Fortitude].StatusValue));

            // Apply the reactive FrozenMirror status to self for this turn
            selfCharacter.CharacterStats.ApplyStatus(StatusType.FrozenMirror, affinityDuration);

            if (AudioManager != null)
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}