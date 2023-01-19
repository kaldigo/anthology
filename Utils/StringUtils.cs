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
            query = RemoveSpecialCharacters(query.Trim());
            compare = RemoveSpecialCharacters(compare.Trim());

            var foundWords = new List<string>();

            var queryWordList = query.Split(' ').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
            foreach (var queryWord in queryWordList)
            {
                var wordMatch = false;
                var compareWords = compare.Split(' ').Where(c => !string.IsNullOrWhiteSpace(c)).ToList();
                foreach (var compareWord in compareWords)
                {
                    var a = queryWord.ToLower().Contains(compareWord.ToLower());
                    var b = compareWord.ToLower().Contains(queryWord.ToLower());

                    if (a || b)
                    {
                        wordMatch = a || b;
                    }
                }
                if(wordMatch) foundWords.Add(queryWord);
            }
            
            bool match = queryWordList.Count == foundWords.Count;

            if (match)
            {
                bool test = true;
            }

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
