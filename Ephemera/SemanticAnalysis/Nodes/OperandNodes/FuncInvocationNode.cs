using System.Collections.Generic;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public class FuncInvocationNode : OperandNode
    {
        public string Name { get; }
        public IReadOnlyList<OperandNode> Params { get; }
        public OperandNode PreParam { get; }
        public bool IsConditionalInvocation { get; }
        public FuncNamedNode NamedFunc { get; }
        public bool HasConditionalInvocation { get; }
        public FuncInvocationExpr FuncInvocationExpr { get; }

        public FuncInvocationNode(FuncInvocationExpr expr, TypeDescriptor typeDescriptor, IReadOnlyList<OperandNode> @params, OperandNode preParam, bool isConditionalInvocation, FuncNamedNode namedFunc = null)
            : base(expr, typeDescriptor)
        {
            Name = expr.Identifier.Word;
            Params = @params;
            PreParam = preParam;
            IsConditionalInvocation = isConditionalInvocation;
            NamedFunc = namedFunc;

            var invocation = preParam as FuncInvocationNode;
            HasConditionalInvocation = isConditionalInvocation || invocation?.HasConditionalInvocation == true;

            FuncInvocationExpr = expr;
        }

        public FuncInvocationNode With(TypeDescriptor typeDescriptor)
        {
            return new FuncInvocationNode(FuncInvocationExpr, typeDescriptor, Params, PreParam, IsConditionalInvocation, NamedFunc);
        }
    }
}
