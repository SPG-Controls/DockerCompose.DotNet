namespace DockerCompose.Model.Models
{
    public class Secret
    {
        public string Source { get; set; }

        public string Target { get; set; }

        public string Uid { get; set; }

        public string Gid { get; set; }

        public int? Mode { get; set; }
    }
}
