namespace Ephemera.Parsing.Expressions
{
    public class FuncDefinitionExpr : FuncNamedExpr
    {
        public BlockExpr Body { get; }

        public FuncDefinitionExpr(FuncSignatureExpr signature, BlockExpr body)
            : base(signature)
        {
            Body = body;
        }
    }
}
