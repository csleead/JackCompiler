using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class LetStatementGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.Let };

    public static IElement Compile(TokenReader tokenReader)
    {
        var element = new NonTerminalElement(NonTerminalElementKind.LetStatement);

        if (!Match(tokenReader))
        {
            throw new InvalidOperationException($"Expected keyword 'let', got '{tokenReader.Current}");
        }

        // let keyword
        element.AddChild(new TerminalElement(tokenReader.Current));
        tokenReader.Advance();

        // TODO: implement let with indexer
        if (tokenReader.Current is not Identifier identifier)
        {
            throw new ParsingException($"Expected identifier after var, got '{tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(identifier));
        tokenReader.Advance();

        if (tokenReader.Current is Symbol { Kind: SymbolKind.OpenSquareBracket } openIndexer)
        {
            element.AddChild(new TerminalElement(openIndexer));
            tokenReader.Advance();

            element.AddChild(ExpressionGrammar.Compile(tokenReader));

            if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseSquareBracket } closeIndexer)
            {
                throw new ParsingException($"Expected ']' after expression, got '{tokenReader.Current}");
            }
            element.AddChild(new TerminalElement(closeIndexer));
            tokenReader.Advance();
        }

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.Equal } equal)
        {
            throw new ParsingException($"Expected '=' in a let statement, got '{tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(equal));
        tokenReader.Advance();

        element.AddChild(ExpressionGrammar.Compile(tokenReader));

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.SemiColon } semiColon)
        {
            throw new ParsingException($"Expected ';' at the end of a let statement, got '{tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(semiColon));
        tokenReader.Advance();

        return element;
    }
}
