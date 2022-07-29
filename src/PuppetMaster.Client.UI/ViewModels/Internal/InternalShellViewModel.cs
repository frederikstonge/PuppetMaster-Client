using System.Collections.Generic;
using Caliburn.Micro;

namespace PuppetMaster.Client.UI.ViewModels.Internal
{
    public class InternalShellViewModel : Conductor<IInternalShellTabItem>.Collection.OneActive, IMainPaneItem
    {
        public InternalShellViewModel(IEnumerable<IInternalShellTabItem> tabs)
        {
            DisplayName = "Internal";
            Items.AddRange(tabs);
        }

        public string? Icon { get; set; }

        public object? Tag { get; set; }
    }
}
