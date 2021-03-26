namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents a string
    /// </summary>
    public class HclStringElement : HclElement
    {
        public override string Type => StringType;

        public override string ToString(bool naked, int indent)
        {
            /*
             * ToString() is designed to return the HCL representation of this element. Naked was an older option
             * that was used to indicate that only the Value was to be returned. For string elements, naked meant
             * to return the value without any quotes.
             *
             * It is better to use the Value property for this use case, but this logic is retained for compatibility.
             */
            if (naked) return ProcessedValue;

            var indentString = GetIndent(indent);
            return indentString + "\"" + EscapeQuotes(ProcessedValue) + "\"";
        }
    }
}