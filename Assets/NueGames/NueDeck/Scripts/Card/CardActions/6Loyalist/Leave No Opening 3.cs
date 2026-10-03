using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using Unity.Mathematics;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class LeaveNoOpening3 : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.LeaveNoOpening3;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var selfCharacter = actionParameters.SelfCharacter;

            selfCharacter.CharacterStats.ApplyStatus(StatusType.Block, 
            Mathf.RoundToInt(selfCharacter.CharacterStats.StatusDict[StatusType.Pursuit].StatusValue * CardScaling.AddWisdom(2f)));

            if (FxManager != null)
                FxManager.PlayFx(selfCharacter.transform, FxType.LeaveNoOpening3, new Vector3(0, 0.4f, 0));
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(AudioActionType.LeaveNoOpening3);
        }
    }
}