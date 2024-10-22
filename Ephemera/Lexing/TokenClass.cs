namespace Ephemera.Lexing;

public enum TokenClass
{
    NumberLiteral,
    StringLiteral,

    //operators
    PlusOperator,
    MinusOperator,
    TimesOperator,
    DivisionOperator,
    PercentOperator,

    AssignmentOperator,

    GreaterOperator,
    GreaterOrEqualsOperator,
    LessOperator,
    LessOrEqualsOperator,
    EqualsOperator,
    NotEqualsOperator,
    NegationOperator,
    AndOperator,
    OrOperator,

    FatArrow,

    DotOperator,
    DotDotOperator,
    QuestionDotOperator,
    QuestionQuestionOperator,
    //operators

    OpenParen,
    CloseParen,
    OpenSquare,
    CloseSquare,
    OpenCurly,
    CloseCurly,

    Colon,
    Comma,
    QuestionMark,

    Identifier,
    TypeParam,

    //keywords
    IfKeyword,
    ElifKeyword,
    ElseKeyword,

    TrueKeyword,
    FalseKeyword,

    DefKeyword,

    UnitKeyword,
    BoolKeyword,
    NumberKeyword,
    StringKeyword,
    NullKeyword,

    WhileKeyword,
    SkipKeyword,
    BreakKeyword,

    FunKeyword,
    PreKeyword,
    ReturnKeyword,
    //keywords

    AttributeStart,
    AttributeEnd,

    //trivia
    Whitespace,
    NewLine,
    //trivia
}