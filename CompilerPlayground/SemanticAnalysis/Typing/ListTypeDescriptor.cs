namespace CompilerPlayground.SemanticAnalysis.Typing
{
    public class ListTypeDescriptor : TypeDescriptor
    {
        public TypeDescriptor ElementType { get; }

        public ListTypeDescriptor(TypeDescriptor elementType, bool isNullable = false) : base(isNullable)
        {
            ElementType = elementType;
        }

        public override TypeDescriptor With(bool isNullable)
        {
            return IsEmpty(this) ? isNullable ? EmptyNullable : Empty : new ListTypeDescriptor(ElementType, isNullable);
        }

        public static readonly ListTypeDescriptor Empty = new ListTypeDescriptor(new TypeParamDescriptor("Empty"));
        static readonly ListTypeDescriptor EmptyNullable = new ListTypeDescriptor(new TypeParamDescriptor("Empty"), true);
        public static bool IsEmpty(TypeDescriptor type) => type == Empty || type == EmptyNullable;

        public override string ToString() => $"[{ElementTypeString}]{(IsNullable ? "?" : "")}";
        private string ElementTypeString => IsEmpty(this) ? "" : ElementType.ToString();
    }
}
