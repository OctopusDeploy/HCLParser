using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Sprache;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     A Sprache parser for the HCL library.
    ///     The goal of this parser is to have no false negatives. Every valid HCL file should be
    ///     parsed by the Parsers in this class.
    ///     It it very likely that these parsers will parse templates that are not valid HCL. This
    ///     is OK though, as the terraform exe will ultimately be the source of truth.
    /// </summary>
    public class HclParser
    {
        private const string StringKeyword = "string";
        private const string BoolKeyword = "bool";
        private const string NumberKeyword = "number";
        private const string AnyKeyword = "any";

        /// <summary>
        ///     The \n char
        /// </summary>
        public const char LineBreak = (char)10;

        /// <summary>
        ///     New in 0.12 - the ability to mark a block as dynamic
        /// </summary>
        public static readonly Parser<string> Dynamic =
            Parse.String("dynamic").Text().Named("Dynamic configuration block");

        /// <summary>
        ///     A regex used to normalize line endings
        /// </summary>
        public static readonly Regex LineEndNormalize = new Regex("\r\n?|\n");

        /// <summary>
        ///     A regex that matches the numbers that can be assigned to a property
        ///     matches floats, scientific and hex numbers
        /// </summary>
        public static readonly Regex NumberRegex = new Regex(@"-?(0x)?\d+(e(\+|-)?\d+)?(\.\d*(e(\+|-)?\d+)?)?");

        /// <summary>
        ///     A regex that matches true and false
        /// </summary>
        public static readonly Regex TrueFalse = new Regex(@"true|false", RegexOptions.IgnoreCase);

        /// <summary>
        ///     Represents the equals token
        /// </summary>
        public static readonly Parser<char> Equal = Parse.Char('=').WithWhiteSpace();

        /// <summary>
        ///     Represents the colon token. This is new in 0.12 as a way of defining maps.
        /// </summary>
        public static readonly Parser<char> Colon = Parse.Char(':').WithWhiteSpace();

        /// <summary>
        ///     An equals sign or colon can be used for assigment in maps.
        /// </summary>
        public static readonly Parser<char> EqualsOrColon = Equal.Or(Colon);

        /// <summary>
        ///     Open bracket
        /// </summary>
        public static readonly Parser<char> LeftBracket = Parse.Char('(').Token();

        /// <summary>
        ///     Close bracket
        /// </summary>
        public static readonly Parser<char> RightBracket = Parse.Char(')').Token();

        /// <summary>
        ///     Array start token
        /// </summary>
        public static readonly Parser<char> LeftSquareBracket = Parse.Char('[').Token();

        /// <summary>
        ///     Array end token
        /// </summary>
        public static readonly Parser<char> RightSquareBracket = Parse.Char(']').Token();

        /// <summary>
        ///     Object start token
        /// </summary>
        public static readonly Parser<char> LeftCurly = Parse.Char('{').Token();

        /// <summary>
        ///     Object end token
        /// </summary>
        public static readonly Parser<char> RightCurly = Parse.Char('}').Token();

        /// <summary>
        ///     Comma token
        /// </summary>
        public static readonly Parser<char> Comma = Parse.Char(',').Token();

        /// <summary>
        ///     An escaped interpolation curly
        /// </summary>
        public static readonly Parser<string> EscapedDelimiterStartCurly =
            Parse.String("{{").Text().Named("Escaped delimiter");

        /// <summary>
        ///     An escaped interpolation curly
        /// </summary>
        public static readonly Parser<string> EscapedDelimiterEndCurly =
            Parse.String("}}").Text().Named("Escaped delimiter");

        /// <summary>
        ///     The start of an interpolation marker
        /// </summary>
        public static readonly Parser<string> DelimiterStartInterpolated =
            Parse.String("${").Text().Named("Start Interpolation");

        /// <summary>
        ///     Special interpolation char
        /// </summary>
        public static readonly Parser<char> DelimiterStartCurly = Parse.Char('{').Named("StartCurly");

        /// <summary>
        ///     Special interpolation char
        /// </summary>
        public static readonly Parser<char> DelimiterEndCurly = Parse.Char('}').Named("EndCurly");

        /// <summary>
        ///     Special interpolation char
        /// </summary>
        public static readonly Parser<char> DelimiterStartSquare = Parse.Char('[').Named("StartSquare");

        /// <summary>
        ///     Special interpolation char
        /// </summary>
        public static readonly Parser<char> DelimiterEndSquare = Parse.Char(']').Named("EndSquare");

        /// <summary>
        ///     Escaped quote
        /// </summary>
        public static readonly Parser<string> EscapedDelimiterQuote =
            Parse.String("\\\"").Text().Named("Escaped delimiter");

        /// <summary>
        ///     Escape char
        /// </summary>
        public static readonly Parser<string> SingleEscapeQuote =
            Parse.String("\\").Text().Named("Single escape character");

        /// <summary>
        ///     Double escape
        /// </summary>
        public static readonly Parser<string> DoubleEscapeQuote =
            Parse.String("\\\\").Text().Named("Escaped escape character");

        /// <summary>
        ///     Quote char
        /// </summary>
        public static readonly Parser<char> DelimiterQuote = Parse.Char('"').Named("Delimiter");

        /// <summary>
        ///     Start of interpolation
        /// </summary>
        public static readonly Parser<char> DelimiterInterpolation = Parse.Char('$').Named("Interpolated");

        /// <summary>
        ///     An escaped interpolation start
        /// </summary>
        public static readonly Parser<string> EscapedDelimiterInterpolation =
            Parse.Char('$').Repeat(2).Text().Named("Escaped Interpolated");

        /// <summary>
        ///     An escaped interpolation start
        /// </summary>
        public static readonly Parser<string> DoubleEscapedDelimiterInterpolation =
            Parse.Char('$').Repeat(4).Text().Named("Escaped Interpolated");

        /// <summary>
        ///     A section of a string that does not have any special interpolation tokens
        /// </summary>
        public static readonly Parser<string> SimpleLiteralCurly =
            Parse.AnyChar
                .Except(EscapedDelimiterStartCurly)
                .Except(DelimiterStartInterpolated)
                .Except(DelimiterEndCurly)
                .Many().Text().Named("Literal without escape/delimiter character");

        /// <summary>
        ///     A string made up of regular text and interpolation string
        /// </summary>
        public static readonly Parser<string> StringLiteralCurly =
            from start in DelimiterStartInterpolated
            from v in StringLiteralCurly
                .Or(EscapedDelimiterStartCurly)
                .Or(EscapedDelimiterEndCurly)
                .Or(SimpleLiteralCurly).Many()
            from end in DelimiterEndCurly
            select start + string.Concat(v) + end;

        /// <summary>
        ///     Any characters that are not escaped.
        /// </summary>
        public static readonly Parser<string> SimpleLiteralQuote = Parse.AnyChar
            .Except(SingleEscapeQuote)
            .Except(DelimiterQuote)
            .Except(EscapedDelimiterStartCurly)
            .Except(DelimiterStartInterpolated)
            .Many().Text().Named("Literal without escape/delimiter character");

        /// <summary>
        ///     Matches the plain text in a string, or the Interpolation block
        /// </summary>
        public static readonly Parser<string> StringLiteralQuoteContent =
            from curly in StringLiteralCurly.Optional()
            from content in EscapedDelimiterQuote
                .Or(DoubleEscapeQuote)
                .Or(SingleEscapeQuote)
                .Or(EscapedDelimiterInterpolation)
                .Or(DoubleEscapedDelimiterInterpolation)
                .Or(EscapedDelimiterStartCurly)
                .Or(EscapedDelimiterEndCurly)
                .Or(SimpleLiteralQuote).Many()
            select curly.GetOrDefault() + Regex.Unescape(string.Concat(content));


        /// <summary>
        ///     Matches the plain text in a string, or the Interpolation block
        /// </summary>
        public static readonly Parser<string> StringLiteralQuoteContentReverse =
            from combined in (
                from curly in StringLiteralCurly.Optional()
                from content in
                    EscapedDelimiterInterpolation
                        .Or(Parse.AnyChar.Except(DelimiterStartInterpolated).Many().Text())
                select curly.GetOrDefault() + EscapeString(content)).Many()
            select string.Concat(combined);

        /// <summary>
        ///     Matches the plain text in a string, or the Interpolation block
        /// </summary>
        public static readonly Parser<string> StringLiteralQuoteContentNoInterpolation =
            from content in StringLiteralCurly
                .Or(EscapedDelimiterQuote)
                .Or(DoubleEscapeQuote)
                .Or(SingleEscapeQuote)
                .Or(EscapedDelimiterInterpolation)
                .Or(DoubleEscapedDelimiterInterpolation)
                .Or(SimpleLiteralQuote).Many()
            select string.Concat(content);

        /// <summary>
        ///     Represents a multiline comment e.g.
        ///     /*
        ///     Some text goes here
        ///     */
        /// </summary>
        public static readonly Parser<HclElement> MultilineComment =
            (from open in Parse.String("/*")
                from content in Parse.AnyChar.Except(Parse.String("*/"))
                    .Or(Parse.Char(LineBreak))
                    .Many().Text()
                from last in Parse.String("*/")
                select new HclMultiLineCommentElement { Value = content }).Token().Named("Multiline Comment");

        /// <summary><![CDATA[
        /// Represents a HereDoc e.g.
        ///
        /// <<EOF
        /// Some Text
        /// Goes here
        /// EOF
        ///
        /// or
        ///
        /// <<-EOF
        ///   Some Text
        ///   Goes here
        /// EOF
        /// ]]></summary>
        public static readonly Parser<Tuple<string, bool, string>> HereDoc =
            (from open in Parse.Char('<').Repeat(2).Text()
                from indentMarker in Parse.Char('-').Optional()
                from marker in Parse.AnyChar.Except(Parse.Char(LineBreak)).Many().Text()
                from lineBreak in Parse.Char(LineBreak)
                from rest in Parse.AnyChar.Except(Parse.String(marker))
                    .Or(Parse.Char(LineBreak))
                    .Many().Text()
                from last in Parse.String(marker)
                select Tuple.Create(marker, indentMarker.IsDefined, lineBreak + rest)).Token();

        /// <summary>
        ///     Represents the "//" used to start a comment
        /// </summary>
        public static readonly Parser<IEnumerable<char>> ForwardSlashCommentStart =
            from open in Parse.Char('/').Repeat(2)
            select open;

        /// <summary>
        ///     Represents the "#" used to start a comment
        /// </summary>
        public static readonly Parser<IEnumerable<char>> HashCommentStart =
            from open in Parse.Char('#').Once()
            select open;

        /// <summary>
        ///     Represents a single line comment
        /// </summary>
        public static readonly Parser<HclElement> SingleLineComment =
        (
            from open in ForwardSlashCommentStart.Or(HashCommentStart)
            from content in Parse.AnyChar.Except(Parse.Char(LineBreak)).Many().Text().Optional()
            select new HclCommentElement { Value = content.GetOrDefault() }
        ).Token().Named("Single line comment");

        public static readonly Parser<string> IdentifierPlain =
            from value in Parse.Regex(@"(\d|\w|[_\-.])+").Text().Token()
            select value;

        /// <summary>
        ///     Identifiers can be wrapped in quotes to indicate their names are variables. New in 0.12
        /// </summary>
        public static readonly Parser<string> IdentifierVariable =
            from value in Parse.Regex(@"\((\d|\w|[_\-.])+\)").Text().Token()
            select value;

        public static readonly Parser<string> Identifier =
            from value in IdentifierPlain.Or(IdentifierVariable)
            select value;

        /// <summary>
        ///     Represents an indexer in an unquoted string. e.g. a = myvar[b]
        ///     This is lenient, consuming everything between balanced square brackets.
        /// </summary>
        public static readonly Parser<string> ListOrIndexText =
            from open in Parse.Char('[')
            from content in
                StringLiteralQuoteUnTokenised
                    .Or(Parse.AnyChar
                        .Except(Parse.Char('['))
                        .Except(Parse.Char(']'))
                        .Except(Parse.Char('"'))
                        .Except(Parse.Char('\''))
                        .Many().Text())
                    .Or(ListOrIndexText)
                    .Many()
                    .Optional()
            from close in Parse.Char(']')
            select open + string.Join(string.Empty, content.GetOrDefault() ?? Enumerable.Empty<string>()) + close;

        public static readonly Parser<string> GroupText =
            from open in Parse.Char('(').Token()
            from content in
                LogicSymbol
                    .Or(StringLiteralQuoteUnTokenised)
                    .Or(Parse.AnyChar
                        .Except(Parse.Char('('))
                        .Except(Parse.Char(')'))
                        .Except(Parse.Char('"'))
                        .Except(Parse.Char('\''))
                        .Many().Text())
                    .Or(GroupText)
                    .Many()
                    .Optional()
            from close in Parse.Char(')')
            select open + string.Join(string.Empty, content.GetOrDefault() ?? Enumerable.Empty<string>()) + close;

        public static readonly Parser<string> CurlyGroupText =
            from open in Parse.Char('{').Token()
            from content in
                StringLiteralQuoteUnTokenised
                    .Or(Parse.AnyChar
                        .Except(Parse.Char('{'))
                        .Except(Parse.Char('}'))
                        .Except(Parse.Char('"'))
                        .Except(Parse.Char('\''))
                        .Many().Text())
                    .Or(CurlyGroupText)
                    .Many()
                    .Optional()
            from close in Parse.Char('}')
            select open + string.Join(string.Empty, content.GetOrDefault() ?? Enumerable.Empty<string>()) + close;

        /// <summary>
        ///     Math symbols. These are used to indicate places in unquoted values where line breaks can be placed.
        ///     New in 0.12
        /// </summary>
        public static readonly Parser<string> LogicSymbol =
            from mathOperator in
                Parse.String("*")
                    .Or(Parse.String("/"))
                    .Or(Parse.String("%"))
                    .Or(Parse.String("+"))
                    .Or(Parse.String("-"))
                    .Or(Parse.String(">="))
                    .Or(Parse.String("<="))
                    .Or(Parse.String("<"))
                    .Or(Parse.String(">"))
                    .Or(Parse.String("!="))
                    .Or(Parse.String("=="))
                    .Or(Parse.String("&&"))
                    .Or(Parse.String("||"))
                    .Or(Parse.String("?"))
                    .Or(Parse.String(":"))
                    .Or(Parse.String("="))
                    .Text()
                    .RequiredToken()
            select " " + mathOperator + " ";

        /// <summary>
        ///     Match quoted string content, and include the quotes in the result. This is used to match quoted strings
        ///     in a larger unquoted property value.
        /// </summary>
        public static readonly Parser<string> StringLiteralQuoteUnTokenised =
            from start in DelimiterQuote
            from content in StringLiteralQuoteContent.Many().Optional()
            from end in DelimiterQuote
            select "\"" + string.Concat(content.GetOrDefault()) + "\"";

        /// <summary>
        ///     Match quoted string content, and include the quotes in the result. This is used to build up string values,
        ///     so it is untokenized.
        /// </summary>
        public static readonly Parser<string> StringLiteralQuoteUnTokenisedUnQuoted =
            from start in DelimiterQuote
            from content in StringLiteralQuoteContent.Many().Optional()
            from end in DelimiterQuote
            select string.Concat(content.GetOrDefault());

        /// <summary>
        ///     Matches multiple StringLiteralQuoteContent to make up the string. This is used to match block identifiers,
        ///     and so is a token.
        /// </summary>
        public static readonly Parser<string> StringLiteralQuote =
            StringLiteralQuoteUnTokenisedUnQuoted.Token();

        /// <summary>
        ///     Matches an unquoted value. This is a very generalised parser designed to capture fields that can be
        ///     simple expressions like:
        ///     var.vpc_cidr_block
        ///     Complex expressions spanning multiple lines like:
        ///     a == b
        ///     ? c
        ///     : d
        ///     Mixtures of quoted and unquoted strings:
        ///     "a" == b ? "ccc" : d + 1
        ///     Inline lists or objects:
        ///     "a" == b ? [var.vpc_cidr_block] : {var = "hi there"}
        ///     For loops:
        ///     [
        ///     for key, value in module.bootstrap.assets_dist :
        ///     format("##### %s\n%s", key, value)
        ///     ]
        ///     This parser does not attempt to extract any individual elements out of the value (i.e. we are not building
        ///     a calculator here).
        ///     New in 0.12
        /// </summary>
        public static readonly Parser<HclElement> UnquotedContent =
            /*
             * An unquoted string must begin with any character expect for a quote
             * (which would make it a quoted string), a less than (which would make it a HereDoc),
             * hash (which would make it a comment), or whitespace (which is not significant at the
             * start of the string).
             *
             * We can start with parentheses, curly brackets and square brackets. These catch
             * math grouping, and for loops that build up lists or objects.
             */
            from start in Parse.AnyChar
                .Except(Parse.Char('['))
                .Except(Parse.Char(']'))
                .Except(Parse.Char('{'))
                .Except(Parse.Char('}'))
                .Except(Parse.Char('('))
                .Except(Parse.Char(')'))
                .Except(Parse.Char('<'))
                .Except(Parse.Char('#'))
                .Except(Parse.Char('"'))
                .Except(Parse.Char('\''))
                .Except(Parse.WhiteSpace)
                .Once().Text()
                .Or(GroupText)
                .Or(ListOrIndexText)
                .Or(CurlyGroupText)
                .Or(StringLiteralQuoteUnTokenised)
            /*
             * Once we enter an unquoted string, we need to understand where the content ends.
             * We assume any opening bracket will have a matching closing bracket, and consume everything (line breaks
             * included) between them. We also assume that any math symbol can have the right hand side on a new line.
             *
             * We also don't consume commas, which only make sense inside a list.
             *
             * However most of these excluded chars can be included in a quoted string via the StringLiteralQuoteUnTokenised
             * parser.
             */
            from content in
                ListOrIndexText
                    .Or(CurlyGroupText)
                    .Or(GroupText)
                    .Or(LogicSymbol)
                    .Or(StringLiteralQuoteUnTokenised)
                    .Or(Parse.AnyChar
                        .Except(Parse.Char('['))
                        .Except(Parse.Char(']'))
                        .Except(Parse.Char('{'))
                        .Except(Parse.Char('}'))
                        .Except(Parse.Char('('))
                        .Except(Parse.Char(')'))
                        .Except(Parse.Char(','))
                        .Except(Parse.Char('"'))
                        .Except(LogicSymbol)
                        .Except(Parse.LineEnd)
                        .Many()
                        .Text())
                    .Many()
                    .Optional()
            select new HclUnquotedExpressionElement
            {
                Value = start + string.Join(string.Empty, content.GetOrDefault() ?? Enumerable.Empty<string>())
            };

        /// <summary>
        ///     Represents the various values that can be assigned to properties
        ///     i.e. quoted text, numbers and booleans
        /// </summary>
        public static readonly Parser<HclElement> PropertyValue =
            (from value in (from str in StringLiteralQuoteUnTokenisedUnQuoted.WithWhiteSpace()
                        select new HclStringElement { Value = str } as HclElement)
                    .Or(from number in Parse.Regex(NumberRegex).WithWhiteSpace()
                        select new HclNumOrBoolElement { Value = number })
                    .Or(from boolean in Parse.Regex(TrueFalse).WithWhiteSpace()
                        select new HclNumOrBoolElement { Value = boolean })
                // A simple property ends at the end of the line, the end of the file, a comment, comma, end brackets, or comments
                // Note that we don't consume delimiters like colons, brackets or comment starts
                from endOfLine in Parse.LineTerminator.Or(Parse.Regex(@"[#{}\[\],]|//|/\*")).PreviewRequired()
                select value
            ).Token();

        /// <summary>
        ///     New in 0.12 - An primitive definition without quotes
        /// </summary>
        public static readonly Parser<HclElement> UnquotedPrimitiveTypeProperty =
            (from value in Parse.String(StringKeyword)
                    .Or(Parse.String(NumberKeyword))
                    .Or(Parse.String(BoolKeyword))
                    .Or(Parse.String(AnyKeyword))
                    .Text()
                select new HclPrimitiveTypeElement { Value = value }).Token();

        /// <summary>
        ///     New in 0.12 - An primitive definition with quotes
        /// </summary>
        public static readonly Parser<HclElement> QuotedPrimitiveTypeProperty =
            (from startQuote in DelimiterQuote
                from value in Parse.String(StringKeyword)
                    .Or(Parse.String(NumberKeyword))
                    .Or(Parse.String(BoolKeyword))
                    .Or(Parse.String(AnyKeyword))
                    .Text()
                from endQuote in DelimiterQuote
                select new HclPrimitiveTypeElement { Value = value }).Token();

        /// <summary>
        ///     New in 0.12 - An primitive definition
        /// </summary>
        public static readonly Parser<HclElement> PrimitiveTypeProperty =
            UnquotedPrimitiveTypeProperty
                .Or(QuotedPrimitiveTypeProperty);

        /// <summary>
        ///     New in 0.12 - An object definition. Todo: Add comment elements.
        /// </summary>
        public static readonly Parser<HclElement> ObjectTypeProperty =
            (from objectType in Parse.String("object(").Token()
                from openCurly in LeftCurly
                from content in
                (
                    from value in ElementTypedObjectProperty
                        .Or(PrimitiveTypeObjectProperty)
                    from comma in Comma.Optional()
                    select value
                ).Token().Many()
                from closeCurly in RightCurly
                from closeBracket in RightBracket
                select new HclObjectTypeElement { Children = content }).Token();

        /// <summary>
        ///     New in 0.12 - An set definition
        /// </summary>
        public static readonly Parser<HclElement> SetTypeProperty =
            (from objectType in Parse.String("set(").Token()
                from value in MapTypeProperty
                    .Or(ObjectTypeProperty)
                    .Or(ListTypeProperty)
                    .Or(SetTypeProperty)
                    .Or(TupleTypeProperty)
                    .Or(PrimitiveTypeProperty)
                from closeBracket in RightBracket
                select new HclSetTypeElement { Child = value }).Token();

        /// <summary>
        ///     New in 0.12 - An list definition
        /// </summary>
        public static readonly Parser<HclElement> ListTypeProperty =
            (from objectType in Parse.String("list(").Token()
                from value in MapTypeProperty
                    .Or(ObjectTypeProperty)
                    .Or(ListTypeProperty)
                    .Or(SetTypeProperty)
                    .Or(TupleTypeProperty)
                    .Or(PrimitiveTypeProperty)
                from closeBracket in RightBracket
                select new HclListTypeElement { Child = value }).Token();

        /// <summary>
        ///     New in 0.12 - An tuple definition.
        /// </summary>
        public static readonly Parser<HclElement> TupleTypeProperty =
            (from objectType in Parse.String("tuple(").Token()
                from openSquare in LeftSquareBracket
                from content in
                (
                    from value in MapTypeProperty
                        .Or(ObjectTypeProperty)
                        .Or(ListTypeProperty)
                        .Or(SetTypeProperty)
                        .Or(TupleTypeProperty)
                        .Or(PrimitiveTypeProperty)
                    from comma in Comma.Optional()
                    select value
                ).Token().Many()
                from closeSquare in RightSquareBracket
                from closeBracket in RightBracket
                select new HclTupleTypeElement { Children = content }).Token();

        /// <summary>
        ///     New in 0.12 - An map definition
        /// </summary>
        public static readonly Parser<HclElement> MapTypeProperty =
            (from objectType in Parse.String("map(").Token()
                from value in MapTypeProperty
                    .Or(ObjectTypeProperty)
                    .Or(ListTypeProperty)
                    .Or(SetTypeProperty)
                    .Or(TupleTypeProperty)
                    .Or(PrimitiveTypeProperty)
                from closeBracket in RightBracket
                select new HclMapTypeElement { Child = value }).Token();

        /// <summary>
        ///     The value of an individual item in a list
        /// </summary>
        public static readonly Parser<HclElement> LiteralListValue =
            from value in PropertyValue
                .Or(UnquotedContent)
            select value;

        /// <summary>
        ///     The value of an individual heredoc item in a list
        /// </summary>
        public static readonly Parser<HclElement> HereDocListValue =
            from hereDoc in HereDoc
            select new HclHereDocElement
            {
                Marker = hereDoc.Item1,
                Trimmed = hereDoc.Item2,
                Value = hereDoc.Item3
            };

        /// <summary>
        ///     Represents the contents of a map/object
        /// </summary>
        public static readonly Parser<HclElement> MapValue =
        (
            from lbracket in LeftCurly
            from content in Properties.Optional()
            from rbracket in RightCurly
            select new HclMapElement { Children = content.GetOrDefault() }
        ).Token();

        /// <summary>
        ///     Represents a list/tuple/set. Lists can be embedded.
        /// </summary>
        public static readonly Parser<HclElement> ListValue =
        (
            from open in LeftSquareBracket
            from content in
            (
                from embeddedValues in ListValue
                    .Or(MapValue)
                    .Or(LiteralListValue)
                    .Or(HereDocListValue)
                    .Or(SingleLineComment)
                    .Or(MultilineComment)
                from comma in Comma.Optional()
                select embeddedValues
            ).Token().Many()
            from close in RightSquareBracket
            select new HclListElement { Children = content }
        ).Token();

        /// <summary>
        ///     Represents a value that can be assigned to a property.
        ///     Note equals or colons used to separated keys from values:
        ///     https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#collection-values
        /// </summary>
        public static readonly Parser<HclElement> UnquotedNameUnquotedElementProperty =
            from name in Identifier
            from eql in EqualsOrColon
            from value in UnquotedContent
            select new HclUnquotedExpressionPropertyElement { Name = name, Child = value, NameQuoted = false };

        /// <summary>
        ///     Represents a value that can be assigned to a property
        ///     Note equals or colons used to separated keys from values:
        ///     https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#collection-values
        /// </summary>
        public static readonly Parser<HclElement> QuotedNameUnquotedElementProperty =
            from name in StringLiteralQuote
            from eql in EqualsOrColon
            from value in UnquotedContent
            select new HclUnquotedExpressionPropertyElement { Name = name, Child = value, NameQuoted = true };

        /// <summary>
        ///     Represents a value that can be assigned to a property
        ///     Note equals or colons used to separated keys from values:
        ///     https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#collection-values
        /// </summary>
        public static readonly Parser<HclElement> ElementProperty =
            from name in Identifier
            from eql in EqualsOrColon
            from value in PropertyValue
            select new HclSimplePropertyElement { Name = name, Child = value, NameQuoted = false };

        /// <summary>
        ///     Represents a value that can be assigned to a property
        ///     Note equals or colons used to separated keys from values:
        ///     https://github.com/hashicorp/hcl/blob/hcl2/hclsyntax/spec.md#collection-values
        /// </summary>
        public static readonly Parser<HclElement> QuotedElementProperty =
            from name in StringLiteralQuote
            from eql in EqualsOrColon
            from value in PropertyValue
            select new HclSimplePropertyElement { Name = name, Child = value, NameQuoted = true };

        /// <summary>
        ///     Represents a multiline string
        /// </summary>
        public static readonly Parser<HclElement> ElementMultilineProperty =
            from name in Identifier
            from eql in Equal
            from value in HereDoc
            select new HclHereDocPropertyElement
            {
                Name = name,
                NameQuoted = false,
                Marker = value.Item1,
                Trimmed = value.Item2,
                Value = value.Item3
            };

        /// <summary>
        ///     Represents a multiline string
        /// </summary>
        public static readonly Parser<HclElement> QuotedHclElementMultilineProperty =
            from name in StringLiteralQuote
            from eql in Equal
            from value in HereDoc
            select new HclHereDocPropertyElement
            {
                Name = name,
                NameQuoted = true,
                Marker = value.Item1,
                Trimmed = value.Item2,
                Value = value.Item3
            };

        /// <summary>
        ///     Represents a list property
        /// </summary>
        public static readonly Parser<HclElement> ElementListProperty =
            from name in Identifier.Or(StringLiteralQuote)
            from eql in Equal
            from value in ListValue
            select new HclListPropertyElement { Name = name, Children = value.Children, NameQuoted = false };

        /// <summary>
        ///     Represents a list property
        /// </summary>
        public static readonly Parser<HclElement> QuotedHclElementListProperty =
            from name in StringLiteralQuote
            from eql in Equal
            from value in ListValue
            select new HclListPropertyElement { Name = name, Children = value.Children, NameQuoted = true };

        /// <summary>
        ///     Represent a map assigned to a named value
        /// </summary>
        public static readonly Parser<HclElement> ElementMapProperty =
            from name in Identifier.Or(StringLiteralQuote)
            from eql in Equal
            from properties in MapValue
            select new HclMapPropertyElement { Name = name, Children = properties.Children };

        /// <summary>
        ///     New in 0.12 - Represent a property holding a type
        /// </summary>
        public static readonly Parser<HclElement> ElementTypedObjectProperty =
            (from name in Identifier.Or(StringLiteralQuote)
                from eql in Equal
                from value in MapTypeProperty
                    .Or(ObjectTypeProperty)
                    .Or(ListTypeProperty)
                    .Or(SetTypeProperty)
                    .Or(TupleTypeProperty)
                select new HclTypePropertyElement { Name = name, Child = value, NameQuoted = false }).Token();

        /// <summary>
        ///     New in 0.12 - An plain type definition
        /// </summary>
        public static readonly Parser<HclElement> PrimitiveTypeObjectProperty =
            (from name in Identifier.Or(StringLiteralQuote)
                from eql in Equal
                from value in PrimitiveTypeProperty
                select new HclTypePropertyElement { Name = name, Child = value, NameQuoted = false }).Token();

        /// <summary>
        ///     Represents a named element with child properties
        /// </summary>
        public static readonly Parser<HclElement> NameElement =
            from dynamic in Dynamic.Optional()
            from name in Identifier.Or(StringLiteralQuote)
            from lbracket in LeftCurly
            from properties in Properties.Optional()
            from rbracket in RightCurly
            select new HclElement { Name = name, Children = properties.GetOrDefault() };

        /// <summary>
        ///     Represents a named element with a value and child properties
        /// </summary>
        public static readonly Parser<HclElement> NameValueElement =
            from dynamic in Dynamic.Optional()
            from name in Identifier
            from eql in Equal.Optional()
            from value in Identifier.Or(StringLiteralQuote)
            from lbracket in LeftCurly
            from properties in Properties.Optional()
            from rbracket in RightCurly
            select new HclElement { Name = name, Value = value, Children = properties.GetOrDefault() };

        /// <summary>
        ///     Represents named elements with values and types. These are things like resources.
        /// </summary>
        public static readonly Parser<HclElement> NameValueTypeElement =
            from dynamic in Dynamic.Optional()
            from name in Identifier
            from value in Identifier.Or(StringLiteralQuote)
            from type in Identifier.Or(StringLiteralQuote)
            from lbracket in LeftCurly
            from properties in Properties.Optional()
            from rbracket in RightCurly
            select new HclElement { Name = name, Value = value, Type = type, Children = properties.GetOrDefault() };

        /// <summary>
        ///     Represents the properties that can be added to an element
        /// </summary>
        public static readonly Parser<IEnumerable<HclElement>> Properties =
            (from value in NameElement
                    .Or(ElementTypedObjectProperty)
                    .Or(NameValueElement)
                    .Or(NameValueTypeElement)
                    .Or(ElementProperty)
                    .Or(QuotedElementProperty)
                    .Or(ElementListProperty)
                    .Or(QuotedHclElementListProperty)
                    .Or(ElementMapProperty)
                    .Or(ElementMultilineProperty)
                    .Or(QuotedHclElementMultilineProperty)
                    .Or(SingleLineComment)
                    .Or(MultilineComment)
                    .Or(UnquotedNameUnquotedElementProperty)
                    .Or(QuotedNameUnquotedElementProperty)
                from comma in Comma.Optional()
                select value).Many().Token();

        /// <summary>
        ///     The top level document. If you are parsing a HCL file, this is the Parser to use.
        ///     This is just a collection of child objects.
        /// </summary>
        public static readonly Parser<HclElement> HclTemplate =
            from children in Properties.End()
            select new HclRootElement { Children = children };

        /// <summary>
        ///     Replace line breaks with the Unix style line breaks
        /// </summary>
        /// <param name="template">The text to normalize</param>
        /// <returns>The text with normalized line breaks</returns>
        public static string NormalizeLineEndings(string template)
        {
            return LineEndNormalize.Replace(template, "\n");
        }

        public static string EscapeString(string template)
        {
            return template
                .Replace("\\", "\\\\")
                .Replace("\n", "\\n")
                .Replace("\a", "\\a")
                .Replace("\b", "\\b")
                .Replace("\f", "\\f")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t")
                .Replace("\v", "\\v")
                .Replace("\"", "\\\"");
        }
    }

    internal static class HclSpracheExtensions
    {
        /// <summary>
        ///     Like Token(), but whitespace is required
        /// </summary>
        public static Parser<T> RequiredToken<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return from leading in Parse.WhiteSpace.AtLeastOnce()
                from item in parser
                from trailing in Parse.WhiteSpace.AtLeastOnce()
                select item;
        }

        /// <summary>
        ///     An option to Token() which does not consume line breaks
        /// </summary>
        public static Parser<T> WithWhiteSpace<T>(this Parser<T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));

            return from leading in Parse.WhiteSpace.Except(Parse.LineEnd).Many()
                from item in parser
                from trailing in Parse.WhiteSpace.Except(Parse.LineEnd).Many()
                select item;
        }

        /// <summary>
        ///     Matches the parser, but does not consume the matched result. This is much like a positive lookahead
        ///     in a regex.
        /// </summary>
        public static Parser<T> PreviewRequired<T>(this Parser<T> parser)
        {
            if (parser == null)
                throw new ArgumentNullException(nameof(parser));
            return i =>
            {
                var result = parser(i);
                return result.WasSuccessful
                    ? Result.Success(result.Value, i)
                    : Result.Failure<T>(i, "Failed the preview", result.Expectations);
            };
        }
    }
}