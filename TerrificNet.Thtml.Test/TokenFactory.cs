using System;
using System.Collections.Generic;
using System.Linq;
using TerrificNet.Thtml.LexicalAnalysis;

namespace TerrificNet.Thtml.Test
{
    static internal class TokenFactory
    {
        public static Token EmptyElement(string tagName, int position)
        {
            return Composite(position, TokenCategory.EmptyElement, 
                BracketOpen,
                i => Name(tagName, i),
                Slash,
                BracketClose);
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
            if (attributeTokenFactories.Length > 0)
            {
                factories.Add(Whitespace);
                factories.AddRange(attributeTokenFactories);
            }
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
            return AttributeWithContentExtended(a, name, b => AttributeContent(content, b));
        }

        public static Token AttributeWithContentExtended(int a, string name, params Func<int, Token>[] attributeContent)
        {
            var factories = new List<Func<int, Token>>();
            factories.AddRange(new Func<int, Token>[]
            {
                b => Name(name, b),
                Equal,
                Quote
            });
            factories.AddRange(attributeContent);
            factories.Add(Quote);

            return Composite(a,
                TokenCategory.Attribute,
                factories.ToArray());
        }

        public static Token HandlebarsSimple(int position, string name)
        {
            return Composite(position,
                TokenCategory.External,
                HandlebarsStart,
                HandlebarsStart,
                a => Composite(a, TokenCategory.HandlebarsEvaluate,
                    c => Expression(a, name)),
                HandlebarsEnd,
                HandlebarsEnd);
        }

        public static Token Expression(int position, params string[] name)
        {
            var factories = name.SelectMany(NameAndDot);
            return Expression(position, factories.Take(name.Length*2 - 1).ToArray());
        }

        public static Token Expression(int position, params Func<int, Token>[] tokenFactories)
        {
            return Composite(position,
                TokenCategory.HandlebarsExpression,
                tokenFactories);
        }

        private static IEnumerable<Func<int, Token>> NameAndDot(string name)
        {
            yield return a => Name(name, a);
            yield return Dot;
        }

        public static Token Dot(int position)
        {
            return new Token(TokenCategory.Dot, ".", position, position + 1);
        }

        public static Token HandlebarsBlockStart(int position, string name, Func<int, Token> expressionFactory)
        {
            return Composite(position,
                TokenCategory.External,
                HandlebarsStart,
                HandlebarsStart,
                a => Composite(a, TokenCategory.HandlebarsBlockStart,
                    Hash,
                    b => Name(name, b),
                    Whitespace,
                    expressionFactory),
                HandlebarsEnd,
                HandlebarsEnd);
        }

        public static Token HandlebarsBlockEnd(int position, string name)
        {
            return Composite(position,
                TokenCategory.External,
                HandlebarsStart,
                HandlebarsStart,
                a => Composite(a, TokenCategory.HandlebarsBlockEnd,
                    Slash,
                    b => Name(name, b)),
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

        public static Token Hash(int position)
        {
            return new Token(TokenCategory.Hash, "#", position, position + 1);
        }

        public static Token IfStartExpression(string expression, int position)
        {
            return TokenFactory.Composite(position,
                TokenCategory.External,
                TokenFactory.HandlebarsStart,
                TokenFactory.HandlebarsStart,
                a => TokenFactory.Composite(a, TokenCategory.HandlebarsBlockStart,
                    TokenFactory.Hash,
                    b => TokenFactory.Name("if", b),
                    TokenFactory.Whitespace,
                    b => TokenFactory.Expression(b, expression)),
                TokenFactory.HandlebarsEnd,
                TokenFactory.HandlebarsEnd);
        }

        public static Token IfEndExpression(int i)
        {
            return TokenFactory.Composite(i,
                TokenCategory.External,
                TokenFactory.HandlebarsStart,
                TokenFactory.HandlebarsStart,
                a => TokenFactory.Composite(a, TokenCategory.HandlebarsBlockEnd,
                    TokenFactory.Slash,
                    b => TokenFactory.Name("if", b)),
                TokenFactory.HandlebarsEnd,
                TokenFactory.HandlebarsEnd);
        }
    }
}