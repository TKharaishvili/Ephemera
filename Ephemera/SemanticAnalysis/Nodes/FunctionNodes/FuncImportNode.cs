using System.Collections.Generic;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using Ephemera.Lexing;

namespace Ephemera.SemanticAnalysis.Nodes
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
