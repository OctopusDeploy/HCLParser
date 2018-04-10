using System.Linq;

namespace Octopus.Core.Parsers.Hcl
{
    /// <summary>
    /// Represents a map assigned to a property
    /// </summary>
    public class HclMapPropertyElement : HclMapElement
    {
        public override string Type => MapPropertyType;
        
        public override string ToString(bool naked, int indent)
        {
            if (naked)
            {
                return base.ToString(true, indent);
            }
            
            var indentString = GetIndent(indent);
            return indentString + OriginalName + " = {\n" +
                string.Join("\n", Children?.Select(child => child.ToString(indent + 1)) ?? Enumerable.Empty<string>()) +
                "\n" + indentString + "}";
        }
    }
}