using System;
using DockerCompose.Model.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DockerCompose.Model.Converters
{
    internal sealed class VolumeYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => typeof(IVolume).IsAssignableFrom(type);

        public static BindMount BuildBindMount(string source, string target, bool bindReadOnly)
        {
            return new BindMount
            {
                Source = source,
                Target = target,
                ReadOnly = bindReadOnly
            };
        }

        public object ReadYaml(IParser parser, Type type)
        {
            if (parser.TryConsume<Scalar>(out var scalar))
            {
                // Convert short-form definition
                if (scalar == null || string.IsNullOrEmpty(scalar.Value))
                    throw new YamlException("Cannot parse missing short-form volume");

                if (scalar.Value.StartsWith("./") || scalar.Value.StartsWith("/"))
                {   // Bind mount. Examples are:
                    // ./nginx/.espasswd:/opt/elk/.espasswd:ro
                    // /:/hostfs:ro
                    var splits = scalar.Value.Split(':');

                    if (splits.Length == 3)
                        return new BindMount { Source = splits[0], Target = splits[1], ReadOnly = splits[2] == "ro" };
                    else if (splits.Length == 2)
                        return new BindMount { Source = splits[0], Target = splits[1] };

                    throw new YamlException($"Cannot parse short-form bind-mount volume : {scalar.Value}");
                }
                else
                {   // Volume mount. Examples are:
                    // dbdata1:/var/lib/mysql
                    var splits = scalar.Value.Split(':');
                    if (splits.Length == 2)
                        return new Volume { Source = splits[0], Target = splits[1] };

                    throw new YamlException($"Cannot parse short-form volume : {scalar.Value}");
                }
            }
            else if (parser.TryConsume<MappingStart>(out _))
            {
                string volumeType = null;
                string source = null;
                string target = null;
                bool? readOnly = null;

                // TODO volume, bind, tmpfs options

                while (parser.TryConsume<Scalar>(out var scalar1) && parser.TryConsume<Scalar>(out var scalar2))
                {
                    string property = scalar1.Value;
                    string value = scalar2.Value;

                    if (property == "type")
                    {
                        volumeType = value;
                    }
                    else if (property == "source")
                    {
                        source = value;
                    }
                    else if (property == "target")
                    {
                        target = value;
                    }
                    else if (property == "read_only")
                    {
                        readOnly = bool.Parse(value);
                    }
                    else throw new YamlException($"{property} not a supported volume property yet");
                }

                parser.Consume<MappingEnd>();

                if (volumeType == "volume")
                {
                    return new Volume
                    {
                        Source = source,
                        Target = target,
                        ReadOnly = readOnly
                    };
                }
                else if (volumeType == "bind")
                {
                    return new BindMount
                    {
                        Source = source,
                        Target = target,
                        ReadOnly = readOnly
                    };
                }
                else if (volumeType == "tmpfs")
                {
                    return new TMPFS
                    {
                        Source = source,
                        Target = target,
                        ReadOnly = readOnly
                    };
                }
                else throw new YamlException("Unsupported volume type");
            }
            else throw new YamlException("Volume should be a scalar or mapping");
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (emitter == null)
                throw new ArgumentNullException(nameof(emitter));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            if (value is IVolume node)
            {
                emitter.Emit(new Scalar(null, "type"));
                emitter.Emit(new Scalar(null, node.Type));

                if (node.Source == null)
                {
                    throw new YamlException("Volume.Source undefined");
                }

                emitter.Emit(new Scalar(null, "source"));
                emitter.Emit(new Scalar(null, node.Source));

                if (node.Target == null)
                {
                    throw new YamlException("Volume.Target undefined");
                }

                emitter.Emit(new Scalar(null, "target"));
                emitter.Emit(new Scalar(null, node.Target));

                if (node.ReadOnly.HasValue)
                {
                    emitter.Emit(new Scalar(null, "read_only"));
                    emitter.Emit(new Scalar(null, node.ReadOnly.ToString()));
                }

                // TODO Volume, Bind, TMPFS Options
            }

            emitter.Emit(new MappingEnd());
        }
    }
}
