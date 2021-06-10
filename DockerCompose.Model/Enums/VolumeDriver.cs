using System.Runtime.Serialization;

namespace DockerCompose.Model.Enums
{
    /// <summary>
    /// Valid options are:
    ///     driver: "local"
    ///     TODO
    /// </summary>
    public enum VolumeDriver
    {
        [EnumMember(Value = "local")]
        Local
    }
}
