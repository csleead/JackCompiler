using System.Reflection.Metadata;
using System.Text;

namespace JackCompiler;

public class VmCodeWriter
{
    private readonly StringBuilder _code = new();

    public string GetCode() => _code.ToString();

    public void Function(string functionName, int parameterCount)
    {
        _code.AppendLine($"function {functionName} {parameterCount}");
    }

    public void Return()
    {
        _code
            .AppendLine("push constant 0")
            .AppendLine("return");
    }

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

    internal void Neg()
    {
        _code.AppendLine("neg");
    }
}

public enum MemorySegment
{
    Constant,
    Local,
}

public static class MemorySegmentExtension
{
    public static string SegmentName(this MemorySegment segment) => segment switch
    {
        MemorySegment.Constant => "constant",
        MemorySegment.Local => "local",
        _ => throw new Exception($"Unknown segment: {segment}")
    };
}