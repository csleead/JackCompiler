namespace JackCompiler;

public class SymbolTable
{
    private readonly Dictionary<string, IdentifierInfo> _identifiers = new();
    private readonly Dictionary<IdentifierKind, int> _offsets = new();

    private readonly SymbolTable? _classLevelTable;
    private readonly string _className;

    public SymbolTable(string className)
    {
        _className = className;
    }

    private SymbolTable(SymbolTable classLevelTable)
    {
        _className = classLevelTable._className;
        _classLevelTable = classLevelTable;
    }

    public SymbolTable StartSubroutine()
    {
        var table = new SymbolTable(this);
        table.AddIdentifier("this", _className, IdentifierKind.Arg);
        return table;
    }

    public void AddIdentifier(string name, string type, IdentifierKind kind)
    {
        if (!_offsets.ContainsKey(kind))
        {
            _offsets.Add(kind, 0);
        }

        _identifiers.Add(name, new IdentifierInfo(type, kind, _offsets[kind]));
        _offsets[kind]++;
    }

    public IdentifierInfo GetIdentifier(string name)
    {
        return _identifiers.GetValueOrDefault(name)
            ?? _classLevelTable?.GetIdentifier(name)
            ?? throw new Exception($"Identifier {name} not found");
    }
}

public enum IdentifierKind
{
    Static,
    Field,
    Arg,
    Var,
}

public record class IdentifierInfo(string Type, IdentifierKind Kind, int Offset)
{
}