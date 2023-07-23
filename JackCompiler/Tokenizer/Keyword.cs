namespace JackCompiler.Tokenizer;

public record Keyword(KeywordKind Kind) : IToken
{
    public string ToXmlElement() => $"<keyword> {Kind.ToString().ToLowerInvariant()} </keyword>";
}

public enum KeywordKind
{
    Class,
    Constructor,
    Function,
    Method,
    Field,
    Static,
    Var,
    Int,
    Char,
    Boolean,
    Void,
    True,
    False,
    Null,
    This,
    Let,
    Do,
    If,
    Else,
    While,
    Return,
}
