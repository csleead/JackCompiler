using JackCompiler.Tokenizer;

namespace JackCompiler.Grammar;

public class IfStatementGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.If };

    public static IElement Compile(TokenReader tokenReader)
    {
        if (!Match(tokenReader))
        {
            throw new ParsingException($"Expected if, got {tokenReader.Current}");
        }

        var element = new NonTerminalElement(NonTerminalElementKind.IfStatement);

        element.AddChild(new TerminalElement(tokenReader.Current));
        tokenReader.Advance(); // Consume if keyword

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

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.OpenCurlyBracket } trueOpen)
        {
            throw new ParsingException($"Expected {{ for if true statements, got {tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(trueOpen));
        tokenReader.Advance();

        element.AddChild(StatementsGrammar.Compile(tokenReader));

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseCurlyBracket } trueClose)
        {
            throw new ParsingException($"Expected }} for if true statements, got {tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(trueClose));
        tokenReader.Advance();

        if (tokenReader.Current is Keyword { Kind: KeywordKind.Else } @else)
        {
            element.AddChild(new TerminalElement(@else));
            tokenReader.Advance();

            if (tokenReader.Current is not Symbol { Kind: SymbolKind.OpenCurlyBracket } falseOpen)
            {
                throw new ParsingException($"Expected {{ for if false statements, got {tokenReader.Current}");
            }
            element.AddChild(new TerminalElement(falseOpen));
            tokenReader.Advance();

            element.AddChild(StatementsGrammar.Compile(tokenReader));

            if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseCurlyBracket } falseClose)
            {
                throw new ParsingException($"Expected }} for if false statements, got {tokenReader.Current}");
            }
            element.AddChild(new TerminalElement(falseClose));
            tokenReader.Advance();
        }

        return element;
    }
}
