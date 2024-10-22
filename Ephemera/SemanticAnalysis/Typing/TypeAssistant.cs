using Ephemera.Parsing;
using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Typing;

public static class TypeAssistant
{
    public static bool IsUnit(this TypeDescriptor type)
    {
        var simple = type as SimpleTypeDescriptor;
        return simple?.SimpleType == SimpleType.Unit;
    }

    public static bool IsEmptyList(this TypeDescriptor type) => ListTypeDescriptor.IsEmpty(type);

    public static bool NotIdentified(this TypeDescriptor type)
    {
        while (true)
        {
            if (type == NullTypeDescriptor.Null || type.IsEmptyList())
            {
                return true;
            }

            var list = type as ListTypeDescriptor;
            if (list != null)
            {
                type = list.ElementType;
                continue;
            }
            return false;
        }
    }

    public static TypeDescriptor ToTypeDescriptor(this TypeExpr expr) => TypeDescriptor.Create(expr);

    public static bool IsGeneric(this TypeDescriptor type)
    {
        while (true)
        {
            if (type is TypeParamDescriptor) return true;

            var list = type as ListTypeDescriptor;
            if (list != null)
            {
                type = list.ElementType;
                continue;
            }
            return false;
        }
    }
}
