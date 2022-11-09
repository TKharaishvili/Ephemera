﻿using CompilerPlayground.Parsing;

namespace CompilerPlayground.SemanticAnalysis.Typing
{
    public class CompositeTypeDescriptor : SimpleTypeDescriptor
    {
        private CompositeTypeDescriptor(bool isNullable = false)
            : base(SimpleType.Bool, isNullable)
        {
        }

        public static readonly CompositeTypeDescriptor NumberToBoolComposite = new CompositeTypeDescriptor();
    }
}
