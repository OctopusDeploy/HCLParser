using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    public class HclForLoopElement : HclElement
    {
        public override string Type => ForLoopType;

        public override string ProcessedValue => Value ?? "";

        public string Variable { get; }

        public string Collection { get; }

        public string Statements { get; }

        public string IfStatement { get; }

        public char StartBracket { get; }

        public char EndBracket { get; }

        public HclForLoopElement(char startBracket, char endBracket, string variable, string collection, string statements, string ifStatement)
        {
            StartBracket = startBracket;
            EndBracket = endBracket;
            Variable = variable;
            Collection = collection;
            Statements = statements;
            IfStatement = ifStatement;
        }

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            return indentString + StartBracket +
                   "for " + Variable + " in " + Collection + " : " + Statements + IfStatement +
                   EndBracket;
        }
    }
}