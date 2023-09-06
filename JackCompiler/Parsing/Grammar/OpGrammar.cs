using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class OpGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Symbol
        {
            Kind: SymbolKind.Plus
            or SymbolKind.Minus
            or SymbolKind.Multiply
            or SymbolKind.Divide
            or SymbolKind.And
            or SymbolKind.Or
            or SymbolKind.LowerThan
            or SymbolKind.GreaterThan
            or SymbolKind.Equal
        };

    public static IElement Compile(TokenReader tokenReader)
    {
        if (!Match(tokenReader))
        {
            throw new ParsingException($"Expected an operator but got {tokenReader.Current}");
        }

        var element = new TerminalElement(tokenReader.Current);
        tokenReader.Advance();
        return element;
    }
}
