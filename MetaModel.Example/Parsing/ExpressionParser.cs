using MetaModel.Example.Contracts;
using MetaModel.Example.Expressions;
using MetaModel.Example.Properties;
using System.Globalization;

namespace MetaModel.Example.Parsing;

// Very small recursive descent parser supporting: assignment, + - * /, numbers, and vector literals [a,b,c]
public static class ExpressionParser
{
    public static INode Parse(string input)
    {
        var tokenizer = new Tokenizer(input);
        var expr = ParseAssignment(tokenizer);
        if (tokenizer.Current.Type != TokenType.EOF)
            throw new Exception("Unexpected trailing input");
        return expr;
    }

    // New: parse a semicolon-separated script into a ScriptNode
    public static ScriptNode ParseScript(string input)
    {
        var tokenizer = new Tokenizer(input);
        var statements = new List<INode>();
        while (tokenizer.Current.Type != TokenType.EOF)
        {
            var stmt = ParseAssignment(tokenizer);
            statements.Add(stmt);
            if (tokenizer.Current.Type == TokenType.Semicolon)
            {
                tokenizer.Advance();
                // Allow trailing semicolon
                if (tokenizer.Current.Type == TokenType.EOF) break;
                continue;
            }
            // If not semicolon, expect EOF; otherwise error
            if (tokenizer.Current.Type != TokenType.EOF)
                throw new Exception($"Expected ';' or EOF, found {tokenizer.Current.Type}");
        }
        return new ScriptNode(statements);
    }

    private static INode ParseAssignment(Tokenizer tz)
    {
        if (tz.Current.Type == TokenType.Identifier)
        {
            var ident = tz.Current.Text;
            tz.Advance();
            if (tz.Current.Type == TokenType.Equal)
            {
                tz.Advance();
                var rhs = ParseExpression(tz);
                return new AssignmentNode(ident, rhs);
            }
            // Not an assignment; we consumed the identifier already; just treat it as variable and continue parsing additive starting from that node
            // So we create a VariableNode and then let additive continue with remaining tokens.
            var first = new VariableNode(ident, new NumberNode(0));
            return ParseAdditiveContinuation(tz, first);
        }
        return ParseExpression(tz);
    }

    private static INode ParseExpression(Tokenizer tz) => ParseAdditive(tz);

    private static INode ParseAdditiveContinuation(Tokenizer tz, INode left)
    {
        var node = left;
        while (tz.Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = tz.Current.Type;
            tz.Advance();
            var right = ParseMultiplicative(tz);
            node = op switch
            {
                TokenType.Plus => new AddNode(node, right),
                TokenType.Minus => new SubtractNode(node, right),
                _ => node
            };
        }
        return node;
    }

    private static INode ParseAdditive(Tokenizer tz)
    {
        var node = ParseMultiplicative(tz);
        while (tz.Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var op = tz.Current.Type;
            tz.Advance();
            var right = ParseMultiplicative(tz);
            node = op switch
            {
                TokenType.Plus => new AddNode(node, right),
                TokenType.Minus => new SubtractNode(node, right),
                _ => node
            };
        }
        return node;
    }

    private static INode ParseMultiplicative(Tokenizer tz)
    {
        var node = ParsePrimary(tz);
        while (tz.Current.Type is TokenType.Star or TokenType.Slash)
        {
            var op = tz.Current.Type;
            tz.Advance();
            var right = ParsePrimary(tz);
            node = op switch
            {
                TokenType.Star => new MultiplyNode(node, right),
                TokenType.Slash => new DivideNode(node, right),
                _ => node
            };
        }
        return node;
    }

    private static INode ParsePrimary(Tokenizer tz)
    {
        return tz.Current.Type switch
        {
            TokenType.Number => ParseNumber(tz),
            TokenType.Identifier => ParseVariable(tz),
            TokenType.LParen => ParseParen(tz),
            TokenType.LBracket => ParseVector(tz),
            _ => throw new Exception($"Unexpected token {tz.Current.Type}")
        };
    }

    private static INode ParseParen(Tokenizer tz)
    {
        tz.Advance(); // consume '('
        var expr = ParseExpression(tz);
        if (tz.Current.Type != TokenType.RParen) throw new Exception("Missing closing parenthesis");
        tz.Advance();
        return expr;
    }

    private static INode ParseNumber(Tokenizer tz)
    {
        var text = tz.Current.Text;
        tz.Advance();
        var value = double.Parse(text, CultureInfo.InvariantCulture);
        return new NumberNode(value);
    }

    private static INode ParseVariable(Tokenizer tz)
    {
        var name = tz.Current.Text;
        tz.Advance();
        return new VariableNode(name, new NumberNode(0)); // placeholder zero, will fetch from context
    }

    private static INode ParseVector(Tokenizer tz)
    {
        tz.Advance(); // consume '['
        var components = new List<double>();
        while (tz.Current.Type != TokenType.RBracket)
        {
            if (tz.Current.Type != TokenType.Number)
                throw new Exception("Vector components must be numbers");
            var numNode = (NumberNode)ParseNumber(tz);
            components.Add(numNode.Value);
            if (tz.Current.Type == TokenType.Comma)
            {
                tz.Advance();
                continue;
            }
            if (tz.Current.Type == TokenType.RBracket)
                break;
            throw new Exception("Expected ',' or ']' in vector literal");
        }
        if (tz.Current.Type != TokenType.RBracket) throw new Exception("Missing closing bracket in vector literal");
        tz.Advance();
        return new VectorNode(components);
    }

    #region Tokenizer
    private enum TokenType { Number, Plus, Minus, Star, Slash, LParen, RParen, Equal, Identifier, LBracket, RBracket, Comma, Semicolon, EOF }
    private sealed record Token(TokenType Type, string Text);

    private sealed class Tokenizer
    {
        private readonly string _s;
        private int _pos;
        private Token? _pushBack;
        public Token Current { get; private set; }
        public Tokenizer(string s)
        {
            _s = s;
            Advance();
        }
        public void Advance()
        {
            if (_pushBack != null)
            {
                Current = _pushBack;
                _pushBack = null;
                return;
            }
            SkipWs();
            if (_pos >= _s.Length) { Current = new Token(TokenType.EOF, ""); return; }
            var c = _s[_pos];
            if (char.IsDigit(c) || c == '.')
            {
                var start = _pos;
                while (_pos < _s.Length && (char.IsDigit(_s[_pos]) || _s[_pos] == '.')) _pos++;
                Current = new Token(TokenType.Number, _s[start.._pos]);
                return;
            }
            if (char.IsLetter(c))
            {
                var start = _pos;
                while (_pos < _s.Length && (char.IsLetterOrDigit(_s[_pos]) || _s[_pos] == '_')) _pos++;
                Current = new Token(TokenType.Identifier, _s[start.._pos]);
                return;
            }
            _pos++;
            Current = c switch
            {
                '+' => new Token(TokenType.Plus, "+"),
                '-' => new Token(TokenType.Minus, "-"),
                '*' => new Token(TokenType.Star, "*"),
                '/' => new Token(TokenType.Slash, "/"),
                '(' => new Token(TokenType.LParen, "("),
                ')' => new Token(TokenType.RParen, ")"),
                '[' => new Token(TokenType.LBracket, "["),
                ']' => new Token(TokenType.RBracket, "]"),
                '=' => new Token(TokenType.Equal, "="),
                ',' => new Token(TokenType.Comma, ","),
                ';' => new Token(TokenType.Semicolon, ";"),
                _ => throw new Exception($"Unexpected char '{c}'")
            };
        }
        private void SkipWs() { while (_pos < _s.Length && char.IsWhiteSpace(_s[_pos])) _pos++; }
        public void PushBack(Token token) => _pushBack = token;
    }
    #endregion
}
