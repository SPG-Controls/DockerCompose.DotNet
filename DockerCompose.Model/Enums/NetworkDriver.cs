using System.Runtime.Serialization;

namespace DockerCompose.Model.Enums
{
    /// <summary>
    /// Valid options are:
    ///     driver: "overlay"
    ///     TODO
    /// </summary>
    public enum NetworkDriver
    {
        [EnumMember(Value = "overlay")]
        Overlay
    }
}
