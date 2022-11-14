using System.Collections.Generic;
using Ephemera.Lexing;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class FuncDefinitionNode : FuncNamedNode
    {
        public BlockNode Body { get; }

        public FuncDefinitionNode(Expr expr, Token identifier, IReadOnlyList<DefinitionNode> @params, TypeDescriptor returnType, bool isExtension, BlockNode body)
            : base(expr, identifier, @params, returnType, isExtension)
        {
            Body = body;
        }
    }
}
