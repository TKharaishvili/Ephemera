using Ephemera.Lexing;

namespace Ephemera.Parsing.Expressions
{
    public class FuncNamedExpr : FuncDefinitionBaseExpr
    {
        public bool IsExtension { get; }
        public Token Identifier { get; }

        public FuncNamedExpr(FuncSignatureExpr signature) : base(signature)
        {
            IsExtension = signature.IsExtension;
            Identifier = signature.Identifier;
        }
    }
}
