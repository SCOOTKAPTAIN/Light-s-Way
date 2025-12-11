using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class FollowThrough2: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.FollowThrough2;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            if (actionParameters.TargetCharacter.CharacterStats.IsDeath) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var hasBlock = targetCharacter.CharacterStats.StatusDict[StatusType.Block].IsActive && targetCharacter.CharacterStats.StatusDict[StatusType.Block].StatusValue > 0;
            if (!hasBlock)
            {
            selfCharacter.CharacterStats.ApplyStatus(StatusType.Pursuit, 5);

             FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.FollowThrough2, new Vector3(0.2f,0.2f, 0));
             AudioManager.PlayOneShot(AudioActionType.FollowThrough2);
            }
            else
            {
                return;
            }
    

           
              
        }
    }
}