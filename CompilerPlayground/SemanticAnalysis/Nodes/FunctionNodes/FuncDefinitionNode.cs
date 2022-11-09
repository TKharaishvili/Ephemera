using System.Collections.Generic;
using CompilerPlayground.Lexing;
using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
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
