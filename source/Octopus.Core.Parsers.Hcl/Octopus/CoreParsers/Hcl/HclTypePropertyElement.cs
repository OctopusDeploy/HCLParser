using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    /// Represents a string assigned to a property
    /// </summary>
    public class HclTypePropertyElement : HclElement
    {
        public override string Type => TypePropertyType;

        public override string ToString(bool naked, int indent)
        {
            var indentString = indent == -1 ? string.Empty : GetIndent(indent);
            var nextIndent = indent == -1 ? -1 : indent + 1;
            var separator = indent == -1 ? ", " : "\n";

            return indentString + OriginalName + " = " +
                   string.Join(separator,
                       Children?.Select(child => child.ToString(nextIndent)) ?? Enumerable.Empty<string>());
        }
    }
}