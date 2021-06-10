using DockerCompose.Model;
using FluentAssertions;
using Xunit;
using SPG.DockerCompose.Test.Helpers;

namespace SPG.DockerCompose.Test
{
    public class DockerComposeNetworkTests
    {
        [Fact(DisplayName = "Docker: Check & update a docker compose")]
        public void CheckComposeTest()
        {
            var text = TestHelper.LoadEmbeddedTextResource("SPG.DockerCompose.Test.docker-compose-networks.yml");

            var compose = DockerComposeConfiguration.ReadFromString(text);
            string results = DockerComposeConfiguration.WriteToString(compose);

            var compose2 = DockerComposeConfiguration.ReadFromString(results);
            string results2 = DockerComposeConfiguration.WriteToString(compose2);

            compose.Should().BeEquivalentTo(compose2);
            results2.Should().BeEquivalentTo(results);
        }
    }
}
