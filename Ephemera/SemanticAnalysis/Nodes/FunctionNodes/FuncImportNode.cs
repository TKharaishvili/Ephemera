using System.Collections.Generic;
using Ephemera.Parsing.Expressions;
using Ephemera.SemanticAnalysis.Typing;
using Ephemera.Lexing;

namespace Ephemera.SemanticAnalysis.Nodes
{
    public record FuncImportNode(FuncImportExpr FuncImportExpr, Token Identifier, IReadOnlyList<DefinitionNode> Params, TypeDescriptor ReturnType, bool IsExtension) : FuncNamedNode(FuncImportExpr, Identifier, Params, ReturnType, IsExtension)
    {
        public string ImportedFuncName { get; } = FuncImportExpr.ImportedFuncName;
    }
}
