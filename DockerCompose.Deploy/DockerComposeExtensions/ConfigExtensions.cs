using Docker.DotNet.Models;

namespace DockerCompose.Deploy.DockerComposeExtensions
{
    public static class ConfigExtensions
    {
        /// <summary>
        /// Build a list of mounts from the docker-compose volumes
        /// </summary>
        public static SwarmConfigReference BuildConfigReference(this Model.Models.Config config, string configId)
        {
            return new SwarmConfigReference
            {
                File = new ConfigReferenceFileTarget
                {
                    Name = config.Target,
                    GID = "0",
                    UID = "0",
                    Mode = 0444
                },
                ConfigName = config.Source,
                ConfigID = configId
            };
        }
    }
}
