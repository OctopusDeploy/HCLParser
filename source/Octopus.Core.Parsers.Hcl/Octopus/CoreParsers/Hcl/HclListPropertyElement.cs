namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Represents a list assigned to a property
    /// </summary>
    public class HclListPropertyElement : HclListElement
    {
        public override string Type => ListPropertyType;

        public override string Value => PrintArray(-1);

        public override string ToString(bool naked, int indent)
        {
            if (naked) return base.ToString(true, indent);

            var indentString = GetIndent(indent);
            return indentString + OriginalName + " = " + PrintArray(indent);
        }
    }
}