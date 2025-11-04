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
    }
}