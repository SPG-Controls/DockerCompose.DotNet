using System.Runtime.Serialization;

namespace DockerCompose.Model.Enums
{
    /// <summary>
    /// Valid options are host and ingress
    /// </summary>
    public enum PortMode
    {
        [EnumMember(Value = "host")]
        Host,
        [EnumMember(Value = "ingress")]
        Ingress
    }
}
