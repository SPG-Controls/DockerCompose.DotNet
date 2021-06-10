namespace DockerCompose.Model.Models
{
    public interface ITopLevelConfig { }

    public class FileTopLevelConfig : ITopLevelConfig
    {
        public string File { get; set; }
    }

    public class ExternalTopLevelConfig : ITopLevelConfig
    {
        public bool? External { get; set; }
    }
}
