using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    /// Represents a string
    /// </summary>
    public class HclUnquotedStringElement : HclElement
    {
        public override string Type => StringType;

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return indentString + Value;
        }
    }
}