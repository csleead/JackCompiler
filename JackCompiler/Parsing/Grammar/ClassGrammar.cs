using JackCompiler.Grammar;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class ClassGrammar : IGrammar
{
    public static IElement Compile(TokenReader tokenReader)
    {
        if (tokenReader.Current is not Keyword { Kind: KeywordKind.Class } keyword)
        {
            throw new ParsingException("Excepting a class keyword");
        }

        var classElement = new NonTerminalElement(NonTerminalElementKind.Class);
        classElement.AddChild(new TerminalElement(keyword));
        tokenReader.Advance();

        if (tokenReader.Current is not Identifier identifier)
        {
            throw new ParsingException("Excepting a class name identifier");
        }
        classElement.AddChild(new TerminalElement(identifier));
        tokenReader.Advance();

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.OpenCurlyBracket } open)
        {
            throw new ParsingException("Excepting a class open curly bracket");
        }
        classElement.AddChild(new TerminalElement(open));
        tokenReader.Advance();

        while (ClassVarDecGrammar.Match(tokenReader))
        {
            var classVarDec = ClassVarDecGrammar.Compile(tokenReader);
            classElement.AddChild(classVarDec);
        }

        while (SubroutineDecGrammar.Match(tokenReader))
        {
            var subroutineDec = SubroutineDecGrammar.Compile(tokenReader);
            classElement.AddChild(subroutineDec);
        }

        if (tokenReader.Current is not Symbol { Kind: SymbolKind.CloseCurlyBracket } close)
        {
            throw new ParsingException("Excepting a class close curly bracket");
        }
        classElement.AddChild(new TerminalElement(close));
        tokenReader.Advance();

        return classElement;
    }

    public static bool Match(TokenReader tokenReader) =>
        tokenReader.Current is Keyword { Kind: KeywordKind.Class };
}
