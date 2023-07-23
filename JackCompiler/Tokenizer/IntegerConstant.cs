namespace JackCompiler.Tokenizer;
public record IntegerConstant(int Value) : IToken
{
    public string ToXmlElement() => $"<integerConstant> {Value} </integerConstant>";
}
