using DockerCompose.Model.Models;
using System.Collections.Generic;

namespace DockerCompose.Model
{
    public static class DockerComposeBuilder
    {
        public static Dictionary<string, Service> BuildServices()
        {
            return new Dictionary<string, Service>();
        }

        public static Dictionary<string, TopLevelNetwork> BuildNetworks()
        {
            return new Dictionary<string, TopLevelNetwork>();
        }

        public static Dictionary<string, TopLevelVolume> BuildVolumes()
        {
            return new Dictionary<string, TopLevelVolume>();
        }

        public static Dictionary<string, ITopLevelSecret> BuildSecrets()
        {
            return new Dictionary<string, ITopLevelSecret>();
        }

        public static Dictionary<string, ITopLevelConfig> BuildConfigs()
        {
            return new Dictionary<string, ITopLevelConfig>();
        }
    }
}
