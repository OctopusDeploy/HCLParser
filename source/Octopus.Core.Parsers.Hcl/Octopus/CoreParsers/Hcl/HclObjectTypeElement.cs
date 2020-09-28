using System;
using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    /// Represents a map assigned to a property
    /// </summary>
    public class HclObjectTypeElement : HclElement
    {
        public override string Type => MapPropertyType;

        public override string ToString(bool naked, int indent)
        {
            var parentString = GetIndent(Math.Max(0, indent - 1));
            return "object({\n" +
                string.Join("\n", Children?.Select(child => child.ToString(indent + 1)) ?? Enumerable.Empty<string>()) +
                "\n" + parentString + "})";
        }
    }
}