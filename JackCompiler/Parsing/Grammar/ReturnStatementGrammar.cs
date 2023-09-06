using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class ReturnStatementGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.Return };

    public static IElement Compile(TokenReader tokenReader)
    {
        if (!Match(tokenReader))
        {
            throw new ParsingException("Expected return keyword");
        }

        var element = new NonTerminalElement(NonTerminalElementKind.ReturnStatement);

        element.AddChild(new TerminalElement(tokenReader.Current));
        tokenReader.Advance(); // Consume return keyword

        if (ExpressionGrammar.Match(tokenReader))
        {
            element.AddChild(ExpressionGrammar.Compile(tokenReader));
        }

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.SemiColon } semi)
        {
            throw new ParsingException("Expected a semicolon at the end of a retrun statement");
        }

        element.AddChild(new TerminalElement(semi));
        tokenReader.Advance();

        return element;
    }
}
