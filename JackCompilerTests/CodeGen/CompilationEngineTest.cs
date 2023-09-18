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
    [InlineData("JackSource/Average/Main.jack")]
    public async Task Test(string source)
    {
        var file = Path.Join(Directory.GetCurrentDirectory(), "../../..", source);
        var reader = await Tokenizer.FromSourceFile(file);
        var element = Parser.Parse(reader);

        var vmCode = CompilationEngine.Compile(element);
        await Verify(vmCode)
            .UseParameters(source);
    }

    [Theory]
    [InlineData("JackSource/Square-CodeGen")]
    public async Task TestDirectory(string sourceDir)
    {

        var dir = Path.Join(Directory.GetCurrentDirectory(), "../../..", sourceDir);

        var dic = new Dictionary<string, string>();
        foreach (var file in Directory.GetFiles(dir, "*.jack"))
        {
            var reader = await Tokenizer.FromSourceFile(file);
            var element = Parser.Parse(reader);
            var vmCode = CompilationEngine.Compile(element);

            var name = Path.GetFileNameWithoutExtension(file);
            dic.Add(name, vmCode);
        }

        await Verify(dic)
            .UseParameters(sourceDir);
    }
}
