using Ephemera.Lexing;
using Ephemera.Parsing.Expressions;
using Ephemera.Reusable;
using System.Collections.Generic;

namespace Ephemera.Parsing;

public class Parser
{
    public static ParseResult<RootExpr> Parse(IReadOnlyList<Token> tokens)
    {
        var index = 0;
        var expressions = new List<Expr>();

        while (true)
        {
            var result = Parse(tokens, index);
            if (result == null) break;
            if (result.Fail) return result.Cast<RootExpr>();

            expressions.Add(result.Expr);
            index = result.Index;
        }

        return new RootExpr(expressions).ToParseResult(index);
    }

    static IParseResult<Expr> Parse(IReadOnlyList<Token> tokens, int index)
    {
        var token = tokens.At(index);
        if (token == null) return null;

        if (token.Class == TokenClass.Identifier && tokens.At(index + 1)?.Class == TokenClass.AssignmentOperator)
        {
            return ParseAssignmentExpr(tokens, index);
        }
        else if (token.IsSimpleOperand() || token.IsUnaryOperator() && tokens.At(index + 1).IsSimpleOperand())
        {
            return ParseExpression(tokens, index);
        }
        else if (token.Class == TokenClass.IfKeyword)
        {
            return ParseIfExpression(tokens, index);
        }
        else if (token.Class == TokenClass.DefKeyword)
        {
            return ParseDefinition(tokens, index);
        }
        else if (token.Class == TokenClass.WhileKeyword)
        {
            return ParseWhileExpression(tokens, index);
        }
        else if (token.Class == TokenClass.SkipKeyword || token.Class == TokenClass.BreakKeyword)
        {
            return new KeywordExpr(token).ToParseResult(index + 1);
        }
        else if (token.Class == TokenClass.AttributeStart)
        {
            return ParseFuncImport(tokens, index);
        }
        else if (token.Class == TokenClass.FunKeyword)
        {
            return ParseFuncDefinition(tokens, index);
        }
        else if (token.Class == TokenClass.ReturnKeyword)
        {
            return ParseReturn(token, tokens, index + 1);
        }

        return ErrorResult<Expr>($"Invalid token: '{token.Word}'", token.Start);
    }

    static ParseResult<ReturnExpr> ParseReturn(Token returnToken, IReadOnlyList<Token> tokens, int index)
    {
        var operand = default(OperandExpr);
        var token = tokens.At(index);
        if (token?.IsSimpleOperand() == true || token?.IsUnaryOperator() == true)
        {
            var result = ParseExpression(tokens, index);
            if (result.Fail) return result.Cast<ReturnExpr>();
            index = result.Index;
            operand = result.Expr;
        }
        return new ReturnExpr(returnToken, operand).ToParseResult(index);
    }

    static ParseResult<FuncDefinitionExpr> ParseFuncDefinition(IReadOnlyList<Token> tokens, int index)
    {
        var signature = ParseFuncSignature(tokens, index);
        if (signature.Fail) return signature.Cast<FuncDefinitionExpr>();

        var block = ParseBlock(tokens, signature.Index);
        if (block.Fail) return block.Cast<FuncDefinitionExpr>();

        return new FuncDefinitionExpr(signature.Expr, block.Expr).ToParseResult(block.Index);
    }

    static ParseResult<FuncImportExpr> ParseFuncImport(IReadOnlyList<Token> tokens, int index)
    {
        var attrStart = tokens.At(index);
        var importedFuncName = tokens.At(index + 1);
        var attrEnd = tokens.At(index + 2);
        if (attrStart?.Class != TokenClass.AttributeStart ||
            importedFuncName?.Class != TokenClass.StringLiteral ||
            attrEnd?.Class != TokenClass.AttributeEnd)
        {
            return ErrorResult<FuncImportExpr>("Function import attribute couldn't be parsed", attrStart?.Lexeme.StartPosition);
        }

        var signature = ParseFuncSignature(tokens, index + 3);
        if (signature.Fail) return signature.Cast<FuncImportExpr>();
        return new FuncImportExpr(signature.Expr, importedFuncName.Lexeme.Word).ToParseResult(signature.Index);
    }

    static ParseResult<FuncSignatureExpr> ParseFuncSignature(IReadOnlyList<Token> tokens, int index)
    {
        var token = tokens.At(index);
        if (token?.Class != TokenClass.FunKeyword)
        {
            return ErrorResult<FuncSignatureExpr>("'fun' keyword expected", tokens.At(index - 1)?.Lexeme.EndPosition);
        }

        var funcName = tokens.At(++index);
        if (funcName?.Class != TokenClass.Identifier)
        {
            return ErrorResult<FuncSignatureExpr>("Function name expected after 'fun' keyword", token.Lexeme.EndPosition);
        }

        var openParen = tokens.At(++index);
        if (openParen?.Class != TokenClass.OpenParen)
        {
            return ErrorResult<FuncSignatureExpr>("'(' expected", funcName.Lexeme.EndPosition);
        }

        var paramList = new ExtList<ParamDeclarationExpr>();
        token = tokens.At(++index);

        var isExtension = false;
        var parameterExpected = false;
        if (token?.Class == TokenClass.PreKeyword)
        {
            isExtension = true;
            parameterExpected = true;
            token = tokens.At(++index);
        }

        while (true)
        {
            if (token?.Class == TokenClass.Identifier)
            {
                var paramName = token;
                token = tokens.At(++index);
                var type = default(TypeExpr);
                if (token?.Class == TokenClass.Colon)
                {
                    var typeResult = ParseTypeExpr(tokens, index + 1, token, true);
                    if (typeResult.Fail) return typeResult.Cast<FuncSignatureExpr>();
                    type = typeResult.Expr;
                    index = typeResult.Index;
                    token = tokens.At(index);
                }

                paramList += new ParamDeclarationExpr(paramName, type);
                if (token?.Class == TokenClass.Comma)
                {
                    token = tokens.At(++index);
                    parameterExpected = true;
                    continue;
                }
            }
            else if (parameterExpected)
            {
                return ErrorResult<FuncSignatureExpr>("Parameter name expected", token?.Lexeme.StartPosition);
            }
            break;
        }

        if (token?.Class != TokenClass.CloseParen)
        {
            return ErrorResult<FuncSignatureExpr>("'(' expected", token?.Lexeme.StartPosition);
        }

        var returnType = default(TypeExpr);
        token = tokens.At(++index);
        if (token?.Class == TokenClass.Colon)
        {
            var typeResult = ParseTypeExpr(tokens, index + 1, token, true);
            if (typeResult.Fail) return typeResult.Cast<FuncSignatureExpr>();
            returnType = typeResult.Expr;
            index = typeResult.Index;
        }
        return new FuncSignatureExpr(funcName, isExtension, paramList, returnType).ToParseResult(index);
    }

    static ParseResult<AssignmentExpr> ParseAssignmentExpr(IReadOnlyList<Token> tokens, int index)
    {
        var identifier = tokens.At(index);
        if (identifier.Class != TokenClass.Identifier)
        {
            return ErrorResult<AssignmentExpr>("Identifier expected", identifier.Lexeme.StartPosition);
        }

        var assignment = tokens.At(++index);
        if (assignment?.Class != TokenClass.AssignmentOperator)
        {
            return ErrorResult<AssignmentExpr>("'=' operator expected", identifier.Lexeme.EndPosition);
        }

        var result = ParseExpression(tokens, index + 1);
        if (result.Fail)
        {
            return result.Cast<AssignmentExpr>();
        }
        return new AssignmentExpr(new IdentifierExpr(identifier), result.Expr).ToParseResult(result.Index);
    }

    static ParseResult<WhileExpr> ParseWhileExpression(IReadOnlyList<Token> tokens, int index)
    {
        var token = tokens.At(index);
        if (token.Class != TokenClass.WhileKeyword)
        {
            return new CodeError("'while' keyword expected", 1, token.Lexeme.StartPosition, token.Lexeme.EndPosition)
                .ToParseResult<WhileExpr>();
        }

        var result = ParseCondition(tokens, index + 1);
        if (result.Fail)
        {
            return result.Cast<WhileExpr>();
        }
        var condition = result.Expr;
        index = result.Index;

        var blockResult = ParseBlock(tokens, index);
        if (blockResult.Fail)
        {
            return blockResult.Cast<WhileExpr>();
        }

        var block = blockResult.Expr;
        index = blockResult.Index;

        return new WhileExpr(condition, block).ToParseResult(index);
    }

    static ParseResult<OperandExpr> ParseFuncInvocation(IReadOnlyList<Token> tokens, int index, OperandExpr preParam, bool isConditionalInvocation)
    {
        while (true)
        {
            var identifier = tokens.At(index);
            if (identifier?.Class != TokenClass.Identifier)
            {
                return ErrorResult<OperandExpr>("Identifier expected", identifier?.Start);
            }

            var openParen = tokens.At(++index);
            if (openParen?.Class != TokenClass.OpenParen)
            {
                return ErrorResult<OperandExpr>("'(' expected", identifier.End);
            }

            var token = tokens.At(++index);
            var parameters = new ExtList<Expr>();
            if (token?.Class != TokenClass.CloseParen)
            {
                while (true)
                {
                    var result = ParseExpression(tokens, index, 0, token);
                    if (result.Fail) return result.Cast<OperandExpr>();
                    parameters += result.Expr;
                    token = tokens.At(index = result.Index);

                    if (token?.Class == TokenClass.Comma)
                    {
                        index++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (token?.Class != TokenClass.CloseParen)
            {
                return ErrorResult<OperandExpr>("')' expected", token?.Start);
            }

            preParam = new FuncInvocationExpr(identifier, parameters, preParam, isConditionalInvocation);

            token = tokens.At(++index);
            if (token?.Class == TokenClass.DotOperator || token?.Class == TokenClass.QuestionDotOperator)
            {
                index++;
                isConditionalInvocation = token.Class == TokenClass.QuestionDotOperator;
                continue;
            }
            break;
        }
        return preParam.ToParseResult(index);
    }

    static IParseResult<OperandExpr> ParseListOrRange(IReadOnlyList<Token> tokens, int index, Token openSquare)
    {
        var token = tokens.At(index);
        if (token?.Class == TokenClass.CloseSquare)
        {
            return new ListExpr(openSquare, new List<OperandExpr>(), token).ToParseResult(index + 1);
        }

        var operandResult = ParseExpression(tokens, index, prevToken: openSquare);
        if (operandResult.Fail) return operandResult;
        var firstOperand = operandResult.Expr;

        token = tokens.At(index = operandResult.Index);
        if (token?.Class == TokenClass.DotDotOperator)
        {
            return ParseRange(tokens, index + 1, firstOperand, token);
        }

        return ParseList(tokens, index, openSquare, firstOperand);
    }

    static ParseResult<RangeExpr> ParseRange(IReadOnlyList<Token> tokens, int index, OperandExpr left, Token rangeToken)
    {
        var result = ParseExpression(tokens, index + 1);
        if (result.Fail) return result.Cast<RangeExpr>();

        var token = tokens.At(index = result.Index);
        var right = result.Expr;

        if (token?.Class == TokenClass.CloseSquare)
        {
            return new RangeExpr(left, rangeToken, right).ToParseResult(index + 1);
        }
        return ErrorResult<RangeExpr>("']' expected", right.End);
    }

    static ParseResult<ListExpr> ParseList(IReadOnlyList<Token> tokens, int index, Token openSquare, OperandExpr expr)
    {
        var items = new List<OperandExpr> { expr };
        var token = tokens.At(index);

        while (true)
        {
            if (token?.Class == TokenClass.Comma)
            {
                var result = ParseExpression(tokens, index + 1, prevToken: token);
                if (result.Fail) return result.Cast<ListExpr>();

                items.Add(expr = result.Expr);
                token = tokens.At(index = result.Index);
                continue;
            }
            break;
        }

        if (token?.Class == TokenClass.CloseSquare)
        {
            return new ListExpr(openSquare, items, token).ToParseResult(index + 1);
        }
        return ErrorResult<ListExpr>("']' expected", expr.End);
    }

    static ParseResult<OperandExpr> ParseExpression(IReadOnlyList<Token> tokens, int index, int prevOperatorPrecedence = 0, Token prevToken = null)
    {
        var token = tokens.At(index);
        if (token == null && prevToken != null)
        {
            return ErrorResult<OperandExpr>("Operand expected", prevToken.Lexeme.EndPosition);
        }

        var isUnaryOperator = token.IsUnaryOperator();
        Token unaryOperatorToken = null;
        var leftOperand = default(OperandExpr);

        if (isUnaryOperator)
        {
            unaryOperatorToken = token;
            token = tokens.At(++index);
        }

        if (token.Class == TokenClass.NullKeyword)
        {
            leftOperand = new NullExpr(token);
        }
        else if (token.Class.IsLiteral())
        {
            leftOperand = new LiteralExpr(token);
        }
        else if (token.Class == TokenClass.OpenSquare)
        {
            var result = ParseListOrRange(tokens, index + 1, token);
            if (result.Fail) return result.Cast<OperandExpr>();
            index = result.Index - 1;
            leftOperand = result.Expr;
        }
        else if (token.Class == TokenClass.OpenParen)
        {
            var result = ParseExpression(tokens, index + 1, 0, token);
            if (result.Fail) return result;

            leftOperand = result.Expr;
            var closeToken = tokens.At(index = result.Index);

            if (closeToken?.Class != TokenClass.CloseParen)
            {
                return ErrorResult<OperandExpr>("')' expected", leftOperand.End);
            }
            leftOperand = new ParenthesizedExpr(token, leftOperand, closeToken);
        }
        else if (token.Class == TokenClass.Identifier && tokens.At(index + 1)?.Class != TokenClass.OpenParen)
        {
            leftOperand = new IdentifierExpr(token);
        }

        var isFunctionInvocation = false;
        var isConditionalInvocation = false;
        if (leftOperand != null && tokens.At(index + 1)?.Class.IsAccessor() == true)
        {
            isFunctionInvocation = true;
            isConditionalInvocation = tokens.At(index + 1).Class == TokenClass.QuestionDotOperator;
            index += 2;
        }
        else if (token.Class == TokenClass.Identifier && tokens.At(index + 1)?.Class == TokenClass.OpenParen)
        {
            isFunctionInvocation = true;
        }

        if (isFunctionInvocation)
        {
            var result = ParseFuncInvocation(tokens, index, leftOperand, isConditionalInvocation);
            if (result.Fail) return result;
            leftOperand = result.Expr;
            index = result.Index - 1;
        }

        if (leftOperand == null)
        {
            return ErrorResult<OperandExpr>("Operand expected", token.Lexeme.StartPosition);
        }

        if (unaryOperatorToken != null)
        {
            leftOperand = new UnaryOperationExpr(unaryOperatorToken, leftOperand);
        }

        ++index;
        while (true)
        {
            token = tokens.At(index);
            if (token.IsBinaryOperator())
            {
                var precedence = GetOperatorPrecedence(token.Class);
                if (precedence >= prevOperatorPrecedence)
                {
                    var result = ParseExpression(tokens, index + 1, precedence, token);
                    if (result.Fail) return result;

                    leftOperand = new BinaryOperationExpr(leftOperand, result.Expr, token);
                    index = result.Index;
                    continue;
                }
            }
            return leftOperand.ToParseResult(index);
        }
    }

    static int GetOperatorPrecedence(TokenClass @class)
    {
        switch (@class)
        {
            case TokenClass.QuestionQuestionOperator:
                return 1;

            case TokenClass.OrOperator:
                return 2;

            case TokenClass.AndOperator:
                return 3;

            case TokenClass.EqualsOperator:
            case TokenClass.NotEqualsOperator:
                return 4;

            case TokenClass.GreaterOperator:
            case TokenClass.GreaterOrEqualsOperator:
            case TokenClass.LessOperator:
            case TokenClass.LessOrEqualsOperator:
                return 5;

            case TokenClass.PlusOperator:
            case TokenClass.MinusOperator:
                return 6;

            case TokenClass.TimesOperator:
            case TokenClass.DivisionOperator:
            case TokenClass.PercentOperator:
                return 7;

            default:
                return 0;
        }
    }

    static ParseResult<IfExpr> ParseIfExpression(IReadOnlyList<Token> tokens, int index)
    {
        var ifToken = tokens.At(index);
        if (ifToken?.Class != TokenClass.IfKeyword &&
            ifToken?.Class != TokenClass.ElifKeyword)
        {
            return new CodeError("'if' keyword expected", 1, index, index).ToParseResult<IfExpr>();
        }

        var result = ParseCondition(tokens, index + 1);
        if (result.Fail)
        {
            return result.Cast<IfExpr>();
        }
        var condition = result.Expr;
        index = result.Index;

        var token = tokens.At(index);
        if (token == null)
        {
            return new CodeError("if expression must have a body", 1, index, index).ToParseResult<IfExpr>();
        }

        var blockResult = ParseIfElseClause(tokens, index);
        if (blockResult.Fail)
        {
            return blockResult.Cast<IfExpr>();
        }
        var block = blockResult.Expr;
        index = blockResult.Index;

        token = tokens.At(index);
        if (token?.Class == TokenClass.ElifKeyword)
        {
            var elifResult = ParseIfExpression(tokens, index);
            if (elifResult.Fail) return elifResult.Cast<IfExpr>();

            return new IfExpr(ifToken, condition, block, elifResult.Expr).ToParseResult(elifResult.Index);
        }
        else if (token?.Class == TokenClass.ElseKeyword)
        {
            var elseResult = ParseIfElseClause(tokens, index + 1);
            if (elseResult.Fail) return elseResult.Cast<IfExpr>();

            return new IfExpr(ifToken, condition, block, elseClause: elseResult.Expr).ToParseResult(elseResult.Index);
        }
        else
        {
            return new IfExpr(ifToken, condition, block).ToParseResult(index);
        }
    }

    static ParseResult<ParenthesizedExpr> ParseCondition(IReadOnlyList<Token> tokens, int index)
    {
        var openParen = tokens.At(index);
        if (openParen?.Class != TokenClass.OpenParen)
        {
            return ErrorResult<ParenthesizedExpr>("'(' expected", openParen?.Start);
        }

        var result = ParseExpression(tokens, index + 1);
        if (result.Fail) return result.Cast<ParenthesizedExpr>();

        var closeParen = tokens.At(result.Index);
        if (closeParen?.Class != TokenClass.CloseParen)
        {
            return ErrorResult<ParenthesizedExpr>("')' expected", result.Expr.End);
        }

        return new ParenthesizedExpr(openParen, result.Expr, closeParen).ToParseResult(result.Index + 1);
    }

    static ParseResult<BlockExpr> ParseIfElseClause(IReadOnlyList<Token> tokens, int index)
    {
        var token = tokens.At(index);
        if (token.Class == TokenClass.OpenCurly)
        {
            return ParseBlock(tokens, index);
        }
        else
        {
            var result = ParseExpression(tokens, index);
            return result.Fail ? result.Cast<BlockExpr>() :
                new BlockExpr(result.Expr).ToParseResult(result.Index);
        }
    }

    static ParseResult<BlockExpr> ParseBlock(IReadOnlyList<Token> tokens, int index)
    {
        var token = tokens.At(index);
        if (token?.Class != TokenClass.OpenCurly)
        {
            return ErrorResult<BlockExpr>("'{' expected", index);
        }

        var expressions = new ExtList<Expr>();
        token = tokens.At(++index);
        while (token?.Class != TokenClass.CloseCurly)
        {
            var result = Parse(tokens, index);
            if (result == null) break;
            if (result.Fail) return result.Cast<BlockExpr>();

            expressions += result.Expr;
            token = tokens.At(index = result.Index);
        }

        if (token?.Class != TokenClass.CloseCurly)
        {
            return ErrorResult<BlockExpr>("'}' expected", index);
        }
        return new BlockExpr(expressions).ToParseResult(index + 1);
    }

    static IParseResult<DefinitionExpr> ParseDefinition(IReadOnlyList<Token> tokens, int index)
    {
        var token = tokens.At(index);
        if (token?.Class != TokenClass.DefKeyword)
        {
            return ErrorResult<DefinitionExpr>("'def' keyword expected", token?.Start);
        }

        var identifier = tokens.At(++index);
        if (identifier?.Class != TokenClass.Identifier)
        {
            return ErrorResult<DefinitionExpr>("Identifier expected", identifier?.Start ?? token.End);
        }

        var type = default(TypeExpr);
        token = tokens.At(++index);
        if (token?.Class == TokenClass.Colon)
        {
            var typeResult = ParseTypeExpr(tokens, index + 1, token);
            if (typeResult.Fail) return typeResult.Cast<DefinitionExpr>();
            type = typeResult.Expr;
            token = tokens.At(index = typeResult.Index);
        }

        if (token?.Class != TokenClass.AssignmentOperator)
        {
            return ErrorResult<DefinitionExpr>("'=' expected", token?.Start);
        }

        var result = ParseExpression(tokens, index + 1);
        if (result.Fail) return result.Cast<DefinitionExpr>();
        return new DefinitionExpr(identifier, type, result.Expr).ToParseResult(result.Index);
    }

    public static IParseResult<TypeExpr> ParseTypeExpr(IReadOnlyList<Token> tokens, int index, Token prevToken, bool allowGenerics = false)
    {
        var token = tokens.At(index);

        if (token == null)
        {
            return ErrorResult<TypeExpr>("Type expected", prevToken.Lexeme.EndPosition);
        }

        if (token.Class.IsBuiltinType())
        {
            var isNullable = IsNullable(tokens, ++index);
            var simpleType = token.Class.ToSimpleType();
            return new SimpleTypeExpr(token, simpleType, isNullable).ToParseResult(isNullable ? index + 1 : index);
        }

        if (token.Class == TokenClass.OpenSquare)
        {
            var result = ParseTypeExpr(tokens, index + 1, token, allowGenerics);
            if (result.Fail) return result;

            index = result.Index;
            token = tokens.At(index);

            if (token?.Class != TokenClass.CloseSquare)
            {
                return ErrorResult<TypeExpr>("']' expected", result.Expr.End);
            }

            var isNullable = IsNullable(tokens, ++index);
            return new ListTypeExpr(result.Expr, isNullable).ToParseResult(isNullable ? index + 1 : index);
        }

        if (token.Class == TokenClass.TypeParam)
        {
            if (!allowGenerics)
            {
                return ErrorResult<TypeExpr>("Generic types are only allowed in function definition", token.Lexeme.StartPosition);
            }
            return new TypeParamExpr(token).ToParseResult(index + 1);
        }

        if (token.Class == TokenClass.OpenParen)
        {
            index++;
            var paramTypes = new ExtList<TypeExpr>();
            if (tokens.At(index)?.Class == TokenClass.OpenParen && tokens.At(index + 1)?.Class == TokenClass.CloseParen)
            {
                index += 2;
            }
            else
            {
                while (true)
                {
                    var typeResult = ParseTypeExpr(tokens, index, token, true);
                    if (typeResult.Fail) return typeResult;

                    index = typeResult.Index;
                    paramTypes += typeResult.Expr;

                    token = tokens.At(index);
                    if (token.Class != TokenClass.Comma)
                    {
                        break;
                    }
                    index++;
                }
            }

            prevToken = token;
            token = tokens.At(index);
            var returnType = default(TypeExpr);
            if (token?.Class == TokenClass.FatArrow)
            {
                var typeResult = ParseTypeExpr(tokens, index + 1, token, true);
                if (typeResult.Fail) return typeResult;
                index = typeResult.Index;
                returnType = typeResult.Expr;
                token = tokens.At(index);
            }

            if (token?.Class != TokenClass.CloseParen)
            {
                return ErrorResult<TypeExpr>("')' expected", prevToken.Lexeme.EndPosition);
            }

            var isNullable = IsNullable(tokens, ++index);
            return new FunctionTypeExpr(paramTypes, returnType, isNullable).ToParseResult(isNullable ? index + 1 : index);
        }
        return ErrorResult<TypeExpr>("Type couldn't be parsed", prevToken.Lexeme.EndPosition);
    }

    static bool IsNullable(IReadOnlyList<Token> tokens, int index) => tokens.At(index)?.Class == TokenClass.QuestionMark;

    static ParseResult<T> ErrorResult<T>(string message, int position)
        where T : Expr
    {
        return new CodeError(message, 1, position, position).ToParseResult<T>();
    }

    static ParseResult<T> ErrorResult<T>(string message, int? position)
        where T : Expr
    {
        return ErrorResult<T>(message, position ?? -1);
    }
}