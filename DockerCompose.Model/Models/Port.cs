namespace DockerCompose.Model.Models
{
    public class Port
    {
        public uint? Target { get; set; }

        public uint? Published { get; set; }

        public string Protocol { get; set; }

        public string Mode { get; set; }
    }
}
