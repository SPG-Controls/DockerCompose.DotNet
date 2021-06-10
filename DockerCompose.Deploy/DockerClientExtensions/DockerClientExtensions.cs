using Docker.DotNet;
using Docker.DotNet.Models;
using DockerCompose.Deploy.DockerComposeExtensions;
using DockerCompose.Model;
using DockerCompose.Model.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Diagnostics;

namespace DockerCompose.Deploy.DockerClientExtensions
{
    public static class DockerClientExtensions
    {        
        public static async Task<IList<SecretReference>> GetSecretReferencesAsync(this DockerClient client, string stackName, DockerComposeConfiguration compose, Service service)
        {
            if (service.Secrets == null)
                return null;

            var secretList = await client.Secrets.ListAsync();

            return service.Secrets
                .Select(secret =>
                {
                    var topLevelSecret = compose.Secrets[secret.Source];
                    if (topLevelSecret is FileTopLevelSecret)
                    {
                        secret.Source = $"{stackName}_{secret.Source}";
                    }
                    var secretID = secretList.First(secretItem => secretItem.Spec.Name == secret.Source).ID;
                    return secret.BuildSecretReference(secretID);
                })
                .ToList();
        }

        public static async Task<IList<SwarmConfigReference>> GetConfigReferencesAsync(this DockerClient client, string stackName, DockerComposeConfiguration compose, Service service)
        {
            if (service.Configs == null)
                return null;

            var configList = await client.Configs.ListAsync();

            return service.Configs
                .Select(config =>
                {
                    var topLevelConfig = compose.Configs[config.Source];
                    if (topLevelConfig is FileTopLevelConfig)
                    {
                        config.Source = $"{stackName}_{config.Source}";
                    }
                    var configId = configList.First(configItem => configItem.Spec.Name == config.Source).ID;
                    return config.BuildConfigReference(configId);
                })
                .ToList();
        }

        public static async Task WaitForServiceTasksToStartAsync(this DockerClient client, string stackName, string serviceName, int? replicas = 1, int timeout = 120, ILogger log = null)
        {
            replicas = replicas.HasValue ? replicas : 1;

            log?.LogInformation($"{serviceName}: Waiting for {replicas} tasks to start");

            var timer = new Stopwatch();
            timer.Start();

            // Poll the state of the service
            while (timer.Elapsed.TotalSeconds < timeout)
            {
                // Gets tasks by the service id
                var tasks = await client.Tasks.ListAsync(new TasksListParameters
                {
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                    {
                        {
                            "service", new Dictionary<string, bool>
                            {
                                { $"{stackName}_{serviceName}", true }
                            }
                        }
                    }
                });

                var runningTasks = tasks.Where(task => task.Status.State == TaskState.Running);

                log?.LogInformation($"{serviceName}: Num running tasks {runningTasks.Count()} / {tasks.Count()}");
                log?.LogInformation($"{serviceName}: {tasks.FirstOrDefault()?.Status?.State}");

                if (runningTasks.Count() >= replicas) {
                    log?.LogInformation($"{serviceName}: All tasks running");
                    return;
                }

                // Wait before polling again
                await Task.Delay(1000);
            }

            log?.LogError($"{serviceName}: Timeout starting service");
            throw new Exception("Timeout starting service");
        }

        // TODO support multiple replicas -> get task logs for each tasks
        public static async Task ListenForOutputInService(this DockerClient client, string stackName, string serviceName, string stdOutListenPhrase, int occurances, ILogger log = null)
        {
            // Create stream of the containers output
            var stream = await client.Swarm.GetServiceLogsAsync($"{stackName}_{serviceName}", false, new ServiceLogsParameters
            {
                Follow = true, // Keep connection open to listen for new output in container
                ShowStdout = true,
                ShowStderr = true
            });

            int numOccurances = 0;

            // Listen for phrase in stream
            var reader = new MultiplexedStreamReader(stream);

            while (true)
            {
                var line = await reader.ReadLineAsync(default);

                log?.LogInformation($"{line}");

                // Check if line contains the phrase
                if (line != default && line.Contains(stdOutListenPhrase))
                    numOccurances++;

                // Check if number of occurances has been reached
                if (numOccurances == occurances)
                    return;
            }
        }

        public static async Task WaitContainerAsync(this DockerClient client, string containerId, ILogger log = null)
        {
            try
            {
                if (string.IsNullOrEmpty(containerId))
                {
                    log?.LogInformation("Container has already been removed");

                    return;
                }

                else
                {
                    await client.Containers.WaitContainerAsync(containerId);
                }
            }

            catch (DockerApiException e)
            {
                if (e.StatusCode == HttpStatusCode.NotFound)
                    return;
                else
                    throw;
            }
        }

        /// <summary>
        /// Removes a service and waits for the tasks to exit
        /// </summary>
        /// <param name="client"></param>
        /// <param name="serviceName"></param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task RemoveServiceAsync(this DockerClient client, string serviceName, ILogger log = null)
        {
            try
            {
                var waitTask = client.Tasks.WaitServiceTasksAsync(serviceName);

                await client.Swarm.RemoveServiceAsync(serviceName);

                await waitTask;
            }

            catch (DockerApiException ex)
            {
                if (ex.StatusCode != HttpStatusCode.NotFound)
                    throw;
            }
        }

        /// <summary>
        /// Initialize the swarm with checks
        /// </summary>
        public static async Task InitSwarmWithChecksAsync(this DockerClient client, string advertiseAddress = null, ILogger log = null)
        {
            try
            {
                log?.LogInformation("Initialising swarm");

                var systemInfo = await client.System.GetSystemInfoAsync();
                if (systemInfo == null)
                {
                    log?.LogError("Docker system information not available!");
                    throw new ApplicationException("Docker unavailable");
                }

                if (systemInfo.Swarm?.Cluster != null)
                {
                    log?.LogInformation("Swarm already initialised");
                    return;
                }

                await client.Swarm.InitSwarmAsync(new SwarmInitParameters
                {
                    ListenAddr = "0.0.0.0",
                    AdvertiseAddr = advertiseAddress,
                });
            }

            catch (DockerApiException e)
            {
                log?.LogError(e, "Error while initialising swarm");
                throw;
            };
        }
    }
}
