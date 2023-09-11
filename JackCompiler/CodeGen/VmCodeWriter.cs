using System.Reflection.Metadata;
using System.Text;

namespace JackCompiler;

public class VmCodeWriter
{
    private readonly StringBuilder _code = new();

    public string GetCode() => _code.ToString();

    public void Function(string functionName, int localVariableCount)
    {
        _code.AppendLine($"function {functionName} {localVariableCount}");
    }

    public void Return() =>
        _code.AppendLine("return");

    public void Call(string functionName, int parameterCount, bool discardReturnValue)
    {
        _code.AppendLine($"call {functionName} {parameterCount}");
        if (discardReturnValue)
        {
            _code.AppendLine($"pop temp 0");
        }
    }

    public void Push(MemorySegment segment, int offset)
    {
        _code.AppendLine($"push {segment.SegmentName()} {offset}");
    }

    public void Pop(MemorySegment segment, int offset)
    {
        _code.AppendLine($"pop {segment.SegmentName()} {offset}");
    }

    public void Add()
    {
        _code.AppendLine("add");
    }

    public void Multiply()
    {
        _code.AppendLine($"call Math.multiply 2");
    }

    public void Neg()
    {
        _code.AppendLine("neg");
    }

    public void Not()
    {
        _code.AppendLine("not");
    }

    public void Label(string name)
    {
        _code.AppendLine($"label {name}");
    }

    public void Goto(string name)
    {
        _code.AppendLine($"goto {name}");
    }

    public void IfGoto(string name)
    {
        _code.AppendLine($"if-goto {name}");
    }

    public void Sub() =>
        _code.AppendLine($"sub");

    public void Eq() =>
        _code.AppendLine($"eq");

    public void Gt() =>
        _code.AppendLine($"gt");

    public void Lt() =>
        _code.AppendLine($"lt");

    public void And() =>
        _code.AppendLine($"and");

    public void Or() =>
        _code.AppendLine($"or");

    public void MemoryAlloc(int numOfFields)
    {
        _code.AppendLine($"push constant {numOfFields}");
        _code.AppendLine("call Memory.alloc 1");
        _code.AppendLine("pop pointer 0");
    }
}

public enum MemorySegment
{
    Constant,
    Local,
    Argument,
    This,
    Pointer,
}

public static class MemorySegmentExtension
{
    public static string SegmentName(this MemorySegment segment) => segment switch
    {
        MemorySegment.Constant => "constant",
        MemorySegment.Local => "local",
        MemorySegment.Argument => "argument",
        MemorySegment.This => "this",
        MemorySegment.Pointer => "pointer",
        _ => throw new Exception($"Unknown segment: {segment}")
    };
}