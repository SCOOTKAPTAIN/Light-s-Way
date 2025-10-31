using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class SacredProtection : CardActionBase
    {
        public override CardActionType ActionType => CardActionType.SacredProtection;
        public override void DoAction(CardActionParameters actionParameters)
        {

            if (actionParameters.SelfCharacter != null)
            {
                var removed = actionParameters.SelfCharacter.CharacterStats.ClearDebuffs();
                if (removed > 0)
                    actionParameters.SelfCharacter.CharacterStats.Heal(removed);
            }

            if (FxManager != null)
                FxManager.PlayFx(actionParameters.SelfCharacter.transform, FxType.SacredProtection, new Vector3(0.05f,0.5f,0f));
            
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}