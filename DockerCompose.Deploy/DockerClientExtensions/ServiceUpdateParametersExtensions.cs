using Docker.DotNet.Models;
using System.Collections.Generic;

namespace DockerCompose.Deploy.DockerClientExtensions
{
    public static class ServiceUpdateParametersExtensions
    {
        public static ServiceUpdateParameters AddSecrets(this ServiceUpdateParameters parameters, IList<SecretReference> secrets)
        {
            parameters.Service.TaskTemplate.ContainerSpec.Secrets = secrets;

            return parameters;
        }

        public static ServiceUpdateParameters AddConfigs(this ServiceUpdateParameters parameters, IList<SwarmConfigReference> configs)
        {
            parameters.Service.TaskTemplate.ContainerSpec.Configs = configs;

            return parameters;
        }

        public static ServiceUpdateParameters AddAuth(this ServiceUpdateParameters parameters, AuthConfig dockerHubAuth)
        {
            parameters.RegistryAuth = dockerHubAuth;

            return parameters;
        }

        public static ServiceUpdateParameters AddVersion(this ServiceUpdateParameters parameters, Version version)
        {
            parameters.Version = (long)version.Index;

            return parameters;
        }
    }
}
