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

        [TestCase("var.region == \"\"")]
        [TestCase("var.region == blah")]
        [TestCase("var.region == blah + 3 - 2 * 1")]
        [TestCase("var.manual_deploy_enabled ? \"STOP_DEPLOYMENT\" : \"CONTINUE_DEPLOYMENT\"")]
        [TestCase("\"a\" == \"a\"")]
        [TestCase("(a + b =\n    c) +\n    ddd ?\n    \"e\" :\n    \"f\"", "(a + b =\n    c) + ddd ? \"e\" : \"f\"")]
        [TestCase("a +\n b =\n c +\n d", "a + b = c + d")]
        public void TestText(string index, string expected = null)
        {
            var result = HclParser.UnquotedContent.Parse(index);
            result.ToString().Should().Be(expected ?? index);
        }

        [TestCase("test = \"a\" == \"a\"")]
        public void TestTextAssignment(string index)
        {
            var result = HclParser.UnquotedNameUnquotedElementProperty.Parse(index);
            result.ToString().Should().Be(index);
        }

        [TestCase("blah \"==\" {test = \"a\" == \"a\"}")]
        [TestCase("blah \"==\" {test = \"a\"}")]
        public void TestTextAssignmentInElement(string index)
        {
            var result = HclParser.NameValueElement.Parse(index);
            result.ToString(-1).Should().Be(index);
        }

        [TestCase("\"a\"", "\"a\"")]
        [TestCase("\"a\"  ", "\"a\"")]
        public void TestStringLiteralSuccess(string index, string expected)
        {
            var result = HclParser.PropertyValue.Parse(index);
            result.ToString().Should().Be(expected);

        }

        [TestCase("\"a\" something")]
        [TestCase("\"a\" \"b\"")]
        [TestCase("\"a\" == \"b\"")]
        [TestCase("\"a\" ==")]
        public void TestStringLiteralFailures(string index)
        {
            try
            {
                var result = HclParser.PropertyValue.Parse(index);
                Assert.Fail("should have not parsed");
            }
            catch
            {
                // all ok
            }
        }

        [TestCase("test = var.manual_deploy_enabled ? \"STOP_DEPLOYMENT\" : \"CONTINUE_DEPLOYMENT\"")]
        [TestCase("template = file(\"task-definitions/covid-portal.json\", \"2\", \"\")")]
        [TestCase("allocation_id = aws_eip.covidportal_natgw.*.id[count.index]")]
        public void TestUnquotedElementProperty(string index)
        {
            var result = HclParser.UnquotedNameUnquotedElementProperty.Parse(index);
            result.ToString().Should().Be(index);
        }

        [TestCase("[\n  var.region\n]")]
        public void TestUnquotedList(string index)
        {
            var result = HclParser.ListValue.Parse(index);
            result.ToString().Should().Be(index);
        }

        [Test]
        [TestCase("{\n  \"Name\" = \"${var.network_name}-ip\"\n}")]
        public void TestMapValue(string index)
        {
            var result = HclParser.MapValue.Parse(index);
            result.ToString().Should().Be(index);
        }

        [Test]
        [TestCase("\"Name\" = \"${var.network_name}-ip\"")]
        public void TestQuotedElementProperty(string index)
        {
            var result = HclParser.QuotedElementProperty.Parse(index);
            result.ToString().Should().Be(index);
        }

        [Test]
        [TestCase("${var.network_name}-ip")]
        public void TestStringLiteralQuote(string index)
        {
            var result = HclParser.StringLiteralQuoteContent.Parse(index);
            result.Should().Be(index);
        }

        [Test]
        [TestCase("${var.network_name}-ip")]
        public void TestStringLiteralQuoteContent(string index)
        {
            var result = HclParser.StringLiteralQuoteContent.Parse(index);
            result.Should().Be(index);
        }

        [Test]
        [TestCase("locals {\n  tags = merge(\"var.tags\")\n}")]
        [TestCase("locals {\n  tags = merge(\"var.tags1\", \"var.tags2\")\n}")]
        [TestCase("locals {\n  tags = merge(var.tags, {\"Name\" = \"${var.network_name}-ip\"})\n}")]
        [TestCase("locals {\n  tags = merge({\"Name\" = \"${var.network_name}-ip\"})\n}")]
        [TestCase("locals {\n  depends_on = [\n    aws_s3_bucket.bucket\n  ]\n}")]
        public void TestAssignmentInElement(string index)
        {
            var result = HclParser.NameElement.Parse(index);
            result.ToString().Should().Be(index);
        }

        [TestCase("depends_on = [\n  aws_s3_bucket.bucket\n]")]
        public void TestListAssignment(string index)
        {
            var result = HclParser.ElementListProperty.Parse(index);
            result.ToString().Should().Be(index);
        }

        [TestCase("(hi)")]
        public void TestGroupText(string index)
        {
            var result = HclParser.GroupText.Parse(index);
            result.Should().Be(index);

            var result2 = HclParser.UnquotedContent.Parse(index);
            result2.Value.Should().Be(index);
        }

        [TestCase("{for l in keys(local.id_context) : title(l) => local.id_context[l] if length(local.id_context[l]) > 0}")]
        [TestCase("[for l in keys(local.id_context) : title(l) => local.id_context[l] if length(local.id_context[l]) > 0]")]
        public void TestForLoop(string index)
        {
            var result = HclParser.UnquotedContent.Parse(index);
            result.ToString().Should().Be(index);
        }

        [TestCase("{foo: 2}", "{foo = 2}")]
        [TestCase("{foo: 2, bar:\"a\"}", "{foo = 2, bar = \"a\"}")]
        [TestCase("{foo: 2, bar:\"a\", baz = null}", "{foo = 2, bar = \"a\", baz = null}")]
        public void TestObject(string index, string expected)
        {
            var result = HclParser.MapValue.Parse(index);
            result.ToString(-1).Should().Be(expected);
        }

        /// <summary>
        /// A couple of specific examples to test the parser against. These live in files because modifying
        /// line endings in strings is hard work.
        /// </summary>
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
        [TestCase("hcl2example17.txt")]
        [TestCase("hcl2example18.txt")]
        [TestCase("hcl2example19.txt")]
        public void GenericExamples(string file)
        {
            var template = TerraformLoadTemplate(file, HCL2TemplateSamples);
            var parsed = HclParser.HclTemplate.Parse(template);
            var printed = parsed.ToString();
            var reparsed = HclParser.HclTemplate.Parse(printed);
            var reprinted = reparsed.ToString();
            printed.Should().Be(reprinted);
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
            var printed = parsed.ToString();
            var reparsed = HclParser.HclTemplate.Parse(printed);
            var reprinted = reparsed.ToString();
            printed.Should().Be(reprinted);
        }

        /// <summary>
        /// 100 random terraform examples found on GitHub to test the parser on.
        /// </summary>
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
        [TestCase("hcl2githubexample25.tf")]
        [TestCase("hcl2githubexample26.tf")]
        [TestCase("hcl2githubexample27.tf")]
        [TestCase("hcl2githubexample28.tf")]
        [TestCase("hcl2githubexample29.tf")]
        [TestCase("hcl2githubexample30.tf")]
        [TestCase("hcl2githubexample31.tf")]
        [TestCase("hcl2githubexample32.tf")]
        [TestCase("hcl2githubexample33.tf")]
        [TestCase("hcl2githubexample34.tf")]
        [TestCase("hcl2githubexample35.tf")]
        [TestCase("hcl2githubexample36.tf")]
        [TestCase("hcl2githubexample37.tf")]
        [TestCase("hcl2githubexample38.tf")]
        [TestCase("hcl2githubexample39.tf")]
        [TestCase("hcl2githubexample40.tf")]
        [TestCase("hcl2githubexample41.tf")]
        [TestCase("hcl2githubexample42.tf")]
        [TestCase("hcl2githubexample43.tf")]
        [TestCase("hcl2githubexample44.tf")]
        [TestCase("hcl2githubexample45.tf")]
        [TestCase("hcl2githubexample46.tf")]
        [TestCase("hcl2githubexample47.tf")]
        [TestCase("hcl2githubexample48.tf")]
        [TestCase("hcl2githubexample49.tf")]
        [TestCase("hcl2githubexample50.tf")]
        [TestCase("hcl2githubexample51.tf")]
        [TestCase("hcl2githubexample52.tf")]
        [TestCase("hcl2githubexample53.tf")]
        [TestCase("hcl2githubexample54.tf")]
        [TestCase("hcl2githubexample55.tf")]
        [TestCase("hcl2githubexample56.tf")]
        [TestCase("hcl2githubexample57.tf")]
        [TestCase("hcl2githubexample58.tf")]
        [TestCase("hcl2githubexample59.tf")]
        [TestCase("hcl2githubexample60.tf")]
        [TestCase("hcl2githubexample61.tf")]
        [TestCase("hcl2githubexample62.tf")]
        [TestCase("hcl2githubexample63.tf")]
        [TestCase("hcl2githubexample64.tf")]
        [TestCase("hcl2githubexample65.tf")]
        [TestCase("hcl2githubexample66.tf")]
        [TestCase("hcl2githubexample67.tf")]
        [TestCase("hcl2githubexample68.tf")]
        [TestCase("hcl2githubexample69.tf")]
        [TestCase("hcl2githubexample70.tf")]
        [TestCase("hcl2githubexample71.tf")]
        [TestCase("hcl2githubexample72.tf")]
        [TestCase("hcl2githubexample73.tf")]
        [TestCase("hcl2githubexample74.tf")]
        [TestCase("hcl2githubexample75.tf")]
        [TestCase("hcl2githubexample76.tf")]
        [TestCase("hcl2githubexample77.tf")]
        [TestCase("hcl2githubexample78.tf")]
        [TestCase("hcl2githubexample79.tf")]
        [TestCase("hcl2githubexample80.tf")]
        [TestCase("hcl2githubexample81.tf")]
        [TestCase("hcl2githubexample82.tf")]
        [TestCase("hcl2githubexample83.tf")]
        [TestCase("hcl2githubexample84.tf")]
        [TestCase("hcl2githubexample85.tf")]
        [TestCase("hcl2githubexample86.tf")]
        [TestCase("hcl2githubexample87.tf")]
        [TestCase("hcl2githubexample88.tf")]
        [TestCase("hcl2githubexample89.tf")]
        [TestCase("hcl2githubexample90.tf")]
        [TestCase("hcl2githubexample91.tf")]
        [TestCase("hcl2githubexample92.tf")]
        [TestCase("hcl2githubexample93.tf")]
        [TestCase("hcl2githubexample94.tf")]
        [TestCase("hcl2githubexample95.tf")]
        [TestCase("hcl2githubexample96.tf")]
        [TestCase("hcl2githubexample97.tf")]
        [TestCase("hcl2githubexample98.tf")]
        [TestCase("hcl2githubexample99.tf")]
        [TestCase("hcl2githubexample100.tf")]
        public void RandomGitHubExamples(string file)
        {
            var template = TerraformLoadTemplate(file, HCL2TemplateSamples);
            var parsed = HclParser.HclTemplate.Parse(template);
            var printed = parsed.ToString();
            var reparsed = HclParser.HclTemplate.Parse(printed);
            var reprinted = reparsed.ToString();
            printed.Should().Be(reprinted);
        }

        [Test]
        [TestCase("hcl2objectproperty.txt", "vpc = object({id = \"string\", cidr_block = \"string\"})")]
        [TestCase("hcl2objectproperty2.txt", "vpc = object({id = \"string\", cidr_block = \"string\", vpc = object({id = \"string\", cidr_block = \"string\"})})")]
        public void ObjectProperty(string file, string result)
        {
            var template = TerraformLoadTemplate(file, HCL2TemplateSamples);
            var parsed = HclParser.ElementTypedObjectProperty.Parse(template);
            parsed.ToString(-1).Should().Be(result);
        }
    }
}