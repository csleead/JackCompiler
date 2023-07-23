using System.Security.Principal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Tokenizer;

public class Tokenizer
{
    private static readonly IReadOnlyDictionary<string, KeywordKind> Keywords = new Dictionary<string, KeywordKind>()
    {
        { "class", KeywordKind.Class },
        { "constructor", KeywordKind.Constructor },
        { "function", KeywordKind.Function },
        { "method", KeywordKind.Method },
        { "field", KeywordKind.Field },
        { "static", KeywordKind.Static },
        { "var", KeywordKind.Var },
        { "int", KeywordKind.Int },
        { "char", KeywordKind.Char },
        { "boolean", KeywordKind.Boolean },
        { "void", KeywordKind.Void },
        { "true", KeywordKind.True },
        { "false", KeywordKind.False },
        { "null", KeywordKind.Null },
        { "this", KeywordKind.This },
        { "let", KeywordKind.Let },
        { "do", KeywordKind.Do },
        { "if", KeywordKind.If },
        { "else", KeywordKind.Else },
        { "while", KeywordKind.While },
        { "return", KeywordKind.Return },
    };

    private static readonly string Symbols = "{}()[].,;+-*/&|<>=~";

    private readonly string _code;
    private int _cursor = 0;

    private Tokenizer(string code)
    {
        _code = code;
    }

    public static async Task<TokenReader> FromSourceFile(string sourceFile)
    {
        var code = await File.ReadAllTextAsync(sourceFile);
        var tokens = new Tokenizer(code).Tokenize();
        return new TokenReader(tokens);
    }

    private List<IToken> Tokenize()
    {
        var result = new List<IToken>();
        while (_cursor < _code.Length)
        {
            var code = _code[_cursor..];
            if (_code[_cursor] is ' ' or '\n' or '\r' or '\t')
            {
                _cursor++;
            }
            else if (_code[_cursor] is '/')
            {
                if (_code[_cursor + 1] is '/')
                {
                    var endOfLine = _code.IndexOf('\n', _cursor + 2);
                    _cursor = endOfLine == -1 ? _code.Length : endOfLine + 1;
                }
                else if (_code[_cursor + 1] is '*')
                {
                    var end = _code.IndexOf("*/", _cursor + 2);
                    if (end < -1)
                    {
                        throw new TokenizerException("Cannot find end of block comment");
                    }

                    _cursor = end + 2;
                }
                else
                {
                    result.Add(new Symbol(SymbolKind.Divide));
                    _cursor++;
                }
            }
            else if (Symbols.Contains(_code[_cursor]))
            {
                var token = _code[_cursor] switch
                {
                    '{' => new Symbol(SymbolKind.OpenCurlyBracket),
                    '}' => new Symbol(SymbolKind.CloseCurlyBracket),
                    '(' => new Symbol(SymbolKind.OpenBracket),
                    ')' => new Symbol(SymbolKind.CloseBracket),
                    '[' => new Symbol(SymbolKind.OpenSquareBracket),
                    ']' => new Symbol(SymbolKind.CloseSquareBracket),
                    '.' => new Symbol(SymbolKind.Dot),
                    ',' => new Symbol(SymbolKind.Comma),
                    ';' => new Symbol(SymbolKind.SemiColon),
                    '+' => new Symbol(SymbolKind.Plus),
                    '-' => new Symbol(SymbolKind.Minus),
                    '*' => new Symbol(SymbolKind.Multiply),
                    '/' => new Symbol(SymbolKind.Divide),
                    '&' => new Symbol(SymbolKind.And),
                    '|' => new Symbol(SymbolKind.Or),
                    '>' => new Symbol(SymbolKind.GreaterThan),
                    '<' => new Symbol(SymbolKind.LowerThan),
                    '=' => new Symbol(SymbolKind.Equal),
                    '~' => new Symbol(SymbolKind.Inverse),
                    _ => throw new TokenizerException($"Unexpected symbol '{_code[_cursor]}'"),
                };
                result.Add(token);

                _cursor++;
            }
            else if (IsDigit(_code[_cursor]))
            {
                var start = _cursor;

                _cursor++;
                while (_cursor < _code.Length && IsDigit(_code[_cursor]))
                {
                    _cursor++;
                }
                result.Add(new IntegerConstant(int.Parse(_code.Substring(start, _cursor - start))));
            }
            else if (_code[_cursor] is '\"')
            {
                var start = _cursor;
                _cursor++;
                while (_cursor < _code.Length && _code[_cursor] is not '\"')
                {
                    _cursor++;
                }
                _cursor++;

                var value = _code[start.._cursor].Replace("\"", "");
                result.Add(new StringConstant(value));
            }
            else if (IsStartingIdentifierChar(_code[_cursor]))
            {
                var start = _cursor;
                _cursor++;
                while (_cursor < _code.Length && IsIdentifierChar(_code[_cursor]))
                {
                    _cursor++;
                }

                var value = _code.Substring(start, _cursor - start);
                if (Keywords.TryGetValue(value, out var kind))
                {
                    result.Add(new Keyword(kind));
                }
                else
                {
                    result.Add(new Identifier(value));
                }
            }
            else
            {
                throw new TokenizerException($"Unexpected character '{_code[_cursor]}'");
            }
        }

        return result;
    }

    private static bool IsDigit(char c) => c >= '0' && c <= '9';
    private static bool IsStartingIdentifierChar(char c) => c is (>= 'A' and <= 'Z') or (>= 'a' and <= 'z') or '_';
    private static bool IsIdentifierChar(char c) => IsDigit(c) || IsStartingIdentifierChar(c);
};
