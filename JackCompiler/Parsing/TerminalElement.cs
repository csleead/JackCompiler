using JackCompiler.Tokenizer;

namespace JackCompiler;

public record TerminalElement(IToken Token) : IElement
{
    public string ToXmlElement() =>
        Token.ToXmlElement();
}
