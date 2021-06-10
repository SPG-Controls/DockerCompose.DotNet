using DockerCompose.Model.Converters;
using System;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace DockerCompose.Model
{
    public static class DockerComposeConvert
    {
        public static string Serialize(DockerComposeConfiguration input)
        {
            var serializer = new SerializerBuilder()
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithTypeConverter(new StringEnumYamlConverter())
                .WithTypeConverter(new CommandListYamlTypeConverter())
                .WithTypeConverter(new VolumeYamlTypeConverter())
                .WithTypeConverter(new PortYamlTypeConverter())
                .WithTypeConverter(new SysctlsYamlTypeConverter())
                .WithTypeConverter(new LimitsYamlTypeConverter())
                .WithTypeConverter(new ReservationsYamlTypeConverter())
                .Build();

            return serializer.Serialize(input);
        }

        public static bool TrySerialize(DockerComposeConfiguration input, out string result)
        {
            try
            {
                result = Serialize(input);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result = null;
                return false;
            }
        }

        public static DockerComposeConfiguration Deserialize(string input, bool ignoreUnmatched = true)
        {
            var builder = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .WithTypeConverter(new StringEnumYamlConverter())
                .WithTypeConverter(new TopLevelSecretYamlTypeConverter())
                .WithTypeConverter(new TopLevelConfigYamlTypeConverter())
                .WithTypeConverter(new CommandListYamlTypeConverter())
                .WithTypeConverter(new VolumeYamlTypeConverter())
                .WithTypeConverter(new PortYamlTypeConverter())
                .WithTypeConverter(new SysctlsYamlTypeConverter())
                .WithTypeConverter(new LimitsYamlTypeConverter())
                .WithTypeConverter(new ReservationsYamlTypeConverter())
                .WithTypeConverter(new NetworksYamlTypeConverter());

            // Prevents exception when there is unsupported options
            if (ignoreUnmatched) builder.IgnoreUnmatchedProperties();

            var deserializer = builder.Build();

            return deserializer.Deserialize<DockerComposeConfiguration>(input);
        }

        public static bool TryDeserialize(string input, out DockerComposeConfiguration result)
        {
            try
            {
                result = Deserialize(input);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                result = null;
                return false;
            }
        }
    }
}
