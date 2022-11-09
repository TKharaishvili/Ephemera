using CompilerPlayground.Lexing;
using System;

namespace CompilerPlayground.Parsing
{
    public enum SimpleType
    {
        Unit,
        Bool,
        Number,
        String,
    }

    public static class SimpleTypeAssistant
    {
        public static SimpleType ToSimpleType(this TokenClass @class)
        {
            switch (@class)
            {
                case TokenClass.UnitKeyword:
                    return SimpleType.Unit;
                case TokenClass.BoolKeyword:
                    return SimpleType.Bool;
                case TokenClass.NumberKeyword:
                    return SimpleType.Number;
                case TokenClass.StringKeyword:
                    return SimpleType.String;
            }

            throw new NotSupportedException();
        }
    }
}
