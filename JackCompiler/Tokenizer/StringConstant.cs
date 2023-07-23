namespace JackCompiler.Tokenizer;

public record StringConstant(string Value) : IToken
{
    public string ToXmlElement() => $"<stringConstant> {Value} </stringConstant>";
}
