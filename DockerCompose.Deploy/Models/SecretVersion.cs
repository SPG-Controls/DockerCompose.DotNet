using Docker.DotNet.Models;

namespace DockerCompose.Deploy.Models
{
    public class SecretVersion
    {
        public string Name { get; set; }

        public ulong Version { get; set; }

        public Secret Secret { get; set; }
    }
}
