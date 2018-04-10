using System.Linq;

namespace Octopus.Core.Parsers.Hcl
{
    /// <summary>
    /// Represents a string
    /// </summary>
    public class HclStringElement : HclElement
    {
        public override string Type => StringType;
        
        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return indentString + "\"" + EscapeQuotes(ProcessedValue) + "\"";
        }
    }
}