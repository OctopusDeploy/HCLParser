namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents a multiline comment
    /// </summary>
    public class HclMultiLineCommentElement : HclElement
    {
        public override string Type => CommentType;

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return indentString + "/*" + ProcessedValue + "*/";
        }
    }
}