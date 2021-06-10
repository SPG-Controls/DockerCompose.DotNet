using System;
using DockerCompose.Model.Models;
using Docker.DotNet.Models;
using System.Threading.Tasks;
using DockerCompose.Deploy.Enums;
using Microsoft.Extensions.Logging;
using DockerCompose.Deploy.DockerClientExtensions;
using DockerCompose.Deploy.DockerComposeExtensions;

namespace DockerCompose.Deploy
{
    public static class DeployStackService
    {
        public static async Task<ServiceCreateResponse> DeployStackServiceAsync(this StackDeployer deployer, string serviceName, string stdOutListenPhrase, int occurances = 1, ILogger log = null)
        {
            var response = await deployer.DeployStackServiceAsync(serviceName, ServiceDeployMode.WaitForTasks, log: log);

            // Task to listen for output in the container
            log?.LogInformation($"{serviceName}: Waiting for service to be ready");
            await deployer.Client.ListenForOutputInService(deployer.StackName, serviceName, stdOutListenPhrase, occurances, log: log);

            return response;
        }

        public static async Task<ServiceCreateResponse> DeployStackServiceAsync(this StackDeployer deployer, string serviceName, ServiceDeployMode serviceDeployMode = ServiceDeployMode.WaitForTasks, int? waitForReplicas = null, ILogger log = null)
        {
            if (string.IsNullOrEmpty(serviceName))
                throw new ArgumentNullException(nameof(serviceName));

            Service service = deployer.DockerCompose.Services[serviceName];

            log?.LogInformation($"{serviceName}: Building service specification");
            
            // Get secret IDs and build references
            var secrets = await deployer.Client.GetSecretReferencesAsync(deployer.StackName, deployer.DockerCompose, service);

            // Get config IDs and build references
            var configs = await deployer.Client.GetConfigReferencesAsync(deployer.StackName, deployer.DockerCompose, service);

            // Build parameters
            var serviceCreateParameters = service
                .BuildCreateParameters(deployer.StackName, serviceName, deployer.DockerCompose.Networks)
                .AddSecrets(secrets)
                .AddConfigs(configs)
                .AddAuth(deployer.DockerHubAuth);

            // Create service
            log?.LogInformation($"{serviceName}: Creating service");
            var response = await deployer.Client.Swarm.CreateServiceAsync(serviceCreateParameters);

            // Wait for service tasks to start
            if (serviceDeployMode == ServiceDeployMode.WaitForTasks)
            {
                // Wait for tasks to start
                log?.LogInformation($"{serviceName}: Waiting for service task to start");
                await deployer.Client.WaitForServiceTasksToStartAsync(deployer.StackName, serviceName, waitForReplicas ?? service?.Deploy?.Replicas ?? 1, 60 * 5, log: log);
            }

            return response;
        }
    }
}
