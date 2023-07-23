namespace JackCompiler.Tokenizer;

public record Symbol(SymbolKind Kind) : IToken
{
    private static readonly Dictionary<SymbolKind, string> XmlMap = new()
    {
        { SymbolKind.OpenCurlyBracket, "{" },
        { SymbolKind.CloseCurlyBracket, "}" },
        { SymbolKind.OpenBracket, "(" },
        { SymbolKind.CloseBracket, ")" },
        { SymbolKind.OpenSquareBracket, "[" },
        { SymbolKind.CloseSquareBracket, "]" },
        { SymbolKind.Dot, "." },
        { SymbolKind.Comma, "," },
        { SymbolKind.SemiColon, ";" },
        { SymbolKind.Plus, "+" },
        { SymbolKind.Minus, "-" },
        { SymbolKind.Multiply, "*" },
        { SymbolKind.Divide, "/" },
        { SymbolKind.And, "&amp;" },
        { SymbolKind.Or, "|" },
        { SymbolKind.GreaterThan, "&gt;" },
        { SymbolKind.LowerThan, "&lt;" },
        { SymbolKind.Equal, "=" },
        { SymbolKind.Inverse, "~" },
    };

    public string ToXmlElement() => $"<symbol> {XmlMap[Kind]} </symbol>";
}

public enum SymbolKind
{
    OpenCurlyBracket,
    CloseCurlyBracket,
    OpenBracket,
    CloseBracket,
    OpenSquareBracket,
    CloseSquareBracket,
    Dot,
    Comma,
    SemiColon,
    Plus,
    Minus,
    Multiply,
    Divide,
    And,
    Or,
    GreaterThan,
    LowerThan,
    Equal,
    Inverse,
}