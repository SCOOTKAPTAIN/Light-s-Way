using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class Slash: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.Slash;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;

            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;

            var value = GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            
              

            value = Mathf.RoundToInt(NueGames.NueDeck.Scripts.Utils.DamageEffects.ApplyFragileAndPursuit(targetCharacter, selfCharacter, value));

            

            // If attacker has Perfect Harmony permanent status, apply 1 Burning and 1 Frostbite
            if (selfCharacter.CharacterStats.StatusDict.ContainsKey(StatusType.PerfectHarmony) && selfCharacter.CharacterStats.StatusDict[StatusType.PerfectHarmony].IsActive)
            {
                FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.PerfectHarmonySlash);
                AudioManager.PlayOneShot(AudioActionType.PerfectHarmonySlash);
                targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);
                targetCharacter.CharacterStats.ApplyStatus(StatusType.Burning, 1, selfCharacter);
                targetCharacter.CharacterStats.ApplyStatus(StatusType.Frostbite, 1, selfCharacter);
               
            }
            else
            {
                FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.Slash);
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
                targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value), false, "red", selfCharacter);
            }


          

          
        }
    }
}