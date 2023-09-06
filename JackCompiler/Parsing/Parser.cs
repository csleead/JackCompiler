using JackCompiler.Tokenizer;

namespace JackCompiler;

public static class Parser
{
    public static IElement Parse(TokenReader tokenReader)
    {
        tokenReader.Advance();
        var classElement = ClassGrammar.Compile(tokenReader);
        return classElement;
    }
}
