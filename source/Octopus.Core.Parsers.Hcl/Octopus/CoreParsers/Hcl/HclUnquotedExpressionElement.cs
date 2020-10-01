using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    /// Represents the collection of values that can make up an unquoted property value
    /// </summary>
    public class HclUnquotedExpressionElement : HclElement
    {
        public override string Type => UnquotedType;

        //public override string Value => base.Value ?? string.Empty;

        public override string ToString(bool naked, int indent)
        {
            return string.Join(" ", Children?.Select(child => child.ToString(0)) ?? Enumerable.Empty<string>());
        }
    }
}