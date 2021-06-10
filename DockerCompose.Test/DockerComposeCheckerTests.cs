using DockerCompose.Model;
using DockerCompose.Model.Enums;
using SPG.Common.Docker;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using SPG.Docker.API.Handlers;
using SPG.DockerCompose.Test.Helpers;

namespace SPG.DockerCompose.Test
{
    public class DockerComposeCheckerTests
    {
        [Fact(DisplayName = "Docker: Check & update a docker compose")]
        public void CheckComposeTest()
        {
            var text = TestHelper.LoadEmbeddedTextResource("SPG.DockerCompose.Test.docker-compose.yml");

            var compose = DockerComposeConfiguration.ReadFromString(text);

            var checker = new DockerComposeChecker(new NullLoggerFactory());

            var result = checker.UpdateToIdeal(compose, out var missingSecrets, out var _missingConfigs);

            result.Services[Defaults.ArcoServiceName].DependsOn.Contains("elasticsearch").Should().BeTrue("Should have added elasticservice dependancy");

            var s1000service = result.Services[Defaults.S1000GatewayServiceName];
            s1000service.Deploy.RestartPolicy.Condition.Should().Be(RestartCondition.OnFailure, "Should restart on failure");
            s1000service.Deploy.Resources.Limits.Cpus.Should().Be("2", "Should limit to 2 CPUs");
            s1000service.Deploy.Resources.Reservations.Memory.Should().Be("2G", "Should reserve 2GB");
        }
    }
}
