using System.Xml.Linq;
using JackCompiler;
using JackCompiler.Tokenizer;

if (args.Length == 0)
{
    Console.WriteLine("Usage: <input>");
    return;
}

var inputPath = GetInputPath();
if (File.Exists(inputPath))
{
    await CompileFile(inputPath);
}
else if (Directory.Exists(inputPath))
{
    var files = Directory.GetFiles(inputPath, "*.jack");
    foreach (var file in files)
    {
        await CompileFile(file);
    }
}
else
{
    Console.WriteLine($"File or directory not found: {inputPath}");
}

string GetInputPath()
{
    var input = args[0];
    if (Path.IsPathFullyQualified(input))
    {
        return input;
    }

    var currentDir = Directory.GetCurrentDirectory();
    return Path.Join(currentDir, input);
}

async Task CompileFile(string sourcePath)
{
    var tokenReader = await Tokenizer.FromSourceFile(sourcePath);
    var parseTree = Parser.Parse(tokenReader);

    var xmlOutPath = GetXmlOutputPath(sourcePath);
    var formattedXml = XDocument.Parse(parseTree.ToXmlElement()).ToString();
    if (File.Exists(xmlOutPath))
    {
        File.Delete(xmlOutPath);
    }
    await File.WriteAllTextAsync(xmlOutPath, formattedXml);
}

string GetXmlOutputPath(string sourcePath)
{
    var outputDir = Path.GetDirectoryName(sourcePath);
    var outputPath = Path.Join(outputDir, Path.GetFileNameWithoutExtension(sourcePath) + ".xml");
    return outputPath;
}