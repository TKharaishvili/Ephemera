namespace Ephemera.SemanticAnalysis.Typing;

public class NullTypeDescriptor : TypeDescriptor
{
    private NullTypeDescriptor() : base(true)
    {
    }

    public static readonly NullTypeDescriptor Null = new NullTypeDescriptor();

    public override string ToString() => "null";
}