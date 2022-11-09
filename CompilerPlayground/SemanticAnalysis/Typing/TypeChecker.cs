using System.Collections.Generic;
using System.Linq;

namespace CompilerPlayground.SemanticAnalysis.Typing
{
    public static class TypeChecker
    {
        public static TypeDescriptor ClosestSupertype(this IEnumerable<TypeDescriptor> types) =>
            types.Any() ? types.Aggregate(ClosestSupertype) : null;

        public static TypeDescriptor ClosestSupertype(TypeDescriptor x, TypeDescriptor y)
        {
            if (x == null || y == null) return null;

            if (x == NullTypeDescriptor.Null) return y.With(true);

            if (y == NullTypeDescriptor.Null) return x.With(true);

            var simpleX = x as SimpleTypeDescriptor;
            var simpleY = y as SimpleTypeDescriptor;
            if (simpleX != null && simpleY != null)
            {
                return ClosestSupertype(simpleX, simpleY);
            }

            var listX = x as ListTypeDescriptor;
            var listY = y as ListTypeDescriptor;
            if (listX != null && listY != null)
            {
                return ClosestSupertype(listX, listY);
            }

            return null;
        }

        private static TypeDescriptor ClosestSupertype(SimpleTypeDescriptor x, SimpleTypeDescriptor y)
        {
            return x.SimpleType == y.SimpleType ? x.With(x.IsNullable || y.IsNullable) : null;
        }

        private static TypeDescriptor ClosestSupertype(ListTypeDescriptor x, ListTypeDescriptor y)
        {
            if (x.IsEmptyList()) return y.With(x.IsNullable || y.IsNullable);
            if (y.IsEmptyList()) return x.With(x.IsNullable || y.IsNullable);

            var elementType = ClosestSupertype(x.ElementType, y.ElementType);
            if (elementType == null) return null;
            return new ListTypeDescriptor(elementType, x.IsNullable || y.IsNullable);
        }

        static bool AssignmentInvalid(TypeDescriptor x, TypeDescriptor y)
        {
            return x == NullTypeDescriptor.Null ||
                   x == InvalidTypeDescriptor.Invalid ||
                   y == InvalidTypeDescriptor.Invalid ||
                   x.IsEmptyList();
        }

        public static bool IsAssignable(TypeDescriptor x, TypeDescriptor y)
        {
            if (AssignmentInvalid(x, y)) return false;

            if (x.IsNullable && y == NullTypeDescriptor.Null) return true;

            var typeParamX = x as TypeParamDescriptor;
            if (typeParamX != null && y != NullTypeDescriptor.Null) return true;

            if (!x.IsNullable && y.IsNullable) return false;

            var simpleX = x as SimpleTypeDescriptor;
            var simpleY = y as SimpleTypeDescriptor;
            if (simpleX != null && simpleY != null)
            {
                return simpleX.SimpleType == simpleY.SimpleType;
            }

            var listX = x as ListTypeDescriptor;
            var listY = y as ListTypeDescriptor;
            if (listX != null && listY != null)
            {
                return IsAssignable(listX, listY);
            }

            return false;
        }

        static bool IsAssignable(ListTypeDescriptor x, ListTypeDescriptor y)
        {
            while (true)
            {
                if (y.IsEmptyList()) return true;

                var elementX = x.ElementType;
                var elementY = y.ElementType;
                if (elementX.IsNullable && elementY == NullTypeDescriptor.Null) return true;

                var typeParamX = elementX as TypeParamDescriptor;
                if (typeParamX != null) return true;

                var simpleX = elementX as SimpleTypeDescriptor;
                var simpleY = elementY as SimpleTypeDescriptor;
                if (simpleX != null && simpleY != null)
                {
                    return simpleX.SimpleType == simpleY.SimpleType && simpleX.IsNullable == simpleY.IsNullable;
                }

                x = elementX as ListTypeDescriptor;
                y = elementY as ListTypeDescriptor;
                if (x != null && y != null)
                {
                    if (x.IsNullable != y.IsNullable) return false;
                    continue;
                }

                return false;
            }
        }
    }
}
