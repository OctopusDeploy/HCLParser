using System;
using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    public class HclSetTypeElement : HclElement
    {
        public override string Type => ObjectPropertyType;

        public override string ProcessedValue => Value ?? "";

        public override string ToString(bool naked, int indent)
        {
            var parentString = GetIndent(Math.Max(0, indent - 1));
            return "set(\n" +
                   string.Join("\n", Children?.Select(child => child.ToString(indent + 1)) ?? Enumerable.Empty<string>()) +
                   "\n" + parentString + ")";
        }
    }
}