namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents the collection of values that can make up an unquoted property value
    /// </summary>
    public class HclUnquotedExpressionElement : HclElement
    {
        public override string Type => UnquotedType;

        public override string ToString(bool naked, int indent)
        {
            return Value;
        }
    }
}