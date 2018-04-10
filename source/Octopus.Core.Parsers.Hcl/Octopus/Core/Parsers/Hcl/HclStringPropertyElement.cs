using System.Linq;

namespace Octopus.Core.Parsers.Hcl
{
    /// <summary>
    /// Represents a string assigned to a property
    /// </summary>
    public class HclStringPropertyElement : HclElement
    {
        public override string Type => StringPropertyType;
        
        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            if (naked)
            {
                return ProcessedValue;
            }
            
            return indentString + OriginalName + " = \"" + EscapeQuotes(ProcessedValue) + "\"";
        }
    }
}