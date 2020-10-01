﻿using System.Linq;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    /// Represents a string assigned to a property
    /// </summary>
    public class HclUnquotedExpressionPropertyElement : HclElement
    {
        public override string Type => StringPropertyType;

        public override string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);
            if (naked)
            {
                return ProcessedValue;
            }

            return indentString + OriginalName + " = " +
                   string.Join(" ", Children?.Select(child => child.ToString(0)) ?? Enumerable.Empty<string>());
        }
    }
}