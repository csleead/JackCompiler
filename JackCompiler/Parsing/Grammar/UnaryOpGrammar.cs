using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class UnaryOpGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Symbol { Kind: SymbolKind.Minus or SymbolKind.Inverse };

    public static IElement Compile(TokenReader tokenReader)
    {
        if (!Match(tokenReader))
        {
            throw new Exception($"Expected unary operator but got {tokenReader.Current}");
        }

        var element = new TerminalElement(tokenReader.Current);
        tokenReader.Advance();
        return element;
    }
}