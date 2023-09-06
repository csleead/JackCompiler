using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class SubroutineBodyGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Symbol { Kind: SymbolKind.OpenCurlyBracket };

    public static IElement Compile(TokenReader tokenReader)
    {
        var body = new NonTerminalElement(NonTerminalElementKind.SubroutineBody);

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.OpenCurlyBracket } open)
        {
            throw new ParsingException("Excepting a subroutine body open curly bracket");
        }
        body.AddChild(new TerminalElement(open));
        tokenReader.Advance();

        while (VarDecGrammar.Match(tokenReader))
        {
            var varDec = VarDecGrammar.Compile(tokenReader);
            body.AddChild(varDec);
        }

        if (StatementsGrammar.Match(tokenReader))
        {
            var statements = StatementsGrammar.Compile(tokenReader);
            body.AddChild(statements);
        }

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseCurlyBracket } close)
        {
            throw new ParsingException("Excepting a subroutine body close curly bracket");
        }
        body.AddChild(new TerminalElement(close));
        tokenReader.Advance();

        return body;
    }
}
