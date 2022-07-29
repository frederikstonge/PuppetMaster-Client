using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using MahApps.Metro.Controls;
using PuppetMaster.Client.UI.ViewModels;

namespace PuppetMaster.Client.UI.Converters
{
    public sealed class PaneListToHamburgerMenuItemCollectionConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var viewModels = value as ICollection<IMainPaneItem>;

            var collection = new HamburgerMenuItemCollection();
            if (viewModels == null)
            {
                return collection;
            }

            foreach (var vm in viewModels)
            {
                var item = new HamburgerMenuIconItem
                {
                    Label = vm.DisplayName,
                    Icon = vm.Icon,
                    Tag = vm
                };

                vm.Tag = item;
                collection.Add(item);
            }

            return collection;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
