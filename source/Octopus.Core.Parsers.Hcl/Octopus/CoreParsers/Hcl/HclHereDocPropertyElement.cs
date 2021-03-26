namespace Octopus.CoreParsers.Hcl
{
    /// <summary><![CDATA[
    /// Represents the value of a heredoc string assigned to a property.
    /// Heredocs starting with << are printed as is.
    /// Heredocs starting with <<- have any leading whitespace trimmed 
    /// ]]></summary>
    public class HclHereDocPropertyElement : HclHereDocElement
    {
        public override string Type => HeredocStringPropertyType;

        public override string ToString(bool naked, int indent)
        {
            if (naked) return base.ToString(true, indent);

            var indentString = GetIndent(indent);
            return indentString + OriginalName + " = " + base.ToString(false, 0);
        }
    }
}