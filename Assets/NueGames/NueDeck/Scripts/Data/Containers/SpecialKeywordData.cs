using System;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Data.Containers
{
    [CreateAssetMenu(fileName = "Special Keyword", menuName = "NueDeck/Containers/Special Keyword Data", order = 0)]
    public class SpecialKeywordData : ScriptableObject
    {
        [SerializeField] private List<SpecialKeywordBase> specialKeywordBaseList;
        public List<SpecialKeywordBase> SpecialKeywordBaseList => specialKeywordBaseList;
        
        
    }

    [Serializable]
    public class SpecialKeywordBase
    {
        [SerializeField] private SpecialKeywords specialKeyword;
        [SerializeField] private string header;
        [SerializeField][TextArea] private string contentText;

        public SpecialKeywords SpecialKeyword => specialKeyword;

        // Designer-set header (display name). If empty, fall back to enum name or an override.
        public string Header => header;

        public string GetHeader(string overrideKeywordHeader = "")
        {
            if (!string.IsNullOrEmpty(overrideKeywordHeader)) return overrideKeywordHeader;
            if (!string.IsNullOrEmpty(Header)) return Header;
            return specialKeyword.ToString();
        }

        public string GetContent(string overrideContent = "")
        {
            return string.IsNullOrEmpty(overrideContent) ? contentText : overrideContent;
        }

        /// <summary>
        /// Returns header with current status value in parentheses.
        /// Example: "Block (10)" for Block status with value 10.
        /// Only shows parentheses if the status has a value greater than 0.
        /// </summary>
        public string GetHeaderWithStatusValue(NueGames.NueDeck.Scripts.Characters.CharacterStats characterStats, string overrideKeywordHeader = "")
        {
            string baseHeader = GetHeader(overrideKeywordHeader);
            
            if (characterStats == null)
                return baseHeader;

            // Map the special keyword to a status type to get the value
            StatusType correspondingStatus = MapKeywordToStatus(specialKeyword);
            
            if (correspondingStatus == StatusType.None)
                return baseHeader;

            if (characterStats.StatusDict.ContainsKey(correspondingStatus))
            {
                int statusValue = characterStats.StatusDict[correspondingStatus].StatusValue;
                if (statusValue > 0)  // Only show parentheses if value is greater than 0
                {
                    return $"{baseHeader} ({statusValue})";
                }
            }

            return baseHeader;
        }

        /// <summary>
        /// Maps special keywords to their corresponding status types.
        /// </summary>
        private StatusType MapKeywordToStatus(SpecialKeywords keyword)
        {
            return keyword switch
            {
                SpecialKeywords.Block => StatusType.Block,
                SpecialKeywords.Strength => StatusType.Strength,
                SpecialKeywords.Poison => StatusType.Poison,
                SpecialKeywords.Stun => StatusType.Stun,
                SpecialKeywords.Fragile => StatusType.Fragile,
                SpecialKeywords.Bleeding => StatusType.Bleeding,
                SpecialKeywords.Pursuit => StatusType.Pursuit,
                SpecialKeywords.Armor => StatusType.Armor,
                SpecialKeywords.Frostbite => StatusType.Frostbite,
                SpecialKeywords.Burning => StatusType.Burning,
                SpecialKeywords.Frozen => StatusType.Frozen,
                SpecialKeywords.Combustion => StatusType.Burning, // Combustion uses Burning status
                SpecialKeywords.Fortitude => StatusType.Fortitude,
                SpecialKeywords.FrozenMirror => StatusType.FrozenMirror,
                SpecialKeywords.BlazingSurge => StatusType.BlazingSurge,
                SpecialKeywords.PerfectHarmony => StatusType.PerfectHarmony,
                SpecialKeywords.SteadyBarricade => StatusType.SteadyBarricade,
                SpecialKeywords.TheMastermind => StatusType.Mastermind,
                SpecialKeywords.Weakness => StatusType.Weak,
                SpecialKeywords.Judged => StatusType.Judged,
                SpecialKeywords.Sabotaged => StatusType.Sabotaged,
               // SpecialKeywords.TheBestDefence => StatusType.TheBestDefense,
                _ => StatusType.None
            };
        }

        /// <summary>
        /// Returns content with dynamic status values substituted based on the provided character stats.
        /// Replaces placeholders like {Block}, {Strength}, {Poison}, etc. with actual status values.
        /// Also supports formula placeholders like {Fragile*10} for calculated values.
        /// </summary>
        public string GetContentWithStatusValues(NueGames.NueDeck.Scripts.Characters.CharacterStats characterStats, string overrideContent = "")
        {
            if (characterStats == null)
                return GetContent(overrideContent);

            string content = GetContent(overrideContent);

            // First pass: Replace formula placeholders (e.g., {Fragile*10})
            foreach (StatusType statusType in System.Enum.GetValues(typeof(StatusType)))
            {
                if (statusType == StatusType.None) continue;

                if (characterStats.StatusDict.ContainsKey(statusType))
                {
                    int statusValue = characterStats.StatusDict[statusType].StatusValue;
                    string statusName = statusType.ToString();
                    
                    // Check for multiplication formulas: {StatusType*number}
                    string multiplyPattern = "{" + statusName + "*";
                    int startIndex = 0;
                    while (startIndex < content.Length && (startIndex = content.IndexOf(multiplyPattern, startIndex)) != -1)
                    {
                        int endIndex = content.IndexOf("}", startIndex);
                        if (endIndex != -1)
                        {
                            string fullPlaceholder = content.Substring(startIndex, endIndex - startIndex + 1);
                            string formula = fullPlaceholder.Substring(multiplyPattern.Length, fullPlaceholder.Length - multiplyPattern.Length - 1);
                            
                            if (int.TryParse(formula, out int multiplier))
                            {
                                int result = statusValue * multiplier;
                                content = content.Replace(fullPlaceholder, result.ToString());
                                // Restart search from beginning since content changed
                                startIndex = 0;
                            }
                            else
                            {
                                startIndex = endIndex + 1;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    
                    // Check for division formulas: {StatusType/number}
                    string dividePattern = "{" + statusName + "/";
                    startIndex = 0;
                    while (startIndex < content.Length && (startIndex = content.IndexOf(dividePattern, startIndex)) != -1)
                    {
                        int endIndex = content.IndexOf("}", startIndex);
                        if (endIndex != -1)
                        {
                            string fullPlaceholder = content.Substring(startIndex, endIndex - startIndex + 1);
                            string formula = fullPlaceholder.Substring(dividePattern.Length, fullPlaceholder.Length - dividePattern.Length - 1);
                            
                            if (int.TryParse(formula, out int divisor) && divisor != 0)
                            {
                                int result = statusValue / divisor;
                                content = content.Replace(fullPlaceholder, result.ToString());
                                // Restart search from beginning since content changed
                                startIndex = 0;
                            }
                            else
                            {
                                startIndex = endIndex + 1;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }

            // Second pass: Replace simple status placeholders (e.g., {Strength})
            foreach (StatusType statusType in System.Enum.GetValues(typeof(StatusType)))
            {
                if (statusType == StatusType.None) continue;

                if (characterStats.StatusDict.ContainsKey(statusType))
                {
                    int statusValue = characterStats.StatusDict[statusType].StatusValue;
                    string placeholder = "{" + statusType.ToString() + "}";
                    content = content.Replace(placeholder, statusValue.ToString());
                }
            }

            return content;
        }
    }
}