using System;
using System.Collections.Generic;
using DockerCompose.Model.Models;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DockerCompose.Model.Converters
{
    internal sealed class NetworksYamlTypeConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type == typeof(Dictionary<string, Network>);

        public object ReadYaml(IParser parser, Type type)
        {
            var networks = new Dictionary<string, Network>();

            // Short format
            if (parser.TryConsume<SequenceStart>(out var sequenceStart))
            {
                while (parser.TryConsume<Scalar>(out var scalar))
                {
                    networks.Add(scalar.Value, null);
                }

                parser.Consume<SequenceEnd>();
            }

            // Long format
            else if (parser.TryConsume<MappingStart>(out var networksMappingStart))
            {
                while (parser.TryConsume<Scalar>(out var networkNameScalar))
                {
                    var networkName = networkNameScalar.Value;

                    // Empty network (name only) e.g. some-network:
                    if (parser.TryConsume<Scalar>(out var emptyNetworkScalar))
                    {
                        networks.Add(networkName, null);
                    }

                    // Network with properties defined
                    else if (parser.TryConsume<MappingStart>(out var networkMappingStart))
                    {
                        var network = new Network();

                        while (parser.TryConsume<Scalar>(out var networkPropertyNameScalar))
                        {
                            // List of network aliases
                            if (networkPropertyNameScalar.Value == "aliases")
                            {
                                // Check for empty scalar
                                if (parser.TryConsume<Scalar>(out var emptyAliasesScalar)) { }

                                // Read through sequence
                                else if (parser.TryConsume<SequenceStart>(out var aliasesSequence))
                                {
                                    network.Aliases = new List<string>();

                                    while (parser.TryConsume<Scalar>(out var aliasScalar))
                                    {
                                        network.Aliases.Add(aliasScalar.Value);
                                    }

                                    parser.Consume<SequenceEnd>();
                                }

                                else throw new YamlException("Network aliases must be sequence");
                            }

                            // IP4 Address
                            else if (networkPropertyNameScalar.Value == "ipv4_address")
                            {
                                network.Ipv4Address = parser.Consume<Scalar>().Value;
                            }

                            // IP6 Address
                            else if (networkPropertyNameScalar.Value == "ipv6_address")
                            {
                                network.Ipv6Address = parser.Consume<Scalar>().Value;
                            }

                            else throw new YamlException($"Unkown network property {networkPropertyNameScalar.Value}");
                        }

                        networks.Add(networkName, network);

                        parser.Consume<MappingEnd>();
                    }

                    else throw new YamlException("Network should be scalar or mapping");
                }

                parser.Consume<MappingEnd>();
            }

            else throw new YamlException("networks should be a sequence or mapping");

            return networks;
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            throw new NotImplementedException();
        }
    }
}
