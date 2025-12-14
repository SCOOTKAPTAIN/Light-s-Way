using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class FollowThrough3: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.FollowThrough3;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
         

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var hasBlock = targetCharacter.CharacterStats.StatusDict[StatusType.Block].IsActive && targetCharacter.CharacterStats.StatusDict[StatusType.Block].StatusValue > 0;
            if (!hasBlock)
            {
                var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.FollowThrough3,new Vector3(0f,0,0));
              
            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);
            AudioManager.PlayOneShot(AudioActionType.FollowThrough3);

            }
            else
            {
                return;
            }
              
        }
    }
}