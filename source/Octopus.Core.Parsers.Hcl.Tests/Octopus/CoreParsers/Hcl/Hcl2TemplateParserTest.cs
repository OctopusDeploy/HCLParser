using FluentAssertions;
using NUnit.Framework;
using Sprache;

namespace Octopus.CoreParsers.Hcl
{
    [TestFixture]
    public class Hcl2TemplateParserTest : TerraformTemplateLoader
    {
        [Test]
        public void ExampleFromDocs()
        {
            var template = TerraformLoadTemplate("hcl2examplefromdocs.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(3);
        }

        [Test]
        [TestCase("hcl2forloop.txt")]
        [TestCase("hcl2forloop2.txt")]
        public void ForLoopObject(string file)
        {
            var template = TerraformLoadTemplate(file);
            var parsed = HclParser.ForLoopObjectValue.Parse(template);
            parsed.StartBracket.Should().Be('{');
            parsed.EndBracket.Should().Be('}');
            parsed.Variable.Should().Be("s");
            parsed.Collection.Should().Be("aws_elastic_beanstalk_environment.example.all_settings");
            parsed.Statements.Should().Be("s.name => s.value");
            parsed.IfStatement.Should().Be("if s.namespace == \"aws:ec2:vpc\"");
        }

        [Test]
        [TestCase("hcl2forloop3.txt")]
        public void ForLoopList(string file)
        {
            var template = TerraformLoadTemplate(file);
            var parsed = HclParser.ForLoopListValue.Parse(template);
            parsed.StartBracket.Should().Be('[');
            parsed.EndBracket.Should().Be(']');
            parsed.Variable.Should().Be("o");
            parsed.Collection.Should().Be("var.list");
            parsed.Statements.Should().Be("o.interfaces[0].name[1]");
            parsed.IfStatement.Should().Be("");
        }

        [Test]
        [TestCase("hcl2ifstatement.txt")]
        [TestCase("hcl2ifstatement2.txt")]
        [TestCase("hcl2ifstatement3.txt")]
        public void IfStatement(string file)
        {
            var template = TerraformLoadTemplate(file);
            var parsed = HclParser.IfStatement.Parse(template);
            parsed.Should().Be("if s.namespace == \"aws:ec2:vpc\"");
        }

        [Test]
        [TestCase("[0]")]
        [TestCase("[*]")]
        [TestCase("[99]")]
        public void ListIndex(string index)
        {
            var result = HclParser.ListIndex.Parse(index);
            result.Should().Be(index);
        }

        [Test]
        [TestCase("[blah]")]
        [TestCase("]")]
        [TestCase("[")]
        [TestCase("[]")]
        public void ListIndexFail(string index)
        {
            try
            {
                HclParser.ListIndex.Parse(index);
                Assert.Fail("Parsing should have failed");
            }
            catch
            {
                // all good
            }
        }

        [TestCase("hcl2example1.txt")]
        [TestCase("hcl2example2.txt")]
        [TestCase("hcl2example3.txt")]
        [TestCase("hcl2example4.txt")]
        [TestCase("hcl2example5.txt")]
        [TestCase("hcl2example6.txt")]
        [TestCase("hcl2example7.txt")]
        [TestCase("hcl2example8.txt")]
        [TestCase("hcl2example9.txt")]
        public void GenericExamples(string file)
        {
            var template = TerraformLoadTemplate(file);
            var parsed = HclParser.HclTemplate.Parse(template);
            var reprinted = parsed.ToString();
        }

        [Test]
        [TestCase("hcl2objectproperty.txt", "vpc = object({\n    id = \"string\"\n    cidr_block = \"string\"\n})")]
        [TestCase("hcl2objectproperty2.txt", "vpc = object({\n    id = \"string\"\n    cidr_block = \"string\"\n    vpc = object({\n        id = \"string\"\n        cidr_block = \"string\"\n    })\n})")]
        public void ObjectProperty(string file, string result)
        {
            var template = TerraformLoadTemplate(file);
            var parsed = HclParser.HclElementTypedObjectProperty.Parse(template);
            parsed.ToString().Should().Be(result);
        }
    }
}