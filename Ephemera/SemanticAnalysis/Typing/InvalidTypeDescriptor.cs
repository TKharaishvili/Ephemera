namespace Ephemera.SemanticAnalysis.Typing;

public class InvalidTypeDescriptor : TypeDescriptor
{
    public InvalidTypeDescriptor() { }
    public static readonly InvalidTypeDescriptor Invalid = new InvalidTypeDescriptor();

    public override string ToString() => "Invalid";
}