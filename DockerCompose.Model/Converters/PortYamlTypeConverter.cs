using System;
using DockerCompose.Model.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DockerCompose.Model.Converters
{
    // Code to convert short port definition

    //public static string GetPortName(string protocol, uint publishedPort)
    //{
    //    if (protocol != "tcp")
    //        return null;

    //    switch (publishedPort)
    //    {
    //        case 80:
    //            return "http";
    //        case 443:
    //            return "ssl";
    //        case 7016:
    //            return "s1000";
    //        default:
    //            return null;
    //    }
    //}

    //    if (service?.Ports == null)
    //        return null;

    //    return service.Ports.Select(p =>
    //    {
    //        var portConfig = new PortConfig { Protocol = "tcp" };

    //        if (p is Dictionary<string, string>)
    //        {
    //            var lookup = p as Dictionary<string, string>;

    //            portConfig.Protocol = lookup.TryGetValue("protocol", out string protocol) ? protocol : "tcp";
    //            portConfig.PublishMode = lookup.TryGetValue("mode", out string mode) ? mode : "ingress"; // Docs state default is ingress

    //            if (lookup.TryGetValue("target", out string target))
    //            {
    //                if (uint.TryParse(target, out uint value))
    //                    portConfig.TargetPort = value;
    //            }

    //            if (lookup.TryGetValue("published", out string published))
    //            {
    //                if (uint.TryParse(published, out uint value))
    //                    portConfig.PublishedPort = value;
    //            }

    //            if (lookup.TryGetValue("name", out string name))
    //            {
    //                portConfig.Name = name;
    //            }

    //            else
    //            {
    //                portConfig.Name = GetPortName(portConfig.Protocol, portConfig.PublishedPort);
    //            }
    //        }

    //        else if (p is string)
    //        {
    //            var splits = p.ToString().Split(':');
    //            if (splits.Length > 2)
    //            {
    //                log.LogError($"Can't parse image ports {p}");
    //                throw new SpgInvalidCommandException($"Can't parse image ports {p}");
    //            }

    //            // Published port is first, target port is second
    //            if (uint.TryParse(splits[0], out uint pubPort))
    //                portConfig.PublishedPort = pubPort;

    //            // If only one port supplied, then use if for both published & target
    //            if (splits.Length == 1)
    //                portConfig.TargetPort = portConfig.PublishedPort;
    //            else if (uint.TryParse(splits[1], out uint targetPort))
    //                portConfig.TargetPort = targetPort;

    //            portConfig.Name = GetPortName(portConfig.Protocol, portConfig.PublishedPort);
    //        }

    //        else throw new InvalidCastException("Port must be of type string or Dictionary<string, string>");

    //        return portConfig;
    //    }).ToList();

    internal sealed class PortYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Port);

        public object ReadYaml(IParser parser, Type type)
        {
            if (parser.TryConsume<Scalar>(out var scalar))
            {
                // Convert short-form port definition
                if (scalar == null || string.IsNullOrEmpty(scalar.Value))
                    throw new YamlException("Cannot parse missing short-form port");

                // Example short-form ports:
                // 9001
                // 9001:9002
                var splits = scalar.Value.Split(':');

                if (splits.Length == 2)
                    return new Port { Published = uint.Parse(splits[0]), Target = uint.Parse(splits[1]) };
                else if (splits.Length == 1)
                    return new Port { Published = uint.Parse(splits[0]), Target = uint.Parse(splits[0]) };

                throw new YamlException($"Cannot parse short-form port : {scalar.Value}");
            }
            else if (parser.TryConsume<MappingStart>(out _))
            {
                var port = new Port();

                while (parser.TryConsume<Scalar>(out var scalar1) && parser.TryConsume<Scalar>(out var scalar2))
                {
                    string property = scalar1.Value;
                    string value = scalar2.Value;

                    if (property == "target")
                    {
                        port.Target = uint.Parse(value);
                    }
                    else if (property == "published")
                    {
                        port.Published = uint.Parse(value);
                    }
                    else if (property == "protocol")
                    {
                        port.Protocol = value;
                    }
                    else if (property == "mode")
                    {
                        port.Mode = value;
                    }
                    else throw new YamlException($"{property} not a supported volume property yet");
                }

                parser.Consume<MappingEnd>();

                return port;
            }
            else throw new YamlException("Port should be a scalar or mapping");
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var node = value as Port;

            emitter.Emit(new MappingStart(null, null, false, MappingStyle.Block));

            if (node.Target.HasValue)
            {
                emitter.Emit(new Scalar(null, "target"));
                emitter.Emit(new Scalar(null, node.Target.Value.ToString()));
            }

            if (node.Published.HasValue)
            {
                emitter.Emit(new Scalar(null, "published"));
                emitter.Emit(new Scalar(null, node.Published.Value.ToString()));
            }

            if (node.Protocol != null)
            {
                emitter.Emit(new Scalar(null, "protocol"));
                emitter.Emit(new Scalar(null, node.Protocol));
            }

            if (node.Mode != null)
            {
                emitter.Emit(new Scalar(null, "mode"));
                emitter.Emit(new Scalar(null, node.Mode));
            }

            emitter.Emit(new MappingEnd());
        }
    }
}
