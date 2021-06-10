using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerCompose.Deploy.Models;
using Microsoft.Extensions.Logging;

namespace DockerCompose.Deploy.DockerClientExtensions
{
    public static class ConfigsOperationsExtensions
    {
        public static async Task<ConfigVersion> GetLatestConfigVersionAsync(this IConfigsOperations client, string configName, ILogger log = null)
        {
            log?.LogInformation($"Getting latest version of config: {configName}");

            var configs = await client.ListAsync();

            SwarmConfig latestConfig = null;
            ulong latestConfigVersion = 0;

            foreach (var config in configs)
            {
                // Filter secrets with correct name and a version
                if (config.TryGetNameVersion(out var name, out var version) &&
                    name == configName &&
                    (version > latestConfigVersion))
                {
                    latestConfig = config;
                    latestConfigVersion = version;
                }
            }

            if (latestConfig == null)
            {
                return null;
            }

            return new ConfigVersion
            {
                Name = configName,
                Version = latestConfigVersion,
                Config = latestConfig
            };
        }

        private static bool TryGetNameVersion(this SwarmConfig config, out string secretName, out ulong version)
        {
            // Set defaults
            secretName = null;
            version = 0;

            // Get version delimiter index
            var idx = config.Spec.Name.LastIndexOf('.');
            if (idx < 0)
            {
                return false;
            }

            // Extract name and version
            secretName = config.Spec.Name.Substring(0, idx);
            var versionStr = config.Spec.Name.Substring(idx + 1);

            // Parse version
            if (!ulong.TryParse(versionStr, out version))
                return false;

            return true;
        }
    }
}
