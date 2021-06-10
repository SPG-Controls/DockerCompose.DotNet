using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using DockerCompose.Deploy.DockerClientExtensions;
using DockerCompose.Model;
using DockerCompose.Model.Models;
using Microsoft.Extensions.Logging;

namespace DockerCompose.Deploy
{
    public class StackDeployer
    {
        private static ulong INITIAL_VERSION = 1;

        public DockerClient Client;
        public string StackName;
        public DockerComposeConfiguration DockerCompose;
        public AuthConfig DockerHubAuth;

        public List<string> VersionedSecrets { get; set; }

        public List<string> VersionedConfigs { get; set; }

        public StackDeployer(DockerClient client, string stackName)
        {
            if (client == null)
                throw new ArgumentNullException(nameof(client));

            if (string.IsNullOrEmpty(stackName))
                throw new ArgumentNullException(nameof(stackName));

            Client = client;
            StackName = stackName;
            VersionedSecrets = new List<string>();
            VersionedConfigs = new List<string>();
        }

        public StackDeployer(DockerClient client, string stackName, DockerComposeConfiguration dockerCompose)
            : this(client, stackName)
        {
            if (dockerCompose == null)
                throw new ArgumentNullException(nameof(dockerCompose));

            DockerCompose = dockerCompose;
        }

        public void SetDockerCompose(DockerComposeConfiguration dockerCompose)
        {
            if (dockerCompose == null)
                throw new ArgumentNullException(nameof(dockerCompose));

            DockerCompose = dockerCompose;
        }

        public void AddDockerHubAuth(AuthConfig dockerHubAuth)
        {
            if (dockerHubAuth == null)
                throw new ArgumentNullException(nameof(dockerHubAuth));

            DockerHubAuth = dockerHubAuth;
        }

        #region Services

        public async Task AddStackService(string serviceName, Service service)
        {
            // Convert secret names to latest version names
            if (service.Secrets != null)
            {
                foreach (var secret in service.Secrets)
                {
                    if (VersionedSecrets.Contains(secret.Source))
                    {
                        var latestSecret = await Client.Secrets.GetLatestSecretVersionAsync(secret.Source);
                        secret.Source = latestSecret.Secret.Spec.Name;
                    }
                }
            }

            DockerCompose.Services[serviceName] = service;
        }

        #endregion Services

        #region Secrets

        public async Task<SecretCreateResponse> DeployStackSecretAsync(string secretName, string data, bool enableVersioning = false, IDictionary<string, string> labels = null, ILogger log = null)
        {
            return await DeployStackSecretAsync(secretName, Encoding.UTF8.GetBytes(data), enableVersioning, labels, log);
        }

        public async Task<SecretCreateResponse> DeployStackSecretAsync(string secretName, byte[] data, bool enableVersioning = false, IDictionary<string, string> labels = null, ILogger log = null)
        {
            if (DockerCompose == null)
                throw new InvalidOperationException("Docker compose not defined");

            // Find secret in docker compose
            var secret = DockerCompose.Secrets[secretName];

            var secretNameVersion = secretName;

            // Make sure labels are initialised
            if (labels == null) labels = new Dictionary<string, string>();

            // Convert to versioning
            if (enableVersioning)
                secretNameVersion = EnableSecretVersioning(secretName);

            // Add stack label if file config
            if (secret is FileTopLevelSecret)
            {
                secretNameVersion = $"{StackName}_{secretNameVersion}";
                labels.Add("com.docker.stack.namespace", StackName);
            }

            return await Client.Secrets.CreateAsync(new SecretSpec
            {
                Name = secretNameVersion,
                Data = data,
                Labels = labels
            });
        }

        public string EnableSecretVersioning(string secretName)
        {
            var secret = DockerCompose.Secrets[secretName];

            var secretNameVersion = CreateVersionName(secretName, INITIAL_VERSION);

            // Change docker compose to new secret name_version
            DockerCompose.Secrets.Remove(secretName);
            DockerCompose.Secrets[secretNameVersion] = secret;

            // Update all secret names in services
            foreach (var (serviceName, service) in DockerCompose.Services)
            {
                if (service.Secrets == null)
                    continue;

                foreach (var serviceSecret in service.Secrets.Where(conf => conf.Source == secretName))
                {
                    serviceSecret.Source = secretNameVersion;
                }
            }

            // Add to list of versioned secrets
            VersionedSecrets.Add(secretName);

            return secretNameVersion;
        }

        #endregion Secrets

        #region Configs

        public async Task<ConfigCreateResponse> DeployStackConfigAsync(string configName, string data, bool enableVersioning = false, IDictionary<string, string> labels = null, ILogger log = null)
        {
            return await DeployStackConfigAsync(configName, Encoding.UTF8.GetBytes(data), enableVersioning, labels, log);
        }

        public async Task<ConfigCreateResponse> DeployStackConfigAsync(string configName, byte[] data, bool enableVersioning = false, IDictionary<string, string> labels = null, ILogger log = null)
        {
            if (DockerCompose == null)
                throw new InvalidOperationException("Docker compose not defined");

            // Find config in docker compose
            if (!DockerCompose.Configs.TryGetValue(configName, out var config))
            {
                throw new InvalidOperationException("Can't locate config in compose : " + configName);
            }

            var configNameVersion = configName;

            // Make sure labels are initialised
            if (labels == null)
                labels = new Dictionary<string, string>();

            // Convert to versioning
            if (enableVersioning)
                configNameVersion = EnableConfigVersioning(configName);

            // Add stack label if file config
            if (config is FileTopLevelConfig)
            {
                configNameVersion = $"{StackName}_{configNameVersion}";
                labels.Add("com.docker.stack.namespace", StackName);
            }

            return await Client.Configs.CreateAsync(new ConfigSpec
            {
                Name = configNameVersion,
                Data = data,
                Labels = labels
            });
        }

        public string EnableConfigVersioning(string configName)
        {
            var config = DockerCompose.Configs[configName];

            var configNameVersion = CreateVersionName(configName, INITIAL_VERSION);

            // Change docker compose to new config name_version
            DockerCompose.Configs.Remove(configName);
            DockerCompose.Configs[configNameVersion] = config;

            // Update all config names in services
            foreach (var (serviceName, service) in DockerCompose.Services)
            {
                if (service.Configs == null)
                    continue;

                foreach (var serviceConfig in service.Configs.Where(conf => conf.Source == configName))
                {
                    serviceConfig.Source = configNameVersion;
                }
            }

            // Add to list of versioned configs
            VersionedConfigs.Add(configName);

            return configNameVersion;
        }

        #endregion Configs

        #region Versions

        public string CreateVersionName(string name, ulong version)
        {
            return $"{name}.{version}";
        }

        public string GetConfigVersionName(string name)
        {
            return $"{name}";
        }

        #endregion Versions
    }
}
