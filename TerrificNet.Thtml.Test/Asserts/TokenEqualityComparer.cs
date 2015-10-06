using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml.Test.Asserts
{
    internal class TokenEqualityComparer : IEqualityComparer<Token>
    {
        public bool Equals(Token x, Token y)
        {
            var xc = x as CompositeToken;
            var yc = y as CompositeToken;
            if (xc != null && yc != null)
            {
                return x.Category == y.Category && x.Start == y.Start && x.End == y.End 
                       && ((xc.Tokens == null && yc.Tokens == null) || (xc.Tokens?.SequenceEqual(yc.Tokens, this) ?? false));
            }
            return x.Category == y.Category && String.Equals(x.Lexem, y.Lexem) && x.Start == y.Start && x.End == y.End;
        }

        public int GetHashCode(Token obj)
        {
            unchecked
            {
                var token = obj as CompositeToken;
                if (token != null)
                    return ((int) token.Category*397) ^ (token.Lexem?.GetHashCode() ?? 0) ^ token.Start ^ token.End ^ (token.Tokens?.GetHashCode() ?? 1);

                return ((int)obj.Category * 397) ^ (obj.Lexem?.GetHashCode() ?? 0) ^ obj.Start ^ obj.End;
            }
        }
    }
}