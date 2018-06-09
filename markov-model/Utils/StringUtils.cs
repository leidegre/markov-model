using System;

namespace MarkovModel.Utils
{
    public static class StringUtils
    {
        private readonly static char[] _whiteSpace = new[] { '\t', '\n', '\r', '\u0020', '\u00A0' };
        private readonly static string _space = "\u0020";
        private readonly static string[] _empty = new string[0];

        public static string[] Words(string s)
        {
            if (s == null)
            {
                return _empty;
            }
            var words = s.Split(_whiteSpace, int.MaxValue, StringSplitOptions.RemoveEmptyEntries);
            return words;
        }

        public static string Normalize(string s)
        {
            var words = Words(s);
            if (words.Length == 0)
            {
                return string.Empty;
            }
            var normal = string.Join(_space, words);
            return normal;
        }

        public static string Capitalize(string s)
        {
            var words = Words(s);
            if (words.Length == 0)
            {
                return string.Empty;
            }
            for (int i = 0; i < words.Length; i++)
            {
                var x = words[i];
                words[i] = x.Substring(0, 1).ToUpperInvariant() + x.Substring(1).ToLowerInvariant();
            }
            var capit = string.Join(_space, words);
            return capit;
        }
    }
}
