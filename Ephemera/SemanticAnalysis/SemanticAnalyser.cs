using Ephemera.Lexing;
using Ephemera.Parsing;
using Ephemera.Parsing.Expressions;
using Ephemera.Reusable;
using Ephemera.SemanticAnalysis.Typing;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Ephemera.SemanticAnalysis;

public class SemanticAnalyser
{
    private ImmutableList<DefinitionNode> symbolTable = ImmutableList.Create<DefinitionNode>();
    private Dictionary<FuncNamedExpr, FuncNamedNode> functions = new Dictionary<FuncNamedExpr, FuncNamedNode>();

    private ExtList<CodeError> codeErrors = new ExtList<CodeError>();
    public IReadOnlyList<CodeError> CodeErrors => codeErrors;

    private uint loopCount;
    private bool insideLoop => loopCount > 0;

    private bool insideFunction => funcStack.Count > 0;
    public Stack<FuncNamedNode> funcStack = new Stack<FuncNamedNode>();

    public IReadOnlyList<SemanticNode> Analyse(RootExpr expr) => Analyse(expr.Expressions);

    IReadOnlyList<SemanticNode> Analyse(IReadOnlyList<Expr> expressions, ExtList<Token> scopeVariables = null)
    {
        ScanFuncDefinitions(expressions);

        var savedSymbolTable = symbolTable;
        scopeVariables = scopeVariables ?? new ExtList<Token>();
        var nodes = new ExtList<SemanticNode>(expressions.Count);
        foreach (var expr in expressions)
        {
            var node = Analyse(expr);
            nodes += node;

            ValidateDefinition(node, expr, scopeVariables);
            ValidateKeyword(node, expr);
        }

        symbolTable = savedSymbolTable;
        return nodes;
    }

    void ScanFuncDefinitions(IReadOnlyList<Expr> expressions)
    {
        foreach (var expr in expressions)
        {
            var funcImport = expr as FuncImportExpr;
            if (funcImport != null)
            {
                var funcNode = AnalyseFuncImport(funcImport);
                functions.Add(funcImport, funcNode);
            }

            var funcDefinition = expr as FuncDefinitionExpr;
            if (funcDefinition != null)
            {
                var funcNode = AnalyseFuncDefinition(funcDefinition);
                functions.Add(funcDefinition, funcNode);
            }
        }
    }

    public FuncDefinitionNode AnalyseFuncDefinition(FuncDefinitionExpr expr)
    {
        ValidateFuncDefinition(expr.Identifier);
        var savedSymbolTable = symbolTable;
        var scopeVariables = new ExtList<Token>();
        var paramNodes = new ExtList<DefinitionNode>(expr.Parameters.Count);
        AnalyseFuncSignature(expr.Parameters, scopeVariables, paramNodes);

        var declaredReturnType = expr.ReturnType?.ToTypeDescriptor();
        funcStack.Push(new FuncNamedNode(expr, expr.Identifier, paramNodes, declaredReturnType, expr.IsExtension));

        var body = AnalyseBlock(expr.Body, scopeVariables);

        funcStack.Pop();
        symbolTable = savedSymbolTable;

        var result = ExecutionAnalyser.Run(body);
        var detectedReturnType = default(TypeDescriptor);
        if (result.HasReturn)
        {
            detectedReturnType = result.ReturnNodes.Select(n => n.Type).ClosestSupertype();
            if (detectedReturnType == null)
            {
                AddError("Type mismatch between return statements", expr.Identifier);
            }

            if (!detectedReturnType.IsUnit() && !result.AlwaysReturns)
            {
                AddError("Not all code paths return a value", expr.Identifier);
            }
        }
        else
        {
            detectedReturnType = body.Type;
        }

        if (declaredReturnType == null && detectedReturnType.NotIdentified())
        {
            AddError("Return type couldn't be inferred. Please, provide it explicitly", expr.Identifier);
        }

        var typesMatch = declaredReturnType == null || detectedReturnType == null || TypeChecker.IsAssignable(declaredReturnType, detectedReturnType);

        if (!typesMatch)
        {
            AddError("Type mismatch between declared and detected return types", expr.Identifier);
        }

        var returnType = declaredReturnType ?? detectedReturnType ?? InvalidTypeDescriptor.Invalid;
        return new FuncDefinitionNode(expr, expr.Identifier, paramNodes, returnType, expr.IsExtension, body);
    }

    public FuncImportNode AnalyseFuncImport(FuncImportExpr expr)
    {
        ValidateFuncDefinition(expr.Identifier);

        var scopeVariables = new ExtList<Token>();
        var paramNodes = new ExtList<DefinitionNode>(expr.Parameters.Count);

        var savedSymbolTable = symbolTable;
        AnalyseFuncSignature(expr.Parameters, scopeVariables, paramNodes);
        symbolTable = savedSymbolTable;

        var returnType = expr.ReturnType != null ? TypeDescriptor.Create(expr.ReturnType) ?? InvalidTypeDescriptor.Invalid : SimpleTypeDescriptor.Unit;
        return new FuncImportNode(expr, expr.Identifier, paramNodes, returnType, expr.IsExtension);
    }

    void ValidateFuncDefinition(Token identifier)
    {
        if (functions.Any(f => f.Value.Name == identifier.Word))
        {
            AddError("Function with the same name is already defined", identifier);
        }
    }

    public void AnalyseFuncSignature(IReadOnlyList<ParamDeclarationExpr> parameters, ExtList<Token> scopeVariables, ExtList<DefinitionNode> paramNodes)
    {
        foreach (var param in parameters)
        {
            if (param.Type == null)
            {
                AddError("Parameter type required", param.Identifier);
            }

            if (scopeVariables.Any(v => v.Word == param.Name))
            {
                AddError("Parameter with the same name is already declared", param.Identifier);
            }

            var type = param.Type != null ? TypeDescriptor.Create(param.Type) : InvalidTypeDescriptor.Invalid;
            var definition = new DefinitionNode(param, param.Identifier, type);

            scopeVariables += param.Identifier;
            paramNodes += definition;
            symbolTable = symbolTable.Add(definition);
        }
    }

    void ValidateDefinition(SemanticNode node, Expr expr, ExtList<Token> scopeVariables)
    {
        var definition = node as VariableDefinitionNode;
        if (definition != null)
        {
            var isDuplicate = scopeVariables.Any(v => v.Word == definition.Name);
            if (isDuplicate)
            {
                AddError("Variable with the same name is already declared in this scope", expr);
            }
            scopeVariables += definition.Identifier;
            symbolTable = symbolTable.Add(definition);
        }
    }

    void ValidateKeyword(SemanticNode node, Expr expr)
    {
        if (insideLoop) return;
        var keyword = node as KeywordNode;
        var tokenClass = keyword?.KeywordExpr.Keyword.Class;
        if (tokenClass == TokenClass.BreakKeyword || tokenClass == TokenClass.SkipKeyword)
        {
            var keywordString = TokenAssistant.GetKeyword(tokenClass.Value);
            AddError($"'{keywordString}' keyword is only valid inside a loop", keyword);
        }
    }

    SemanticNode Analyse(Expr expr)
    {
        var operand = expr as OperandExpr;
        if (operand != null)
        {
            var node = AnalyseOperand(operand);
            if (node is not FuncInvocationNode)
            {
                AddError(ErrorMessages.CantUseExpressionsAsStatements, node);
            }
            return node;
        }

        var definition = expr as DefinitionExpr;
        if (definition != null)
        {
            return AnalyseDefinition(definition);
        }

        var ifExpr = expr as IfExpr;
        if (ifExpr != null)
        {
            return AnalyseIf(ifExpr);
        }

        var whileExpr = expr as WhileExpr;
        if (whileExpr != null)
        {
            return AnalyseWhile(whileExpr);
        }

        var keywordExpr = expr as KeywordExpr;
        if (keywordExpr != null)
        {
            return new KeywordNode(keywordExpr);
        }

        var assignment = expr as AssignmentExpr;
        if (assignment != null)
        {
            return AnalyseAssignment(assignment);
        }

        var func = expr as FuncNamedExpr;
        if (func != null)
        {
            var funcNode = default(FuncNamedNode);
            functions.TryGetValue(func, out funcNode);
            if (funcNode != null)
            {
                return funcNode;
            }
            else
            {
                AddError("Functions can only be defined in the outermost scope", func.Identifier);
            }
        }

        var returnExpr = expr as ReturnExpr;
        if (returnExpr != null)
        {
            return AnalyseReturn(returnExpr);
        }

        //todo this should never happen, ever!
        return new InvalidNode(expr);
    }

    ReturnNode AnalyseReturn(ReturnExpr expr)
    {
        if (!insideFunction)
        {
            AddError("'return' keyword can only be used inside a function", expr.ReturnToken.Start, expr.ReturnToken.End);
        }

        var value = expr.Value != null ? AnalyseOperand(expr.Value) : null;
        var type = value?.TypeDescriptor ?? SimpleTypeDescriptor.Unit;
        return new ReturnNode(expr, type, value);
    }

    SemanticNode AnalyseAssignment(AssignmentExpr expr)
    {
        var definition = symbolTable.LastOrDefault(d => d.Name == expr.Identifier.Token.Lexeme.Word);
        if (definition == null)
        {
            AddError("The variable has not been defined at this point in code", expr.Identifier);
        }

        var source = AnalyseOperand(expr.Source) as OperandNode;
        if (source == null)
        {
            AddError("Assignment source is invalid", expr.Source);
        }

        if (definition != null && source != null && !TypeChecker.IsAssignable(definition.Type, source.TypeDescriptor))
        {
            AddError("Type mismatch between the expression and the variable", expr);
        }

        var identifier = new IdentifierNode(expr.Identifier, definition, definition?.Type ?? InvalidTypeDescriptor.Invalid);
        return new AssignmentNode(expr, identifier, source);
    }

    WhileNode AnalyseWhile(WhileExpr expr)
    {
        var condition = AnalyseOperand(expr.Condition);
        var type = condition.TypeDescriptor as SimpleTypeDescriptor;
        if (type == null || type.SimpleType != SimpleType.Bool || type.IsNullable)
        {
            AddError("The type of a condition must be bool", expr.Condition);
        }

        loopCount++;
        var block = AnalyseBlock(expr.Block);
        loopCount--;

        return new WhileNode(expr, condition, block);
    }

    OperandNode AnalyseOperand(OperandExpr expr)
    {
        var unaryOperation = expr as UnaryOperationExpr;
        if (unaryOperation != null)
        {
            return AnalyseUnaryOperation(unaryOperation);
        }

        var binaryOperation = expr as BinaryOperationExpr;
        if (binaryOperation != null)
        {
            return AnalyseBinaryOperation(binaryOperation);
        }

        var literal = expr as LiteralExpr;
        if (literal != null)
        {
            return AnalyseLiteral(literal);
        }

        var parenthesized = expr as ParenthesizedExpr;
        if (parenthesized != null)
        {
            return new ParenthesizedNode(expr, AnalyseOperand(parenthesized.Expr));
        }

        var identifier = expr as IdentifierExpr;
        if (identifier != null)
        {
            return AnalyseIdentifier(identifier);
        }

        var funcInvocation = expr as FuncInvocationExpr;
        if (funcInvocation != null)
        {
            var invocation = AnalyseFuncInvocation(funcInvocation);
            return invocation.With(invocation.TypeDescriptor.With(invocation.TypeDescriptor.IsNullable || invocation.HasConditionalInvocation));
        }

        var nullExpr = expr as NullExpr;
        if (nullExpr != null)
        {
            return new NullNode(nullExpr);
        }

        var list = expr as ListExpr;
        if (list != null)
        {
            return AnalyseList(list);
        }

        return new OperandNode(expr, InvalidTypeDescriptor.Invalid);
    }

    ListNode AnalyseList(ListExpr expr)
    {
        var items = expr.Items.Select(AnalyseOperand).ToList();
        var type = default(TypeDescriptor);
        if (items.Any())
        {
            var itemType = items.Select(n => n.TypeDescriptor).ClosestSupertype();

            if (itemType == null)
            {
                type = InvalidTypeDescriptor.Invalid;
                AddError("Type mismatch between list elements", expr.Start, expr.End);
            }
            else
            {
                type = new ListTypeDescriptor(itemType);
            }
        }
        else
        {
            type = ListTypeDescriptor.Empty;
        }
        return new ListNode(expr, type, items);
    }

    FuncInvocationNode AnalyseFuncInvocation(FuncInvocationExpr expr)
    {
        var invocationPreParam = expr.PreParam as FuncInvocationExpr;
        var preParam = invocationPreParam != null ? AnalyseFuncInvocation(invocationPreParam) : expr.PreParam != null ? AnalyseOperand(expr.PreParam) : null;

        var paramCount = expr.Params.Count + (preParam == null ? 0 : 1);
        var parameters = new ExtList<OperandNode>(paramCount);
        if (preParam != null)
        {
            parameters += preParam;
        }
        var postParams = expr.Params.Select(p => AnalyseOperand(p as OperandExpr)).ToList();
        parameters += postParams;

        var func = functions.Values.LastOrDefault(f => f.Name == expr.Identifier.Word);

        if (func == null && funcStack.PeekSafe()?.Name == expr.Identifier.Word)
        {
            var currentFunc = funcStack.PeekSafe();
            if (currentFunc.ReturnType == null)
            {
                AddError($@"For recursive calls please provide the return type for function: '{currentFunc.Name}' explicitly", currentFunc.Expr);
                return new FuncInvocationNode(expr, InvalidTypeDescriptor.Invalid, postParams, preParam, expr.IsConditionalInvocation);
            }
            else
            {
                func = currentFunc;
            }
        }

        if (func != null)
        {
            if (!func.IsExtension && preParam != null)
            {
                AddError($"The function: '{func.Name}' can't be used as an extension", expr.Identifier);
            }

            if (func.Params.Count != parameters.Count)
            {
                AddError("The number of required parameters is different from the number of supplied parameters", expr.Identifier);
            }
            else
            {
                for (int i = 0; i < parameters.Count; i++)
                {
                    var declaredType = func.Params[i].Type;
                    if (declaredType != null)
                    {
                        var suppliedParameter = parameters[i];
                        var suppliedType = suppliedParameter.TypeDescriptor;
                        if (i == 0 && expr.IsConditionalInvocation)
                        {
                            if (suppliedType.IsNullable)
                            {
                                suppliedType = suppliedType.With(!expr.IsConditionalInvocation);
                            }
                            else
                            {
                                AddError("Conditional invocation is not allowed on a non-nullable variable", suppliedParameter);
                            }
                        }

                        var isAssignable = TypeChecker.IsAssignable(declaredType, suppliedType);
                        if (!isAssignable)
                        {
                            AddError("Type mismatch between the declared parameter and the supplied one", suppliedParameter);
                        }
                    }
                }
            }
            return new FuncInvocationNode(expr, func.ReturnType, postParams, preParam, expr.IsConditionalInvocation, func);
        }
        else
        {
            AddError("Function with this name hasn't been defined", expr.Identifier);
            return new FuncInvocationNode(expr, InvalidTypeDescriptor.Invalid, postParams, preParam, expr.IsConditionalInvocation);
        }
    }

    IdentifierNode AnalyseIdentifier(IdentifierExpr expr)
    {
        var definition = symbolTable.LastOrDefault(d => d.Name == expr.Token.Lexeme.Word);
        if (definition == null)
        {
            AddError("The variable has not been defined at this point in code", expr);
        }
        return new IdentifierNode(expr, definition, definition?.Type ?? InvalidTypeDescriptor.Invalid);
    }

    VariableDefinitionNode AnalyseDefinition(DefinitionExpr definition)
    {
        var source = AnalyseOperand(definition.Source);

        TypeDescriptor type;
        if (definition.Type != null)
        {
            type = TypeDescriptor.Create(definition.Type);

            var matches = TypeChecker.IsAssignable(type, source.TypeDescriptor);
            if (!matches)
            {
                AddError("Type mismatch between the expression and the variable", definition);
            }
        }
        else if (source.TypeDescriptor.NotIdentified())
        {
            AddError("Expression type couldn't be inferred. Please, provide the type explicitly", definition.Start);
            type = InvalidTypeDescriptor.Invalid;
        }
        else
        {
            type = source.TypeDescriptor == CompositeTypeDescriptor.NumberToBoolComposite ?
                                SimpleTypeDescriptor.Bool : source.TypeDescriptor;
        }

        return new VariableDefinitionNode(definition, type, source);
    }

    OperandNode AnalyseLiteral(LiteralExpr literal)
    {
        switch (literal.Token.Class)
        {
            case TokenClass.NumberLiteral:
                return new NumberLiteralNode(literal);
            case TokenClass.StringLiteral:
                return new StringLiteralNode(literal);
            case TokenClass.TrueKeyword:
            case TokenClass.FalseKeyword:
                return new BooleanLiteralNode(literal);
        }
        return new OperandNode(literal, InvalidTypeDescriptor.Invalid);
    }

    OperandNode AnalyseBinaryOperation(BinaryOperationExpr expr)
    {
        var left = AnalyseOperand(expr.Left);
        var right = AnalyseOperand(expr.Right);

        if (expr.Operator.Class == TokenClass.QuestionQuestionOperator)
        {
            if (OperatorAssistant.IsQuestionQuestionApplicable(left.TypeDescriptor, right.TypeDescriptor))
            {
                return new QuestionQuestionOperationNode(expr, right.TypeDescriptor, left, right);
            }
        }
        else
        {
            var operatorSignature = OperatorAssistant.FindSignature(expr.Operator.Class);
            var type = operatorSignature?.GetResult(left.TypeDescriptor, right.TypeDescriptor);

            if (type != null)
            {
                if (type == CompositeTypeDescriptor.NumberToBoolComposite)
                {
                    return new NumberBooleanOperationNode(expr, left, right, type);
                }

                if (type.SimpleType == SimpleType.Bool)
                {
                    return new BooleanOperationNode(expr, left, right, type);
                }

                if (type.SimpleType == SimpleType.Number)
                {
                    return new NumberOperationNode(expr, left, right, type);
                }

                if (type.SimpleType == SimpleType.String)
                {
                    return new StringConcatNode(expr, left, right, type);
                }
            }
        }

        AddError("Invalid operation", left);
        return new OperandNode(expr, InvalidTypeDescriptor.Invalid);
    }

    OperandNode AnalyseUnaryOperation(UnaryOperationExpr expr)
    {
        var operand = AnalyseOperand(expr.Operand);
        var typeDescriptor = operand.TypeDescriptor as SimpleTypeDescriptor;

        if (expr.Operator.Class == TokenClass.NegationOperator && typeDescriptor?.SimpleType == SimpleType.Bool)
        {
            return new UnaryOperationNode(expr, operand, SimpleTypeDescriptor.Bool.With(typeDescriptor.IsNullable));
        }

        if (expr.Operator.Class == TokenClass.MinusOperator && typeDescriptor?.SimpleType == SimpleType.Number)
        {
            return new UnaryOperationNode(expr, operand, SimpleTypeDescriptor.Number.With(typeDescriptor.IsNullable));
        }

        AddError("Invalid operation", expr);
        return new OperandNode(expr, InvalidTypeDescriptor.Invalid);
    }

    IfNode AnalyseIf(IfExpr expr)
    {
        var condition = AnalyseOperand(expr.Condition);
        var conditionType = condition.TypeDescriptor as SimpleTypeDescriptor;
        if (conditionType == null || conditionType.SimpleType != SimpleType.Bool || conditionType.IsNullable)
        {
            AddError("The type of a condition must be bool", expr.Condition);
        }

        var block = AnalyseBlock(expr.Block);

        var elseIfNode = expr.ElseIfClause != null ? AnalyseIf(expr.ElseIfClause) : null;
        var elseNode = expr.ElseClause != null ? AnalyseBlock(expr.ElseClause) : null;

        TypeDescriptor type = null;
        if (elseNode != null)
        {
            type = TypeChecker.ClosestSupertype(block.Type, elseNode.Type);
        }
        else if (elseIfNode != null)
        {
            type = TypeChecker.ClosestSupertype(block.Type, elseIfNode.Type);
        }
        else
        {
            type = block.Type;
        }

        return new IfNode(expr, condition, block, elseIfNode, elseNode, type);
    }

    BlockNode AnalyseBlock(BlockExpr block, ExtList<Token> scopeVariables = null)
    {
        var nodes = Analyse(block.Expressions, scopeVariables);
        var node = nodes.LastOrDefault() as OperandNode;
        return new BlockNode(block, nodes, node?.TypeDescriptor ?? SimpleTypeDescriptor.Unit);
    }

    void AddError(string message, SemanticNode node)
    {
        AddError(message, node.Expr);
    }

    void AddError(string message, Expr expr)
    {
        AddError(message, expr.Start, expr.End);
    }

    void AddError(string message, Token token)
    {
        AddError(message, token.Start, token.End);
    }

    void AddError(string message, int start, int? end = null)
    {
        codeErrors += new CodeError(message, 1, start, end ?? start);
    }
}