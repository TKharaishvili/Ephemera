﻿using CompilerPlayground.Parsing.Expressions;
using CompilerPlayground.SemanticAnalysis.Typing;

namespace CompilerPlayground.SemanticAnalysis.Nodes
{
    public class QuestionQuestionOperationNode : OperandNode
    {
        public OperandNode Left { get; }
        public OperandNode Right { get; }

        public QuestionQuestionOperationNode(Expr expr, TypeDescriptor type, OperandNode left, OperandNode right)
            : base(expr, type)
        {
            Left = left;
            Right = right;
        }
    }
}
