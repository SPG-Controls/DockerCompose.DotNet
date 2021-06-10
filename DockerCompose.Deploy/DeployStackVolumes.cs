using System;
using System.Threading.Tasks;
using DockerCompose.Model.Models;
using Docker.DotNet.Models;
using DockerCompose.Model.Extensions;
using System.Collections.Generic;

namespace DockerCompose.Deploy
{
    public static class DeployStackVolumes
    {
        public static async Task DeployStackVolumesAsync(this StackDeployer deployer)
        {

            foreach (var (volumeName, volume) in deployer.DockerCompose.Volumes)
            {
                await deployer.Client.Volumes.CreateAsync(new VolumesCreateParameters
                {
                    Name = $"{deployer.StackName}_{volumeName}",
                    Driver = volume.Driver.HasValue ? volume.Driver.Value.GetValue() : null
                });
            }
        }
    }
}
