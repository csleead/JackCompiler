using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class ExpressionGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        TermGrammar.Match(tokenReader);

    public static IElement Compile(TokenReader tokenReader)
    {
        var element = new NonTerminalElement(NonTerminalElementKind.Expression);

        var term = TermGrammar.Compile(tokenReader);
        element.AddChild(term);

        while (OpGrammar.Match(tokenReader))
        {
            element.AddChild(OpGrammar.Compile(tokenReader));
            element.AddChild(TermGrammar.Compile(tokenReader));
        }

        return element;
    }
}
