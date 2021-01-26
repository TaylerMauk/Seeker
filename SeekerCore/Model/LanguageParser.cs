using System;
using System.Collections.Generic;

namespace SeekerCore.Model
{
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
        public SearchConstructs.CriteriaInfo[] Parse(string phrase)
        {
            List<SearchConstructs.CriteriaInfo> criteria = new List<SearchConstructs.CriteriaInfo>();
            string[] keyPhrases = phrase.Split(SEPARATOR_CHAR);

            foreach (string s in keyPhrases)
                criteria.AddRange(ParsePhrase(s));

            return criteria.ToArray();
        }

        /// <summary>
        /// Translates the given CriteriaInfo array into a more user-friendly explanation
        /// </summary>
        /// <param name="criteria">CriteriaInfo array to translate</param>
        public void GetFriendlyTranslation(SearchConstructs.CriteriaInfo[] criteria)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Parses the given phrase into CriteriaInfo struct(s). Will recurse for
        /// chained FlipFlop criteria.
        /// </summary>
        /// <param name="phrase">Phrase to parse</param>
        /// <returns>List of generated CriteriaInfo structs</returns>
        private List<SearchConstructs.CriteriaInfo> ParsePhrase(string phrase)
        {
            List<SearchConstructs.CriteriaInfo> resultCriteria = new List<SearchConstructs.CriteriaInfo>();
            SearchConstructs.CriteriaInfo resultCriterion;

            phrase.Trim();

            if (phrase.Contains(FLIPFLOP_CHAR))
            {
                // Parse chained FlipFlop criteria
                string[] flipFlops = phrase.Split(FLIPFLOP_CHAR);
                foreach (string s in flipFlops)
                    resultCriteria.AddRange(ParsePhrase(s));
            }

            if (phrase.StartsWith(EXCLUDE_CHAR))
            {
                resultCriterion.type = SearchConstructs.CriteriaType.EXCLUDE;
                resultCriterion.keyphrase = phrase.TrimStart(EXCLUDE_CHAR);
            }
            else
            {
                resultCriterion.type = SearchConstructs.CriteriaType.SINGLE;
                resultCriterion.keyphrase = phrase;
            }

            resultCriteria.Add(resultCriterion);
            return resultCriteria;
        }
    }
}
