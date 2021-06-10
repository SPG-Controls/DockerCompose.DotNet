using System.IO;

namespace SPG.DockerCompose.Test.Helpers
{
    public static class TestHelper
    {
        public static string LoadEmbeddedTextResource(string name)
        {
            using var resourceStream = typeof(TestHelper).Assembly.GetManifestResourceStream(name);
            using var sr = new StreamReader(resourceStream);
            var output = sr.ReadToEnd();

            return output;
        }
    }
}
