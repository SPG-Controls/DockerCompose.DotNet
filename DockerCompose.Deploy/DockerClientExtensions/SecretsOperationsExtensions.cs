using Docker.DotNet;
using Docker.DotNet.Models;
using DockerCompose.Deploy.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace DockerCompose.Deploy.DockerClientExtensions
{
    public static class SecretsOperationsExtensions
    {
        public static async Task<SecretVersion> GetLatestSecretVersionAsync(this ISecretsOperations client, string secretName, ILogger log = null)
        {
            log?.LogInformation($"Getting latest version of secret: {secretName}");

            var secrets = await client.ListAsync();

            Docker.DotNet.Models.Secret latestSecret = null;
            ulong latestSecretVersion = 0;

            foreach (var secret in secrets)
            {
                // Filter secrets with correct name and a version
                if (secret.TryGetNameVersion(out var name, out var version) &&
                    name == secretName &&
                    version > latestSecretVersion)
                {
                    latestSecret = secret;
                    latestSecretVersion = version;
                }
            }

            if (latestSecret == null)
                return null;

            return new SecretVersion
            {
                Name = secretName,
                Version = latestSecretVersion,
                Secret = latestSecret
            };
        }

        private static bool TryGetNameVersion(this Secret secret, out string secretName, out ulong version)
        {
            // Set defaults
            secretName = null;
            version = 0;

            // Get version delimiter index
            var idx = secret.Spec.Name.LastIndexOf('.');
            if (idx < 0)
                return false;
            
            // Extract name and version
            secretName = secret.Spec.Name.Substring(0, idx);
            var versionStr = secret.Spec.Name.Substring(idx + 1);

            // Parse version
            if (!ulong.TryParse(versionStr, out version))
                return false;

            return true;
        }
    }
}
