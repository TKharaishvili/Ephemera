namespace CompilerPlayground.Parsing.Expressions
{
    public class AssignmentExpr : Expr
    {
        public IdentifierExpr Identifier { get; }
        public OperandExpr Source { get; }

        public AssignmentExpr(IdentifierExpr identifier, OperandExpr source)
        {
            Identifier = identifier;
            Source = source;
        }

        public override string ToString() => $"{Identifier} = {Source}";
        public override int Start => Identifier.Start;
        public override int End => Source.End;
    }
}
