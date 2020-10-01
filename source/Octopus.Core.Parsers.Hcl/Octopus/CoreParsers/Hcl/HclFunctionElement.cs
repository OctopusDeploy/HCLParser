using System;
using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    public class HclFunctionElement : HclElement
    {
        public override string Type => FunctionType;

        public override string ProcessedValue => Value ?? "";

        public override string ToString(bool naked, int indent)
        {
            return OriginalName + "(" +
                   string.Join(", ", Children?.Select(child => child.ToString(indent)) ?? Enumerable.Empty<string>()) +
                   ")";
        }
    }
}