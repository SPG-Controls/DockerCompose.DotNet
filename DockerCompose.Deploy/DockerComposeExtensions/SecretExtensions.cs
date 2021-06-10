using Docker.DotNet.Models;

namespace DockerCompose.Deploy.DockerComposeExtensions
{
    public static class SecretExtensions
    {
        /// <summary>
        /// Build a list of mounts from the docker-compose volumes
        /// </summary>
        public static SecretReference BuildSecretReference(this Model.Models.Secret secret, string secretID)
        {
            return new SecretReference
            {
                File = new SecretReferenceFileTarget
                {
                    Name = secret.Target,
                    GID = "0",
                    UID = "0",
                    Mode = 0444
                },
                SecretName = secret.Source,
                SecretID = secretID
            };
        }
    }
}
