using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml.Test
{
    static internal class TokenFactory
    {
        public static Token EmptyElement(string lexem, int position, int end)
        {
            return new CompositeToken(TokenCategory.EmptyElement, lexem, position, end);
        }

        public static Token Content(string lexem, int start)
        {
            return new Token(TokenCategory.Content, lexem, start, start + lexem.Length);
        }

        public static Token ElementEnd(string lexem, int position, int end)
        {
            return new CompositeToken(TokenCategory.ElementEnd, lexem, position, end);
        }

        public static Token EndToken(int position)
        {
            return new Token(TokenCategory.EndDocument, position, position);
        }

        public static Token Whitespace(int position)
        {
            return Whitespace(" ", position);
        }

        public static Token Whitespace(string lexem, int position)
        {
            return new Token(TokenCategory.Whitespace, lexem, position, position + lexem.Length);
        }

        public static Token ElementStart(string tagName, int position, params Func<int, Token>[] attributeTokenFactories)
        {
            var factories = new List<Func<int, Token>>
            {
                BracketOpen,
                i => Name(tagName, i)
            };
            factories.AddRange(attributeTokenFactories);
            factories.Add(BracketClose);

            return Composite(position, TokenCategory.ElementStart, factories.ToArray());
        }

        public static Token ElementEnd(string tagName, int position)
        {
            return Composite(position, TokenCategory.ElementEnd,
                BracketOpen,
                Slash,
                i => Name(tagName, i),
                BracketClose);
        }

        public static Token Composite(int start, TokenCategory tokenCategory, params Func<int, Token>[] tokenFactories)
        {
            return new CompositeToken(tokenCategory, List(start, tokenFactories).ToList());
        }

        public static IEnumerable<Token> DocumentList(params Func<int, Token>[] tokenFactories)
        {
            var token = new Token(TokenCategory.StartDocument, 0, 0);
            yield return token;

            foreach (var listToken in List(token.End, tokenFactories))
            {
                yield return listToken;
                token = listToken;
            }

            yield return EndToken(token.End);
        }

        private static IEnumerable<Token> List(int start, params Func<int, Token>[] tokenFactories)
        {
            foreach (var factory in tokenFactories)
            {
                var token = factory(start);
                start = token.End;
                yield return token;
            }
        }

        public static Token BracketOpen(int position)
        {
            return new Token(TokenCategory.BracketOpen, "<", position, position + 1);
        }

        public static Token Name(string name, int position)
        {
            return new Token(TokenCategory.Name, name, position, position + name.Length);
        }

        public static Token BracketClose(int position)
        {
            return new Token(TokenCategory.BracketClose, ">", position, position + 1);
        }

        public static Token Equal(int position)
        {
            return new Token(TokenCategory.Equality, "=", position, position + 1);
        }

        public static Token Quote(int position)
        {
            return new Token(TokenCategory.Quote, "\"", position, position + 1);
        }

        public static Token AttributeContent(string content, int position)
        {
            return new Token(TokenCategory.AttributeContent, content, position, position + content.Length);
        }

        public static Token AttributeWithoutContent(int a, string name)
        {
            return Composite(a,
                TokenCategory.Attribute,
                b => Name(name, b));
        }

        public static Token AttributeWithContent(int a, string name, string content)
        {
            return Composite(a,
                TokenCategory.Attribute,
                b => Name(name, b),
                Equal,
                Quote,
                b => AttributeContent(content, b),
                Quote);
        }

        public static Token HandlebarsSimple(int position, string name)
        {
            return Composite(position,
                TokenCategory.HandlebarsEvaluate,
                HandlebarsStart,
                HandlebarsStart,
                a => Name(name, a),
                HandlebarsEnd,
                HandlebarsEnd);
        }

        public static Token Slash(int position)
        {
            return new Token(TokenCategory.Slash, "/", position, position + 1);
        }

        public static Token HandlebarsStart(int position)
        {
            return new Token(TokenCategory.HandlebarsStart, "{", position, position + 1);
        }

        public static Token HandlebarsEnd(int position)
        {
            return new Token(TokenCategory.HandlebarsEnd, "}", position, position + 1);
        }
    }
}