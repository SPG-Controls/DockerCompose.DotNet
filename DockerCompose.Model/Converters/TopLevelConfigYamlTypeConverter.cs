using DockerCompose.Model.Models;
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DockerCompose.Model.Converters
{
    internal sealed class TopLevelConfigYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => typeof(ITopLevelConfig).IsAssignableFrom(type);

        public object ReadYaml(IParser parser, Type type)
        {
            parser.Consume<MappingStart>();

            string property = parser.Consume<Scalar>().Value;
            string value = parser.Consume<Scalar>().Value;

            parser.Consume<MappingEnd>();

            if (property == "external")
            {
                return new ExternalTopLevelConfig
                {
                    External = bool.Parse(value)
                };
            }

            else if (property == "file")
            {
                return new FileTopLevelConfig
                {
                    File = value
                };
            }
            
            else throw new Exception($"{property} not a supported secret property");
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
