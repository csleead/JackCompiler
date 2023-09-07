using System;
using Argon;
using JackCompiler;

namespace JackCompilerTests;

[UsesVerify]
public class SymbolTableTest
{
    [Fact]
    public async Task TestClassLevelSymbolTable()
    {
        var classLevelTable = new SymbolTable("DummyClass");
        classLevelTable.AddIdentifier("fieldX", "int", IdentifierKind.Field);
        classLevelTable.AddIdentifier("fieldY", "string", IdentifierKind.Field);
        classLevelTable.AddIdentifier("staticFieldX", "boolean", IdentifierKind.Static);
        classLevelTable.AddIdentifier("staticFieldY", "string", IdentifierKind.Static);

        var result = new[] { "fieldX", "fieldY", "staticFieldX", "staticFieldY" }
            .Select(name => (name, classLevelTable.GetIdentifier(name)))
            .ToDictionary(x => x.Item1, x => x.Item2);
        await Verify(result)
            .AddExtraSettings(x => x.DefaultValueHandling = DefaultValueHandling.Include);
    }

    [Fact]
    public async Task TestSubroutineLevelTable()
    {
        var classLevelTable = new SymbolTable("DummyClass");
        classLevelTable.AddIdentifier("fieldX", "int", IdentifierKind.Field);
        classLevelTable.AddIdentifier("staticFieldX", "int", IdentifierKind.Static);

        var subRoutineLevelTable = classLevelTable.StartSubroutine();
        subRoutineLevelTable.AddIdentifier("localX", "int", IdentifierKind.Var);
        subRoutineLevelTable.AddIdentifier("localY", "int", IdentifierKind.Var);
        subRoutineLevelTable.AddIdentifier("arg0", "int", IdentifierKind.Arg);
        subRoutineLevelTable.AddIdentifier("arg1", "int", IdentifierKind.Arg);

        var result = new[] { "fieldX", "staticFieldX", "localX", "localY", "this", "arg0", "arg1" }
            .Select(name => (name, subRoutineLevelTable.GetIdentifier(name)))
            .ToDictionary(x => x.Item1, x => x.Item2);
        await Verify(result)
            .AddExtraSettings(x => x.DefaultValueHandling = DefaultValueHandling.Include);
    }
}
