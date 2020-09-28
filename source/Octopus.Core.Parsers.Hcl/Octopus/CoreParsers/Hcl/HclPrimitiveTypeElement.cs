using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    public class HclPrimitiveTypeElement : HclElement
    {
        public override string Type => PrimitivePropertyType;

        public override string ProcessedValue => Value ?? "";

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return indentString + "\"" + Value + "\"";
        }
    }
}