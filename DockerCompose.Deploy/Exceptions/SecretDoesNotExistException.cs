namespace DockerCompose.Deploy.Exceptions
{
    public class SecretDoesNotExistException : DockerComposeDeployException
    {
        public SecretDoesNotExistException(string message) : base(message)
        {

        }
    }
}
