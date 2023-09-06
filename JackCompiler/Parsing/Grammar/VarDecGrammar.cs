using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class VarDecGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.Var };

    public static IElement Compile(TokenReader tokenReader)
    {
        var result = new NonTerminalElement(NonTerminalElementKind.VarDec);

        if (tokenReader.Current is not Keyword { Kind: KeywordKind.Var } varKeyword)
        {
            throw new ParsingException("Excepting a 'var' keyword");
        }
        result.AddChild(new TerminalElement(varKeyword));
        tokenReader.Advance();

        var type = TypeGrammar.Compile(tokenReader);
        result.AddChild(type);

        if (tokenReader.Current is not Identifier varName1)
        {
            throw new ParsingException("Excepting an identifier for local variable name");
        }
        result.AddChild(new TerminalElement(varName1));
        tokenReader.Advance();

        while (tokenReader.Current is Symbol { Kind: SymbolKind.Comma } comma)
        {
            result.AddChild(new TerminalElement(comma));
            tokenReader.Advance();

            if (tokenReader.Current is not Identifier varName)
            {
                throw new ParsingException("Excepting an identifier for local variable name");
            }
            result.AddChild(new TerminalElement(varName));
            tokenReader.Advance();
        }

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.SemiColon } semi)
        {
            throw new ParsingException("Excepting an semicolon after local variable declaration");
        }
        result.AddChild(new TerminalElement(semi));
        tokenReader.Advance();

        return result;
    }
}
