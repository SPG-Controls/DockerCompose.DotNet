using DockerCompose.Deploy.DockerClientExtensions;
using DockerCompose.Deploy.DockerComposeExtensions;
using System.Threading.Tasks;

namespace DockerCompose.Deploy
{
    public static class UpdateStackService
    {
        public static async Task UpdateStackServiceAsync(this StackDeployer deployer, string serviceName)
        {
            // Inspect service to get latest version
            var serviceInspect = await deployer.Client.Swarm.InspectServiceAsync($"{deployer.StackName}_{serviceName}");

            // Get latest spec from compose
            var service = deployer.DockerCompose.Services[serviceName];

            // Get secret IDs and build references
            var secrets = await deployer.Client.GetSecretReferencesAsync(deployer.StackName, deployer.DockerCompose, service);

            // Get config IDs and build references
            var configs = await deployer.Client.GetConfigReferencesAsync(deployer.StackName, deployer.DockerCompose, service);

            // Build parameters
            var serviceUpdaterParameters = service
                .BuildUpdateParameters(deployer.StackName, serviceName, deployer.DockerCompose.Networks)
                .AddSecrets(secrets)
                .AddConfigs(configs)
                .AddAuth(deployer.DockerHubAuth)
                .AddVersion(serviceInspect.Version);

            // Update service spec
            await deployer.Client.Swarm.UpdateServiceAsync($"{deployer.StackName}_{serviceName}", serviceUpdaterParameters);
        }
    }
}
