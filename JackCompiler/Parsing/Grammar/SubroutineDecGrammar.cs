using JackCompiler.Grammar;
using JackCompiler.Tokenizer;
using System.Xml.Linq;

namespace JackCompiler;

public class SubroutineDecGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.Constructor or KeywordKind.Function or KeywordKind.Method };

    public static IElement Compile(TokenReader tokenReader)
    {
        var element = new NonTerminalElement(NonTerminalElementKind.SubroutineDec);

        if (tokenReader.Current is not Keyword { Kind: KeywordKind.Constructor or KeywordKind.Function or KeywordKind.Method } keyword)
        {
            throw new ParsingException("Expecting a 'constructor', 'function' or 'method' keyword for subroutine declaration.");
        }
        element.AddChild(new TerminalElement(keyword));
        tokenReader.Advance();

        if (tokenReader.Current is Keyword { Kind: KeywordKind.Void } @void)
        {
            element.AddChild(new TerminalElement(@void));
            tokenReader.Advance();
        }
        else
        {
            var type = TypeGrammar.Compile(tokenReader);
            element.AddChild(type);
        }

        if (tokenReader.Current is not Identifier identifier)
        {
            throw new ParsingException("Excepting a subroutine name identifier");
        }
        element.AddChild(new TerminalElement(identifier));
        tokenReader.Advance();

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.OpenBracket } openBracket)
        {
            throw new ParsingException("Excepting a subroutine parameter list open bracket");
        }
        element.AddChild(new TerminalElement(openBracket));
        tokenReader.Advance();

        element.AddChild(CompileParameterList(tokenReader));

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseBracket } closeBracket)
        {
            Console.WriteLine(tokenReader.Current);
            throw new ParsingException("Excepting a subroutine parameter list close bracket");
        }
        element.AddChild(new TerminalElement(closeBracket));
        tokenReader.Advance();

        var body = SubroutineBodyGrammar.Compile(tokenReader);
        element.AddChild(body);

        return element;
    }

    private static IElement CompileParameterList(TokenReader tokenReader)
    {
        var element = new NonTerminalElement(NonTerminalElementKind.ParameterList);

        var isFirstParam = true;
        while (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseBracket })
        {
            if (!isFirstParam)
            {
                if (tokenReader.Current is not Symbol { Kind: SymbolKind.Comma } comma)
                {
                    throw new ParsingException("Excepting a comma to seperate subroutine parameters");
                }

                element.AddChild(new TerminalElement(comma));
                tokenReader.Advance();
            }

            var type = TypeGrammar.Compile(tokenReader);
            element.AddChild(type);

            if (tokenReader.Current is not Identifier parameterName)
            {
                throw new ParsingException("Excepting a parameter name identifier");
            }
            element.AddChild(new TerminalElement(parameterName));
            tokenReader.Advance();

            isFirstParam = false;
        }

        return element;
    }
}
