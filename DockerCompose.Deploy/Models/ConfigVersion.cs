using Docker.DotNet.Models;

namespace DockerCompose.Deploy.Models
{
    public class ConfigVersion
    {
        public string Name { get; set; }

        public ulong Version { get; set; }

        public SwarmConfig Config { get; set; }
    }
}
