using CompilerPlayground.Lexing;
using CompilerPlayground.Reusable;
using System.Collections.Generic;

namespace CompilerPlayground.Parsing.Expressions
{
    public class FuncInvocationExpr : OperandExpr
    {
        public Token Identifier { get; }
        public IReadOnlyList<Expr> Params { get; }
        public OperandExpr PreParam { get; }
        public bool IsConditionalInvocation { get; }

        public FuncInvocationExpr(Token identifier, IReadOnlyList<Expr> @params, OperandExpr preParam, bool isConditionalInvocation = false)
        {
            Identifier = identifier;
            Params = @params;
            PreParam = preParam;
            IsConditionalInvocation = isConditionalInvocation;
        }

        public override string ToString() =>
            $"{(PreParam != null ? PreParam.ToString() + Accessor : "")}{Identifier}({Params.JoinStrings(", ")})";

        string Accessor => IsConditionalInvocation ? "?." : ".";
    }
}
