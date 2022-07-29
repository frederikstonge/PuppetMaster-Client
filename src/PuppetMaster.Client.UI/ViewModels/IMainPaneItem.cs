using System.Threading.Tasks;
using Caliburn.Micro;

namespace PuppetMaster.Client.UI.ViewModels
{
    public interface IMainPaneItem : IScreen
    {
        public string? Icon { get; set; }

        public object? Tag { get; set; }
    }
}
