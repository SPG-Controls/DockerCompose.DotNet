using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace DockerCompose.Model.Extensions
{
    public static class EnumExtensions
    {
        public static string GetValue(this Enum value)
        {
            FieldInfo fieldInfo = value.GetType().GetField(value.ToString());

            if (fieldInfo == null) return null;

            var attribute = (EnumMemberAttribute)fieldInfo.GetCustomAttribute(typeof(EnumMemberAttribute));

            return attribute.Value;
        }

        public static T GetEnumByMemberAttribute<T>(string attributeValue)
        {
            string matchingEnumName = Enum.GetNames(typeof(T))
                .First(enumName =>
                {
                    FieldInfo fieldInfo = typeof(T).GetField(enumName);

                    var attribute = (EnumMemberAttribute)fieldInfo.GetCustomAttribute(typeof(EnumMemberAttribute));

                    return attribute.Value == attributeValue ? true : false;
                });

            return (T)Enum.Parse(typeof(T), matchingEnumName);
        }
    }
}
