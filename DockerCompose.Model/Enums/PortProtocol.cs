using System.Runtime.Serialization;

namespace DockerCompose.Model.Enums
{
    /// <summary>
    /// Valid options are tcp and udp
    /// </summary>
    public enum PortProtocol
    {
        [EnumMember(Value = "tcp")]
        TCP,
        [EnumMember(Value = "udp")]
        UDP
    }
}
