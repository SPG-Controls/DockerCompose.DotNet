using DockerCompose.Deploy.DockerClientExtensions;
using DockerCompose.Deploy.Exceptions;
using DockerCompose.Model.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DockerCompose.Deploy
{
    public static class UpdateStackConfig
    {
        public static async Task<IEnumerable<string>> UpdateStackConfigAsync(this StackDeployer deployer, string configName, string data, bool updateServices, ILogger log = null)
        {
            return await deployer.UpdateStackConfigAsync(configName, Encoding.UTF8.GetBytes(data), updateServices, log);
        }

        public static async Task<IEnumerable<string>> UpdateStackConfigAsync(this StackDeployer deployer, string configName, byte[] data, bool updateServices, ILogger log = null)
        {
            // Config versioning must be enabled to update config
            if (!deployer.VersionedConfigs.Contains(configName))
                throw new InvalidOperationException("Config versioning must be enabled");

            // Find latest config name_version in docker compose
            // Doing this repairs the docker compose if the config name_version is out of sync with docker
            var currentComposeConfigName = deployer.GetLatestConfigNameFromCompose(configName);

            var topLevelConfig = deployer.DockerCompose.Configs[currentComposeConfigName];

            var newNamePrefix = "";
            var realDockerConfigName = configName;

            // Add stack name if its a file top level config
            // This is what docker stack deploy does
            if (topLevelConfig is FileTopLevelConfig)
            {
                realDockerConfigName = $"{deployer.StackName}_{configName}";
                newNamePrefix = $"{deployer.StackName}_";
            }

            // Find latest version in docker configs
            var configVersionInfo = await deployer.Client.Configs.GetLatestConfigVersionAsync(realDockerConfigName);
            if (configVersionInfo == null)
                throw new ConfigDoesNotExistException($"{configName} not found in docker configs");

            // Increment version
            var newConfigName = deployer.CreateVersionName(configName, configVersionInfo.Version + 1);

            // Create new config
            var newConfigSpec = configVersionInfo.Config.Spec;
            newConfigSpec.Name = $"{newNamePrefix}{newConfigName}";
            newConfigSpec.Data = data;

            // Update docker compose top level configs
            deployer.DockerCompose.Configs.Remove(currentComposeConfigName);
            deployer.DockerCompose.Configs.Add(newConfigName, topLevelConfig);

            var updatedServices = new List<string>();

            // Update docker compose service config
            foreach (var (serviceName, service) in deployer.DockerCompose.Services)
            {
                if (service.Configs == null)
                    continue;

                bool serviceUpdated = false;
                
                // Update all the secret versions
                foreach (var config in service.Configs)
                {
                    if (config.Source == currentComposeConfigName)
                    {
                        serviceUpdated = true;
                        config.Source = newConfigName;
                    }
                }

                // Update service spec if changed and requested
                if (serviceUpdated)
                {
                    // Add to list of updated services
                    updatedServices.Add(serviceName);

                    // Update service spec if requested
                    if (updateServices)
                        await deployer.UpdateStackServiceAsync(serviceName);
                }
            }

            await deployer.Client.Configs.CreateAsync(newConfigSpec);

            return updatedServices;
        }

        public static string GetLatestConfigNameFromCompose(this StackDeployer deployer, string configName)
        {
            string latestComposeConfigName = null;

            foreach (var key in deployer.DockerCompose.Configs.Keys)
            {
                Console.WriteLine(key);
                // Find the version delimiter
                int idx = key.LastIndexOf('.');

                // Skip if no delimiter found
                if (idx < 0)
                    continue;

                // Extract name and version
                var baseConfigName = key.Substring(0, idx);
                var versionStr = key.Substring(idx + 1);

                // Check if version number is valid
                if (!ulong.TryParse(versionStr, out var version))
                    continue;

                // Check if names match
                if (baseConfigName == configName)
                    latestComposeConfigName = key;
            }

            if (latestComposeConfigName == null)
            {
                throw new ConfigDoesNotExistException($"Config {configName} does not exist in the compose file");
            }

            return latestComposeConfigName;
        }
    }
}
