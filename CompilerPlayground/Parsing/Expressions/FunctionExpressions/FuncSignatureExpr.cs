using CompilerPlayground.Lexing;
using System.Collections.Generic;

namespace CompilerPlayground.Parsing.Expressions
{
    public class FuncSignatureExpr : Expr
    {
        public Token Identifier { get; }
        public bool IsExtension { get; }
        public IReadOnlyList<ParamDeclarationExpr> Parameters { get; }
        public TypeExpr ReturnType { get; }

        public FuncSignatureExpr(Token identifier, bool isExtension, IReadOnlyList<ParamDeclarationExpr> parameters, TypeExpr returnType)
        {
            Identifier = identifier;
            IsExtension = isExtension;
            Parameters = parameters;
            ReturnType = returnType;
        }
    }
}
