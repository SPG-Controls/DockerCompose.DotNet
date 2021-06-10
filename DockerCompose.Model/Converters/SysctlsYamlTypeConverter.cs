using DockerCompose.Model.Models;
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DockerCompose.Model.Converters
{
    internal sealed class SysctlsYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Sysctls);

        public object ReadYaml(IParser parser, Type type)
        {
            var sysctls = new Sysctls();

            if (parser.TryConsume<SequenceStart>(out _))
            {
                while (parser.TryConsume<Scalar>(out var scalar))
                {
                    string value = scalar.Value;

                    var parts = value.Split('=');

                    if (parts.Length == 2)
                        sysctls.Add(parts[0], parts[1]);
                    else
                        throw new YamlException($"sysctls {value} must have 1 = sign in it");
                }

                parser.Consume<SequenceEnd>();
            }

            else throw new YamlException("Sysctls should be a yml sequence");

            return sysctls.Count > 0 ? sysctls : null;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (emitter == null)
                throw new ArgumentNullException(nameof(emitter));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Any));

            if (value != null && value is Sysctls node && node.Count > 0)
            {
                foreach (var entry in node)
                {
                    emitter.Emit(new Scalar(null, $"{entry.Key}={entry.Value}"));
                }
            }

            emitter.Emit(new SequenceEnd());
        }
    }
}
