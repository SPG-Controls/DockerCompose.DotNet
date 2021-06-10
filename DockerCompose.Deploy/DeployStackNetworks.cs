using System.Threading.Tasks;
using System.Collections.Generic;
using Docker.DotNet.Models;
using DockerCompose.Model.Extensions;

namespace DockerCompose.Deploy
{
    public static class DeployStackNetworks
    {
        public static async Task DeployStackNetworksAsync(this StackDeployer deployer)
        {
            foreach (var (networkName, network) in deployer.DockerCompose.Networks)
            {
                if (!(network.External.HasValue && network.External.Value))
                {
                    await deployer.Client.Networks.CreateNetworkAsync(new NetworksCreateParameters
                    {
                        Name = $"{deployer.StackName}_{networkName}",
                        CheckDuplicate = true, // Returns a 409 Conflict response if it already exists
                        Driver = network.Driver.Value.GetValue(),
                        Attachable = network.Attachable.Value,
                        Labels = new Dictionary<string, string>()
                        {
                            { "com.docker.stack.namespace", deployer.StackName }
                        },
                        Options = network.DriverOpts
                    });
                }
            }
        }
    }
}
