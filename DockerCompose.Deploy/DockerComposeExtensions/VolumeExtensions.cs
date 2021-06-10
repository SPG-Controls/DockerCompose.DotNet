using Docker.DotNet.Models;
using System;

namespace DockerCompose.Deploy.DockerComposeExtensions
{
    public static class VolumeExtensions
    {
        /// <summary>
        /// Build a list of mounts from the docker-compose volumes
        /// </summary>
        public static Mount BuildMount(this Model.Models.IVolume volume, string stackName)
        {
            var mount = new Mount
            {
                Type = volume.Type,
                Source = volume.Source,
                Target = volume.Target
            };

            if (volume.ReadOnly.HasValue)
            {
                mount.ReadOnly = volume.ReadOnly.Value;
            }

            if (volume is Model.Models.Volume)
            {
                var dockerVol = volume as Model.Models.Volume;

                // Add stack name to volume name
                mount.Source = $"{stackName}_{mount.Source}";

                mount.VolumeOptions = dockerVol?.VolumeOptions == null ? null : new VolumeOptions
                {
                    NoCopy = dockerVol.VolumeOptions.NoCopy.Value
                };
            }

            else if (volume is Model.Models.BindMount)
            {
                var dockerVol = volume as Model.Models.BindMount;

                mount.BindOptions = dockerVol?.BindOptions == null ? null : new BindOptions
                {
                    Propagation = dockerVol.BindOptions.Propagation
                };
            }

            else if (volume is Model.Models.TMPFS)
            {
                var dockerVol = volume as Model.Models.TMPFS;

                mount.TmpfsOptions = dockerVol?.TmpfsOptions == null ? null : new TmpfsOptions
                {
                    SizeBytes = dockerVol.TmpfsOptions.SizeBytes.Value,
                    Mode = dockerVol.TmpfsOptions.Mode.Value
                };
            }

            else throw new NotImplementedException("Build on this volume type is not implemented");

            return mount;
        }
    }
}
