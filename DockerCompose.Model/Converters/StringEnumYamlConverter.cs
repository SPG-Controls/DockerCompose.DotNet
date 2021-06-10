using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace DockerCompose.Model.Converters
{
    public class StringEnumYamlConverter : IYamlTypeConverter
    {
        public bool Accepts(Type type) => type.IsEnum || Nullable.GetUnderlyingType(type)?.IsEnum == true;

        public object ReadYaml(IParser parser, Type type)
        {
            var parsedEnum = parser.Consume<Scalar>();

            var nonNullableType = Nullable.GetUnderlyingType(type) ?? type;

            var serializableValues = nonNullableType
                    .GetMembers()
                    .Select(m => new KeyValuePair<string, MemberInfo>(m.GetCustomAttributes<EnumMemberAttribute>(true)
                    .Select(ema => ema.Value).FirstOrDefault(), m))
                    .Where(pa => !string.IsNullOrEmpty(pa.Key))
                    .ToDictionary(pa => pa.Key, pa => pa.Value);

            if (!serializableValues.ContainsKey(parsedEnum.Value))
            {
                throw new YamlException(parsedEnum.Start, parsedEnum.End, $"Value '{parsedEnum.Value}' not found in enum '{nonNullableType.Name}'");
            }

            return Enum.Parse(nonNullableType, serializableValues[parsedEnum.Value].Name);
        }

        public void WriteYaml(IEmitter emitter, object value, Type type)
        {
            var enumMember = type.GetMember(value.ToString()).FirstOrDefault();
            var yamlValue = enumMember?.GetCustomAttributes<EnumMemberAttribute>(true).Select(ema => ema.Value).FirstOrDefault() ?? value.ToString();
            emitter.Emit(new Scalar(yamlValue));
        }
    }
}
