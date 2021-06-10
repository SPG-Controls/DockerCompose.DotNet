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
    public static class UpdateStackSecret
    {
        public static async Task<IEnumerable<string>> UpdateStackSecretAsync(this StackDeployer deployer, string secretName, string data, bool updateServices, ILogger log = null)
        {
            return await deployer.UpdateStackSecretAsync(secretName, Encoding.UTF8.GetBytes(data), updateServices, log);
        }

        public static async Task<IEnumerable<string>> UpdateStackSecretAsync(this StackDeployer deployer, string secretName, byte[] data, bool updateServices, ILogger log = null)
        {
            // Secret versioning must be enabled to update secret
            if (!deployer.VersionedSecrets.Contains(secretName))
                throw new InvalidOperationException("Secret versioning must be enabled");

            // Find latest secret name_version in docker compose
            // Doing this repairs the docker compose if the secret name_version is out of sync with docker
            var currentComposeSecretName = deployer.GetLatestSecretNameFromCompose(secretName);

            var topLevelSecret = deployer.DockerCompose.Secrets[currentComposeSecretName];

            var newNamePrefix = "";
            var realDockerSecretName = secretName;

            // Add stack name if its a file top level config
            // This is what docker stack deploy does
            if (topLevelSecret is FileTopLevelSecret)
            {
                realDockerSecretName = $"{deployer.StackName}_{secretName}";
                newNamePrefix = $"{deployer.StackName}_";
            }

            // Find latest version in docker secrets
            var secretVersionInfo = await deployer.Client.Secrets.GetLatestSecretVersionAsync(realDockerSecretName);
            if (secretVersionInfo == null)
                throw new SecretDoesNotExistException($"{secretName} not found in docker secrets");

            // Increment version
            string newSecretName = deployer.CreateVersionName(secretName, secretVersionInfo.Version + 1);

            // Create new secret
            var newSecretSpec = secretVersionInfo.Secret.Spec;
            newSecretSpec.Name = $"{newNamePrefix}{newSecretName}";
            newSecretSpec.Data = data;

            // Update docker compose top level secrets
            deployer.DockerCompose.Secrets.Remove(currentComposeSecretName);
            deployer.DockerCompose.Secrets.Add(newSecretName, topLevelSecret);

            var updatedServices = new List<string>();

            // Update docker compose service secrets
            foreach (var (serviceName, service) in deployer.DockerCompose.Services)
            {
                if (service.Secrets == null)
                    continue;

                bool serviceUpdated = false;

                foreach (var secret in service.Secrets)
                {
                    if (secret.Source == currentComposeSecretName)
                    {
                        serviceUpdated = true;
                        secret.Source = newSecretName;
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

            await deployer.Client.Secrets.CreateAsync(newSecretSpec);

            return updatedServices;
        }

        public static string GetLatestSecretNameFromCompose(this StackDeployer deployer, string secretName)
        {
            string latestComposeSecretName = null;

            foreach (var key in deployer.DockerCompose.Secrets.Keys)
            {
                // Find the version delimiter
                int idx = key.LastIndexOf('.');

                // Skip if no delimiter found
                if (idx < 0)
                    continue;

                // Extract name and version
                var baseSecretName = key.Substring(0, idx);
                var versionStr = key.Substring(idx + 1);

                // Check if the version is a valid number
                if (!ulong.TryParse(versionStr, out var version))
                    continue;

                // Check if names match
                if (baseSecretName == secretName)
                    latestComposeSecretName = key;
            }

            if (latestComposeSecretName == null)
                throw new InvalidOperationException($"Secret {secretName} does not exist in the compose file");

            return latestComposeSecretName;
        }
    }
}
