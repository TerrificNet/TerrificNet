using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace TerrificNet.ViewEngine.IO
{
    public class GlobPattern
    {
        private readonly string _pattern;
        private static readonly HashSet<char> RegexSpecialChars = new HashSet<char>(new[] { '[', '\\', '^', '$', '.', '|', '?', '*', '+', '(', ')' });
        private readonly Regex _regex;

        private GlobPattern(string pattern)
        {
            _pattern = pattern;
            bool isWildcard;
            _regex = new Regex(GlobToRegex(pattern, out isWildcard));

            this.IsWildcard = isWildcard;
        }

        public bool IsWildcard { get; }

        public static GlobPattern Create(string pattern)
        {
            return new GlobPattern(pattern);
        }

        public bool IsMatch(PathInfo path)
        {
            return _regex.IsMatch(path.ToString());
        }

        private static string GlobToRegex(string glob, out bool wildcard)
        {
            wildcard = false;
            StringBuilder regex = new StringBuilder();
            bool characterClass = false;

            regex.Append("^");

            foreach (var c in glob)
            {
                if (characterClass)
                {
                    if (c == ']') characterClass = false;
                    regex.Append(c);
                    continue;
                }

                switch (c)
                {
                    case '*':
                        regex.Append(".*");
                        wildcard = true;
                        break;
                    case '?':
                        regex.Append(".");
                        wildcard = true;
                        break;
                    case '[':
                        characterClass = true;
                        wildcard = true;
                        regex.Append(c);
                        break;
                    default:
                        if (RegexSpecialChars.Contains(c)) regex.Append('\\');
                        regex.Append(c);
                        break;
                }
            }

            regex.Append("$");

            return regex.ToString();
        }

        public static GlobPattern AllWithExtension(string extension)
        {
            return Create("**." + extension);
        }

        public readonly static GlobPattern All = new GlobPattern("**");

        public static GlobPattern Exact(string item)
        {
            return Create(item);
        }
    }
}