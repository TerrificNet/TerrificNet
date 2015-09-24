using System;
using System.Linq;

namespace TerrificNet.Thtml.LexicalAnalysis
{
    internal static class CharacterClasses
    {
        public static readonly char[] WhitespaceCharacters = { (char)0x20, (char)0x9, (char)0xD, (char)0xA };
        public static readonly char[] NotInAttributeValueCharacters = { '<', '&', '"' };
        public static readonly char[] NotInContentValueCharacters = { '<', '&' };

        public static bool IsNameChar(char c)
        {
            return Check(
                () => IsNameStartChar(c),
                () => c == '-',
                () => InRange(c, '0', '9'),
                () => c == 0xB7,
                () => InRange(c, (char)0x0300, (char)0x036F),
                () => InRange(c, (char)0x203F, (char)0x2040)
                );
        }

        public static bool IsNameStartChar(char c)
        {
            return Check(
                () => c == ':',
                () => InRange(c, 'A', 'Z'),
                () => c == '_',
                () => InRange(c, 'a', 'z'),
                () => InRange(c, (char)0xC0, (char)0xD6),
                () => InRange(c, (char)0xD8, (char)0xF6),
                () => InRange(c, (char)0xF8, (char)0x2FF),
                () => InRange(c, (char)0x370, (char)0x37D),
                () => InRange(c, (char)0x37F, (char)0x1FFF),
                () => InRange(c, (char)0x200C, (char)0x200D),
                () => InRange(c, (char)0x2070, (char)0x218F),
                () => InRange(c, (char)0x2C00, (char)0x2FEF),
                () => InRange(c, (char)0x3001, (char)0xD7FF),
                () => InRange(c, (char)0xF900, (char)0xFDCF),
                () => InRange(c, (char)0xFDF0, (char)0xFFFD)//,
                //() => InRange(c, (char)0x10000, (char)0xEFFFF)
                );
        }

        private static bool Check(params Func<bool>[] predicates)
        {
            foreach (var predicate in predicates)
            {
                if (predicate())
                    return true;
            }
            return false;
        }

        private static bool InRange(char c, char lower, char upper)
        {
            return c >= lower && c <= upper;
        }

        public static bool IsCharData(char c1, char c2)
        {
            return (c1 != '{' || c2 != '{') && !NotInAttributeValueCharacters.Contains(c1);
        }

        public static bool IsAttributeValue(char c1, char c2)
        {
            return (c1 != '{' || c2 != '{') && !NotInAttributeValueCharacters.Contains(c1);
        }
    }
}