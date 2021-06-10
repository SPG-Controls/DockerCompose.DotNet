using DockerCompose.Model.Enums;
using System.Collections.Generic;

namespace DockerCompose.Model.Models
{
    public class TopLevelNetwork
    {
        public bool? External { get; set; }

        public NetworkDriver? Driver { get; set; }

        public bool? Attachable { get; set; }

        public IDictionary<string, string> DriverOpts { get; set; }
    }
}
