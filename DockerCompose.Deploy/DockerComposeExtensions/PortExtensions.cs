using Docker.DotNet.Models;

namespace DockerCompose.Deploy.DockerComposeExtensions
{
    public static class PortExtensions
    {
        /// <summary>
        /// Build a list of mounts from the docker-compose volumes
        /// </summary>
        public static PortConfig BuildPortConfig(this Model.Models.Port port)
        {
            return new PortConfig
            {
                Protocol = port.Protocol,
                PublishedPort = port.Published.Value,
                TargetPort = port.Target.Value,
                PublishMode = port.Mode
            };
        }
    }
}
