using PuppetMaster.Client.UI.ViewModels;

namespace PuppetMaster.Client.UI.Messages
{
    public class MatchMessage
    {
        public MatchMessage(MatchViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public MatchViewModel ViewModel { get; set; }
    }
}
