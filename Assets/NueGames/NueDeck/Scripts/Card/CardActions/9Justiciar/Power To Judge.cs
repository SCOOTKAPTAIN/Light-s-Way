using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using NueGames.NueDeck.Scripts.Card;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class PowerToJudge : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.PowerToJudge;
        public override void DoAction(CardActionParameters actionParameters)
        {
           var targetCharacter = actionParameters.TargetCharacter;
           var selfCharacter = actionParameters.SelfCharacter;
            
            if (!targetCharacter) return;

            if (actionParameters.IsFirstTarget)
                selfCharacter.CharacterStats.ApplyStatus(
                    StatusType.Strength,
                    Mathf.RoundToInt(CardScaling.AddAffinity(2f)));

            targetCharacter.CharacterStats.ApplyStatus(
                StatusType.Fragile,
                Mathf.RoundToInt(CardScaling.AddPotency(3f)),
                selfCharacter);


            if (FxManager != null)
                FxManager.PlayFx(selfCharacter.transform, FxType.PowerToJudge, new Vector3(0f, 0.4f, 0f));
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}