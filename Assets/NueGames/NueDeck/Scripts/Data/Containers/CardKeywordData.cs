using System;
using System.Collections.Generic;
using NueGames.NueDeck.Scripts.Enums;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Data.Containers
{
    /// <summary>
    /// Card-specific keyword data that displays static tooltip information.
    /// Unlike SpecialKeywordData, this does NOT pull dynamic values from character status.
    /// Use this for card tooltips to avoid showing 0% or incorrect values.
    /// </summary>
    [CreateAssetMenu(fileName = "Card Keyword Data", menuName = "NueDeck/Containers/Card Keyword Data", order = 1)]
    public class CardKeywordData : ScriptableObject
    {
        [SerializeField] private List<CardKeywordBase> cardKeywordBaseList;
        public List<CardKeywordBase> CardKeywordBaseList => cardKeywordBaseList;
    }

    [Serializable]
    public class CardKeywordBase
    {
        [SerializeField] private SpecialKeywords specialKeyword;
        [SerializeField] private string header;
        [SerializeField][TextArea] private string contentText;

        public SpecialKeywords SpecialKeyword => specialKeyword;
        public string Header => header;
        public string ContentText => contentText;

        /// <summary>
        /// Returns the header for this keyword.
        /// For cards, this is always static (no dynamic character values).
        /// </summary>
        public string GetHeader(string overrideKeywordHeader = "")
        {
            if (!string.IsNullOrEmpty(overrideKeywordHeader)) return overrideKeywordHeader;
            if (!string.IsNullOrEmpty(Header)) return Header;
            return specialKeyword.ToString();
        }

        /// <summary>
        /// Returns the content for this keyword.
        /// For cards, this is always static (no dynamic character values).
        /// </summary>
        public string GetContent(string overrideContent = "")
        {
            return string.IsNullOrEmpty(overrideContent) ? contentText : overrideContent;
        }
    }
}
