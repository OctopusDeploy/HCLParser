﻿using FluentAssertions;
using NUnit.Framework;
using Sprache;

namespace Octopus.CoreParsers.Hcl
{
    [TestFixture]
    public class Hcl2TemplateParserTest : TerraformTemplateLoader
    {
        const string HCL2TemplateSamples = "HCL2TemplateSamples";

        [Test]
        public void ExampleFromDocs()
        {
            var template = TerraformLoadTemplate("hcl2examplefromdocs.tf", HCL2TemplateSamples);
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(3);
        }

        [Test]
        [TestCase("hcl2forloop.txt")]
        [TestCase("hcl2forloop2.txt")]
        public void ForLoopObject(string file)
        {
            var template = TerraformLoadTemplate(file, HCL2TemplateSamples);
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
            var template = TerraformLoadTemplate(file, HCL2TemplateSamples);
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
            var template = TerraformLoadTemplate(file, HCL2TemplateSamples);
            var parsed = HclParser.IfStatement.Parse(template);
            parsed.Should().Be("if s.namespace == \"aws:ec2:vpc\"");
        }

        [Test]
        [TestCase("[0]")]
        [TestCase("[*]")]
        [TestCase("[99]")]
        [TestCase("[a.b]")]
        public void ListIndex(string index)
        {
            var result = HclParser.ListIndex.Parse(index);
            result.Should().Be(index);
        }

        [Test]
        [TestCase("[ ]")]
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

        [TestCase("var.region == 0 ? data.aws_region.this.name : var.region")]
        [TestCase("var.region == \"\" ? data.aws_region.this.name : var.region")]
        [TestCase("var.region == \"\" ? data.aws_region.this.name : a ? b : c")]
        [TestCase("var.region == \"\" ? data.aws_region.this.name : a ? b : c")]
        public void TestTernary(string index)
        {
            var result = HclParser.TernaryLogic.Parse(index);
            result.Value.Should().Be(index);
        }

        [TestCase("var.region == \"\"")]
        [TestCase("var.region == blah")]
        [TestCase("var.region == blah + 3 - 2 * 1")]
        public void TestText(string index)
        {
            var result = HclParser.QuotedOrUnquotedText.Parse(index);
            result.Value.Should().Be(index);
        }

        [Test]
        [TestCase("tags = merge(\"var.tags\")")]
        [TestCase("tags = merge(\"var.tags\", \"blah\")")]
        [TestCase("tags = merge(\"var.tags\", \"blah\", merge(\"var.tags\", \"blah\"))")]
        [TestCase("tags = merge(\"var.tags\", {\n  \"Name\" = \"${var.network_name}-ip\"\n})")]
        [TestCase("tags = merge({\n  \"Name\" = \"${var.network_name}-ip\"\n})")]
        public void TestFunctionAssignment(string index)
        {
            var result = HclParser.HclFunctionProperty.Parse(index);
            result.ToString().Should().Be(index);
        }

        [Test]
        [TestCase("locals {\n  tags = merge(\"var.tags\")\n}")]
        [TestCase("locals {\n  tags = merge(\"var.tags1\", \"var.tags2\")\n}")]
        [TestCase("locals {\n  tags = merge(var.tags, {\"Name\" = \"${var.network_name}-ip\"})\n}")]
        [TestCase("locals {\n  tags = merge({\"Name\" = \"${var.network_name}-ip\"})\n}")]
        [TestCase("locals {\n  depends_on = [\n  aws_s3_bucket.bucket\n]\n}")]
        public void TestFunctionAssignmentInElement(string index)
        {
            var result = HclParser.HclNameElement.Parse(index);
            result.ToString().Should().Be(index);
        }

        [TestCase("merge(\"var.tags\", \"blah\")")]
        public void TestFunctionCall(string index)
        {
            var result = HclParser.FunctionCall.Parse(index);
            result.ToString().Should().Be(index);
        }

        [TestCase("depends_on = [\n  aws_s3_bucket.bucket\n]")]
        public void TestListAssignment(string index)
        {
            var result = HclParser.HclElementListProperty.Parse(index);
            result.ToString().Should().Be(index);
        }

        [Test]
        [TestCase("merge(\"var.tags\")")]
        public void TestUnquotedString(string index)
        {
            try
            {
                var result = HclParser.StringLiteralUnquotedContent.Parse(index);
                Assert.Fail();
            }
            catch
            {
                // all good
            }
        }

        [Test]
        [TestCase("hcl2example1.txt")]
        [TestCase("hcl2example2.txt")]
        [TestCase("hcl2example3.txt")]
        [TestCase("hcl2example4.txt")]
        [TestCase("hcl2example5.txt")]
        [TestCase("hcl2example6.txt")]
        [TestCase("hcl2example7.txt")]
        [TestCase("hcl2example8.txt")]
        [TestCase("hcl2example9.txt")]
        [TestCase("hcl2example10.txt")]
        [TestCase("hcl2example11.txt")]
        [TestCase("hcl2example12.txt")]
        [TestCase("hcl2example13.txt")]
        [TestCase("hcl2example14.txt")]
        [TestCase("hcl2example15.txt")]
        [TestCase("hcl2example16.txt")]
        public void GenericExamples(string file)
        {
            var template = TerraformLoadTemplate(file, HCL2TemplateSamples);
            var parsed = HclParser.HclTemplate.Parse(template);
            var reprinted = parsed.ToString();
        }

        /// <summary>
        /// Examples from https://github.com/hashicorp/hcl/tree/hcl2/hclwrite/fuzz
        /// </summary>
        [Test]
        [TestCase("attr.hcl")]
        [TestCase("attr-expr.hcl")]
        [TestCase("attr-literal.hcl")]
        [TestCase("block-attrs.hcl")]
        [TestCase("block-comment.hcl")]
        [TestCase("block-empty.hcl")]
        [TestCase("block-nested.hcl")]
        [TestCase("complex.hcl")]
        [TestCase("empty.hcl")]
        [TestCase("escape-dollar.hcl")]
        [TestCase("escape-newline.hcl")]
        [TestCase("function-call.hcl")]
        [TestCase("hash-comment.hcl")]
        [TestCase("index.hcl")]
        [TestCase("int.hcl")]
        [TestCase("int-tmpl.hcl")]
        [TestCase("just-interp.hcl")]
        [TestCase("literal.hcl")]
        [TestCase("lots-of-comments.hcl")]
        [TestCase("slash-comment.hcl")]
        [TestCase("splat-attr.hcl")]
        [TestCase("splat-dot-full.hcl")]
        [TestCase("splat-full.hcl")]
        [TestCase("traversal-dot-index.hcl")]
        [TestCase("traversal-dot-index-terminal.hcl")]
        [TestCase("traversal-index.hcl")]
        [TestCase("utf8.hcl")]
        [TestCase("var.hcl")]
        public void CorpusExamples(string file)
        {
            var template = TerraformLoadTemplate(file, "corpus");
            var parsed = HclParser.HclTemplate.Parse(template);
            var reprinted = parsed.ToString();
        }

        [TestCase("hcl2githubexample1.tf")]
        [TestCase("hcl2githubexample2.tf")]
        [TestCase("hcl2githubexample3.tf")]
        [TestCase("hcl2githubexample4.tf")]
        [TestCase("hcl2githubexample5.tf")]
        [TestCase("hcl2githubexample6.tf")]
        [TestCase("hcl2githubexample7.tf")]
        [TestCase("hcl2githubexample8.tf")]
        [TestCase("hcl2githubexample9.tf")]
        [TestCase("hcl2githubexample10.tf")]
        [TestCase("hcl2githubexample11.tf")]
        [TestCase("hcl2githubexample12.tf")]
        [TestCase("hcl2githubexample13.tf")]
        [TestCase("hcl2githubexample14.tf")]
        [TestCase("hcl2githubexample15.tf")]
        [TestCase("hcl2githubexample16.tf")]
        [TestCase("hcl2githubexample17.tf")]
        [TestCase("hcl2githubexample18.tf")]
        [TestCase("hcl2githubexample19.tf")]
        [TestCase("hcl2githubexample20.tf")]
        [TestCase("hcl2githubexample21.tf")]
        [TestCase("hcl2githubexample22.tf")]
        [TestCase("hcl2githubexample23.tf")]
        [TestCase("hcl2githubexample24.tf")]
        public void RandomGitHubExamples(string file)
        {
            var template = TerraformLoadTemplate(file, HCL2TemplateSamples);
            var parsed = HclParser.HclTemplate.Parse(template);
            var reprinted = parsed.ToString();
        }

        [Test]
        [TestCase("hcl2objectproperty.txt", "vpc = object({\n    id = \"string\"\n    cidr_block = \"string\"\n})")]
        [TestCase("hcl2objectproperty2.txt", "vpc = object({\n    id = \"string\"\n    cidr_block = \"string\"\n    vpc = object({\n        id = \"string\"\n        cidr_block = \"string\"\n    })\n})")]
        public void ObjectProperty(string file, string result)
        {
            var template = TerraformLoadTemplate(file, HCL2TemplateSamples);
            var parsed = HclParser.HclElementTypedObjectProperty.Parse(template);
            parsed.ToString().Should().Be(result);
        }
    }
}