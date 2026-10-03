using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using NueGames.NueDeck.Scripts.Managers;
using UnityEngine;

namespace NueGames.NueDeck.Scripts.Card
{
    public static class CardScaling
    {
        private static readonly Regex DescriptionExpression = new Regex(@"\{([^{}]+)\}", RegexOptions.Compiled);
        private static readonly Regex ExpressionToken = new Regex(@"\s*(?<number>\d+(?:\.\d+)?)|\s*(?<name>[A-Za-z][A-Za-z0-9]*)|\s*(?<operator>[+\-*/()])", RegexOptions.Compiled);
        private static readonly Regex AffinityExpression = new Regex(@"^\s*(?<base>\d+(?:\.\d+)?)\s*\+\s*Affinity\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static float AddWisdom(float value) => value + GetStat("Wisdom");

        public static float AddPotency(float value) => value + GetStat("Potency");

        public static float AddAffinity(float value) => ScaleByAffinity(value);

        public static float ScaleByAffinity(float value)
        {
            var scaledValue = value * (1f + GetStat("Affinity") * 0.1f);
            return Mathf.FloorToInt(scaledValue + 0.5f);
        }

        public static float MultiplyByAffinity(float value) => value * (1f + GetStat("Affinity") * 0.1f);

        public static string ResolveDescription(string description)
        {
            if (string.IsNullOrEmpty(description))
                return description;

            return DescriptionExpression.Replace(description, match =>
            {
                var affinityMatch = AffinityExpression.Match(match.Groups[1].Value);
                if (affinityMatch.Success && float.TryParse(affinityMatch.Groups["base"].Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var affinityBase))
                    return FormatValue(ScaleByAffinity(affinityBase));

                if (!TryEvaluate(match.Groups[1].Value, out var value))
                    return match.Value;

                return FormatValue(value);
            });
        }

        private static float GetStat(string name)
        {
            var data = GameManager.Instance != null ? GameManager.Instance.PersistentGameplayData : null;
            if (data == null)
                return 0f;

            switch (name.ToLowerInvariant())
            {
                case "proficiency": return data.Proficiency;
                case "arcana": return data.Arcana;
                case "wisdom": return data.Wisdom;
                case "potency": return data.Potency;
                case "affinity": return data.Affinity;
                case "metabolism": return data.Metabolism;
                case "vigor": return data.Vigor;
                case "insight": return data.Insight;
                case "abundance": return data.Abundance;
                case "capacity": return data.Capacity;
                case "radiance": return data.Radiance;
                case "light": return data.Light;
                case "mana": return data.CurrentMana;
                case "maxmana": return data.MaxMana;
                case "drawcount": return data.DrawCount;
                case "maxcardonhand": return data.MaxCardOnHand;
                default: return 0f;
            }
        }

        private static bool TryEvaluate(string expression, out float value)
        {
            var tokens = new List<string>();
            var index = 0;
            while (index < expression.Length)
            {
                var match = ExpressionToken.Match(expression, index);
                if (!match.Success || match.Index != index)
                {
                    value = 0f;
                    return false;
                }

                tokens.Add(match.Value.Trim());
                index = match.Index + match.Length;
            }

            var parser = new ExpressionParser(tokens);
            return parser.TryParse(out value) && parser.AtEnd;
        }

        private static string FormatValue(float value)
        {
            return Mathf.Approximately(value, Mathf.Round(value))
                ? Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture)
                : value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private sealed class ExpressionParser
        {
            private readonly List<string> _tokens;
            private int _index;

            public ExpressionParser(List<string> tokens)
            {
                _tokens = tokens;
            }

            public bool AtEnd => _index == _tokens.Count;

            public bool TryParse(out float value)
            {
                if (!TryParseAdditive(out value))
                    return false;

                return true;
            }

            private bool TryParseAdditive(out float value)
            {
                if (!TryParseMultiplicative(out value))
                    return false;

                while (TryRead("+") || TryRead("-"))
                {
                    var operation = _tokens[_index - 1];
                    if (!TryParseMultiplicative(out var right))
                        return false;

                    value = operation == "+" ? value + right : value - right;
                }

                return true;
            }

            private bool TryParseMultiplicative(out float value)
            {
                if (!TryParsePrimary(out value))
                    return false;

                while (TryRead("*") || TryRead("/"))
                {
                    var operation = _tokens[_index - 1];
                    if (!TryParsePrimary(out var right))
                        return false;

                    if (operation == "/" && Mathf.Approximately(right, 0f))
                        return false;

                    value = operation == "*" ? value * right : value / right;
                }

                return true;
            }

            private bool TryParsePrimary(out float value)
            {
                if (TryRead("(") && TryParseAdditive(out value) && TryRead(")"))
                    return true;

                if (_index >= _tokens.Count)
                {
                    value = 0f;
                    return false;
                }

                var token = _tokens[_index++];
                if (float.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                    return true;

                if (Regex.IsMatch(token, @"^[A-Za-z][A-Za-z0-9]*$"))
                {
                    value = GetStat(token);
                    return true;
                }

                value = 0f;
                return false;
            }

            private bool TryRead(string token)
            {
                if (_index >= _tokens.Count || _tokens[_index] != token)
                    return false;

                _index++;
                return true;
            }
        }
    }
}