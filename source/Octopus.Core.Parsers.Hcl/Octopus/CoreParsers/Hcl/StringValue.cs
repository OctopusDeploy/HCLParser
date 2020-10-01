namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    /// This is used to capture the value of a string, and record if it was quoted, wrapped in parentheses or not.
    /// </summary>
    public class StringValue
    {
        public Wrapper Wrapper { get; }

        public string WrapperStart
        {
            get
            {
                switch (Wrapper)
                {
                    case Wrapper.DoubleQuotes:
                        return "\"";
                    case Wrapper.Parentheses:
                        return "(";
                    default:
                        return string.Empty;
                }
            }
        }

        public string WrapperEnd
        {
            get
            {
                switch (Wrapper)
                {
                    case Wrapper.DoubleQuotes:
                        return "\"";
                    case Wrapper.Parentheses:
                        return ")";
                    default:
                        return string.Empty;
                }
            }
        }

        public string Value { get; }

        public string OriginalValue => WrapperStart + Value + WrapperEnd;

        public StringValue(string value, Wrapper wrapper)
        {
            Wrapper = wrapper;
            Value = value;
        }

        public StringValue(string value)
        {
            Wrapper = Wrapper.None;
            Value = value;
        }
    }

    public enum Wrapper
    {
        None,
        DoubleQuotes,
        Parentheses
    }
}