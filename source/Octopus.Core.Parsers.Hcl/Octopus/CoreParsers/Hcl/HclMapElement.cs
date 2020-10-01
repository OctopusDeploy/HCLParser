using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    /// Represents a map
    /// </summary>
    public class HclMapElement : HclElement
    {
        public override string Type => MapType;

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            var lineBreak = indent == -1 ? string.Empty : "\n";
            return indentString + "{" + lineBreak +
                string.Join("\n", Children?.Select(child => child.ToString(indent + 1)) ?? Enumerable.Empty<string>()) +
                lineBreak + indentString + "}";
        }
    }
}