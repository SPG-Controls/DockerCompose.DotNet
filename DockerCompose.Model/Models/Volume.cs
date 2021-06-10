namespace DockerCompose.Model.Models
{
    public interface IVolume
    {
        public string Type { get; }

        public string Source { get; set; }

        public string Target { get; set; }

        public bool? ReadOnly { get; set; }
    }

    public class Volume : IVolume
    {
        public string Type { get; } = "volume";

        public string Source { get; set; }

        public string Target { get; set; }

        public bool? ReadOnly { get; set; }

        public VolumeOptions VolumeOptions { get; set; }
    }

    public class BindMount : IVolume
    {
        public string Type { get; } = "bind";

        public string Source { get; set; }

        public string Target { get; set; }

        public bool? ReadOnly { get; set; }

        public BindOptions BindOptions { get; set; }
    }

    public class TMPFS : IVolume
    {
        public string Type { get; } = "tmpfs";

        public string Source { get; set; }

        public string Target { get; set; }

        public bool? ReadOnly { get; set; }

        public TmpfsOptions TmpfsOptions { get; set; }
    }

    public class VolumeOptions
    {
        public bool? NoCopy { get; set; }
    }

    public class BindOptions
    {
        public string Propagation { get; set; }
    }

    public class TmpfsOptions
    {
        public long? SizeBytes { get; set; }

        public uint? Mode { get; set; }
    }
}
