using Ephemera.Lexing;
using Ephemera.Reusable;
using System.Collections.Generic;

namespace Ephemera.SemanticAnalysis
{
    public class ExecutionAnalyser
    {
        public class Result
        {
            public bool AlwaysReturns { get; }
            public bool HasReturn { get; }
            public bool HasBreak { get; }
            public IReadOnlyList<ReturnNode> ReturnNodes { get; }

            public Result(bool alwaysReturns, bool hasReturn, bool hasBreak, IReadOnlyList<ReturnNode> returnNodes = null)
            {
                AlwaysReturns = alwaysReturns;
                HasReturn = hasReturn;
                HasBreak = hasBreak;
                ReturnNodes = returnNodes;
            }
        }

        public static Result Run(BlockNode block)
        {
            var returnNodes = new ExtList<ReturnNode>();
            var result = Run(block, returnNodes);
            return new Result(result.AlwaysReturns, result.HasReturn, result.HasBreak, returnNodes);
        }

        static Result Run(BlockNode block, ExtList<ReturnNode> returnNodes)
        {
            var alwaysReturns = false;
            var hasReturn = false;
            var hasBreak = false;

            foreach (var node in block.Children)
            {
                var ifNode = node as IfNode;
                if (ifNode != null)
                {
                    var result = Run(ifNode, returnNodes);
                    alwaysReturns = alwaysReturns || result.AlwaysReturns;
                    hasReturn = hasReturn || result.HasReturn;
                    hasBreak = hasBreak || result.HasBreak;
                }

                var whileNode = node as WhileNode;
                if (whileNode != null)
                {
                    var result = Run(whileNode, returnNodes);
                    alwaysReturns = alwaysReturns || result.AlwaysReturns;
                    hasReturn = hasReturn || result.HasReturn;
                    hasBreak = hasBreak || result.HasBreak;
                }

                var returnNode = node as ReturnNode;
                if (returnNode != null)
                {
                    alwaysReturns = true;
                    hasReturn = true;
                    returnNodes += returnNode;
                }

                var breakNode = node as KeywordNode;
                if (breakNode?.KeywordExpr.Keyword.Class == TokenClass.BreakKeyword)
                {
                    hasBreak = true;
                }
            }

            return new Result(alwaysReturns, hasReturn, hasBreak);
        }

        static Result Run(IfNode node, ExtList<ReturnNode> returnNodes)
        {
            var alwaysReturns = default(bool?);
            var hasBreak = false;
            var hasReturn = false;

            var block = node.Block;

            while (true)
            {
                var result = Run(block, returnNodes);
                hasReturn = hasReturn || result.AlwaysReturns;
                hasBreak = hasBreak || result.HasBreak;
                alwaysReturns = alwaysReturns.HasValue ? alwaysReturns.Value && result.AlwaysReturns : result.AlwaysReturns;

                if (node.ElseIfNode == null) break;

                node = node.ElseIfNode;
                block = node.Block;
            }

            if (node.ElseBlock == null)
            {
                alwaysReturns = false;
            }
            else
            {
                var result = Run(node.ElseBlock, returnNodes);
                hasReturn = hasReturn || result.AlwaysReturns;
                hasBreak = hasBreak || result.HasBreak;
                alwaysReturns = alwaysReturns.HasValue ? alwaysReturns.Value && result.AlwaysReturns : result.AlwaysReturns;
            }

            return new Result(alwaysReturns ?? false, hasReturn, hasBreak);
        }

        static Result Run(WhileNode node, ExtList<ReturnNode> returnNodes)
        {
            var result = Run(node.Block, returnNodes);
            return new Result(result.HasReturn && !result.HasBreak, result.HasReturn, result.HasBreak);
        }
    }
}