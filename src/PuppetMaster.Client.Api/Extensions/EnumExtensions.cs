using System.ComponentModel;
using System.Reflection;

namespace PuppetMaster.Client.Valorant.Api.Extensions
{
    internal static class EnumExtensions
    {
        public static string GetEnumDescription(this Enum value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            var fileInfo = value.GetType().GetField(value.ToString());

            var attributes = fileInfo?.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];

            if (attributes != null && attributes.Any())
            {
                return attributes.First().Description;
            }

            return value.ToString();
        }
    }
}
