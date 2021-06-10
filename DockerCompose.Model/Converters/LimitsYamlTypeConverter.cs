using DockerCompose.Model.Models;
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DockerCompose.Model.Converters
{
    internal sealed class LimitsYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Limits);

        public object ReadYaml(IParser parser, Type type)
        {
            var limits = new Limits();

            parser.Consume<MappingStart>();

            while (parser.TryConsume<Scalar>(out var scalar1) && parser.TryConsume<Scalar>(out var scalar2))
            {
                string property = scalar1.Value;
                string value = scalar2.Value;

                if (property == "cpus")
                {
                    limits.Cpus = value;
                }

                else if (property == "memory")
                {
                    limits.Memory = value;
                }

                else throw new Exception($"{property} not a supported limits property yet");
            }

            parser.Consume<MappingEnd>();

            return limits;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (emitter == null)
                throw new ArgumentNullException(nameof(emitter));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            if (value is Limits limits)
            {
                if (!string.IsNullOrWhiteSpace(limits.Cpus))
                {
                    emitter.Emit(new Scalar(null, "cpus"));

                    // Emit with quotes to ensure that it is emitted as a string
                    emitter.Emit(new Scalar(null, null, limits.Cpus, ScalarStyle.SingleQuoted, false, true));
                }

                if (!string.IsNullOrWhiteSpace(limits.Memory))
                {
                    emitter.Emit(new Scalar(null, "memory"));
                    emitter.Emit(new Scalar(null, limits.Memory));
                }
            }

            emitter.Emit(new MappingEnd());
        }
    }
}
