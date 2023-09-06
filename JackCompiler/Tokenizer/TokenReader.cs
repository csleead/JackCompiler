using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JackCompiler.Tokenizer;

public class TokenReader
{
    private readonly IReadOnlyList<IToken> _tokens;
    private int _cursor;

    public TokenReader(IReadOnlyList<IToken> tokens)
    {
        _tokens = tokens;
        _cursor = -1;
    }

    public IToken Current =>
        _tokens[_cursor];

    public void Advance() =>
        _cursor++;

    public bool HasMoreTokens() =>
        _cursor < _tokens.Count - 1;

    public IToken Peek(int n) =>
        _tokens[_cursor + n];
}
