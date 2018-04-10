using System.Linq;

namespace Octopus.Core.Parsers.Hcl
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
            return indentString + "{\n" +
                string.Join("\n", Children?.Select(child => child.ToString(indent + 1)) ?? Enumerable.Empty<string>()) +
                "\n" + indentString + "}";
        }
    }
}