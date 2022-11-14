namespace Ephemera.Parsing.Expressions
{
    public class FuncImportExpr : FuncNamedExpr
    {
        public string ImportedFuncName { get; }

        public FuncImportExpr(FuncSignatureExpr signature, string importedFuncName)
            : base(signature)
        {
            ImportedFuncName = importedFuncName;
        }
    }
}
