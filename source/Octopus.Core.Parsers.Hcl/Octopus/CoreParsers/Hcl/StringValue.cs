namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    /// This is used to capture the value of a string, and record if it was quoted or not
    /// </summary>
    public class StringValue
    {
        public bool Quoted { get; }

        public string Quote => Quoted ? "\"" : string.Empty;
        public string Value { get; }

        public string QuotedValue => Quote + Value + Quote;

        public StringValue(string value, bool quoted)
        {
            Quoted = quoted;
            Value = value;
        }
    }
}