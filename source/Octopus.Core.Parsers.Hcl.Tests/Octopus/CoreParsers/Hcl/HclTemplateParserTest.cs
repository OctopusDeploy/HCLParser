using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using Sprache;

namespace Octopus.CoreParsers.Hcl
{
    /// <summary>
    ///     Tested based on
    ///     https://github.com/hashicorp/hcl/blob/a4b07c25de5ff55ad3b8936cea69a79a3d95a855/hcl/parser/parser_test.go
    /// </summary>
    public class HclTemplateParserTest : TerraformTemplateLoader
    {
        [Test]
        public void ParseHCL1()
        {
            var template = TerraformLoadTemplate("aws.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.First().Value.Should().Match("aws_region");
            parsed.Children.First().Name.Should().Match("variable");
            parsed.Children.First().Children.Should().HaveCount(2);
            parsed.Children.First().Children.FirstOrDefault(prop => "description" == prop.Name).Should().NotBeNull();
            parsed.Children.First().Children.FirstOrDefault(prop => "default" == prop.Name).Should().NotBeNull();
            parsed.Children.First().Children.First(prop => "description" == prop.Name).Value.Should()
                .Match("The AWS region to create things in.");
            parsed.Children.First().Children.First(prop => "default" == prop.Name).Value.Should().Match("us-east-1");
        }

        [Test]
        public void ParseHCL2()
        {
            var template = TerraformLoadTemplate("empty.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(6);
            parsed.Children.First().Value.Should().Match("prod_access_key");
            parsed.Children.First().Name.Should().Match("variable");
            parsed.Children.First().Children.Should().HaveCount(0);
        }

        [Test]
        public void ParseHCL3()
        {
            var template = TerraformLoadTemplate("heredocdescription.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.First().Value.Should().Match("public_key_path");
            parsed.Children.First().Name.Should().Match("variable");
            parsed.Children.First().Children.Should().HaveCount(1);
            parsed.Children.First().Children.First().Name.Should().Match("description");
            parsed.Children.First().Children.First().Value.Should().Match(
                "\nPath to the SSH public key to be used for authentication.\n" +
                "Ensure this keypair is added to your local SSH agent so provisioners can\n" +
                "connect.\n" +
                "Example: ~/.ssh/terraform.pub\n");
        }

        [Test]
        public void ParseHCL4()
        {
            var template = TerraformLoadTemplate("mixedtext.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(7);
            parsed.Children.First().Value.Should().Match("public_key_path");
            parsed.Children.First().Name.Should().Match("variable");
            parsed.Children.First().Children.Should().HaveCount(1);
            parsed.Children.First().Children.First().Name.Should().Match("description");
            parsed.Children.First().Children.First().Value.Should().Match(
                "\nPath to the SSH public key to be used for authentication.\n" +
                "Ensure this keypair is added to your local SSH agent so provisioners can\n" +
                "connect.\n" +
                "Example: ~/.ssh/terraform.pub\n");
        }

        [Test]
        public void StringWithoutInterpolationParsing()
        {
            var template = TerraformLoadTemplate("string_without_interpolation_parsing.txt");
            var parsed = HclParser.StringLiteralQuote.Parse(template);
            parsed.Should().Match("${element(var.remote_port[\"${element(keys(var.remote_port), count.index)}\"], 1)}");
        }

        [Test]
        public void NestedInterpolation()
        {
            var template = TerraformLoadTemplate("nested_interpolation.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
        }

        [Test]
        public void Example1()
        {
            var template = TerraformLoadTemplate("example1.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Where(obj => "variable" == obj.Name).ToList().Should().HaveCount(15);
        }

        [Test]
        public void Example15()
        {
            var template = TerraformLoadTemplate("example15.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
            // This is an example of some nested interpolation
            parsed.Children.First().Children.Any(obj =>
                obj.Name == "backend_port" && obj.Value ==
                "${element(var.remote_port[\"${element(keys(var.remote_port), count.index)}\"], 1)}").Should().BeTrue();
        }

        [Test]
        public void Example29()
        {
            var template = TerraformLoadTemplate("example29.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
        }

        [Test]
        public void Example30()
        {
            var template = TerraformLoadTemplate("example30.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
        }

        [Test]
        public void Example31()
        {
            var template = TerraformLoadTemplate("example31.tf");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(3);
        }

        /// <summary>
        ///     100 random examples of terraform templates from GitHub.
        /// </summary>
        /// <param name="file">The terraform file to parse</param>
        [Test]
        [TestCase("example2.tf")]
        [TestCase("example3.tf")]
        [TestCase("example4.tf")]
        [TestCase("example5.tf")]
        [TestCase("example6.tf")]
        [TestCase("example7.tf")]
        [TestCase("example8.tf")]
        [TestCase("example9.tf")]
        [TestCase("example10.tf")]
        [TestCase("example11.tf")]
        [TestCase("example12.tf")]
        [TestCase("example13.tf")]
        [TestCase("example14.tf")]
        [TestCase("example16.tf")]
        [TestCase("example17.tf")]
        [TestCase("example18.tf")]
        [TestCase("example19.tf")]
        [TestCase("example20.tf")]
        [TestCase("example21.tf")]
        [TestCase("example22.tf")]
        [TestCase("example23.tf")]
        [TestCase("example24.tf")]
        [TestCase("example25.tf")]
        [TestCase("example26.tf")]
        [TestCase("example27.tf")]
        [TestCase("example28.tf")]
        [TestCase("example32.tf")]
        [TestCase("example33.tf")]
        [TestCase("example34.tf")]
        [TestCase("example35.tf")]
        [TestCase("example36.tf")]
        [TestCase("example37.tf")]
        [TestCase("example38.tf")]
        [TestCase("example39.tf")]
        [TestCase("example40.tf")]
        [TestCase("example41.tf")]
        [TestCase("example42.tf")]
        [TestCase("example43.tf")]
        [TestCase("example44.tf")]
        [TestCase("example45.tf")]
        [TestCase("example46.tf")]
        [TestCase("example47.tf")]
        [TestCase("example48.tf")]
        [TestCase("example49.tf")]
        [TestCase("example50.tf")]
        [TestCase("example51.tf")]
        [TestCase("example52.tf")]
        [TestCase("example53.tf")]
        [TestCase("example54.tf")]
        [TestCase("example55.tf")]
        [TestCase("example56.tf")]
        [TestCase("example57.tf")]
        [TestCase("example58.tf")]
        [TestCase("example59.tf")]
        [TestCase("example60.tf")]
        [TestCase("example61.tf")]
        [TestCase("example62.tf")]
        [TestCase("example63.tf")]
        [TestCase("example64.tf")]
        [TestCase("example65.tf")]
        [TestCase("example66.tf")]
        [TestCase("example67.tf")]
        [TestCase("example68.tf")]
        [TestCase("example69.tf")]
        [TestCase("example70.tf")]
        [TestCase("example71.tf")]
        [TestCase("example72.tf")]
        [TestCase("example73.tf")]
        [TestCase("example74.tf")]
        [TestCase("example75.tf")]
        [TestCase("example76.tf")]
        [TestCase("example77.tf")]
        [TestCase("example78.tf")]
        [TestCase("example79.tf")]
        [TestCase("example80.tf")]
        [TestCase("example81.tf")]
        [TestCase("example82.tf")]
        [TestCase("example83.tf")]
        [TestCase("example84.tf")]
        [TestCase("example86.tf")]
        [TestCase("example87.tf")]
        [TestCase("example88.tf")]
        [TestCase("example89.tf")]
        [TestCase("example90.tf")]
        [TestCase("example91.tf")]
        [TestCase("example92.tf")]
        [TestCase("example93.tf")]
        [TestCase("example94.tf")]
        [TestCase("example95.tf")]
        [TestCase("example96.tf")]
        [TestCase("example97.tf")]
        [TestCase("example98.tf")]
        [TestCase("example99.tf")]
        public void GenericExamples(string file)
        {
            var template = TerraformLoadTemplate(file);
            var parsed = HclParser.HclTemplate.Parse(template);
            var reprinted = parsed.ToString();
            var reparsed = HclParser.HclTemplate.Parse(reprinted);
            var reprinted2 = reparsed.ToString();
            reprinted.Should().Match(reprinted2);
            parsed.Should().BeEquivalentTo(reparsed);
        }

        [TestCase("example39.tf")]
        public void OneFileExample(string file)
        {
            var template = TerraformLoadTemplate(file);
            var parsed = HclParser.HclTemplate.Parse(template);
            var reprinted = parsed.ToString();
            var reparsed = HclParser.HclTemplate.Parse(reprinted);
            var reprinted2 = reparsed.ToString();
            reprinted.Should().Match(reprinted2);
            parsed.Should().BeEquivalentTo(reparsed);
        }

        [Test]
        public void QuotedText()
        {
            var template = TerraformLoadTemplate("quotedtext.txt");
            var parsed = HclParser.ElementProperty.Parse(template);
            parsed.Value.Should().Match("\"altitude-nyc-abcd-2017-stage.storage.googleapis.com\"");
        }

        [Test]
        public void UnquotingText()
        {
            var template = TerraformLoadTemplate("quotedtext_raw.txt");
            var parsed = HclParser.StringLiteralQuoteContentReverse.Parse(template);
            parsed.Should().Match("\\\"altitude-nyc-abcd-2017-stage.storage.googleapis.com\\\"");
        }

        [Test]
        public void CommentsAndNameElement()
        {
            var template = TerraformLoadTemplate("test1.txt");
            var parsed = HclParser.HclTemplate.Parse(template);
            var reprinted = parsed.ToString();
            var reparsed = HclParser.HclTemplate.Parse(reprinted);
            var reprinted2 = reparsed.ToString();
            reprinted.Should().Match(reprinted2);
        }

        [Test]
        public void EndingComments()
        {
            var template = TerraformLoadTemplate("test3.txt");
            var parsed = HclParser.HclTemplate.Parse(template);
            var reprinted = parsed.ToString();
            var reparsed = HclParser.HclTemplate.Parse(reprinted);
            var reprinted2 = reparsed.ToString();
            reprinted.Should().Match(reprinted2);
        }

        [Test]
        public void ParseCommentSingleLine()
        {
            var template = TerraformLoadTemplate("commentsingleline.txt");
            var parsed = HclParser.SingleLineComment.Many().Parse(template).ToList();
            parsed.Should().HaveCount(2);
            parsed.All(obj => obj.Value.StartsWith("Hello World")).Should().BeTrue();
        }

        [Test]
        public void ParseComment()
        {
            var template = TerraformLoadTemplate("comment.txt");
            var parsed = HclParser.MultilineComment.Parse(template);
            parsed.Value.Should().Match("\nHello\nWorld\n");
        }

        [Test]
        public void ParseHereDoc()
        {
            var template = TerraformLoadTemplate("multilinestring.txt");
            var parsed = HclParser.HereDoc.Parse(template);
            parsed.Item3.Should().Match("\nHello\nWorld\n");
        }

        [Test]
        public void ParseMap()
        {
            var template = TerraformLoadTemplate("map.tf");
            var parsed = HclParser.NameValueElement.Parse(template);
            parsed.Name.Should().Match("variable");
        }

        [Test]
        public void ParseMapColon()
        {
            var template = TerraformLoadTemplate("map_colon.tf");
            var parsed = HclParser.NameValueElement.Parse(template);
            parsed.Name.Should().Match("variable");
            parsed.Child.Name.Should().Match("default");
            var children = parsed.Child.Children.ToArray();

            children[0].Name.Should().Match("eu-west-1");
            children[0].Value.Should().Match("ami-674cbc1e");
            children[1].Name.Should().Match("us-east-1");
            children[1].Value.Should().Match("ami-1d4e7a66");
            children[2].Name.Should().Match("us-west-1");
            children[2].Value.Should().Match("ami-969ab1f6");
            children[3].Name.Should().Match("us-west-2");
            children[3].Value.Should().Match("ami-8803e0f0");
        }

        [Test]
        public void ParseListWithBool()
        {
            var template = TerraformLoadTemplate("ListWithBool.txt");
            var parsed = HclParser.ElementListProperty.Parse(template);
            parsed.Name.Should().Match("bool");
        }

        [Test]
        public void ParseMapWithListWithBool()
        {
            var template = TerraformLoadTemplate("MapWithListWithBool.txt");
            var parsed = HclParser.ElementMapProperty.Parse(template);
            parsed.Name.Should().Match("permissions");
        }

        [Test]
        public void ParseEmptyResource()
        {
            var template = TerraformLoadTemplate("emptyresource.tf");
            var parsed = HclParser.NameValueTypeElement.Parse(template);
            parsed.Name.Should().Match("resource");
        }

        [Test]
        public void ParseResourceWithChildren()
        {
            var template = TerraformLoadTemplate("resourcewithchildren.tf");
            var parsed = HclParser.NameValueTypeElement.Parse(template);
            parsed.Name.Should().Match("resource");
        }

        [Test]
        public void ParseResource()
        {
            var template = TerraformLoadTemplate("resource.tf");
            var parsed = HclParser.NameValueTypeElement.Parse(template);
            parsed.Name.Should().Match("resource");
        }

        [Test]
        public void StringInterpolationRaw()
        {
            var template = TerraformLoadTemplate("interpolation.txt");
            var parsed = HclParser.StringLiteralCurly.Parse(template);
            parsed.Should().Match("${\"there\"}");
        }

        [Test]
        public void StringInterpolation()
        {
            var template = TerraformLoadTemplate("curlytexttest.txt");
            var parsed = HclParser.StringLiteralQuote.Parse(template);
            parsed.Should().Match("Hi ${\"there\"}");
        }

        [Test]
        public void Basic()
        {
            var template = TerraformLoadTemplate("basic.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(2);
            parsed.FirstOrDefault(element => "foo" == element.Name).Should().NotBeNull();
            parsed.First(element => "foo" == element.Name).Value.Should().Match("bar");
            parsed.FirstOrDefault(element => "bar" == element.Name).Should().NotBeNull();
            parsed.First(element => "bar" == element.Name).Value.Should().Match("${file(\"bing/bong.txt\")}");
        }

        [Test]
        public void BasicIntString()
        {
            var template = TerraformLoadTemplate("basic_int_string.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(1);
            parsed.FirstOrDefault(element => "count" == element.Name).Should().NotBeNull();
            parsed.First(element => "count" == element.Name).Value.Should().Match("3");
        }

        [Test]
        public void BasicSquish()
        {
            var template = TerraformLoadTemplate("basic_squish.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(3);
            parsed.FirstOrDefault(element => "foo" == element.Name).Should().NotBeNull();
            parsed.First(element => "foo" == element.Name).Value.Should().Match("bar");
            parsed.FirstOrDefault(element => "bar" == element.Name).Should().NotBeNull();
            parsed.First(element => "bar" == element.Name).Value.Should().Match("${file(\"bing/bong.txt\")}");
            parsed.FirstOrDefault(element => "foo-bar" == element.Name).Should().NotBeNull();
            parsed.First(element => "foo-bar" == element.Name).Value.Should().Match("baz");
        }

        [Test]
        public void BlockAssign()
        {
            var template = TerraformLoadTemplate("block_assign.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.First().Name.Should().Match("environment");
            parsed.Children.First().Value.Should().Match("aws");
        }

        [Test]
        public void DecodePolicy()
        {
            var template = TerraformLoadTemplate("decode_policy.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(4);
            parsed.Children.FirstOrDefault(obj => "key" == obj.Name && "" == obj.Value).Should().NotBeNull();
            parsed.Children.First(obj => "key" == obj.Name && "" == obj.Value).Children.Should().HaveCount(1);
            parsed.Children.First(obj => "key" == obj.Name && "" == obj.Value).Children.First().Name.Should()
                .Match("policy");
            parsed.Children.First(obj => "key" == obj.Name && "" == obj.Value).Children.First().Value.Should()
                .Match("read");

            parsed.Children.FirstOrDefault(obj => "key" == obj.Name && "foo/" == obj.Value).Should().NotBeNull();
            parsed.Children.First(obj => "key" == obj.Name && "foo/" == obj.Value).Children.Should().HaveCount(1);
            parsed.Children.First(obj => "key" == obj.Name && "foo/" == obj.Value).Children.First().Name.Should()
                .Match("policy");
            parsed.Children.First(obj => "key" == obj.Name && "foo/" == obj.Value).Children.First().Value.Should()
                .Match("write");

            parsed.Children.FirstOrDefault(obj => "key" == obj.Name && "foo/bar/" == obj.Value).Should().NotBeNull();
            parsed.Children.First(obj => "key" == obj.Name && "foo/bar/" == obj.Value).Children.Should().HaveCount(1);
            parsed.Children.First(obj => "key" == obj.Name && "foo/bar/" == obj.Value).Children.First().Name.Should()
                .Match("policy");
            parsed.Children.First(obj => "key" == obj.Name && "foo/bar/" == obj.Value).Children.First().Value.Should()
                .Match("read");

            parsed.Children.FirstOrDefault(obj => "key" == obj.Name && "foo/bar/baz" == obj.Value).Should().NotBeNull();
            parsed.Children.First(obj => "key" == obj.Name && "foo/bar/baz" == obj.Value).Children.Should()
                .HaveCount(1);
            parsed.Children.First(obj => "key" == obj.Name && "foo/bar/baz" == obj.Value).Children.First().Name.Should()
                .Match("policy");
            parsed.Children.First(obj => "key" == obj.Name && "foo/bar/baz" == obj.Value).Children.First().Value
                .Should().Match("deny");
        }

        [Test]
        public void DecodeTFVariable()
        {
            var template = TerraformLoadTemplate("decode_tf_variable.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.FirstOrDefault(obj => obj.Name == "variable" && obj.Value == "foo").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "variable" && obj.Value == "foo").Children.Should().HaveCount(2);
            parsed.Children.First(obj => obj.Name == "variable" && obj.Value == "foo").Children
                .FirstOrDefault(obj => obj.Name == "default" && obj.Value == "bar").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "variable" && obj.Value == "foo").Children
                .FirstOrDefault(obj => obj.Name == "description" && obj.Value == "bar").Should().NotBeNull();

            parsed.Children.First(obj => obj.Name == "variable" && obj.Value == "amis").Children.Should().HaveCount(1);
            parsed.Children.First(obj => obj.Name == "variable" && obj.Value == "amis").Children
                .FirstOrDefault(obj => obj.Name == "default").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "variable" && obj.Value == "amis").Children
                .First(obj => obj.Name == "default").Children.Should().HaveCount(1);
            parsed.Children.First(obj => obj.Name == "variable" && obj.Value == "amis").Children
                .First(obj => obj.Name == "default").Children.First().Name.Should().Match("east");
            parsed.Children.First(obj => obj.Name == "variable" && obj.Value == "amis").Children
                .First(obj => obj.Name == "default").Children.First().Value.Should().Match("foo");
        }

        [Test]
        public void EmptyHCL()
        {
            var template = TerraformLoadTemplate("empty.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.First().Name.Should().Match("resource");
            parsed.Children.First().Value.Should().Match("foo");
            parsed.Children.First().Children.Should().BeEmpty();
        }

        [Test]
        public void Escape()
        {
            var template = TerraformLoadTemplate("escape.hcl");
            var parsed = HclParser.Properties.Parse(template).ToArray();
            parsed.Should().HaveCount(6);
            parsed.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "bar\"baz\\n").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "bar" && obj.Value == "new\nline").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "qux" && obj.Value == "back\\slash").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "qax" && obj.Value == "slash\\:colon").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "nested" && obj.Value == "${HH\\\\:mm\\\\:ss}").Should()
                .NotBeNull();
            parsed.FirstOrDefault(obj =>
                obj.Name == "nestedquotes" && obj.Value == "${\"\\\"stringwrappedinquotes\\\"\"}").Should().NotBeNull();
        }

        [Test]
        public void EscapeBackslash()
        {
            var template = TerraformLoadTemplate("escape_backslash.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.FirstOrDefault(obj => obj.Name == "output").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "output").Children.FirstOrDefault(obj =>
                obj.Name == "one" && obj.Value == @"${replace(var.sub_domain, ""."", ""\\."")}").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "output").Children.FirstOrDefault(obj =>
                obj.Name == "two" && obj.Value == @"${replace(var.sub_domain, ""."", ""\\\\."")}").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "output").Children.FirstOrDefault(obj =>
                    obj.Name == "many" && obj.Value == @"${replace(var.sub_domain, ""."", ""\\\\\\\\."")}").Should()
                .NotBeNull();
        }

        [Test]
        public void Flat()
        {
            var template = TerraformLoadTemplate("flat.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(2);
            parsed.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "bar").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "Key" && obj.Value == "7").Should().NotBeNull();
        }

        [Test]
        public void Float()
        {
            var template = TerraformLoadTemplate("float.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.FirstOrDefault(obj => obj.Name == "a" && obj.Value == "1.02").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "b" && obj.Value == "2").Should().NotBeNull();
        }

        [Test]
        public void ListOfLists()
        {
            var template = TerraformLoadTemplate("list_of_lists.hcl");
            var parsed = HclParser.ElementListProperty.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.Count(child => child.Children.All(grandchild => grandchild.Value == "foo")).Should().Be(1);
            parsed.Children.Count(child => child.Children.All(grandchild => grandchild.Value == "bar")).Should().Be(1);
        }

        [Test]
        public void ListOfMaps()
        {
            var template = TerraformLoadTemplate("list_of_maps.hcl");
            var parsed = HclParser.ElementListProperty.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.Count(child => child.Children.All(grandchild => grandchild.Value == "someval1")).Should()
                .Be(1);
            parsed.Children.Count(child => child.Children.Any(grandchild => grandchild.Value == "someval2")).Should()
                .Be(1);
            parsed.Children.Count(child => child.Children.Any(grandchild => grandchild.Value == "someextraval"))
                .Should().Be(1);
        }

        [Test]
        public void Multiline()
        {
            var template = TerraformLoadTemplate("multiline.hcl");
            var parsed = HclParser.ElementMultilineProperty.Parse(template);
            parsed.Value.Should().Match("\nbar\nbaz\n");
        }

        [Test]
        public void MultilineIndented()
        {
            var template = TerraformLoadTemplate("multiline_indented.hcl");
            var parsed = HclParser.ElementMultilineProperty.Parse(template);
            parsed.Value.Should().Match("\n        bar\n        baz\n      ");
        }

        [Test]
        public void MultilineLiteral()
        {
            var template = TerraformLoadTemplate("multiline_literal.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(1);
            parsed.First().Name.Should().Match("multiline_literal");
            parsed.First().Value.Should().Match("hello\n  world");
        }

        [Test]
        public void MultilineLiteralHil()
        {
            var template = TerraformLoadTemplate("multiline_literal_with_hil.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(1);
            parsed.First().Name.Should().Match("multiline_literal_with_hil");
            parsed.First().Value.Should().Match("${hello\n  world}");
        }

        [Test]
        public void MultilineNoEOF()
        {
            var template = TerraformLoadTemplate("multiline_no_eof.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(2);
            parsed.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "\nbar\nbaz\n").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "key" && obj.Value == "value").Should().NotBeNull();
        }

        [Test]
        public void MultilineNoHangingIndent()
        {
            var template = TerraformLoadTemplate("multiline_no_hanging_indent.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(1);
            parsed.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "\n  baz\n    bar\n      foo\n      ")
                .Should().NotBeNull();
        }

        [Test]
        public void NestedBlockComment()
        {
            var template = TerraformLoadTemplate("nested_block_comment.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(2);
            parsed.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == "\nfoo = \"bar/*\"\n")
                .Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "bar" && obj.Value == "value").Should().NotBeNull();
        }

        [Test]
        public void ObjectWithBool()
        {
            var template = TerraformLoadTemplate("object_with_bool.hcl");
            var parsed = HclParser.NameElement.Parse(template);
            parsed.Name.Should().Match("path");
            parsed.Children.Should().HaveCount(2);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.FirstOrDefault(obj => obj.Name == "policy" && obj.Value == "write").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "permissions").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "permissions").Children.Should().HaveCount(1);
            parsed.Children.First(obj => obj.Name == "permissions").Children
                .FirstOrDefault(obj => obj.Name == "bool" && obj.Children.First().Value == "false").Should()
                .NotBeNull();
        }

        [Test]
        public void Scientific()
        {
            var template = TerraformLoadTemplate("scientific.hcl");
            var parsed = HclParser.Properties.Parse(template).ToList();
            parsed.Should().HaveCount(6);
            parsed.FirstOrDefault(obj => obj.Name == "a" && obj.Value == "1e-10").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "b" && obj.Value == "1e+10").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "c" && obj.Value == "1e10").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "d" && obj.Value == "1.2e-10").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "e" && obj.Value == "1.2e+10").Should().NotBeNull();
            parsed.FirstOrDefault(obj => obj.Name == "f" && obj.Value == "1.2e10").Should().NotBeNull();
        }

        [Test]
        public void SliceExpand()
        {
            var template = TerraformLoadTemplate("slice_expand.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.FirstOrDefault(obj => obj.Name == "service" && obj.Value == "my-service-0").Should()
                .NotBeNull();
            parsed.Children.First(obj => obj.Name == "service" && obj.Value == "my-service-0").Children.Should()
                .HaveCount(1);
            parsed.Children.First(obj => obj.Name == "service" && obj.Value == "my-service-0")
                .Children.FirstOrDefault(obj => obj.Name == "key" && obj.Value == "value").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "service" && obj.Value == "my-service-1").Should()
                .NotBeNull();
            parsed.Children.First(obj => obj.Name == "service" && obj.Value == "my-service-1")
                .Children.FirstOrDefault(obj => obj.Name == "key" && obj.Value == "value").Should().NotBeNull();
        }

        [Test]
        public void Structure()
        {
            var template = TerraformLoadTemplate("structure.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.FirstOrDefault(obj =>
                    obj.Type == HclElement.CommentType && obj.Value == " This is a test structure for the lexer")
                .Should()
                .NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "baz").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo" && obj.Value == "baz").Children.Should().HaveCount(2);
            parsed.Children.First(obj => obj.Name == "foo" && obj.Value == "baz")
                .Children.FirstOrDefault(obj => obj.Name == "key" && obj.Value == "7").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo" && obj.Value == "baz")
                .Children.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "bar").Should().NotBeNull();
        }

        [Test]
        public void StructureFlatMap()
        {
            var template = TerraformLoadTemplate("structure_flatmap.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.Count(obj => obj.Name == "foo").Should().Be(2);
            parsed.Children.FirstOrDefault(obj => obj.Children.All(child => child.Name == "key" && child.Value == "7"))
                .Should().NotBeNull();
            parsed.Children
                .FirstOrDefault(obj => obj.Children.All(child => child.Name == "foo" && child.Value == "bar")).Should()
                .NotBeNull();
        }

        [Test]
        public void StructureList()
        {
            var template = TerraformLoadTemplate("structure_list.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.Count(obj => obj.Name == "foo").Should().Be(2);
            parsed.Children.FirstOrDefault(obj => obj.Children.All(child => child.Name == "key" && child.Value == "7"))
                .Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Children.All(child => child.Name == "key" && child.Value == "12"))
                .Should().NotBeNull();
        }

        [Test]
        public void StructureMulti()
        {
            var template = TerraformLoadTemplate("structure_multi.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.Count(obj => obj.Name == "foo").Should().Be(2);
            parsed.Children.FirstOrDefault(obj => obj.Children.All(child => child.Name == "key" && child.Value == "7"))
                .Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Children.All(child => child.Name == "key" && child.Value == "12"))
                .Should().NotBeNull();
        }

        [Test]
        public void Structure2()
        {
            var template = TerraformLoadTemplate("structure2.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(3);
            parsed.Children.FirstOrDefault(obj =>
                    obj.Type == HclElement.CommentType && obj.Value == " This is a test structure for the lexer")
                .Should()
                .NotBeNull();
            parsed.Children.Count(obj => obj.Name == "foo").Should().Be(2);
            parsed.Children
                .FirstOrDefault(obj => obj.Children?.All(child => child.Name == "key" && child.Value == "7") ?? false)
                .Should().NotBeNull();
            parsed.Children
                .FirstOrDefault(obj => obj.Children?.Any(child => child.Name == "foo" && child.Value == "bar") ?? false)
                .Should().NotBeNull();
        }

        [Test]
        public void TerraformHeroku()
        {
            var template = TerraformLoadTemplate("terraform_heroku.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.FirstOrDefault(obj => obj.Name == "name" && obj.Value == "terraform-test-app").Should()
                .NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "config_vars").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "config_vars").Children.Should().HaveCount(1);
            parsed.Children.First(obj => obj.Name == "config_vars").Children
                .FirstOrDefault(obj => obj.Name == "FOO" && obj.Value == "bar").Should().NotBeNull();
        }

        [Test]
        public void TFVars()
        {
            var template = TerraformLoadTemplate("tfvars.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(3);
            parsed.Children.FirstOrDefault(obj => obj.Name == "regularvar" && obj.Value == "Should work").Should()
                .NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "map.key1" && obj.Value == "Value").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "map.key2" && obj.Value == "Other value").Should()
                .NotBeNull();
        }

        [Test]
        public void EscapedInterpolation()
        {
            var template = TerraformLoadTemplate("escaped_interpolation.txt");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children
                .FirstOrDefault(obj => obj.Name == "one" && obj.Value == "$${replace(var.sub_domain, \".\", \"\\.\")}")
                .Should().NotBeNull();
        }

        [Test]
        public void ArrayComment()
        {
            var template = TerraformLoadTemplate("array_comment.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.First().Children.FirstOrDefault(obj => obj.Value == "1").Should().NotBeNull();
            parsed.Children.First().Children.FirstOrDefault(obj => obj.Value == "2").Should().NotBeNull();
            parsed.Children.First().Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType).Should()
                .NotBeNull();
        }

        [Test]
        public void AssignDeep()
        {
            var template = TerraformLoadTemplate("assign_deep.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.First().Children.First().Children.FirstOrDefault(obj => obj.Name == "foo").Should()
                .NotBeNull();
            parsed.Children.First().Children.First().Children.First(obj => obj.Name == "foo").Children.First().Children
                .FirstOrDefault(obj => obj.Name == "bar").Should().NotBeNull();
        }

        [Test]
        public void Comment()
        {
            var template = TerraformLoadTemplate("comment.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(7);
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "bar").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Foo").Should()
                .NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Bar ").Should()
                .NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == "\n/*\nBaz\n")
                .Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Another")
                .Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Multiple")
                .Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Lines").Should()
                .NotBeNull();
        }

        [Test]
        public void CommentCrlf()
        {
            var template = TerraformLoadTemplate("comment_crlf.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(7);
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "bar").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Foo").Should()
                .NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Bar ").Should()
                .NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == "\n/*\nBaz\n")
                .Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Another")
                .Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Multiple")
                .Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Lines").Should()
                .NotBeNull();
        }

        [Test]
        public void CommentLastLine()
        {
            var template = TerraformLoadTemplate("comment_lastline.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == "foo").Should()
                .NotBeNull();
        }

        [Test]
        public void CommentSingle()
        {
            var template = TerraformLoadTemplate("comment_single.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.FirstOrDefault(obj => obj.Type == HclElement.CommentType && obj.Value == " Hello").Should()
                .NotBeNull();
        }

        [Test]
        public void Complex()
        {
            var template = TerraformLoadTemplate("complex.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(8);
            parsed.Children.FirstOrDefault(obj => obj.Name == "variable" && obj.Value == "groups").Should().NotBeNull();

            parsed.Children.FirstOrDefault(obj => obj.Name == "provider" && obj.Value == "aws").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "provider" && obj.Value == "aws").Children
                .FirstOrDefault(obj => obj.Name == "access_key" && obj.Value == "foo").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "provider" && obj.Value == "aws").Children
                .FirstOrDefault(obj => obj.Name == "secret_key" && obj.Value == "bar").Should().NotBeNull();

            parsed.Children.FirstOrDefault(obj => obj.Name == "provider" && obj.Value == "do").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "provider" && obj.Value == "do").Children
                .FirstOrDefault(obj => obj.Name == "api_key" && obj.Value == "${var.foo}").Should().NotBeNull();

            parsed.Children
                .FirstOrDefault(obj =>
                    obj.Name == "resource" && obj.Value == "aws_security_group" && obj.Type == "firewall").Should()
                .NotBeNull();
            parsed.Children.First(obj =>
                    obj.Name == "resource" && obj.Value == "aws_security_group" && obj.Type == "firewall").Children
                .FirstOrDefault(obj => obj.Name == "count" && obj.Value == "5").Should().NotBeNull();

            parsed.Children
                .FirstOrDefault(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "web")
                .Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "web")
                .Children
                .FirstOrDefault(obj => obj.Name == "ami" && obj.Value == "${var.foo}").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "web")
                .Children
                .FirstOrDefault(obj => obj.Name == "security_groups").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "web")
                .Children
                .FirstOrDefault(obj => obj.Name == "network_interface").Should().NotBeNull();

            parsed.Children
                .FirstOrDefault(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "db")
                .Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "db")
                .Children
                .FirstOrDefault(obj =>
                    obj.Name == "security_groups" && obj.Value == "${aws_security_group.firewall.*.id}").Should()
                .NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "db")
                .Children
                .FirstOrDefault(obj => obj.Name == "VPC" && obj.Value == "foo").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "db")
                .Children
                .FirstOrDefault(obj => obj.Name == "depends_on").Should().NotBeNull();

            parsed.Children.FirstOrDefault(obj => obj.Name == "output" && obj.Value == "web_ip").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "output" && obj.Value == "web_ip").Children
                .FirstOrDefault(obj => obj.Name == "value" && obj.Value == "${aws_instance.web.private_ip}").Should()
                .NotBeNull();
        }

        [Test]
        public void ComplexUnicode()
        {
            var template = TerraformLoadTemplate("complex_unicode.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(8);
            parsed.Children.FirstOrDefault(obj => obj.Name == "a۰۱۸" && obj.Value == "foo").Should().NotBeNull();
        }

        [Test]
        public void ComplexCrlf()
        {
            var template = TerraformLoadTemplate("complex_crlf.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(8);
            parsed.Children.FirstOrDefault(obj => obj.Name == "variable" && obj.Value == "groups").Should().NotBeNull();

            parsed.Children.FirstOrDefault(obj => obj.Name == "provider" && obj.Value == "aws").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "provider" && obj.Value == "aws").Children
                .FirstOrDefault(obj => obj.Name == "access_key" && obj.Value == "foo").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "provider" && obj.Value == "aws").Children
                .FirstOrDefault(obj => obj.Name == "secret_key" && obj.Value == "bar").Should().NotBeNull();

            parsed.Children.FirstOrDefault(obj => obj.Name == "provider" && obj.Value == "do").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "provider" && obj.Value == "do").Children
                .FirstOrDefault(obj => obj.Name == "api_key" && obj.Value == "${var.foo}").Should().NotBeNull();

            parsed.Children
                .FirstOrDefault(obj =>
                    obj.Name == "resource" && obj.Value == "aws_security_group" && obj.Type == "firewall").Should()
                .NotBeNull();
            parsed.Children.First(obj =>
                    obj.Name == "resource" && obj.Value == "aws_security_group" && obj.Type == "firewall").Children
                .FirstOrDefault(obj => obj.Name == "count" && obj.Value == "5").Should().NotBeNull();

            parsed.Children
                .FirstOrDefault(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "web")
                .Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "web")
                .Children
                .FirstOrDefault(obj => obj.Name == "ami" && obj.Value == "${var.foo}").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "web")
                .Children
                .FirstOrDefault(obj => obj.Name == "security_groups").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "web")
                .Children
                .FirstOrDefault(obj => obj.Name == "network_interface").Should().NotBeNull();

            parsed.Children
                .FirstOrDefault(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "db")
                .Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "db")
                .Children
                .FirstOrDefault(obj =>
                    obj.Name == "security_groups" && obj.Value == "${aws_security_group.firewall.*.id}").Should()
                .NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "db")
                .Children
                .FirstOrDefault(obj => obj.Name == "VPC" && obj.Value == "foo").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "resource" && obj.Value == "aws_instance" && obj.Type == "db")
                .Children
                .FirstOrDefault(obj => obj.Name == "depends_on").Should().NotBeNull();

            parsed.Children.FirstOrDefault(obj => obj.Name == "output" && obj.Value == "web_ip").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "output" && obj.Value == "web_ip").Children
                .FirstOrDefault(obj => obj.Name == "value" && obj.Value == "${aws_instance.web.private_ip}").Should()
                .NotBeNull();
        }

        [Test]
        public void ComplexKey()
        {
            var template = TerraformLoadTemplate("complex_key.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo.bar" && obj.Value == "baz").Should().NotBeNull();
        }

        [Test]
        public void KeyWithoutValue()
        {
            try
            {
                var template = TerraformLoadTemplate("key_without_value.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }

        [Test]
        public void List()
        {
            var template = TerraformLoadTemplate("list.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children.FirstOrDefault(obj => obj.Value == "1").Should()
                .NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children.FirstOrDefault(obj => obj.Value == "2").Should()
                .NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children.FirstOrDefault(obj => obj.Value == "foo").Should()
                .NotBeNull();
        }

        [Test]
        public void ListComma()
        {
            var template = TerraformLoadTemplate("list_comma.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children.FirstOrDefault(obj => obj.Value == "1").Should()
                .NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children.FirstOrDefault(obj => obj.Value == "2").Should()
                .NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children.FirstOrDefault(obj => obj.Value == "foo").Should()
                .NotBeNull();
        }

        [Test]
        public void Multiple()
        {
            var template = TerraformLoadTemplate("multiple.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(2);
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "bar").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "key" && obj.Value == "7").Should().NotBeNull();
        }

        [Test]
        public void ObjectKeyAssignWithoutValue()
        {
            try
            {
                var template = TerraformLoadTemplate("object_key_assign_without_value.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }

        [Test]
        public void ObjectKeyAssignWithoutValue2()
        {
            try
            {
                var template = TerraformLoadTemplate("object_key_assign_without_value2.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }

        [Test]
        public void ObjectKeyAssignWithoutValue3()
        {
            try
            {
                var template = TerraformLoadTemplate("object_key_assign_without_value3.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }

        [Test]
        public void ObjectKeyWithoutValue()
        {
            try
            {
                var template = TerraformLoadTemplate("object_key_without_value.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }

        [Test]
        public void ObjectListComma()
        {
            var template = TerraformLoadTemplate("object_list_comma.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children
                .FirstOrDefault(obj => obj.Name == "one" && obj.Value == "1").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children
                .FirstOrDefault(obj => obj.Name == "two" && obj.Value == "2").Should().NotBeNull();
        }

        [Test]
        public void StructureBasic()
        {
            var template = TerraformLoadTemplate("structure_basic.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children
                .FirstOrDefault(obj => obj.Name == "value" && obj.Value == "7").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children
                .FirstOrDefault(obj => obj.Name == "value" && obj.Value == "8").Should().NotBeNull();
            parsed.Children.First(obj => obj.Name == "foo").Children
                .FirstOrDefault(obj => obj.Name == "complex::value" && obj.Value == "9").Should().NotBeNull();
        }

        [Test]
        public void StructureEmpty()
        {
            var template = TerraformLoadTemplate("structure_empty.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(1);
            parsed.Children.FirstOrDefault(obj => obj.Name == "resource" && obj.Value == "foo" && obj.Type == "bar")
                .Should().NotBeNull();
        }

        [Test]
        public void Types()
        {
            var template = TerraformLoadTemplate("types.hcl");
            var parsed = HclParser.HclTemplate.Parse(template);
            parsed.Children.Should().HaveCount(7);
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "bar").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "bar" && obj.Value == "7").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "baz").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "-12").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "bar" && obj.Value == "3.14159").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "foo" && obj.Value == "true").Should().NotBeNull();
            parsed.Children.FirstOrDefault(obj => obj.Name == "bar" && obj.Value == "false").Should().NotBeNull();
        }

        [Test]
        public void MultilineNoMarker()
        {
            try
            {
                var template = TerraformLoadTemplate("multiline_no_marker.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }

        [Test]
        public void UnterminatedObject()
        {
            try
            {
                var template = TerraformLoadTemplate("unterminated_object.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }

        [Test]
        public void UnterminatedObject2()
        {
            try
            {
                var template = TerraformLoadTemplate("unterminated_object_2.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }

        [Test]
        [Ignore(
            "Need to fix this. Leads to a false positive, but that is OK for now, a known issue. The parser works for our needs as long as it never has false negatives, so false positives are ok now. but some of the feedback in the bug bash was to provide better error messages for invalid scripts, which means having a more accurate parser")]
        public void ArrayComment2()
        {
            try
            {
                var template = TerraformLoadTemplate("array_comment_2.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }

        [Test]
        public void GitCrypt()
        {
            try
            {
                var template = TerraformLoadTemplate("git_crypt.hcl");
                var parsed = HclParser.HclTemplate.Parse(template);
                throw new Exception("Parsing should have failed");
            }
            catch (ParseException)
            {
                // all good
            }
        }


        [Test]
        public void NumberRegex()
        {
            HclParser.NumberRegex.Match("1.0").Value.Should().Match("1.0");
            HclParser.NumberRegex.Match("-1.0").Value.Should().Match("-1.0");
            HclParser.NumberRegex.Match("-1.02").Value.Should().Match("-1.02");
            HclParser.NumberRegex.Match("-100000.02").Value.Should().Match("-100000.02");
            HclParser.NumberRegex.Match("1e-10").Value.Should().Match("1e-10");
            HclParser.NumberRegex.Match("1e+10").Value.Should().Match("1e+10");
            HclParser.NumberRegex.Match("1e10").Value.Should().Match("1e10");
            HclParser.NumberRegex.Match("1.2e-10").Value.Should().Match("1.2e-10");
            HclParser.NumberRegex.Match("1.2e10").Value.Should().Match("1.2e10");
            HclParser.NumberRegex.Match("1").Value.Should().Match("1");
            HclParser.NumberRegex.Match("1000").Value.Should().Match("1000");
            HclParser.NumberRegex.Match("0x1").Value.Should().Match("0x1");
            HclParser.NumberRegex.Match("-0x1").Value.Should().Match("-0x1");
        }

        [Test]
        public void TestVariableParsing()
        {
            var template = @"variable ""test"" {
                type = ""string""
            }

            variable ""list"" {
                type = ""list""
            }

            variable ""map"" {
                type = ""map""
            }";

            var result = HclParser.HclTemplate.Parse(template);
            result.Children.Count().Should().Be(3);
            result.Children.First(c => c.Value == "test").Child.Value.Should().Be("string");
            result.Children.First(c => c.Value == "list").Child.Value.Should().Be("list");
            result.Children.First(c => c.Value == "map").Child.Value.Should().Be("map");
        }
    }
}