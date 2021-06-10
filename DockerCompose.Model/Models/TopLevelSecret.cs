namespace DockerCompose.Model.Models
{
    public interface ITopLevelSecret { }

    public class FileTopLevelSecret : ITopLevelSecret
    {
        public string File { get; set; }
    }

    public class ExternalTopLevelSecret : ITopLevelSecret
    {
        public bool? External { get; set; }
    }
}
