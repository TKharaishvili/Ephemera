using System.Collections.Generic;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record FuncInvocationNode(FuncInvocationExpr FuncInvocationExpr, TypeDescriptor TypeDescriptor, IReadOnlyList<OperandNode> Params, OperandNode PreParam, bool IsConditionalInvocation, FuncNamedNode NamedFunc = null) : OperandNode(FuncInvocationExpr, TypeDescriptor)
    {
        public string Name { get; } = FuncInvocationExpr.Identifier.Word;

        //TODO: the implementation of this prop was changed without testing
        //do make sure the previous and current implementations work the same
        //preferrably with a unit test
        public bool HasConditionalInvocation
        {
            get
            {
                return IsConditionalInvocation ||
                    PreParam is FuncInvocationNode { HasConditionalInvocation: true };
            }
        }

        public FuncInvocationNode With(TypeDescriptor typeDescriptor)
        {
            return new FuncInvocationNode(FuncInvocationExpr, typeDescriptor, Params, PreParam, IsConditionalInvocation, NamedFunc);
        }
    }
}
