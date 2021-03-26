namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents a string
    /// </summary>
    public class HclNumOrBoolElement : HclElement
    {
        public override string Type => NumOrBool;

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return indentString + Value;
        }
    }
}