using System;
using JackCompiler;
using JackCompiler.Tokenizer;

namespace JackCompilerTests;

[UsesVerify]
public class VmWriterTest
{
    [Theory]
    [InlineData("JackSource/Seven/Main.jack")]
    [InlineData("JackSource/ConvertToBin/Main.jack")]
    public async Task Test(string source)
    {
        var file = Path.Join(Directory.GetCurrentDirectory(), "../../..", source);
        var reader = await Tokenizer.FromSourceFile(file);
        var element = Parser.Parse(reader);

        var vmCode = CompilationEngine.Compile(element);
        await Verify(vmCode)
            .UseParameters(source);
    }
}
