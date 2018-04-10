using System.Linq;

namespace Octopus.Core.Parsers.Hcl
{
    /// <summary>
    /// Represents the document root
    /// </summary>
    public class HclRootElement : HclElement
    {
        public override string Type => RootType;
        
        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return string.Join("\n", Children?.Select(child => child.ToString(indent)) ?? Enumerable.Empty<string>());
        }
    }
}