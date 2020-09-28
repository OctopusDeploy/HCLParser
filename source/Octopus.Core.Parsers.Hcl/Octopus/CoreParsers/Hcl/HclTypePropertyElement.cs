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
            var indentString = GetIndent(indent);
            return indentString + OriginalName + " = " +
                   string.Join("\n",
                       Children?.Select(child => child.ToString(indent + 1)) ?? Enumerable.Empty<string>());
        }
    }
}