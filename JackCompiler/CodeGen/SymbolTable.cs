using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Headers;

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

    public SymbolTable StartSubroutine(bool isMethod = false)
    {
        var table = new SymbolTable(this);
        if (isMethod)
        {
            table.AddIdentifier("this", _className, IdentifierKind.Arg);
        }
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
        return TryGetIdentifier(name, out var identifierInfo) ?
            identifierInfo :
            throw new Exception($"Identifier {name} not found");
    }

    public bool TryGetIdentifier(string name, [NotNullWhen(true)] out IdentifierInfo? identifierInfo)
    {
        var id1 = _identifiers.GetValueOrDefault(name);
        if (id1 is not null)
        {
            identifierInfo = id1;
            return true;
        }

        if (_classLevelTable is not null && _classLevelTable._identifiers.TryGetValue(name, out var id2))
        {
            identifierInfo = id2;
            return true;
        }

        identifierInfo = default;
        return false;
    }

    public int LocalVarCount =>
        _identifiers.Count(x => x.Value.Kind == IdentifierKind.Var);

    public int ClassFieldCount =>
        (_classLevelTable ?? this)._identifiers.Count(x => x.Value.Kind == IdentifierKind.Field);
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