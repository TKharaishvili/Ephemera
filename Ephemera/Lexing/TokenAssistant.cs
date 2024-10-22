using System.Collections.Generic;
using System.Linq;
using static Ephemera.Lexing.TokenClass;

namespace Ephemera.Lexing;

public static class TokenAssistant
{
    public static readonly IReadOnlyList<TokenClass> BinaryOperators =
        new[] { PlusOperator, MinusOperator, TimesOperator, DivisionOperator, PercentOperator,
                GreaterOperator, GreaterOrEqualsOperator, LessOperator, LessOrEqualsOperator, EqualsOperator, NotEqualsOperator,
                AndOperator, OrOperator, QuestionQuestionOperator};

    public static readonly IReadOnlyList<TokenClass> ArithmeticOperators =
        new[] { PlusOperator, MinusOperator, TimesOperator, DivisionOperator, PercentOperator };

    public static readonly IReadOnlyList<TokenClass> NumberBooleanOperators =
        new[] { GreaterOperator, GreaterOrEqualsOperator, LessOperator, LessOrEqualsOperator };

    public static bool IsArithmeticOperator(this TokenClass tokenClass) => ArithmeticOperators.Contains(tokenClass);
    public static bool IsNumberPredicateOperator(this TokenClass tokenClass) => NumberBooleanOperators.Contains(tokenClass);
    public static bool IsEquativeOperator(this TokenClass tokenClass) => tokenClass == EqualsOperator || tokenClass == NotEqualsOperator;
    public static bool IsUnaryOperator(this TokenClass tokenClass) => tokenClass == MinusOperator || tokenClass == NegationOperator;
    public static bool IsBooleanPredicateOperator(this TokenClass tokenClass) => tokenClass == AndOperator || tokenClass == OrOperator;
    public static bool IsBuiltinType(this TokenClass tokenClass) => tokenClass == NumberKeyword || tokenClass == StringKeyword || tokenClass == BoolKeyword;

    public static bool IsSimpleOperand(this Token token) => token != null &&
        (token.Class.IsLiteral() ||
         token.Class == Identifier ||
         token.Class == OpenParen ||
         token.Class == NullKeyword ||
         token.Class == OpenSquare);

    public static bool IsUnaryOperator(this Token token) => token != null &&
        (token.Class == MinusOperator || token.Class == NegationOperator);

    public static bool IsBinaryOperator(this Token token) => token != null && BinaryOperators.Contains(token.Class);

    public static bool IsAccessor(this TokenClass tokenClass) => tokenClass == DotOperator || tokenClass == QuestionDotOperator;

    public static bool IsLiteral(this TokenClass tokenClass) => tokenClass == NumberLiteral || tokenClass == TrueKeyword || tokenClass == FalseKeyword || tokenClass == StringLiteral;

    private static IReadOnlyDictionary<string, TokenClass> KeywordsByString = new Dictionary<string, TokenClass>
    {
        ["if"] = IfKeyword,
        ["elif"] = ElifKeyword,
        ["else"] = ElseKeyword,
        ["true"] = TrueKeyword,
        ["false"] = FalseKeyword,
        ["def"] = DefKeyword,
        ["number"] = NumberKeyword,
        ["string"] = StringKeyword,
        ["bool"] = BoolKeyword,
        ["null"] = NullKeyword,
        ["while"] = WhileKeyword,
        ["skip"] = SkipKeyword,
        ["break"] = BreakKeyword,
        ["unit"] = UnitKeyword,
        ["fun"] = FunKeyword,
        ["pre"] = PreKeyword,
        ["return"] = ReturnKeyword
    };

    private static IReadOnlyDictionary<TokenClass, string> KeywordsByToken = KeywordsByString.ToDictionary(d => d.Value, d => d.Key);

    public static TokenClass? GetKeyword(string word)
    {
        TokenClass value;
        return KeywordsByString.TryGetValue(word, out value) ? value : default(TokenClass?);
    }

    public static string GetKeyword(TokenClass tokenClass)
    {
        string value;
        return KeywordsByToken.TryGetValue(tokenClass, out value) ? value : "";
    }
}