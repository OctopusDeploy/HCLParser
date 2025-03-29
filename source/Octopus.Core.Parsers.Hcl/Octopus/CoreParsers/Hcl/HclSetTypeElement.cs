﻿using System.Linq;

namespace Octopus.CoreParsers.Hcl;

public class HclSetTypeElement : HclElement
{
    public override string Type => SetPropertyType;

    public override string ProcessedValue => Value ?? "";

    public override string Value => ToString(-1);

    public override string ToString(bool naked, int indent)
    {
        var indentString = indent == -1 ? string.Empty : GetIndent(indent);
        var lineBreak = indent == -1 ? string.Empty : "\n";
        var nextIndent = indent == -1 ? -1 : indent + 1;
        var separator = indent == -1 ? ", " : "\n";

        return "set(" + lineBreak +
               string.Join(separator,
                   Children?.Select(child => child.ToString(nextIndent)) ?? Enumerable.Empty<string>()) +
               lineBreak + indentString + ")";
    }
}