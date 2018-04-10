using System;
using System.Collections.Generic;
using System.Linq;
using Octopus.CoreUtilities.Extensions;
using Sprache;

namespace Octopus.Core.Parsers.Hcl
{
    public class HclElement
    {
        /// <summary>
        /// The name of comment objects
        /// </summary>
        public const string CommentType = "#COMMENT";

        /// <summary>
        /// Name of the root document
        /// </summary>
        public const string RootType = "#ROOT";

        /// <summary>
        /// The type for string, number and boolean elements
        /// </summary>
        public const string StringType = "String";
        
        /// <summary>
        /// The type for multiline string
        /// </summary>
        public const string HeredocStringType = "HeredocString";            

        /// <summary>
        /// The type for list elements
        /// </summary>
        public const string ListType = "List";

        /// <summary>
        /// The type for map elements
        /// </summary>
        public const string MapType = "Map";  
        
        /// <summary>
        /// The type for string, number and boolean elements
        /// </summary>
        public const string StringPropertyType = "StringProperty";
        
        /// <summary>
        /// The type for string, number and boolean elements
        /// </summary>
        public const string HeredocStringPropertyType = "HeredocStringProperty";
        
        /// <summary>
        /// The type for list elements
        /// </summary>
        public const string ListPropertyType = "ListProperty";

        /// <summary>
        /// The type for map elements
        /// </summary>
        public const string MapPropertyType = "MapProperty";  
        
        /// <summary>
        /// The name of the element, #COMMENT for comments, or #ROOT for the
        /// root document element.
        /// e.g. variable, resource
        /// </summary>
        public virtual string Name { get; set; }

        /// <summary>
        /// True if the name was originally in quotes, and false otherwise
        /// </summary>
        public virtual bool NameQuoted { get; set; } = false;
        
        /// <summary>
        /// Returns the string that is used for the element name. If it was originally
        /// quoted, then it will be quoted here. Otherwise the plain name is returned.
        /// </summary>
        public virtual string OriginalName {
            get
            {
                if (NameQuoted)
                {
                    return "\"" + EscapeQuotes(Name) + "\"";
                }

                return Name;
            } 
        }

        /// <summary>
        /// The value of the element, or the comment contents
        /// e.g. my_variable, aws_instance
        /// </summary>
        public virtual string Value { get; set; }

        /// <summary>
        /// The processed value of the element. Defaults to the same as the Value,
        /// but for specialised types this can be a refined value.
        /// </summary>
        public virtual string ProcessedValue => Value;

        /// <summary>
        /// The type of the resource
        /// e.g. ec2
        /// </summary>
        public virtual string Type { get; set; }

        /// <summary>
        /// Any child elements
        /// </summary>
        public virtual IEnumerable<HclElement> Children { get; set; }

        public HclElement()
        {
        }

        /// <summary>
        /// Generates the string used to indent the printed output
        /// </summary>
        /// <param name="indent">The indent amount</param>
        /// <returns>The indent string</returns>
        protected string GetIndent(int indent)
        {
            return new String(' ', indent * 2);
        }        

        public virtual string ToString(bool naked, int indent)
        {
            var indentString = GetIndent(indent);

            return indentString + OriginalName + 
                ProcessedValue?.Map(a => " \"" + EscapeQuotes(a) + "\"") + 
                Type?.Map(a => " \"" + EscapeQuotes(a) + "\"") + 
                " {\n" +
                string.Join("\n", Children?.Select(child => child.ToString(indent + 1)) ?? Enumerable.Empty<string>()) +
                "\n" + indentString + "}";
        }

        public string ToString(int indent)
        {
            return ToString(false, indent);
        }

        public override string ToString()
        {
            return ToString(false, 0);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is HclElement)) 
                return false;

            var hclElement = obj as HclElement;

            if (hclElement.Name != Name)
                return false;
            
            if (hclElement.Value != Value)
                return false;
            
            if (hclElement.Type != Type)
                return false;
            
            if (hclElement.Children == null && Children != null)
                return false;
            
            if (hclElement.Children != null && Children == null)
                return false;

            if (Children != null && hclElement.Children != null)
            {
                var myChildren = Children.ToArray();
                var theirChidlren = hclElement.Children.ToArray();

                if (myChildren.Length != theirChidlren.Length)
                    return false;
                
                for (int i = 0; i < myChildren.Length; ++i)
                {
                    if (!myChildren[i].Equals(theirChidlren[i]))
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            int hash = 17;

            hash = hash * 23 + (Name?.GetHashCode() ?? 0);
            hash = hash * 23 + (Value?.GetHashCode() ?? 0);
            hash = hash * 23 + (Type?.GetHashCode() ?? 0);
            Children?.ToList().ForEach(child => hash = hash * 23 + (child?.GetHashCode() ?? 0));

            return hash;
        }

        protected string EscapeQuotes(string input) => HclParser.StringLiteralQuoteContentReverse.Parse(input);
    }
}