using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents a map
    /// </summary>
    public class HclMapElement : HclElement
    {
        public override string Type => MapType;

        public override string Value => ToString(-1);

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            var lineBreak = indent == -1 ? string.Empty : "\n";
            var nextIndent = indent == -1 ? -1 : indent + 1;
            var separator = indent == -1 ? ", " : "\n";

            return indentString + "{" + lineBreak +
                   string.Join(separator,
                       Children?.Select(child => child.ToString(nextIndent)) ?? Enumerable.Empty<string>()) +
                   lineBreak + indentString + "}";
        }
    }
}