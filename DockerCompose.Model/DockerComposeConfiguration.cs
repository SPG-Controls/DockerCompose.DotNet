using DockerCompose.Model.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace DockerCompose.Model
{
    public class DockerComposeConfiguration
    {
        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string Version { get; set; }

        [YamlMember]
        public Dictionary<string, Service> Services { get; set; }

        [YamlMember]
        public Dictionary<string, TopLevelNetwork> Networks { get; set; }

        [YamlMember]
        public Dictionary<string, TopLevelVolume> Volumes { get; set; }

        [YamlMember]
        public Dictionary<string, ITopLevelSecret> Secrets { get; set; }

        [YamlMember]
        public Dictionary<string, ITopLevelConfig> Configs { get; set; }

        #region Helpers

        public static async Task<string> WriteToFileAsync(DockerComposeConfiguration dockerCompose, string filename)
        {
            string dockerComposeYml = WriteToString(dockerCompose);

            // Create the folder if it doesn't exist
            string dir = Path.GetDirectoryName(filename);
            if (!string.IsNullOrWhiteSpace(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            await File.WriteAllTextAsync(filename, dockerComposeYml);

            return dockerComposeYml;
        }

        public static async Task<DockerComposeConfiguration> ReadFromFileAsync(string filename)
        {
            string dockerComposeYml = await File.ReadAllTextAsync(filename);
            return ReadFromString(dockerComposeYml);
        }

        public static string WriteToString(DockerComposeConfiguration dockerCompose)
        {
            return DockerComposeConvert.Serialize(dockerCompose);
        }

        public static DockerComposeConfiguration ReadFromString(string dockerComposeYml)
        {
            return DockerComposeConvert.Deserialize(dockerComposeYml);
        }

        #endregion
    }
}
