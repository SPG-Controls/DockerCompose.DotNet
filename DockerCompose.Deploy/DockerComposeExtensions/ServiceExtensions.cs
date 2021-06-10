using Docker.DotNet.Models;
using System.Collections.Generic;
using System.Linq;

namespace DockerCompose.Deploy.DockerComposeExtensions
{
    public static class ServiceExtensions
    {
        /// <summary>
        /// Build a list of ports from the docker-compose ports
        /// </summary>
        public static ServiceCreateParameters BuildCreateParameters(this Model.Models.Service service, string stackName, string serviceName, Dictionary<string, Model.Models.TopLevelNetwork> topLevelNetworks)
        {
            //TODO: Healthcheck

            var labels = service.BuildLabels(stackName);
            var mounts = service.BuildMounts(stackName);
            var endpointSpec = service.BuildEndpointSpec();
            var networks = service.BuildNetworks(stackName, serviceName, topLevelNetworks);
            var placement = service.BuildPlacement();

            return new ServiceCreateParameters
            {
                Service = new ServiceSpec
                {
                    Name = $"{stackName}_{serviceName}",
                    TaskTemplate = new TaskSpec
                    {
                        ContainerSpec = new ContainerSpec
                        {
                            Image = service.Image,
                            Command = service.Entrypoint,
                            Args = service.Command,
                            Hostname = service.Hostname,
                            Env = service.Environment,
                            Labels = labels,
                            Mounts = mounts,
                            Sysctls = service.Sysctls
                        },
                        RestartPolicy = DefaultRestartPolicy,
                        Networks = networks,
                        Placement = placement
                    },
                    EndpointSpec = endpointSpec,
                    Labels = new Dictionary<string, string>()
                    {
                        { "com.docker.stack.image", service.Image },
                        { "com.docker.stack.namespace", stackName }
                    },
                    Mode = service?.Deploy == null ? DefaultServiceMode : new ServiceMode
                    {
                        // TODO: Currently doesn't support placement on manager!
                        Replicated = new ReplicatedService
                        {
                            Replicas = service.Deploy.Replicas.HasValue ?
                                (ulong)service.Deploy.Replicas.Value : 1
                        }
                    },
                }
            };
        }

        /// <summary>
        /// Build a list of ports from the docker-compose ports
        /// </summary>
        public static ServiceUpdateParameters BuildUpdateParameters(this Model.Models.Service service, string stackName, string serviceName, Dictionary<string, Model.Models.TopLevelNetwork> topLevelNetworks)
        {
            var labels = service.BuildLabels(stackName);
            var mounts = service.BuildMounts(stackName);
            var endpointSpec = service.BuildEndpointSpec();
            var networks = service.BuildNetworks(stackName, serviceName, topLevelNetworks);
            var placement = service.BuildPlacement();

            return new ServiceUpdateParameters
            {
                Service = new ServiceSpec
                {
                    Name = $"{stackName}_{serviceName}",
                    TaskTemplate = new TaskSpec
                    {
                        ContainerSpec = new ContainerSpec
                        {
                            Image = service.Image,
                            Command = service.Entrypoint,
                            Args = service.Command,
                            Env = service.Environment,
                            Labels = labels,
                            Mounts = mounts,
                            Sysctls = service.Sysctls
                        },
                        RestartPolicy = DefaultRestartPolicy,
                        Networks = networks,
                        Placement = placement
                    },
                    EndpointSpec = endpointSpec,
                    Labels = new Dictionary<string, string>()
                    {
                        { "com.docker.stack.image", service.Image },
                        { "com.docker.stack.namespace", stackName }
                    },
                    Mode = service.Deploy.Replicas <= 1 ? DefaultServiceMode : new ServiceMode
                    {
                        // TODO: Currently doesn't support placement on manager!
                        Replicated = new ReplicatedService
                        {
                            Replicas = 1
                        }
                    },
                }
            };
        }

        public static Dictionary<string, string> BuildLabels(this Model.Models.Service service, string stackName)
        {
            // Add docker stack label
            var labels = new Dictionary<string, string>()
            {
                { "com.docker.stack.namespace", stackName }
            };

            // Get compose labels
            if (service?.Deploy?.Labels != null)
            {
                foreach (var label in service.Deploy.Labels)
                {
                    labels.Add(label.Key, label.Value);
                }
            }

            return labels;
        }

        /// <summary>
        /// Build a list of mounts from the docker-compose volumes
        /// </summary>
        public static List<Mount> BuildMounts(this Model.Models.Service service, string stackName)
        {
            if (service?.Volumes == null)
                return null;

            return service.Volumes.Select(volume => volume.BuildMount(stackName)).ToList();
        }

        /// <summary>
        /// Build a list of ports from the docker-compose ports
        /// </summary>
        public static EndpointSpec BuildEndpointSpec(this Model.Models.Service service)
        {
            if (service?.Ports == null)
                return null;

            return new EndpointSpec
            {
                Ports = service.Ports.Select(port => port.BuildPortConfig()).ToList()
            };
        }

        public static List<NetworkAttachmentConfig> BuildNetworks(this Model.Models.Service service, string stackName, string serviceName, Dictionary<string, Model.Models.TopLevelNetwork> topLevelNetworks)
        {
            if (topLevelNetworks == null)
                return null;

            return service.Networks.Select(network => 
            {
                var topLevelNetwork = topLevelNetworks[network.Key];
                return new NetworkAttachmentConfig
                {
                    // Don't prepend stack name if it's an external network
                    Target = topLevelNetwork.External.HasValue && topLevelNetwork.External.Value
                        ? network.Key
                        : $"{stackName}_{network.Key}",
                    Aliases = network.Value?.Aliases == null ?
                        new List<string> { serviceName } :
                        network.Value.Aliases.Append(serviceName).ToList()
                };
            }).ToList();
        }

        public static Placement BuildPlacement(this Model.Models.Service service)
        {
            var placement = new Placement
            {
                Constraints = service.Deploy?.Placement?.Constraints
            };

            if (service.Deploy?.Placement?.MaxReplicasPerNode != null)
                placement.MaxReplicas = (ulong)service.Deploy.Placement.MaxReplicasPerNode;

            return placement;
        }

        // Defaults

        public static SwarmRestartPolicy DefaultRestartPolicy => new SwarmRestartPolicy
        {
            Condition = "any"
        };

        public static ServiceMode DefaultServiceMode => new ServiceMode
        {
            Replicated = new ReplicatedService
            {
                Replicas = 1
            }
        };
    }
}
