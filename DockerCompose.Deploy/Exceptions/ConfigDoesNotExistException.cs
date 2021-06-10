namespace DockerCompose.Deploy.Exceptions
{
    public class ConfigDoesNotExistException : DockerComposeDeployException
    {
        public ConfigDoesNotExistException(string message) : base(message)
        {

        }
    }
}
