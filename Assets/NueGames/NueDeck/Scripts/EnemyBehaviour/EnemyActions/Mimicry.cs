using System.Linq;
using NueGames.NueDeck.Scripts.Characters;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.EnemyBehaviour.EnemyActions
{
    public class Mimicry : EnemyActionBase
    {
        // Edit this list to control which player buffs Mimicry cannot copy.
        private static readonly StatusType[] ExcludedBuffs =
        {
            StatusType.Proficiency,
            StatusType.FreeNextCard,
            StatusType.GodsAngelBuff,
            StatusType.PerfectHarmony,
            StatusType.Mastermind,
            StatusType.TheBestDefense,
            StatusType.Deadstock,
            StatusType.EndlessChambers,
            StatusType.FiringLine
        };

        public override EnemyActionType ActionType => EnemyActionType.Mimicry;

        public override void DoAction(EnemyActionParameters actionParameters)
        {
            if (actionParameters?.SelfCharacter == null)
                return;

            var player = actionParameters.TargetCharacter;
            if (!(player is AllyBase))
                player = CombatManager?.CurrentMainAlly;

            if (player == null || player.CharacterStats == null)
                return;

            var selfStats = actionParameters.SelfCharacter.CharacterStats;
            var playerStats = player.CharacterStats;

            foreach (var status in playerStats.StatusDict)
            {
                var statusType = status.Key;
                var statusStats = status.Value;

                if (!statusStats.IsActive || statusStats.StatusValue <= 0 ||
                    CharacterStats.DebuffTypes.Contains(statusType) ||
                    statusType == StatusType.None ||
                    ExcludedBuffs.Contains(statusType))
                    continue;

                var copiedValue = Mathf.CeilToInt(statusStats.StatusValue * 0.5f);
                if (copiedValue > 0)
                    selfStats.ApplyStatus(statusType, copiedValue, actionParameters.SelfCharacter);
            }

            PlayActionFx(actionParameters, actionParameters.SelfCharacter.transform, FxType.Mimicry);
            PlayActionAudio(actionParameters, AudioActionType.Mimicry);
        }
    }
}