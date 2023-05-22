using Ephemera.Lexing;
using Ephemera.Parsing;
using Ephemera.Reusable;
using Ephemera.SemanticAnalysis.Nodes;
using Ephemera.SemanticAnalysis.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ephemera.Transpilation
{
    public class CSharpTranspiler
    {
        readonly Dictionary<DefinitionNode, string> symbolTable = new Dictionary<DefinitionNode, string>();
        //int helperVariableCount = 0;
        //string GetHelperVariableName() => "_" + ++helperVariableCount;

        readonly Stack<FuncNamedNode> functions = new Stack<FuncNamedNode>();

        int tabCount = 0;
        string currentTab = "";
        void PushTab() => currentTab = Enumerable.Repeat('\t', ++tabCount).JoinStrings();
        void PopTab() => currentTab = Enumerable.Repeat('\t', --tabCount).JoinStrings();

        StringBuilder Append(StringBuilder sb, params string[] args)
        {
            sb.Append(currentTab);
            sb.AppendAll(args);
            return sb;
        }

        StringBuilder AppendWithLine(StringBuilder sb, params string[] args)
        {
            Append(sb, args).Append(Environment.NewLine);
            return sb;
        }



        public string Transpile(IReadOnlyList<SemanticNode> nodes, bool generateTestingSource = false)
        {
            var code = TranspileInner(nodes);

            var sb = new StringBuilder();
            sb.AppendLine("using System;")
              .AppendLine("using System.Collections.Generic;")
              .AppendLine()
              .AppendLine("public class CustomList<T> : List<T> \n{ \n\t public int Size() => Count; \n\t public override string ToString() => \"[\" + string.Join(\", \", this) + \"]\"; \n}")
              .AppendLine()
              .AppendLine("public class Unit \n{ \n\t public static readonly Unit Self = new Unit(); \n\t public override string ToString() => \"Unit\"; \n}")
              .AppendLine()
              .AppendLine("static Unit _ExecuteWithUnit(Action a) { a(); return Unit.Self; } ")
              .AppendLine()
              .AppendLine("static decimal? _Divide(decimal? x, decimal? y) => y == 0 ? default(decimal?) : x / y;")
              .AppendLine()
              .AppendLine("static decimal? _Percent(decimal? x, decimal? y) => y == 0 ? default(decimal?) : x % y;")
              .AppendLine()
              .AppendLine("public static string AsString(this object obj) => obj?.ToString();")
              .AppendLine()
              .AppendLine("static decimal? ephemeralDecimal;")
              .AppendLine();

            if (generateTestingSource)
            {
                sb.AppendLine("static int trueCount = 0;")
                  .AppendLine("static int falseCount = 0;")
                  .AppendLine()
                  .AppendLine("static bool True()")
                  .AppendLine("{")
                  .AppendLine("\ttrueCount++;")
                  .AppendLine("\treturn true;")
                  .AppendLine("}")
                  .AppendLine()
                  .AppendLine("static bool False()")
                  .AppendLine("{")
                  .AppendLine("\tfalseCount++;")
                  .AppendLine("\treturn false;")
                  .AppendLine("}")
                  .AppendLine()
                  .AppendLine("static int fourCount = 0;")
                  .AppendLine()
                  .AppendLine("static decimal Four()")
                  .AppendLine("{")
                  .AppendLine("\tfourCount++;")
                  .AppendLine("\treturn 4;")
                  .AppendLine("}")
                  .AppendLine();
            }

            sb.AppendLine(code);
            return sb.ToString();
        }

        string TranspileInner(IReadOnlyList<SemanticNode> nodes)
        {
            var sb = new StringBuilder();
            TranspileInner(nodes, sb);
            return sb.ToString();
        }

        StringBuilder TranspileInner(IReadOnlyList<SemanticNode> nodes, StringBuilder sb)
        {
            foreach (var node in nodes)
            {
                var line = Transpile(node);
                if (!string.IsNullOrWhiteSpace(line))
                {
                    sb.AppendLine(line);
                }
            }
            return sb;
        }

        string Transpile(SemanticNode node)
        {
            var definition = node as VariableDefinitionNode;
            if (definition != null)
            {
                return TranspileVariableDefinition(definition);
            }

            var operand = node as OperandNode;
            if (operand != null)
            {
                return TranspileOperand(operand) + ";";
            }

            var ifNode = node as IfNode;
            if (ifNode != null)
            {
                return TranspileIf(ifNode);
            }

            var whileNode = node as WhileNode;
            if (whileNode != null)
            {
                return TranspileWhile(whileNode);
            }

            var keywordNode = node as KeywordNode;
            if (keywordNode != null)
            {
                return Transpile(keywordNode);
            }

            var assignment = node as AssignmentNode;
            if (assignment != null)
            {
                return TranspileAssignment(assignment);
            }

            var funcDefinition = node as FuncDefinitionNode;
            if (funcDefinition != null)
            {
                return TranspileFuncDefinition(funcDefinition);
            }

            var returnNode = node as ReturnNode;
            if (returnNode != null)
            {
                return TranspileReturnNode(returnNode);
            }

            //todo add error here
            return null;
        }

        string TranspileOperand(OperandNode node) => TranspileOperand(node, null);

        string TranspileOperand(OperandNode node, TypeDescriptor targetType = null)
        {
            var number = node as NumberLiteralNode;
            if (number != null)
            {
                return number.Literal.Token.Lexeme.Word + "m";
            }

            var boolean = node as BooleanLiteralNode;
            if (boolean != null)
            {
                return boolean.Literal.Token.Lexeme.Word;
            }

            var @string = node as StringLiteralNode;
            if (@string != null)
            {
                return @string.Literal.Token.Lexeme.Word;
            }

            var identifier = node as IdentifierNode;
            if (identifier != null)
            {
                return symbolTable[identifier.Definition];
            }

            var numberBooleanOperation = node as NumberBooleanOperationNode;
            if (numberBooleanOperation != null)
            {
                return Transpile(numberBooleanOperation);
            }

            var booleanOperation = node as BooleanOperationNode;
            if (booleanOperation != null)
            {
                return Transpile(booleanOperation);
            }

            var numberOperation = node as NumberOperationNode;
            if (numberOperation != null)
            {
                return Transpile(numberOperation);
            }

            var stringConcat = node as StringConcatNode;
            if (stringConcat != null)
            {
                return TranspileStringConcat(stringConcat);
            }

            var parenthesized = node as ParenthesizedNode;
            if (parenthesized != null)
            {
                return $"({TranspileOperand(parenthesized.InnerNode)})";
            }

            var funcInvocation = node as FuncInvocationNode;
            if (funcInvocation != null)
            {
                return TranspileFuncInvocation(funcInvocation);
            }

            var nullNode = node as NullNode;
            if (nullNode != null)
            {
                return "null";
            }

            var unary = node as UnaryOperationNode;
            if (unary != null)
            {
                return $"{unary.Operator.Word}{TranspileOperand(unary.Operand)}";
            }

            var questionQuestion = node as QuestionQuestionOperationNode;
            if (questionQuestion != null)
            {
                return TranspileQuestionQuestionOperation(questionQuestion);
            }

            var list = node as ListNode;
            if (list != null)
            {
                return TranspileList(list, targetType);
            }

            //todo add error here
            return "";
        }

        string TranspileReturnNode(ReturnNode node)
        {
            var func = functions.Peek();
            var returnType = func.ReturnType;
            var value = node.Value != null ? TranspileOperand(node.Value, returnType) : "Unit.Self";
            return "return " + value + ";";
        }

        string TranspileFuncDefinition(FuncDefinitionNode node)
        {
            functions.Push(node);

            var parameters = TranspileParamDefinitions(node.Params, node.IsExtension);
            var returnType = TranspileType(node.ReturnType);

            var sb = new StringBuilder();
            //AppendWithLine(sb, node.IsExtension ? "static " : "", returnType, " ", node.Name, parameters);
            AppendWithLine(sb, "static ", returnType, " ", node.Name, parameters);

            var lastStatement = default(string);
            var returnSimpleType = node.ReturnType as SimpleTypeDescriptor;
            if (returnSimpleType?.SimpleType == SimpleType.Unit)
            {
                lastStatement = "return Unit.Self;";
            }

            TranspileBlock(sb, node.Body.Children, lastStatement);

            functions.Pop();
            return sb.ToString();
        }

        string TranspileParamDefinitions(IReadOnlyList<DefinitionNode> parameters, bool isExtension)
        {
            var set = new HashSet<string>();
            var sb = new StringBuilder();
            sb.Append("(");
            if (isExtension)
            {
                sb.Append("this ");
            }
            for (int i = 0; i < parameters.Count; i++)
            {
                var item = parameters[i];
                var name = RegisterDefinition(item);
                var type = TranspileType(item.Type);
                var typeParam = item.Type as TypeParamDescriptor;
                if (typeParam != null)
                {
                    set.TryAdd(type);
                }

                sb.AppendAll(type, " ", name, i != parameters.Count - 1 ? "," : "");
            }
            sb.Append(")");

            var typeParamString = set.Select(t => t.TrimStart('#')).JoinStrings(", ");
            if (!string.IsNullOrWhiteSpace(typeParamString))
            {
                typeParamString = "<" + typeParamString + ">";
            }

            return typeParamString + sb.ToString();
        }

        string TranspileAssignment(AssignmentNode node)
        {
            var name = symbolTable[node.Identifier.Definition];
            var source = TranspileOperand(node.Source, node.Identifier.TypeDescriptor);
            return $"{name} = {source};";
        }

        string Transpile(KeywordNode node)
        {
            if (node.KeywordExpr.Keyword.Class == TokenClass.SkipKeyword)
            {
                return "continue;";
            }

            if (node.KeywordExpr.Keyword.Class == TokenClass.BreakKeyword)
            {
                return "break;";
            }
            return null;
        }

        string TranspileWhile(WhileNode node)
        {
            var condition = TranspileOperand(node.Condition);
            var sb = new StringBuilder().AppendAll("while ", condition, Environment.NewLine);
            TranspileBlock(sb, node.Block.Children);
            return sb.ToString();
        }

        string TranspileVariableDefinition(VariableDefinitionNode node)
        {
            var type = TranspileType(node.Type);
            var source = TranspileOperand(node.Source, node.Type);
            var name = RegisterDefinition(node);

            return $"{type} {name} = {source};";
        }

        string RegisterDefinition(DefinitionNode node)
        {
            var count = symbolTable.Count(x => x.Key.Name == node.Name);
            var name = count > 0 ? $"_{node.Name}{count}" : $"{node.Name}";

            symbolTable.Add(node, name);
            return name;
        }

        string TranspileList(ListNode node, TypeDescriptor targetType)
        {
            var typeDescriptor = targetType?.IsGeneric() == false ? targetType : node.TypeDescriptor;
            var listType = typeDescriptor as ListTypeDescriptor;

            var type = TranspileType(typeDescriptor);
            var elements = node.Elements.Select(o => TranspileOperand(o, listType?.ElementType)).JoinStrings(", ");
            return $"new {type} {{ {elements} }}";
        }

        string TranspileQuestionQuestionOperation(QuestionQuestionOperationNode operation)
        {
            var left = TranspileOperand(operation.Left);
            var right = TranspileOperand(operation.Right);
            return $"{left} ?? {right}";
        }

        string TranspileStringConcat(StringConcatNode node)
        {
            var left = TranspileOperand(node.Left);
            var right = TranspileOperand(node.Right);
            return $"{left} + {right}";
        }

        //string TranspileFuncInvocation(FuncInvocationNode node, string preParamName, TypeDescriptor preParamType)
        //{
        //    var parameters = node.Params.Zip(node.NamedFunc.Params, (x, y) => TranspileOperand(x, y.Type));
        //    if (preParamName != null && preParamType != null && node.IsConditionalInvocation && IsStruct(preParamType))
        //    {
        //        preParamName += ".Value";
        //    }

        //    var importedFunc = node.NamedFunc as FuncImportNode;
        //    if (importedFunc != null)
        //    {
        //        var name = importedFunc.ImportedFuncName.Trim('"');
        //        if (preParamName != null)
        //        {
        //            name = $"{preParamName}.{name}";
        //        }

        //        var invocation = $"{name}({parameters.JoinStrings(", ")})";

        //        if (node.TypeDescriptor.IsUnit())
        //        {
        //            return $"_Execute(() => {{ {invocation}; return Unit.Self; }} )";
        //        }
        //        return invocation;
        //    }

        //    if (preParamName != null)
        //    {
        //        parameters = parameters.Prepend(preParamName);
        //    }
        //    return $"{node.Name}({parameters.JoinStrings(", ")})";
        //}

        //string TranspileFuncInvocation(FuncInvocationNode node)
        //{
        //    var stack = new Stack<FuncInvocationNode>();
        //    var currentNode = node;
        //    do
        //    {
        //        stack.Push(currentNode);
        //        currentNode = currentNode.PreParam as FuncInvocationNode;
        //    } while (currentNode != null);

        //    var first = stack.Peek();
        //    var variableName = default(string);
        //    var variableType = default(TypeDescriptor);

        //    var sb = new StringBuilder();
        //    var tabCount = 1;
        //    var tab = Enumerable.Repeat("\t", tabCount).JoinStrings();
        //    if (first.PreParam != null)
        //    {
        //        var preParam = TranspileOperand(first.PreParam);
        //        variableName = GetHelperVariableName();
        //        variableType = first.PreParam.TypeDescriptor;
        //        sb.AppendAll(tab, $"var {variableName} = {preParam};", Environment.NewLine);
        //    }

        //    foreach (var item in stack)
        //    {
        //        if (item.IsConditionalInvocation)
        //        {
        //            sb.AppendAll(tab, $"if ({variableName} != null)", Environment.NewLine);
        //            sb.AppendAll(tab, "{", Environment.NewLine);
        //            tab = Enumerable.Repeat("\t", ++tabCount).JoinStrings();
        //        }

        //        var invocation = TranspileFuncInvocation(item, variableName, variableType);
        //        variableName = GetHelperVariableName();
        //        variableType = item.TypeDescriptor;
        //        sb.AppendAll(tab, $"var {variableName} = {invocation};", Environment.NewLine);
        //    }
        //    sb.AppendAll(tab, $"return {variableName};", Environment.NewLine);

        //    var returnNull = tabCount > 1;
        //    while (tabCount > 1)
        //    {
        //        tab = Enumerable.Repeat("\t", --tabCount).JoinStrings();
        //        sb.AppendAll(tab, "}", Environment.NewLine);
        //    }

        //    if (returnNull)
        //    {
        //        var type = TranspileType(node.TypeDescriptor);
        //        sb.AppendAll(tab, $"return default({type});", Environment.NewLine);
        //    }

        //    var result = $"_Execute(() => \n{{\n{sb.ToString()}}})";

        //    return result;
        //}

        string TranspileFuncInvocation(FuncInvocationNode node)
        {
            var stack = new Stack<FuncInvocationNode>();
            var currentNode = node;
            do
            {
                stack.Push(currentNode);
                currentNode = currentNode.PreParam as FuncInvocationNode;
            } while (currentNode != null);

            var first = stack.Peek();
            var preParam = first.PreParam;
            var result = "";
            if (preParam != null)
            {
                result = TranspileOperand(preParam, first.NamedFunc.Params.First().Type);
            }

            foreach (var item in stack)
            {
                if (item.PreParam != null)
                {
                    result += item.IsConditionalInvocation ? "?." : ".";
                }
                result += TranspileSingleFuncInvocation(item);
            }
            return result;
        }

        string TranspileSingleFuncInvocation(FuncInvocationNode node)
        {
            var name = node.Name;
            var wrapWithUnit = false;
            var importedFunc = node.NamedFunc as FuncImportNode;
            if (importedFunc != null)
            {
                name = importedFunc.ImportedFuncName.Trim('"');
                wrapWithUnit = node.TypeDescriptor.IsUnit();
            }

            var declaredParams = node.NamedFunc.IsExtension ? node.NamedFunc.Params.Skip(1) : node.NamedFunc.Params;
            var parameters = node.Params.Zip(declaredParams, (x, y) => TranspileOperand(x, y.Type));
            var paramString = parameters.JoinStrings(", ");
            var invocation = $"{name}({paramString})";
            if (wrapWithUnit)
            {
                invocation = $"_ExecuteWithUnit(() => {invocation})";
            }
            return invocation;
        }

        string Transpile(NumberBooleanOperationNode node)
        {
            var sb = new StringBuilder();
            sb.Append($"{TranspileOperand(node.Left)} {node.BinaryExpr.Operator.Lexeme.Word} ");

            while (node.Right is NumberBooleanOperationNode r)
            {
                node = r;
                sb.Append($"(ephemeralDecimal = {TranspileOperand(node.Left)}) && ephemeralDecimal {node.BinaryExpr.Operator.Lexeme.Word} ");
            }

            sb.Append(TranspileOperand(node.Right));

            return sb.ToString();
        }

        string Transpile(BooleanOperationNode node)
        {
            var left = $"{TranspileOperand(node.Left)} {node.BinaryExpr.Operator.Lexeme.Word} ";
            var right = TranspileOperand(node.Right);
            return left + right;
        }

        string TranspileIf(IfNode node, bool isElseIf = false)
        {
            var keyword = isElseIf ? "else if " : "if ";
            var condition = TranspileOperand(node.Condition);
            var sb = new StringBuilder().AppendAll(keyword, condition, Environment.NewLine);

            TranspileBlock(sb, node.Block.Children);

            if (node.ElseIfNode != null)
            {
                var elseIf = TranspileIf(node.ElseIfNode, true);
                Append(sb, elseIf);
            }

            if (node.ElseBlock != null)
            {
                AppendWithLine(sb, "else");
                TranspileBlock(sb, node.ElseBlock.Children);
            }

            return sb.ToString();
        }

        StringBuilder TranspileBlock(StringBuilder sb, IEnumerable<SemanticNode> nodes, string lastStatement = null)
        {
            AppendWithLine(sb, "{");

            PushTab();
            foreach (var item in nodes)
            {
                var line = Transpile(item);
                AppendWithLine(sb, line);
            }

            if (lastStatement != null)
            {
                AppendWithLine(sb, lastStatement);
            }
            PopTab();

            AppendWithLine(sb, "}");
            return sb;
        }

        string Transpile(NumberOperationNode node)
        {
            var left = TranspileOperand(node.Left);
            var right = TranspileOperand(node.Right);

            if (node.BinaryExpr.Operator.Class == TokenClass.DivisionOperator)
            {
                return $"_Divide({left}, {right})";
            }

            if (node.BinaryExpr.Operator.Class == TokenClass.PercentOperator)
            {
                return $"_Percent({left}, {right})";
            }

            var op = node.BinaryExpr.Operator.Lexeme.Word;
            return $"{left} {op} {right}";
        }

        string TranspileType(TypeDescriptor type)
        {
            var simple = type as SimpleTypeDescriptor;
            if (simple != null) return TranspileType(simple);

            var typeParam = type as TypeParamDescriptor;
            if (typeParam != null) return TranspileType(typeParam);

            var list = type as ListTypeDescriptor;
            if (list != null) return TranspileType(list);

            return "";
        }

        string TranspileType(ListTypeDescriptor type)
        {
            string elementType;
            if (type.ElementType is TypeParamDescriptor || type.ElementType is NullTypeDescriptor)
            {
                elementType = "object";
            }
            else
            {
                elementType = TranspileType(type.ElementType);
            }
            return $"CustomList<{elementType}>";
        }

        string TranspileType(TypeParamDescriptor type) => "T" + type.Name.TrimStart('#');

        string TranspileType(SimpleTypeDescriptor type)
        {
            var result = "";
            switch (type.SimpleType)
            {
                case SimpleType.Unit:
                    result = "Unit";
                    break;
                case SimpleType.Bool:
                    result = "bool";
                    break;
                case SimpleType.Number:
                    result = "Decimal";
                    break;
                case SimpleType.String:
                    return "string";
                default:
                    return "";
            }
            return result + (type.IsNullable ? "?" : "");
        }

        bool IsStruct(TypeDescriptor type)
        {
            var simple = type as SimpleTypeDescriptor;
            return simple != null && (simple.SimpleType == SimpleType.Bool || simple.SimpleType == SimpleType.Number);
        }
    }
}
