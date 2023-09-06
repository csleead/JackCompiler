using System.Text;

namespace JackCompiler;

public record NonTerminalElement(NonTerminalElementKind Kind) : IElement
{
    private readonly List<IElement> _children = new();

    public IReadOnlyList<IElement> Children => _children;

    public void AddChild(IElement element) =>
        _children.Add(element);

    public string ToXmlElement()
    {
        var tagName = Kind switch
        {
            NonTerminalElementKind.Class => "class",
            NonTerminalElementKind.ClassVarDec => "classVarDec",
            NonTerminalElementKind.SubroutineDec => "subroutineDec",
            NonTerminalElementKind.SubroutineBody => "subroutineBody",
            NonTerminalElementKind.VarDec => "varDec",
            NonTerminalElementKind.LetStatement => "letStatement",
            NonTerminalElementKind.Expression => "expression",
            NonTerminalElementKind.Statements => "statements",
            NonTerminalElementKind.Term => "term",
            NonTerminalElementKind.ReturnStatement => "returnStatement",
            NonTerminalElementKind.IfStatement => "ifStatement",
            NonTerminalElementKind.WhileStatement => "whileStatement",
            NonTerminalElementKind.DoStatement => "doStatement",
            NonTerminalElementKind.ExpressionList => "expressionList",
            NonTerminalElementKind.ParameterList => "parameterList",
            _ => throw new NotSupportedException($"Unknown NonTerminalElementKind {Kind}"),
        };

        var sb = new StringBuilder();
        sb.AppendLine($"<{tagName}>");

        foreach (var child in _children)
        {
            sb.AppendLine(child.ToXmlElement());
        }

        sb.AppendLine($"</{tagName}>");

        return sb.ToString();
    }
}

public enum NonTerminalElementKind
{
    Class,
    ClassVarDec,
    SubroutineDec,
    SubroutineBody,
    VarDec,
    LetStatement,
    Expression,
    Statements,
    Term,
    ReturnStatement,
    IfStatement,
    WhileStatement,
    DoStatement,
    ExpressionList,
    ParameterList,
}
