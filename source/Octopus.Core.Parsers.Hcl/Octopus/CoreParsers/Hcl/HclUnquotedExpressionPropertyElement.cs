using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents an unquoted expression assigned to a property. These can be simple unquoted strings, or
    ///     more complex with function calls, math and ternary statements.
    /// </summary>
    public class HclUnquotedExpressionPropertyElement : HclElement
    {
        public override string Type => SimplePropertyType;

        public override string Value =>
            string.Join(" ", Children?.Select(child => child.ToString(-1)) ?? Enumerable.Empty<string>());

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            if (naked) return ProcessedValue;

            return indentString + OriginalName + " = " +
                   string.Join(" ", Children?.Select(child => child.ToString(-1)) ?? Enumerable.Empty<string>());
        }
    }
}