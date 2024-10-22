using Ephemera.Parsing;
using Ephemera.Parsing.Expressions;

namespace Ephemera.SemanticAnalysis.Typing;

public class SimpleTypeDescriptor : TypeDescriptor
{
    public SimpleType SimpleType { get; }

    public SimpleTypeDescriptor(SimpleType simpleType, bool isNullable = false) : base(isNullable)
    {
        SimpleType = simpleType;
    }

    public static SimpleTypeDescriptor Create(SimpleTypeExpr expr) => Create(expr.Type, expr.IsNullable);

    public override TypeDescriptor With(bool isNullable) => Create(SimpleType, isNullable);

    private static SimpleTypeDescriptor Create(SimpleType type, bool isNullable)
    {
        switch (type)
        {
            case SimpleType.Unit:
                return GetUnit(isNullable);
            case SimpleType.Bool:
                return GetBool(isNullable);
            case SimpleType.Number:
                return GetNumber(isNullable);
            case SimpleType.String:
                return GetString(isNullable);
        }
        return null;
    }

    public override string ToString()
    {
        var result = default(string);
        switch (SimpleType)
        {
            case SimpleType.Bool:
                result = "bool";
                break;
            case SimpleType.Number:
                result = "number";
                break;
            case SimpleType.String:
                result = "string";
                break;
        }
        return result != null ? (result + (IsNullable ? "?" : "")) : base.ToString();
    }

    public static readonly SimpleTypeDescriptor Bool = new SimpleTypeDescriptor(SimpleType.Bool);
    public static readonly SimpleTypeDescriptor BoolNullable = new SimpleTypeDescriptor(SimpleType.Bool, true);
    public static SimpleTypeDescriptor GetBool(bool isNullable) => isNullable ? BoolNullable : Bool;

    public static readonly SimpleTypeDescriptor Number = new SimpleTypeDescriptor(SimpleType.Number);
    public static readonly SimpleTypeDescriptor NumberNullable = new SimpleTypeDescriptor(SimpleType.Number, true);
    public static SimpleTypeDescriptor GetNumber(bool isNullable) => isNullable ? NumberNullable : Number;

    public static readonly SimpleTypeDescriptor String = new SimpleTypeDescriptor(SimpleType.String);
    public static readonly SimpleTypeDescriptor StringNullable = new SimpleTypeDescriptor(SimpleType.String, true);
    public static SimpleTypeDescriptor GetString(bool isNullable) => isNullable ? StringNullable : String;

    public static readonly SimpleTypeDescriptor Unit = new SimpleTypeDescriptor(SimpleType.Unit);
    public static readonly SimpleTypeDescriptor UnitNullable = new SimpleTypeDescriptor(SimpleType.Unit, true);
    public static SimpleTypeDescriptor GetUnit(bool isNullable) => isNullable ? UnitNullable : Unit;
}
