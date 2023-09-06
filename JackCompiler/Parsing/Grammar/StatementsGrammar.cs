using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class StatementsGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        LetStatementGrammar.Match(tokenReader)
        || ReturnStatementGrammar.Match(tokenReader)
        || IfStatementGrammar.Match(tokenReader)
        || WhileStatementGrammar.Match(tokenReader)
        || DoStatementGrammar.Match(tokenReader);

    public static IElement Compile(TokenReader tokenReader)
    {
        var element = new NonTerminalElement(NonTerminalElementKind.Statements);
        while (Match(tokenReader))
        {
            if (LetStatementGrammar.Match(tokenReader))
            {
                element.AddChild(LetStatementGrammar.Compile(tokenReader));
            }
            else if (ReturnStatementGrammar.Match(tokenReader))
            {
                element.AddChild(ReturnStatementGrammar.Compile(tokenReader));
            }
            else if (IfStatementGrammar.Match(tokenReader))
            {
                element.AddChild(IfStatementGrammar.Compile(tokenReader));
            }
            else if (WhileStatementGrammar.Match(tokenReader))
            {
                element.AddChild(WhileStatementGrammar.Compile(tokenReader));
            }
            else if (DoStatementGrammar.Match(tokenReader))
            {
                element.AddChild(DoStatementGrammar.Compile(tokenReader));
            }
        }
        return element;
    }
}
