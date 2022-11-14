using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Typing
{
    public class TypeDescriptor
    {
        public bool IsNullable { get; }

        public TypeDescriptor(bool isNullable = false)
        {
            IsNullable = isNullable;
        }

        public virtual TypeDescriptor With(bool isNullable)
        {
            return this;
        }

        public static TypeDescriptor Create(TypeExpr expr)
        {
            var simple = expr as SimpleTypeExpr;
            if (simple != null)
            {
                return SimpleTypeDescriptor.Create(simple);
            }

            var typeParam = expr as TypeParamExpr;
            if (typeParam != null)
            {
                return new TypeParamDescriptor(typeParam.TypeParam.Lexeme.Word);
            }

            var list = expr as ListTypeExpr;
            if (list != null)
            {
                return Create(list);
            }
            return null;
        }

        static ListTypeDescriptor Create(ListTypeExpr expr)
        {
            var elementType = Create(expr.ElementType);
            return elementType != null ? new ListTypeDescriptor(elementType, expr.IsNullable) : null;
        }
    }
}
