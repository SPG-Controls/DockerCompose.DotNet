using DockerCompose.Model.Enums;
using System;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DockerCompose.Model.Models
{
    public class CommandList : List<string>
    { 
        public CommandList() { }

        public CommandList(List<string> commands) : base(commands) { }
    }

    public class Service
    {
        public string Image { get; set; }

        public string Hostname { get; set; }

        public CommandList Entrypoint { get; set; }

        public CommandList Command { get; set; }

        public List<string> Environment { get; set; }

        public List<string> Expose { get; set; }

        public Sysctls Sysctls { get; set; }

        public List<IVolume> Volumes { get; set; }

        public List<Port> Ports { get; set; }

        public List<Config> Configs { get; set; }

        public List<Secret> Secrets { get; set; }

        public List<string> DependsOn { get; set; }

        public Dictionary<string, Network> Networks { get; set; }

        public Logging Logging { get; set; }

        public Deploy Deploy { get; set; }

        public HealthCheck Healthcheck { get; set; }
    }

    public class Sysctls : Dictionary<string, string> { }

    public class Logging
    {
        public string Driver { get; set; }

        public LoggingOptions Options { get; set; }
    }

    public class LoggingOptions
    {
        public string Driver { get; set; }

        [YamlMember(Alias = "max-size", ApplyNamingConventions = false)]
        public string MaxSize { get; set; }
    }

    public class Deploy
    {
        public int? Replicas { get; set; }

        public Placement Placement { get; set; }

        public RestartPolicy RestartPolicy { get; set; }

        public Resources Resources { get; set; }

        public Dictionary<string, string> Labels { get; set; }
    }

    public class PortAttribute
    {
        public string Target { get; set; }
    }

    public class Placement
    {
        public List<string> Constraints { get; set; }

        public int? MaxReplicasPerNode { get; set; }
    }

    public class RestartPolicy
    {
        public RestartCondition Condition { get; set; }

        public string Delay { get; set; }

        public int? MaxAttempts { get; set; }

        public string Window { get; set; }
    }

    public class Resources
    {
        public Limits Limits { get; set; }

        public Reservations Reservations { get; set; }
    }

    public class Limits
    {
        public string Cpus { get; set; }

        public string Memory { get; set; }
    }

    public class Reservations
    {
        public string Cpus { get; set; }

        public string Memory { get; set; }
    }

    public class HealthCheck
    {
        public CommandList test { get; set; }
        
        public string interval { get; set; }
        
        public string timeout { get; set; }

        public int? retries { get; set; }
    }

    public class Network
    {
        public List<string> Aliases { get; set; }

        public string Ipv4Address { get; set; }

        public string Ipv6Address { get; set; }
    }
}
