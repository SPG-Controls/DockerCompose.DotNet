using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DockerCompose.Deploy.DockerClientExtensions
{
    public static class TasksOperationsExtensions
    {
        public static async Task WaitServiceTasksAsync(this ITasksOperations client, string serviceName, ILogger log = null)
        {
            var swarmTasks = await client.ListServiceTasksAsync(serviceName, log);

            while (swarmTasks.Count() > 0)
            {
                await Task.Delay(500);
                swarmTasks = await client.ListServiceTasksAsync(serviceName, log);
            }
        }

        public static async Task<IList<TaskResponse>> ListServiceTasksAsync(this ITasksOperations client, string serviceName, ILogger log = null)
        {
            return await client.ListAsync(new TasksListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    {
                        "service",
                        new Dictionary<string, bool>
                        {
                            { serviceName, true }
                        }
                    }
                }
            });
        }
    }
}
