using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class ClassVarDecGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.Static or KeywordKind.Field };

    public static IElement Compile(TokenReader tokenReader)
    {
        var element = new NonTerminalElement(NonTerminalElementKind.ClassVarDec);

        if (tokenReader.Current is not Keyword { Kind: KeywordKind.Static or KeywordKind.Field } keyword)
        {
            throw new ParsingException("Expecting a 'static' or 'field' keyword for class variable declaration.");
        }
        element.AddChild(new TerminalElement(keyword));
        tokenReader.Advance();

        var type = TypeGrammar.Compile(tokenReader);
        element.AddChild(type);

        if (tokenReader.Current is not Identifier varName)
        {
            throw new ParsingException("Expecting an identifier for class variable name.");
        }
        element.AddChild(new TerminalElement(varName));
        tokenReader.Advance();

        while (tokenReader.Current is Symbol { Kind: SymbolKind.Comma } comma)
        {
            element.AddChild(new TerminalElement(comma));
            tokenReader.Advance();

            if (tokenReader.Current is not Identifier id)
            {
                throw new ParsingException("Expecting an identifier for class variable name.");
            }
            element.AddChild(new TerminalElement(id));
            tokenReader.Advance();
        }

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.SemiColon } semiColon)
        {
            throw new ParsingException("Expecting a semicolon after class variable declaration");
        }
        element.AddChild(new TerminalElement(semiColon));
        tokenReader.Advance();

        return element;
    }
}
