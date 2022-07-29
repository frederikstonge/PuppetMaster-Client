using PuppetMaster.Client.UI.ViewModels;

namespace PuppetMaster.Client.UI.Messages
{
    public class PlayerPickMessage
    {
        public PlayerPickMessage(PlayerPickViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public PlayerPickViewModel ViewModel { get; set; }
    }
}
