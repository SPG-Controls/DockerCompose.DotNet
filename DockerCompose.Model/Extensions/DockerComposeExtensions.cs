using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace DockerCompose.Model.Extensions
{
    public static class DockerComposeExtensions
    {
        /// <summary>
        /// Deploys a docker compose file locally
        /// </summary>
        /// <param name="client"></param>
        /// <param name="dockerComposeFile">string containing docker compose yaml file</param>
        /// <param name="log"></param>
        /// <returns></returns>
        public static async Task<bool> DeployStackAsync(this DockerComposeConfiguration dockerCompose, string stackName)
        {
            Console.WriteLine("Creating process");

            var process = new Process
            {
                StartInfo = new ProcessStartInfo("docker", $"stack deploy -c - {stackName}")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                }
            };

            process.OutputDataReceived += new DataReceivedEventHandler((object sendingProcess, DataReceivedEventArgs outLine) =>
            {
                if (!string.IsNullOrEmpty(outLine.Data))
                {
                    Console.WriteLine(outLine.Data);
                }
            });

            process.ErrorDataReceived += new DataReceivedEventHandler((object sendingProcess, DataReceivedEventArgs outLine) =>
            {
                if (!string.IsNullOrEmpty(outLine.Data))
                {
                    Console.WriteLine(outLine.Data);
                }
            });

            Console.WriteLine("Starting process");

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            Console.WriteLine("Started process");

            // Send docker compose file to docker stack through standard input
            StreamWriter myStreamWriter = process.StandardInput;

            DockerComposeConvert.TrySerialize(dockerCompose, out string dockerComposeString);

            myStreamWriter.WriteLine(dockerComposeString);
            myStreamWriter.Close();

            Console.WriteLine("Docker Compose Written");

            await process.WaitForExitAsync();

            Console.WriteLine("Process Exited");

            int exitCode = process.ExitCode;

            process.Close();
            process.Dispose();

            if (exitCode == 0)
                return true;
            else
                return false;
        }
    }
}
