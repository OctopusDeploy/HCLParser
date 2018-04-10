using System.Linq;
using System.Text.RegularExpressions;

namespace Octopus.Core.Parsers.Hcl
{
    /// <summary>
    /// Represents a list assigned to a property
    /// </summary>
    public class HclListPropertyElement : HclListElement
    {
        public override string Type => ListPropertyType;
        
        public override string ToString(bool naked, int indent)
        {
            if (naked)
            {
                return base.ToString(true, indent);
            }
            
            var indentString = GetIndent(indent);
            return indentString + OriginalName + " = " + PrintArray(indent);
        }
    }
}