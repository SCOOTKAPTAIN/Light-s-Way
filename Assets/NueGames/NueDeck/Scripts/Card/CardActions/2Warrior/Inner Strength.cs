using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class InnerStrength : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.InnerStrength;
        public override void DoAction(CardActionParameters actionParameters)
        {
            var newTarget = actionParameters.TargetCharacter
                ? actionParameters.TargetCharacter
                : actionParameters.SelfCharacter;
            
            if (!newTarget) return;

            newTarget.CharacterStats.ApplyStatus(StatusType.Strength,Mathf.RoundToInt(actionParameters.Value));



            if (FxManager != null)
                FxManager.PlayFx(newTarget.transform, FxType.InnerStrength, new Vector3(0f, 0.4f, 0f));
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}