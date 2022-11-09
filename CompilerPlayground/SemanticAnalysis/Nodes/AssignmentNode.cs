using CompilerPlayground.Parsing.Expressions;

namespace CompilerPlayground.SemanticAnalysis.Nodes
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
