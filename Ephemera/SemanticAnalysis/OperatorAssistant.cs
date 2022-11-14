using Ephemera.Lexing;
using Ephemera.Parsing;
using Ephemera.SemanticAnalysis.Typing;
using System;
using System.Collections.Generic;
using System.Linq;
using static Ephemera.Lexing.TokenClass;

namespace Ephemera.SemanticAnalysis
{
    public class OperatorAssistant
    {
        public class Sig
        {
            public TokenClass Operator { get; }
            public Func<TypeDescriptor, TypeDescriptor, SimpleTypeDescriptor> GetResult { get; }

            public Sig(TokenClass @operator, Func<TypeDescriptor, TypeDescriptor, SimpleTypeDescriptor> getResult)
            {
                Operator = @operator;
                GetResult = getResult;
            }
        }

        static SimpleTypeDescriptor Plus(TypeDescriptor x, TypeDescriptor y)
        {
            var simpleX = x as SimpleTypeDescriptor;
            if (simpleX?.SimpleType == SimpleType.String && !(y is InvalidTypeDescriptor))
            {
                return SimpleTypeDescriptor.String;
            }
            return Arithmetic(simpleX, y as SimpleTypeDescriptor);
        }

        static SimpleTypeDescriptor Arithmetic(TypeDescriptor x, TypeDescriptor y) =>
            Arithmetic(x as SimpleTypeDescriptor, y as SimpleTypeDescriptor);

        static SimpleTypeDescriptor Division(TypeDescriptor x, TypeDescriptor y) =>
            Arithmetic(x as SimpleTypeDescriptor, y as SimpleTypeDescriptor, true);

        static SimpleTypeDescriptor Arithmetic(SimpleTypeDescriptor x, SimpleTypeDescriptor y, bool isNullable = false)
        {
            if (x?.SimpleType == SimpleType.Number && y?.SimpleType == SimpleType.Number)
            {
                return SimpleTypeDescriptor.GetNumber(x.IsNullable || y.IsNullable || isNullable);
            }
            return null;
        }

        static SimpleTypeDescriptor Compare(TypeDescriptor x, TypeDescriptor y)
        {
            var simpleX = x as SimpleTypeDescriptor;
            var simpleY = y as SimpleTypeDescriptor;
            var isValid = (simpleX?.SimpleType == SimpleType.Number || simpleX == CompositeTypeDescriptor.NumberToBoolComposite) &&
                          (simpleY?.SimpleType == SimpleType.Number || simpleY == CompositeTypeDescriptor.NumberToBoolComposite);

            return isValid ? CompositeTypeDescriptor.NumberToBoolComposite : null;
        }

        static SimpleTypeDescriptor Logical(TypeDescriptor x, TypeDescriptor y)
        {
            var simpleX = x as SimpleTypeDescriptor;
            var simpleY = y as SimpleTypeDescriptor;

            var isValid = simpleX?.SimpleType == SimpleType.Bool && simpleY?.SimpleType == SimpleType.Bool;
            return isValid ? SimpleTypeDescriptor.Bool : null;
        }

        static SimpleTypeDescriptor Equals(TypeDescriptor x, TypeDescriptor y)
        {
            if (x == NullTypeDescriptor.Null || y == NullTypeDescriptor.Null)
            {
                return SimpleTypeDescriptor.Bool;
            }

            var simpleX = x as SimpleTypeDescriptor;
            var simpleY = y as SimpleTypeDescriptor;

            if (simpleX != null && simpleY != null)
            {
                return simpleX.SimpleType == simpleY.SimpleType ? SimpleTypeDescriptor.Bool : null;
            }

            return null;
        }

        public static bool IsQuestionQuestionApplicable(TypeDescriptor x, TypeDescriptor y)
        {
            if (!x.IsNullable)
            {
                return false;
            }

            var simpleX = x as SimpleTypeDescriptor;
            var simpleY = y as SimpleTypeDescriptor;
            if (simpleX != null && simpleY != null)
            {
                return simpleX.SimpleType == simpleY.SimpleType;
            }

            return false;
        }

        public static readonly IReadOnlyList<Sig> Sigs = new List<Sig>
        {
            new Sig(PlusOperator, Plus),
            new Sig(MinusOperator, Arithmetic),
            new Sig(TimesOperator, Arithmetic),
            new Sig(DivisionOperator, Division),
            new Sig(PercentOperator, Division),

            new Sig(GreaterOperator, Compare),
            new Sig(GreaterOrEqualsOperator, Compare),
            new Sig(LessOperator, Compare),
            new Sig(LessOrEqualsOperator, Compare),

            new Sig(OrOperator, Logical),
            new Sig(AndOperator, Logical),
            new Sig(EqualsOperator, Equals),
            new Sig(NotEqualsOperator, Equals)
        };

        public static Sig FindSignature(TokenClass @operator)
        {
            return Sigs.FirstOrDefault(s => s.Operator == @operator);
        }
    }
}
