using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class AssignmentNode : SemanticNode
    {
        public IdentifierNode Identifier { get; }
        public OperandNode Source { get; }

        public AssignmentNode(Expr expr, IdentifierNode identifier, OperandNode source)
            : base(expr)
        {
            Identifier = identifier;
            Source = source;
        }
    }
}
