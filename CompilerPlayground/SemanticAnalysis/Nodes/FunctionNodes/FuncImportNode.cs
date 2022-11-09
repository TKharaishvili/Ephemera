using System.Collections.Generic;
using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;
using CompilerPlayground.Lexing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
{
    public class FuncImportNode : FuncNamedNode
    {
        public string ImportedFuncName { get; }

        public FuncImportNode(FuncImportExpr expr, Token identifier, IReadOnlyList<DefinitionNode> @params, TypeDescriptor returnType, bool isExtension)
            : base(expr, identifier, @params, returnType, isExtension)
        {
            ImportedFuncName = expr.ImportedFuncName;
        }
    }
}
