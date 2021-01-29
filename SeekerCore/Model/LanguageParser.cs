using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using static SeekerCore.Model.SearchConstructs;

namespace SeekerCore.Model
{
    /// <summary>
    /// Class for parsing the Seeker search language
    /// </summary>
    class LanguageParser
    {
        private const char SEPARATOR_CHAR = ',';
        private const char EXCLUDE_CHAR = '-';
        private const char FLIPFLOP_CHAR = '|';

        public LanguageParser() { }

        /// <summary>
        /// Parses the user-entered search phrase into CriteriaInfo structs
        /// </summary>
        /// <param name="phrase">Entire search phrase entered by user</param>
        /// <returns>Array of CriteriaInfo</returns>
        public CriteriaInfo[] Parse(string phrase)
        {
            List<CriteriaInfo> criteria = new List<CriteriaInfo>();
            string[] keyPhrases = phrase.Split(SEPARATOR_CHAR);

            foreach (string s in keyPhrases)
                criteria.AddRange(ParsePhrase(s));

            return criteria.ToArray();
        }

        /// <summary>
        /// Translates the given CriteriaInfo array into a more user-friendly explanation
        /// </summary>
        /// <param name="criteria">CriteriaInfo array to translate</param>
        /// <returns>User-friendly translation</returns>
        public string GetFriendlyTranslation(CriteriaInfo[] criteria)
        {
            StringBuilder translation = new StringBuilder(Environment.NewLine);

            for (int i = 0; i < criteria.Length; ++i)
            {
                // Catch any criteria that have the FlipFlop attribute
                if ((criteria[i].type & CriteriaType.FLIPFLOP) == CriteriaType.FLIPFLOP)
                {
                    List<CriteriaInfo> chainedFlipflopCriteria = new List<CriteriaInfo>();
                    int j = i;
                    while (j < criteria.Length && (criteria[j].type & CriteriaType.FLIPFLOP) == CriteriaType.FLIPFLOP)
                    {
                        chainedFlipflopCriteria.Add(criteria[j]);
                        ++j;
                    }

                    translation.Append(
                        GetFriendlyFlipflopTranslation(chainedFlipflopCriteria.ToArray())
                    );

                    // Account for criteria that was just processed
                    i += chainedFlipflopCriteria.Count - 1;
                }
                else
                {
                    if (criteria[i].type == CriteriaType.EXCLUDE)
                        translation.Append("DO NOT ");

                    translation.Append("CONTAIN ");
                    translation.Append(criteria[i].keyphrase);
                }

                if (i != criteria.Length - 1)
                    translation.Append(Environment.NewLine);
            }

            return translation.ToString();
        }

        /// <summary>
        /// Checks if given phrase adheres to Seeker search language rules
        /// </summary>
        /// <param name="phrase">Phrase to validate</param>
        /// <returns>True if the phrase is valid</returns>
        public bool IsPhraseValid(string phrase)
        {
            // Do not allow 2 or more special chars next to each other (excluding EXCLUDE_CHAR)
            Regex multipleSpecialCharRegex = new Regex(
                @"([" + SEPARATOR_CHAR + "\\" + EXCLUDE_CHAR + FLIPFLOP_CHAR + @"]\s*)([" + SEPARATOR_CHAR + FLIPFLOP_CHAR + @"]\s*)+"
            );

            // Do not allow the phrase to end with a special char
            Regex endsWithSpecialCharRegex = new Regex(
                @"[" + SEPARATOR_CHAR + "\\" + EXCLUDE_CHAR + FLIPFLOP_CHAR + @"]\s*$"
            );

            return !(
                multipleSpecialCharRegex.IsMatch(phrase) ||
                endsWithSpecialCharRegex.IsMatch(phrase)
            );
        }

        /// <summary>
        /// Parses the given phrase into CriteriaInfo struct(s). Will recurse for
        /// chained FlipFlop criteria.
        /// </summary>
        /// <param name="phrase">Phrase to parse</param>
        /// <returns>List of generated CriteriaInfo structs</returns>
        private List<CriteriaInfo> ParsePhrase(string phrase)
        {
            List<CriteriaInfo> resultCriteria = new List<CriteriaInfo>();
            CriteriaInfo resultCriterion;
            string parsePhrase = phrase.Trim();

            if (parsePhrase.Contains(FLIPFLOP_CHAR))
            {
                // Parse chained FlipFlop criteria
                string[] flipFlops = phrase.Split(FLIPFLOP_CHAR);
                foreach (string s in flipFlops)
                    resultCriteria.AddRange(ParsePhrase(s));

                // FlipFlop may include an Exclude in it, such criteria will
                // have both attributes set.
                string tmpKeyphrase;
                CriteriaType tmpType;
                for (int i = 0; i < resultCriteria.Count; ++i)
                {
                    tmpKeyphrase = resultCriteria[i].keyphrase;
                    tmpType = resultCriteria[i].type;

                    if (tmpType == CriteriaType.EXCLUDE)
                        tmpType |= CriteriaType.FLIPFLOP;
                    else
                        tmpType = CriteriaType.FLIPFLOP;

                    resultCriteria[i] = new CriteriaInfo {
                        keyphrase = tmpKeyphrase,
                        type = tmpType
                    };
                }

                return resultCriteria;
            }

            if (parsePhrase.StartsWith(EXCLUDE_CHAR))
            {
                resultCriterion.type = CriteriaType.EXCLUDE;
                resultCriterion.keyphrase = parsePhrase.TrimStart(EXCLUDE_CHAR);
            }
            else
            {
                resultCriterion.type = CriteriaType.SINGLE;
                resultCriterion.keyphrase = parsePhrase;
            }

            resultCriteria.Add(resultCriterion);
            return resultCriteria;
        }

        /// <summary>
        /// Translates chained FlipFlop sequences to user-friendly readouts
        /// </summary>
        /// <param name="criteria">CriteriaInfo array of FlipFlop criteria</param>
        /// <returns>User-friendly translation</returns>
        private string GetFriendlyFlipflopTranslation(CriteriaInfo[] criteria)
        {
            StringBuilder translation = new StringBuilder();

            for (int i = 0; i < criteria.Length; ++i)
            {
                if ((criteria[i].type & CriteriaType.EXCLUDE) == CriteriaType.EXCLUDE)
                    translation.Append("DO NOT ");

                translation.Append("CONTAIN ");
                translation.Append(criteria[i].keyphrase);

                if (i != criteria.Length - 1)
                    translation.Append(" OR").Append(Environment.NewLine);
            }

            return translation.ToString();
        }
    }
}
