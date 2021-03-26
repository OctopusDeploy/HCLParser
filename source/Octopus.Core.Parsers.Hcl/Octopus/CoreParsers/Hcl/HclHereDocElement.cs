using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary><![CDATA[
    /// Represents the value of a heredoc string.
    /// Heredocs starting with << are printed as is.
    /// Heredocs starting with <<- have any leading whitespace trimmed 
    /// ]]></summary>
    public class HclHereDocElement : HclElement
    {
        /// <summary>
        ///     true if this heredoc is a trimmed version, and false otherwise
        /// </summary>
        public bool Trimmed { get; set; }

        /// <summary>
        ///     The Heredoc marker e.g. EOF
        /// </summary>
        public string Marker { get; set; }

        public override string Type => HeredocStringType;

        /// <summary>
        ///     Returns the original heredoc if it is not trimmed, or the trimmed version
        /// </summary>
        public override string ProcessedValue
        {
            get
            {
                if (!Trimmed) return Value;

                return Value?.Split('\n')
                    .Select(value => value.TrimStart())
                    .Aggregate("", (total, current) => total + "\n" + current);
            }
        }

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            var markerPrefix = Trimmed ? "<<-" : "<<";
            return indentString + markerPrefix + Marker + Value + Marker;
        }
    }
}