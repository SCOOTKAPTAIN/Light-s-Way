using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using Unity.Mathematics;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class ShieldSiphon1: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.ShieldSiphon1;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var absorbAmount = Mathf.RoundToInt(targetCharacter.CharacterStats.StatusDict[StatusType.Block].StatusValue);

            CombatManager.Instance.SetActionContext("SiphonedShield", absorbAmount);

            targetCharacter.CharacterStats.ClearStatus(StatusType.Block);


            FxManager.PlayFx(targetCharacter.transform, FxType.ShieldSiphon1, new Vector3(-0.2f,0,0));

            if (AudioManager != null)
                AudioManager.PlayOneShot(AudioActionType.ShieldSiphon1);
              
        }
    }
}