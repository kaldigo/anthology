using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Anthology.Utils
{
    public static class StringUtils
    {
        public static bool CompareStrings(string query, string compare)
        {
            if (string.IsNullOrWhiteSpace(query)) return true;
            if (string.IsNullOrWhiteSpace(compare)) return false;

            query = RemoveSpecialCharacters(query.Trim());
            compare = RemoveSpecialCharacters(compare.Trim());

            var foundWords = new List<string>();

            var queryWordList = query.Split(' ').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            foreach (var queryWord in queryWordList)
            {
                if (compare.Split(' ').Where(c => !string.IsNullOrWhiteSpace(c)).Any(c => c.ToLower().Contains(queryWord.ToLower())))
                {
                    foundWords.Add(queryWord);
                }
            }
            
            bool match = queryWordList.Count == foundWords.Count;

            return match;
        }

        public static string RemoveSpecialCharacters(this string str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || c == ' ')
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }
    }
}
