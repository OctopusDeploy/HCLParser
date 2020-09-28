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
                var result = HclParser.ListIndex.Parse(index);
                Assert.Fail("Parsing should have failed");
            }
            catch
            {
                // all good
            }
        }
    }
}