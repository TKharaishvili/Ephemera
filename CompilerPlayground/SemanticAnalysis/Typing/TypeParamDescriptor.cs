namespace CompilerPlayground.SemanticAnalysis.Typing
{
    public class TypeParamDescriptor : TypeDescriptor
    {
        public string Name { get; }

        public TypeParamDescriptor(string name)
        {
            Name = name;
        }
    }
}
