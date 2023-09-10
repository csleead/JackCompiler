using System.Text;
using JackCompiler.Tokenizer;

namespace JackCompiler;

public class CompilationEngine
{
    private readonly NonTerminalElement _classElement;
    private readonly VmCodeWriter _codeWriter = new();
    private int _whileLoopCounter = 0;
    private int _ifCounter = 0;

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

        var symbolTable = classSymbolTable.StartSubroutine();

        AddParametersToSymbolTable(subroutineDec, symbolTable);

        var subroutineBody = subroutineDec.Children
            .OfType<NonTerminalElement>()
            .Single(x => x.Kind == NonTerminalElementKind.SubroutineBody);

        var varCount = AddLocalVariableToSymbolTable(subroutineBody, symbolTable);
        _codeWriter.Function($"{className}.{subroutineName}", varCount);

        var statements = subroutineBody
            .Children
            .OfType<NonTerminalElement>()
            .Single(x => x.Kind == NonTerminalElementKind.Statements);
        CompileStatements(statements, symbolTable);
    }

    private void AddParametersToSymbolTable(NonTerminalElement subroutineDec, SymbolTable symbolTable)
    {
        EnsureElementKind(subroutineDec, NonTerminalElementKind.SubroutineDec);

        var parameterList = (NonTerminalElement)subroutineDec.Children[4];
        for (int i = 0; i < parameterList.Children.Count; i += 3)
        {
            var type = parameterList.Children[i].IsTerminalOfToken<Identifier>(out var identifier) ?
                identifier.Value
                : parameterList.Children[i].AsTerminalOfToken<Keyword>().Kind switch
                {
                    KeywordKind.Int => "int",
                    KeywordKind.Char => "char",
                    KeywordKind.Boolean => "boolean",
                    _ => throw new NotImplementedException($"Unexpected keyword kind {parameterList.Children[i].AsTerminalOfToken<Keyword>().Kind}")
                };
            var parameterName = parameterList.Children[i + 1].AsTerminalOfToken<Identifier>().Value;
            symbolTable.AddIdentifier(parameterName, type, IdentifierKind.Arg);
        }
    }

    private int AddLocalVariableToSymbolTable(NonTerminalElement subroutineBody, SymbolTable symbolTable)
    {
        EnsureElementKind(subroutineBody, NonTerminalElementKind.SubroutineBody);

        var varDecs = subroutineBody.Children
            .OfType<NonTerminalElement>()
            .Where(x => x.Kind == NonTerminalElementKind.VarDec);

        int varCount = 0;
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
                varCount++;
            }
        }
        return varCount;
    }

    private void CompileStatements(NonTerminalElement statements, SymbolTable symbolTable)
    {
        EnsureElementKind(statements, NonTerminalElementKind.Statements);

        foreach (var statement in statements.Children.Cast<NonTerminalElement>())
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
                case NonTerminalElementKind.WhileStatement:
                    CompileWhileStatement(statement, symbolTable);
                    break;
                case NonTerminalElementKind.IfStatement:
                    CompileIfStatement(statement, symbolTable);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }

    private void CompileIfStatement(NonTerminalElement ifStatement, SymbolTable symbolTable)
    {
        EnsureElementKind(ifStatement, NonTerminalElementKind.IfStatement);

        var counter = _ifCounter++;
        CompileExpression((NonTerminalElement)ifStatement.Children[2], symbolTable);
        _codeWriter.Not();
        _codeWriter.IfGoto($"IF_FALSE_{counter}");

        CompileStatements((NonTerminalElement)ifStatement.Children[5], symbolTable);
        _codeWriter.Goto($"IF_END_{counter}");
        _codeWriter.Label($"IF_FALSE_{counter}");

        var elseBlock = ifStatement.Children.ElementAtOrDefault(9);
        if (elseBlock is not null)
        {
            CompileStatements((NonTerminalElement)elseBlock, symbolTable);
        }

        _codeWriter.Label($"IF_END_{counter}");
    }

    private void CompileWhileStatement(NonTerminalElement statement, SymbolTable symbolTable)
    {
        EnsureElementKind(statement, NonTerminalElementKind.WhileStatement);

        var counter = _whileLoopCounter++;
        _codeWriter.Label($"WHILE_START_{counter}");

        CompileExpression((NonTerminalElement)statement.Children[2], symbolTable);
        _codeWriter.Not();
        _codeWriter.IfGoto($"WHILE_END_{counter}");

        CompileStatements((NonTerminalElement)statement.Children[5], symbolTable);
        _codeWriter.Goto($"WHILE_START_{counter}");

        _codeWriter.Label($"WHILE_END_{counter}");
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

            if (terminalElement.Token is Keyword { Kind: KeywordKind.True or KeywordKind.False } booleanLiteral)
            {
                _codeWriter.Push(MemorySegment.Constant, 0);
                if (booleanLiteral.Kind == KeywordKind.True)
                {
                    _codeWriter.Not();
                }
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
                && symbol.Kind is SymbolKind.Minus or SymbolKind.Inverse)
            {
                CompileTerm((NonTerminalElement)term.Children[1], symbolTable);
                if (symbol.Kind == SymbolKind.Minus)
                {
                    _codeWriter.Neg();
                }
                else
                {
                    _codeWriter.Not();
                }
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
        if (@operator.Token is Symbol { Kind: SymbolKind.Minus })
        {
            _codeWriter.Sub();
            return;
        }
        if (@operator.Token is Symbol { Kind: SymbolKind.Equal })
        {
            _codeWriter.Eq();
            return;
        }
        if (@operator.Token is Symbol { Kind: SymbolKind.GreaterThan })
        {
            _codeWriter.Gt();
            return;
        }
        if (@operator.Token is Symbol { Kind: SymbolKind.LowerThan })
        {
            _codeWriter.Lt();
            return;
        }
        if (@operator.Token is Symbol { Kind: SymbolKind.And })
        {
            _codeWriter.And();
            return;
        }
        if (@operator.Token is Symbol { Kind: SymbolKind.Or })
        {
            _codeWriter.Or();
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
