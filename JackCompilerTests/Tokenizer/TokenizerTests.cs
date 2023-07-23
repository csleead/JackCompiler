using Argon;
using System.Text;

namespace JackCompiler.Tokenizer.Tests;

[UsesVerify]
public class TokenizerTests
{
    [Theory]
    [InlineData("JackSource/Square/Main.jack")]
    [InlineData("JackSource/Square/Square.jack")]
    [InlineData("JackSource/Square/SquareGame.jack")]
    [InlineData("JackSource/ArrayTest/Main.jack")]
    [InlineData("JackSource/ExpressionLessSquare/Main.jack")]
    [InlineData("JackSource/ExpressionLessSquare/Square.jack")]
    [InlineData("JackSource/ExpressionLessSquare/SquareGame.jack")]
    public async Task Test(string source)
    {
        var file = Path.Join(Directory.GetCurrentDirectory(), "../../..", source);
        var reader = await Tokenizer.FromSourceFile(file);
        var xml = TokensXml(reader);
        await Verify(xml).UseParameters(source);
    }

    private string TokensXml(TokenReader reader)
    {
        var sb = new StringBuilder();
        while (reader.HasMoreTokens())
        {
            reader.Advance();
            sb.AppendLine(reader.Current.ToXmlElement());
        }
        return sb.ToString();
    }
}