using JackCompiler.Grammar;
using JackCompiler.Tokenizer;
using System.Net;

namespace JackCompiler;

public class DoStatementGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.Do };

    public static IElement Compile(TokenReader tokenReader)
    {
        if (!Match(tokenReader))
        {
            throw new ParsingException("A do statement should start with a do keyword");
        }

        var element = new NonTerminalElement(NonTerminalElementKind.DoStatement);

        element.AddChild(new TerminalElement(tokenReader.Current));
        tokenReader.Advance(); // Consume do keyword

        var callElements = CompileSubroutineCall(tokenReader);
        foreach (var callElement in callElements)
        {
            element.AddChild(callElement);
        }

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.SemiColon } semiColon)
        {
            throw new ParsingException($"Expected ; at the end of a do statement, got {tokenReader.Current}");
        }
        element.AddChild(new TerminalElement(semiColon));
        tokenReader.Advance();

        return element;
    }

    private static IReadOnlyList<IElement> CompileSubroutineCall(TokenReader tokenReader)
    {
        var result = new List<IElement>();

        if (tokenReader.Current is not Identifier firstIdentifier)
        {
            throw new ParsingException($"Expected an identifer, got {tokenReader.Current}");
        }
        result.Add(new TerminalElement(firstIdentifier));
        tokenReader.Advance();


        if (tokenReader.Current is Symbol { Kind: SymbolKind.OpenBracket } openBracket)
        {
            // do subroutine()
            result.Add(new TerminalElement(openBracket));
            tokenReader.Advance();

            result.Add(CompileExpressionList(tokenReader));
        }
        else
        {
            // do variable.subroutine()
            if (tokenReader.Current is not Symbol { Kind: SymbolKind.Dot } dot)
            {
                throw new ParsingException($"Expected a dot, got {tokenReader.Current}");
            }
            result.Add(new TerminalElement(dot));
            tokenReader.Advance();

            if (tokenReader.Current is not Identifier subroutineName)
            {
                throw new ParsingException($"Expected an identifer, got {tokenReader.Current}");
            }
            result.Add(new TerminalElement(subroutineName));
            tokenReader.Advance();

            if (tokenReader.Current is not Symbol { Kind: SymbolKind.OpenBracket } openBracket2)
            {
                throw new ParsingException($"Expected a '(', got {tokenReader.Current}");
            }
            result.Add(new TerminalElement(openBracket2));
            tokenReader.Advance();

            result.Add(CompileExpressionList(tokenReader));
        }

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseBracket } closeBracket)
        {
            throw new ParsingException($"Expected a closing bracket, got {tokenReader.Current}");
        }
        result.Add(new TerminalElement(closeBracket));
        tokenReader.Advance();

        return result;
    }

    private static IElement CompileExpressionList(TokenReader tokenReader)
    {
        var element = new NonTerminalElement(NonTerminalElementKind.ExpressionList);

        if (ExpressionGrammar.Match(tokenReader))
        {
            var expression = ExpressionGrammar.Compile(tokenReader);
            element.AddChild(expression);
            while (tokenReader.Current is Symbol { Kind: SymbolKind.Comma } comma)
            {
                element.AddChild(new TerminalElement(comma));
                tokenReader.Advance();

                element.AddChild(ExpressionGrammar.Compile(tokenReader));
            }
        }

        return element;
    }
}