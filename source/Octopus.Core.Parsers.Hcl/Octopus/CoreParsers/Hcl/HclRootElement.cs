using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents the document root
    /// </summary>
    public class HclRootElement : HclElement
    {
        public override string Type => RootType;

        public override string ToString(bool naked, int indent)
        {
            return string.Join("\n", Children?.Select(child => child.ToString(indent)) ?? Enumerable.Empty<string>());
        }
    }
}