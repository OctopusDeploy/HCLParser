using System.Linq;
using System.Text.RegularExpressions;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    /// Represents a list
    /// </summary>
    public class HclListElement : HclElement
    {
        public override string Type => ListType;
        
        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return indentString + PrintArray(indent);
        }

        protected string PrintArray(int indent)
        {
            var indentString = GetIndent(indent);
            var nextIndentString = GetIndent(indent + 1);

            var startArray = "[\n" +
                Children?.Aggregate("", (total, child) =>
                {
                    // Comments appear without a comma at the end
                    var suffix = child.Type != CommentType ? ", " : "";
                    return total + nextIndentString + child.ToString() + suffix + "\n";
                });
            
            // Retain the comma at the end (if one exists) if the last element is a comment
            if (Children?.LastOrDefault()?.Type != CommentType)
            {
                startArray = new Regex(",$").Replace(startArray.TrimEnd(), "");
            }
            
            return startArray + "\n" + indentString + "]";
        }
    }
}