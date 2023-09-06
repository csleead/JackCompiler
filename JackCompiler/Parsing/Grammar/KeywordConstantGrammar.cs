using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class KeywordConstantGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.True or KeywordKind.False or KeywordKind.Null or KeywordKind.This };

    public static IElement Compile(TokenReader tokenReader)
    {
        if (!Match(tokenReader))
        {
            throw new Exception($"Expected a keyword constant, but got {tokenReader.Current}");
        }

        var element = new TerminalElement(tokenReader.Current);
        tokenReader.Advance();
        return element;
    }
}
