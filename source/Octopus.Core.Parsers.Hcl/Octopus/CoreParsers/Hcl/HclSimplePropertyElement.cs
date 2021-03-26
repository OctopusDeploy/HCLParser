using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents a simple value (string, number or boolean) assigned to a property
    /// </summary>
    public class HclSimplePropertyElement : HclElement
    {
        public override string Type => SimplePropertyType;

        /// <summary>
        ///     This class used to be a simple name/value mapping. It was updated to defer the value to the
        ///     children, but for compatibility the Value property returns the children's values.
        /// </summary>
        public override string Value
        {
            get => string.Join(
                string.Empty,
                Children?.Select(child => child.Value) ?? Enumerable.Empty<string>());
            set { }
        }

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            var children = string.Join(
                string.Empty,
                Children?.Select(child => child.ToString(naked, -1)) ?? Enumerable.Empty<string>());
            if (naked) return children;

            return indentString + OriginalName + " = " + children;
        }
    }
}