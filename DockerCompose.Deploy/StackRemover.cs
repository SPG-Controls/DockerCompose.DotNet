using Docker.DotNet;
using DockerCompose.Deploy.DockerClientExtensions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace DockerCompose.Deploy
{
    public static class StackRemover
    {
        public static async Task RemoveStack(this DockerClient client, string stackName, ILogger log = null)
        {
            try
            {
                // Remove and Wait for Containers
                var swarmTasks = await client.Tasks.ListAsync();

                // Remove Services
                log?.LogInformation("Removing Services");
                var services = await client.Swarm.ListServicesAsync();
                var serviceIds = await Task.WhenAll(services
                    .Where(service => service.Spec.Labels.ContainsKey("com.docker.stack.namespace"))
                    .Where(service => service.Spec.Labels["com.docker.stack.namespace"] == stackName)
                    .Select(async service =>
                    {
                        await client.Swarm.RemoveServiceAsync(service.ID);

                        return service.ID;
                    }));

                // Remove and Wait for Containers
                log?.LogInformation("Waiting for tasks to exit");

                var tasks = swarmTasks
                    ?.Where(task => serviceIds.Contains(task.ServiceID))
                    ?.Select(task => client.WaitContainerAsync(task?.Status?.ContainerStatus?.ContainerID));

                if (tasks != null && tasks.Any())
                    await Task.WhenAll(tasks);

                // Remove Networks
                log?.LogInformation("Removing Networks");
                var networks = await client.Networks.ListNetworksAsync();
                await Task.WhenAll(networks
                    .Where(network => network.Labels != null)
                    .Where(network => network.Labels.ContainsKey("com.docker.stack.namespace"))
                    .Where(network => network.Labels["com.docker.stack.namespace"] == stackName)
                    .Select(network => client.Networks.DeleteNetworkAsync(network.ID)));

                // Remove Secrets
                log?.LogInformation("Removing Secrets");
                var secrets = await client.Secrets.ListAsync();
                await Task.WhenAll(secrets
                    .Where(secret => secret.Spec.Labels.ContainsKey("com.docker.stack.namespace"))
                    .Where(secret => secret.Spec.Labels["com.docker.stack.namespace"] == stackName)
                    .Select(secret => client.Secrets.DeleteAsync(secret.ID)));

                // Remove Configs
                log?.LogInformation("Removing Configs");
                var configs = await client.Configs.ListAsync();
                await Task.WhenAll(configs
                    .Where(config => config.Spec.Labels.ContainsKey("com.docker.stack.namespace"))
                    .Where(config => config.Spec.Labels["com.docker.stack.namespace"] == stackName)
                    .Select(config => client.Configs.DeleteAsync(config.ID)));
            }

            catch (DockerApiException ex)
            {
                if (ex.StatusCode == System.Net.HttpStatusCode.ServiceUnavailable)
                {
                    log?.LogInformation("Node not in swarm mode, nothing to remove");
                }

                else
                {
                    throw;
                }
            }
        }
    }
}
