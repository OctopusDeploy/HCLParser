using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    public class HclMapTypeElement : HclElement
    {
        public override string Type => ObjectPropertyType;

        public override string ProcessedValue => Value ?? "";

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return indentString + "map(\n" +
                   string.Join("\n", Children?.Select(child => child.ToString(indent + 1)) ?? Enumerable.Empty<string>()) +
                   "\n" + indentString + ")";
        }
    }
}