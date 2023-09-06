using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class TermGrammar : IGrammar
{
    public static bool Match(TokenReader tokenReader) =>
        (tokenReader.Current is IntegerConstant or StringConstant or Identifier or Symbol { Kind: SymbolKind.OpenBracket })
        || KeywordConstantGrammar.Match(tokenReader)
        || UnaryOpGrammar.Match(tokenReader);

    public static IElement Compile(TokenReader tokenReader)
    {
        var element = new NonTerminalElement(NonTerminalElementKind.Term);

        if (tokenReader.Current is IntegerConstant integerConstant)
        {
            tokenReader.Advance();
            element.AddChild(new TerminalElement(integerConstant));
        }
        else if (tokenReader.Current is StringConstant stringConstant)
        {
            tokenReader.Advance();
            element.AddChild(new TerminalElement(stringConstant));
        }
        else if (KeywordConstantGrammar.Match(tokenReader))
        {
            element.AddChild(KeywordConstantGrammar.Compile(tokenReader));
        }
        else if (tokenReader.Current is Identifier identifier)
        {
            if (tokenReader.Peek(1) is Symbol { Kind: SymbolKind.OpenSquareBracket } openIndexer)
            {
                // var[<expression>]
                element.AddChild(new TerminalElement(identifier));
                tokenReader.Advance();
                element.AddChild(new TerminalElement(openIndexer));
                tokenReader.Advance();

                var expression = ExpressionGrammar.Compile(tokenReader);
                element.AddChild(expression);

                if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseSquareBracket } closeIndex)
                {
                    throw new ParsingException($"Expected a ']', got {tokenReader.Current}");
                }

                element.AddChild(new TerminalElement(closeIndex));
                tokenReader.Advance();
            }
            else if (tokenReader.Peek(1) is Symbol { Kind: SymbolKind.OpenBracket or SymbolKind.Dot })
            {
                // subroutine call
                var call = CompileSubroutineCall(tokenReader);
                foreach (var e in call)
                {
                    element.AddChild(e);
                }

            }
            else
            {
                element.AddChild(new TerminalElement(identifier));
                tokenReader.Advance();
            }
        }
        else if (tokenReader.Current is Symbol { Kind: SymbolKind.OpenBracket } openBracket)
        {
            element.AddChild(new TerminalElement(openBracket));
            tokenReader.Advance();

            element.AddChild(ExpressionGrammar.Compile(tokenReader));

            if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseBracket } closeBracket)
            {
                throw new ParsingException($"Expecting a ')', got {tokenReader.Current}");
            }
            element.AddChild(new TerminalElement(closeBracket));
            tokenReader.Advance();
        }
        else
        {
            element.AddChild(UnaryOpGrammar.Compile(tokenReader));
            element.AddChild(Compile(tokenReader));
        }
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
