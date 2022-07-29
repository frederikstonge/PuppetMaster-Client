using System;
using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.Controls;
using PuppetMaster.Client.UI.ViewModels;

namespace PuppetMaster.Client.UI.Converters
{
    public sealed class HamburgerMenuItemToPaneConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((IMainPaneItem)value)?.Tag;
        }

        public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((HamburgerMenuIconItem)value)?.Tag;
        }
    }
}
