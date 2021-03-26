using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents a single line comment
    /// </summary>
    public class HclCommentElement : HclElement
    {
        public override string Type => CommentType;

        public override string ProcessedValue => Value ?? "";

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return indentString + string.Join("\n", ProcessedValue.Split('\n').Select(comment => "#" + comment));
        }
    }
}