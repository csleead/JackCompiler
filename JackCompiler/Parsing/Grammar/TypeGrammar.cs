using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class TypeGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Identifier or Keyword { Kind: KeywordKind.Int or KeywordKind.Char or KeywordKind.Boolean };

    public static IElement Compile(TokenReader tokenReader)
    {
        if (tokenReader.Current is Identifier typeIdentifier)
        {
            tokenReader.Advance();
            return new TerminalElement(typeIdentifier);
        }

        if (tokenReader.Current is Keyword { Kind: KeywordKind.Int or KeywordKind.Char or KeywordKind.Boolean } keyword)
        {
            tokenReader.Advance();
            return new TerminalElement(keyword);
        }

        throw new ParsingException($"Expecting a primitive type or an identifier in type but got {tokenReader.Current}");
    }
}
