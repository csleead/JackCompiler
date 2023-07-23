namespace JackCompiler.Tokenizer;

public record Identifier(string Value) : IToken
{
    public string ToXmlElement() => $"<identifier> {Value} </identifier>";
}
