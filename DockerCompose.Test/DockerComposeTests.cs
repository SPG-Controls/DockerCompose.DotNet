using DockerCompose.Model;
using DockerCompose.Model.Models;
using FluentAssertions;
using SPG.DockerCompose.Test.Helpers;
using System.Collections.Generic;
using System.IO;
using Xunit;

namespace SPG.DockerCompose.Test
{
    public class DockerComposeTests
    {
        [Fact(DisplayName = "Docker: Parse compose")]
        public void ParseVersionTest()
        {
            var text = TestHelper.LoadEmbeddedTextResource("SPG.DockerCompose.Test.docker-compose.yml");

            File.WriteAllText("compose.yml", text);

            DockerComposeConvert.TryDeserialize(text, out var results).Should().BeTrue("Can parse file");

            results.Services["arcoservice"].Image.Should().Be("spgcontrols/arcoservice:develop");
        }

        /// <summary>
        /// Serialize/deserialize once to remove any properties that it didn't recognise, and then compare object trees & serailized text
        /// </summary>
        [Fact(DisplayName = "Docker: Serialize & deserialize a docker compose")]
        public void SerializeComposeTest()
        {
            var text = TestHelper.LoadEmbeddedTextResource("SPG.DockerCompose.Test.docker-compose.yml");

            // Serialize/deserialize once to remove any properties that it didn't recognise
            var compose = DockerComposeConfiguration.ReadFromString(text);

            string results = DockerComposeConfiguration.WriteToString(compose);

            var compose2 = DockerComposeConfiguration.ReadFromString(results);
            string results2 = DockerComposeConfiguration.WriteToString(compose2);

            compose.Should().BeEquivalentTo(compose2);
            results2.Should().BeEquivalentTo(results);

            results2.Should().Contain("cpus: '2'", "CPUs text should be a string");
        }
    }
}
