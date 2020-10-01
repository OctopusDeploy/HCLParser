namespace Octopus.CoreParsers.Hcl
{
    public class HclMathSymbol : HclElement
    {
        public override string Type => MathSymbol;

        public override string ToString(bool naked, int indent)
        {
            return Value;
        }
    }
}