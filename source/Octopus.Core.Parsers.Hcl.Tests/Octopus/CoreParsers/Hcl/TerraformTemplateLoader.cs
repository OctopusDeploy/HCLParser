using System.IO;
using System.Reflection;

namespace Octopus.CoreParsers.Hcl
{
    public class TerraformTemplateLoader
    {
        protected string TerraformLoadTemplate(string fileName, string directory = "TemplateSamples")
        {
            var templatesPath = Path.Combine(
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                @"Octopus\CoreParsers\Hcl\" + directory);

            return HclParser.NormalizeLineEndings(File.ReadAllText(Path.Combine(templatesPath, fileName))).Trim();
        }
    }
}