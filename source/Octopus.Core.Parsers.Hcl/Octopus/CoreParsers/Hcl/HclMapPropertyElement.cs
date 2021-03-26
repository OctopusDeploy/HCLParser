using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents a map assigned to a property
    /// </summary>
    public class HclMapPropertyElement : HclMapElement
    {
        public override string Type => MapPropertyType;

        public override string Value => "{" +
                                        string.Join(", ",
                                            Children?.Select(child => child.ToString(-1)) ??
                                            Enumerable.Empty<string>()) +
                                        "}";

        public override string ToString(bool naked, int indent)
        {
            if (naked) return base.ToString(true, indent);

            var indentString = indent == -1 ? string.Empty : GetIndent(indent);
            var lineBreak = indent == -1 ? string.Empty : "\n";
            var nextIndent = indent == -1 ? -1 : indent + 1;
            var separator = indent == -1 ? ", " : "\n";

            return indentString + OriginalName + " = {" + lineBreak +
                   string.Join(separator,
                       Children?.Select(child => child.ToString(nextIndent)) ?? Enumerable.Empty<string>()) +
                   lineBreak + indentString + "}";
        }
    }
}