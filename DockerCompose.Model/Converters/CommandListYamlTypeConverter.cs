using DockerCompose.Model.Models;
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DockerCompose.Model.Converters
{
    internal sealed class CommandListYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(CommandList);

        public object ReadYaml(IParser parser, Type type)
        {
            var commandList = new CommandList();

            if (parser.TryConsume<Scalar>(out var scalar1))
            {
                commandList.Add(scalar1.Value);
            }

            else if (parser.TryConsume<SequenceStart>(out _))
            {
                while (parser.TryConsume<Scalar>(out var scalar2))
                {
                    commandList.Add(scalar2.Value);
                }

                parser.Consume<SequenceEnd>();
            }

            else throw new YamlException("Expected Scalar or Sequence");

            return commandList;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (emitter == null)
                throw new ArgumentNullException(nameof(emitter));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (value is CommandList node && node.Count > 0)
            {
                if (node.Count <= 0)
                    return;

                if (node.Count == 1)
                {   // Treat as scalar for single command
                    emitter.Emit(new Scalar(null, node[0]));
                }
                else
                {   // Treat as list for multiple commands
                    emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Any));

                    foreach (var arg in node)
                    {
                        emitter.Emit(new Scalar(null, arg));
                    }

                    emitter.Emit(new SequenceEnd());
                }
            }
            else
            {
                emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Any));
                emitter.Emit(new SequenceEnd());
            }
        }
    }
}
