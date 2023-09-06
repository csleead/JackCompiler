using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class WhileStatementGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.While };

    public static IElement Compile(TokenReader tokenReader)
    {
        if (!Match(tokenReader))
        {
            throw new ParsingException("A while statement should start with a while keyword");
        }

        var element = new NonTerminalElement(NonTerminalElementKind.WhileStatement);

        element.AddChild(new TerminalElement(tokenReader.Current));
        tokenReader.Advance(); // Consume while keyword

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.OpenBracket } conditionOpen)
        {
            throw new ParsingException($"Expected ( for a if condition, got {tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(conditionOpen));
        tokenReader.Advance();

        element.AddChild(ExpressionGrammar.Compile(tokenReader));

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseBracket } conditionClose)
        {
            throw new ParsingException($"Expected ) for a if condition, got {tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(conditionClose));
        tokenReader.Advance();

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.OpenCurlyBracket } open)
        {
            throw new ParsingException($"Expected {{ for while statements, got {tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(open));
        tokenReader.Advance();

        element.AddChild(StatementsGrammar.Compile(tokenReader));

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseCurlyBracket } close)
        {
            throw new ParsingException($"Expected }} for while statements, got {tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(close));
        tokenReader.Advance();

        return element;
    }
}
