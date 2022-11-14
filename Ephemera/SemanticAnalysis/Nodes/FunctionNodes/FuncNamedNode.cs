using System.Collections.Generic;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using Ephemera.Lexing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class FuncNamedNode : SemanticNode
    {
        public Token Identifier { get; }
        public string Name => Identifier.Word;
        public IReadOnlyList<DefinitionNode> Params { get; }
        public TypeDescriptor ReturnType { get; }
        public bool IsExtension { get; }

        public FuncNamedNode(Expr expr, Token identifier, IReadOnlyList<DefinitionNode> @params, TypeDescriptor returnType, bool isExtension)
            : base(expr)
        {
            Identifier = identifier;
            Params = @params;
            ReturnType = returnType;
            IsExtension = isExtension;
        }
    }
}
