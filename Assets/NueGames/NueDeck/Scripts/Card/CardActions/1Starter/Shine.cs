using NueGames.NueDeck.Scripts.Enums;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card.CardActions
{
    public class Shine: CardActionBase
    {
        public override CardActionType ActionType => CardActionType.Shine;
        public override void DoAction(CardActionParameters actionParameters)
        {
            if (!actionParameters.TargetCharacter) return;
            
            var targetCharacter = actionParameters.TargetCharacter;
            var selfCharacter = actionParameters.SelfCharacter;
            
            var value =  GameManager.PersistentGameplayData.proficiency + actionParameters.Value
             + selfCharacter.CharacterStats.StatusDict[StatusType.Strength].StatusValue;

            if (GameManager.PersistentGameplayData.light < 10)
            {
                CollectionManager.DrawCards(Mathf.RoundToInt(1));
                FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.NoLight);
                AudioManager.PlayOneShot(AudioActionType.NoLight);
                return;
            }

            CollectionManager.DrawCards(Mathf.RoundToInt(1));
            GameManager.PersistentGameplayData.ChangeLight(-10);
            FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.Shine);
            
            // put ts before every damage activations ---------------------------------------------------------------------------------------------------------------------------------
            int fragileStacks = targetCharacter.CharacterStats.StatusDict[StatusType.Fragile].StatusValue;
            float multiplier = 1f + (0.05f * fragileStacks);
            if (fragileStacks > 0)
            {
                value = Mathf.RoundToInt(value * multiplier);
            }
            if (selfCharacter.CharacterStats.StatusDict[StatusType.Pursuit].StatusValue > 0)
            {
                var pursuitValue = Mathf.RoundToInt(selfCharacter.CharacterStats.StatusDict[StatusType.Pursuit].StatusValue * multiplier);
                targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(pursuitValue));
                FxManager.PlayFx(actionParameters.TargetCharacter.transform, FxType.Pursuit);
                FxManager.SpawnStaticText(actionParameters.TargetCharacter.TextSpawnRoot, pursuitValue.ToString());
                AudioManager.PlayOneShot(AudioActionType.Pursuit);
            }
            // ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------

            targetCharacter.CharacterStats.Damage(Mathf.RoundToInt(value));


            if (FxManager != null)
            {
                FxManager.SpawnFloatingText(actionParameters.TargetCharacter.TextSpawnRoot, value.ToString());
            }
            if (AudioManager != null) 
                AudioManager.PlayOneShot(actionParameters.CardData.AudioType);
        }
    }
}