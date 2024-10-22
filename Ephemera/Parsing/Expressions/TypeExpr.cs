using Ephemera.Lexing;
using Ephemera.Reusable;
using System.Collections.Generic;
using System.Linq;

namespace Ephemera.Parsing.Expressions;

public class TypeExpr : Expr
{
    public bool IsNullable { get; }

    public TypeExpr(bool isNullable)
    {
        IsNullable = isNullable;
    }
}

public class SimpleTypeExpr : TypeExpr
{
    public Token Token { get; }
    public SimpleType Type { get; }

    public SimpleTypeExpr(Token token, SimpleType type, bool isNullable = false) : base(isNullable)
    {
        Token = token;
        Type = type;
    }

    public override string ToString() => Token.Lexeme.Word + (IsNullable ? "?" : "");
}

public class ListTypeExpr : TypeExpr
{
    public TypeExpr ElementType { get; }

    public ListTypeExpr(TypeExpr elementType, bool isNullable = false) : base(isNullable)
    {
        ElementType = elementType;
    }

    public override string ToString() => $"[{ElementType}]" + (IsNullable ? "?" : "");
}

public class DefinedTypeExpr : TypeExpr
{
    public IdentifierExpr Identifier { get; }

    public DefinedTypeExpr(Token identifier, bool isNullable = false) : base(isNullable)
    {
        Identifier = new IdentifierExpr(identifier);
    }
}

public class TypeParamExpr : TypeExpr
{
    public Token TypeParam { get; }

    public TypeParamExpr(Token typeParam) : base(false)
    {
        TypeParam = typeParam;
    }

    public override string ToString() => TypeParam.Lexeme.Word;
}

public class FunctionTypeExpr : TypeExpr
{
    public IReadOnlyList<TypeExpr> Params { get; }
    public TypeExpr ReturnType { get; }

    public FunctionTypeExpr(IReadOnlyList<TypeExpr> @params, TypeExpr returnType, bool isNullable) : base(isNullable)
    {
        Params = @params;
        ReturnType = returnType;
    }

    public override string ToString() => "(" + (Params.Any() ? Params.JoinStrings(", ") : "()") + 
                                               (ReturnType != null ? $" => {ReturnType}" : "") +
                                               (IsNullable ? "?" : "") + ")";
}
