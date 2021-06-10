using System.Runtime.Serialization;

namespace DockerCompose.Model.Enums
{
    /// <summary>
    /// Valid options are:
    ///     restart: "no"
    ///     restart: always
    ///     restart: on-failure
    ///     restart: unless-stopped
    /// </summary>
    public enum RestartCondition
    {
        [EnumMember(Value = "no")]
        No,

        [EnumMember(Value = "always")]
        Always,

        [EnumMember(Value = "on-failure")]
        OnFailure,

        [EnumMember(Value = "unless-stopped")]
        UnlessStopped
    }
}
