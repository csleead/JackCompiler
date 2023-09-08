using System.Text;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class CompilationEngine
{
    private readonly NonTerminalElement _classElement;
    private readonly VmCodeWriter _codeWriter = new();

    private CompilationEngine(NonTerminalElement classElement)
    {
        _classElement = classElement;
    }

    public static string Compile(IElement classElement)
    {
        if (classElement is not NonTerminalElement { Kind: NonTerminalElementKind.Class })
        {
            throw new InvalidOperationException("Class element expected");
        }

        var vmWriter = new CompilationEngine((NonTerminalElement)classElement);
        vmWriter.CompileClass();

        return vmWriter._codeWriter.GetCode();
    }

    private void CompileClass()
    {
        var className = GetClassName();
        var classSymbolTable = new SymbolTable(className);

        var subRoutines = _classElement.Children
            .OfType<NonTerminalElement>()
            .Where(x => x.Kind == NonTerminalElementKind.SubroutineDec);

        foreach (var subRoutine in subRoutines)
        {
            CompileSubroutine(subRoutine, classSymbolTable);
        }
    }

    private void CompileSubroutine(NonTerminalElement subroutineDec, SymbolTable classSymbolTable)
    {
        EnsureElementKind(subroutineDec, NonTerminalElementKind.SubroutineDec);

        var className = GetClassName();
        var subroutineName = GetSubroutineName(subroutineDec);
        var parameterCount = GetSubroutineParameterCount(subroutineDec);

        var symbolTable = classSymbolTable.StartSubroutine();

        _codeWriter.Function($"{className}.{subroutineName}", parameterCount);

        var subroutineBody = subroutineDec.Children
            .OfType<NonTerminalElement>()
            .Single(x => x.Kind == NonTerminalElementKind.SubroutineBody);

        AddLocalVariableToSymbolTable(subroutineBody, symbolTable);
        CompileStatements(subroutineBody, symbolTable);
    }

    private void AddLocalVariableToSymbolTable(NonTerminalElement subroutineBody, SymbolTable symbolTable)
    {
        EnsureElementKind(subroutineBody, NonTerminalElementKind.SubroutineBody);

        var varDecs = subroutineBody.Children
            .OfType<NonTerminalElement>()
            .Where(x => x.Kind == NonTerminalElementKind.VarDec);

        foreach (var varDec in varDecs)
        {
            var type = ((TerminalElement)varDec.Children[1]).Token switch
            {
                Keyword { Kind: KeywordKind.Int } => "int",
                Keyword { Kind: KeywordKind.Char } => "char",
                Keyword { Kind: KeywordKind.Boolean } => "boolean",
                Identifier identifier => identifier.Value,
                _ => throw new Exception("Invalid type"),
            };

            var variables = varDec.Children
                .Skip(2)
                .OfType<TerminalElement>()
                .Where(x => x.Token is Identifier)
                .Select(x => (Identifier)x.Token);
            foreach (var v in variables)
            {
                symbolTable.AddIdentifier(v.Value, type, IdentifierKind.Var);
            }
        }
    }

    private void CompileStatements(NonTerminalElement subroutineBody, SymbolTable symbolTable)
    {
        EnsureElementKind(subroutineBody, NonTerminalElementKind.SubroutineBody);

        var statements = subroutineBody
            .Children
            .OfType<NonTerminalElement>()
            .Single(x => x.Kind == NonTerminalElementKind.Statements)
            .Children
            .Cast<NonTerminalElement>();

        foreach (var statement in statements)
        {
            switch (statement.Kind)
            {
                case NonTerminalElementKind.DoStatement:
                    CompileDoStatement(statement, symbolTable);
                    break;
                case NonTerminalElementKind.LetStatement:
                    CompileLetStatement(statement, symbolTable);
                    break;
                case NonTerminalElementKind.ReturnStatement:
                    _codeWriter.Return();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void CompileLetStatement(NonTerminalElement statement, SymbolTable symbolTable)
    {
        EnsureElementKind(statement, NonTerminalElementKind.LetStatement);

        var variableToken = ((TerminalElement)statement.Children[1]).Token;
        var identifier = ((Identifier)variableToken).Value;
        var symbol = symbolTable.GetIdentifier(identifier);

        var expression = statement.Children[3];
        CompileExpression((NonTerminalElement)expression, symbolTable);
        _codeWriter.Pop(MemorySegment.Local, symbol.Offset);
    }

    private void CompileDoStatement(NonTerminalElement statement, SymbolTable symbolTable)
    {
        EnsureElementKind(statement, NonTerminalElementKind.DoStatement);

        var parameterExpressions = statement.Children
            .OfType<NonTerminalElement>()
            .Single(x => x.Kind == NonTerminalElementKind.ExpressionList)
            .Children
            .OfType<NonTerminalElement>()
            .ToList();

        foreach (var parameter in parameterExpressions)
        {
            CompileExpression(parameter, symbolTable);
        }

        _codeWriter.Call(GetFunctionName(), parameterExpressions.Count, true);


        string GetFunctionName()
        {
            var terminalElement1 = (TerminalElement)statement.Children[1];
            var className = ((Identifier)terminalElement1.Token).Value;

            var terminalElement2 = (TerminalElement)statement.Children[3];
            var subroutineName = ((Identifier)terminalElement2.Token).Value;

            return $"{className}.{subroutineName}";
        }
    }

    private void CompileExpression(NonTerminalElement expression, SymbolTable symbolTable)
    {
        EnsureElementKind(expression, NonTerminalElementKind.Expression);

        CompileTerm((NonTerminalElement)expression.Children[0], symbolTable);

        var i = 2;
        while (i < expression.Children.Count)
        {
            var @operator = (TerminalElement)expression.Children[i - 1];
            var nextTerm = (NonTerminalElement)expression.Children[i];

            CompileTerm(nextTerm, symbolTable);
            CompileOperator(@operator);

            i += 2;
        }
    }

    private void CompileTerm(NonTerminalElement term, SymbolTable symbolTable)
    {
        EnsureElementKind(term, NonTerminalElementKind.Term);
        var termValue = term.Children[0];

        if (termValue is TerminalElement terminalElement)
        {
            if (terminalElement.Token is IntegerConstant integerConstant)
            {
                _codeWriter.Push(MemorySegment.Constant, integerConstant.Value);
                return;
            }

            if (term.Children.Count == 1 && terminalElement.Token is Identifier identifier)
            {
                var info = symbolTable.GetIdentifier(identifier.Value);
                _codeWriter.Push(MemorySegment.Local, info.Offset);
                return;
            }

            // '(' <expression> ')'
            if (terminalElement.Token is Symbol { Kind: SymbolKind.OpenBracket })
            {
                var expression = (NonTerminalElement)term.Children[1];
                CompileExpression(expression, symbolTable);
                return;
            }

            // unaryOp <term>
            if (term.Children[0].IsTerminalOfToken<Symbol>(out var symbol)
                && symbol.Kind is SymbolKind.Minus)
            {
                CompileTerm((NonTerminalElement)term.Children[1], symbolTable);
                _codeWriter.Neg();
                return;
            }

            // subroutine call
            if (terminalElement.IsTerminalOfToken<Identifier>(out var _)
                && term.Children[1].IsTerminalOfToken<Symbol>(s => s.Kind == SymbolKind.Dot))
            {
                CompileSubroutineCall(term, symbolTable, false);
                return;
            }
        }

        throw new InvalidOperationException($"Unexpected term element: {termValue}");
    }

    private void CompileSubroutineCall(NonTerminalElement statement, SymbolTable symbolTable, bool isDoStatement)
    {
        var parameterExpressions = statement.Children
            .OfType<NonTerminalElement>()
            .Single(x => x.Kind == NonTerminalElementKind.ExpressionList)
            .Children
            .OfType<NonTerminalElement>()
            .ToList();

        foreach (var parameter in parameterExpressions)
        {
            CompileExpression(parameter, symbolTable);
        }

        _codeWriter.Call(GetFunctionName(), parameterExpressions.Count, isDoStatement);


        string GetFunctionName()
        {
            var terminalElement1 = (TerminalElement)statement.Children[isDoStatement ? 1 : 0];
            var className = ((Identifier)terminalElement1.Token).Value;

            var terminalElement2 = (TerminalElement)statement.Children[isDoStatement ? 3 : 2];
            var subroutineName = ((Identifier)terminalElement2.Token).Value;

            return $"{className}.{subroutineName}";
        }
    }

    private void CompileOperator(TerminalElement @operator)
    {
        if (@operator.Token is Symbol { Kind: SymbolKind.Plus })
        {
            _codeWriter.Add();
            return;
        }
        if (@operator.Token is Symbol { Kind: SymbolKind.Multiply })
        {
            _codeWriter.Multiply();
            return;
        }

        throw new InvalidOperationException($"Unknown operator {@operator.Token}");
    }

    private string GetClassName()
    {
        var element = (TerminalElement)_classElement.Children[1];
        var identifier = (Identifier)element.Token;
        return identifier.Value;
    }

    private static string GetSubroutineName(NonTerminalElement subroutineDec)
    {
        var element = (TerminalElement)subroutineDec.Children[2];
        var identifier = (Identifier)element.Token;
        return identifier.Value;
    }

    private static int GetSubroutineParameterCount(NonTerminalElement subroutineDec)
    {
        var parameterList = subroutineDec.Children
            .OfType<NonTerminalElement>()
            .Where(x => x.Kind == NonTerminalElementKind.ParameterList)
            .Single();

        return parameterList.Children
            .OfType<TerminalElement>()
            .Count(x => x.Token is Identifier);
    }

    private static void EnsureElementKind(NonTerminalElement element, NonTerminalElementKind kind)
    {
        if (element.Kind != kind)
        {
            throw new InvalidOperationException($"Expected {kind} but found {element.Kind}");
        }
    }
}
