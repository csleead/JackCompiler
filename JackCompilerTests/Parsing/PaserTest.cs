using System.Xml.Linq;
using JackCompiler;
using JackCompiler.Tokenizer;

namespace JackCompilerTests;

[UsesVerify]
public class PaserTest
{
    [Fact]
    public async Task Test()
    {
        var file = Path.Join(Directory.GetCurrentDirectory(), "../../..", "JackSource/Test.jack");
        var reader = await Tokenizer.FromSourceFile(file);
        var element = Parser.Parse(reader);
        var xml = XDocument.Parse(element.ToXmlElement());
        await Verify(xml);
    }

    [Theory]
    [InlineData("JackSource/Square/Main.jack")]
    [InlineData("JackSource/Square/Square.jack")]
    [InlineData("JackSource/Square/SquareGame.jack")]
    [InlineData("JackSource/ArrayTest/Main.jack")]
    [InlineData("JackSource/ExpressionLessSquare/Main.jack")]
    [InlineData("JackSource/ExpressionLessSquare/Square.jack")]
    [InlineData("JackSource/ExpressionLessSquare/SquareGame.jack")]
    public async Task Test2(string source)
    {
        var file = Path.Join(Directory.GetCurrentDirectory(), "../../..", source);
        var reader = await Tokenizer.FromSourceFile(file);
        var element = Parser.Parse(reader);
        var xml = XDocument.Parse(element.ToXmlElement());
        await Verify(xml).UseParameters(source); ;
    }
}
