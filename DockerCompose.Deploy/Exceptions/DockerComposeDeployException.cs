using System;

namespace DockerCompose.Deploy.Exceptions
{
    public class DockerComposeDeployException : Exception
    {
        public DockerComposeDeployException(string message) : base(message)
        {

        }
    }
}
