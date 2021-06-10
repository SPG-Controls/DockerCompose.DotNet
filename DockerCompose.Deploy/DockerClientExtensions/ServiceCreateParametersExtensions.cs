using Docker.DotNet.Models;
using System.Collections.Generic;

namespace DockerCompose.Deploy.DockerClientExtensions
{
    public static class ServiceCreateParametersExtensions
    {        
        public static ServiceCreateParameters AddSecrets(this ServiceCreateParameters parameters, IList<SecretReference> secrets)
        {
            parameters.Service.TaskTemplate.ContainerSpec.Secrets = secrets;

            return parameters;
        }

        public static ServiceCreateParameters AddConfigs(this ServiceCreateParameters parameters, IList<SwarmConfigReference> configs)
        {
            parameters.Service.TaskTemplate.ContainerSpec.Configs = configs;

            return parameters;
        }

        public static ServiceCreateParameters AddAuth(this ServiceCreateParameters parameters, AuthConfig dockerHubAuth)
        {
            parameters.RegistryAuth = dockerHubAuth;

            return parameters;
        }
    }
}
