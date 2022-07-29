using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace PuppetMaster.Client.UI.Converters
{
    public class EnumDescriptionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return string.Empty;
            }

            Enum myEnum = (Enum)value;
            string description = GetEnumDescription(myEnum);
            return description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.Empty;
        }

        private string GetEnumDescription(Enum enumObj)
        {
            var fieldInfo = enumObj.GetType().GetField(enumObj!.ToString());
            var attributes = fieldInfo!.GetCustomAttributes(false);

            if (attributes.Length == 0)
            {
                return enumObj.ToString();
            }
            else
            {
                var attribute = attributes.FirstOrDefault() as DescriptionAttribute;
                return attribute!.Description;
            }
        }
    }
}
