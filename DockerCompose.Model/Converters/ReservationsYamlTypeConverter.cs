using DockerCompose.Model.Models;
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DockerCompose.Model.Converters
{
    internal sealed class ReservationsYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Reservations);

        public object ReadYaml(IParser parser, Type type)
        {
            var reservations = new Reservations();

            parser.Consume<MappingStart>();

            while (parser.TryConsume<Scalar>(out var scalar1) && parser.TryConsume<Scalar>(out var scalar2))
            {
                string property = scalar1.Value;
                string value = scalar2.Value;

                if (property == "cpus")
                {
                    reservations.Cpus = value;
                }

                else if (property == "memory")
                {
                    reservations.Memory = value;
                }

                else throw new Exception($"{property} not a supported reservation property yet");
            }

            parser.Consume<MappingEnd>();

            return reservations;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            if (emitter == null)
                throw new ArgumentNullException(nameof(emitter));
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            if (value is Reservations reservations)
            {
                if (!string.IsNullOrWhiteSpace(reservations.Cpus))
                {
                    emitter.Emit(new Scalar(null, "cpus"));

                    // Emit with quotes to ensure that it is emitted as a string
                    emitter.Emit(new Scalar(null, null, reservations.Cpus, ScalarStyle.SingleQuoted, false, true));
                }

                if (!string.IsNullOrWhiteSpace(reservations.Memory))
                {
                    emitter.Emit(new Scalar(null, "memory"));
                    emitter.Emit(new Scalar(null, reservations.Memory));
                }
            }

            emitter.Emit(new MappingEnd());
        }
    }
}
