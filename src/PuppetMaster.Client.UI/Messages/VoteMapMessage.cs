using PuppetMaster.Client.UI.ViewModels;

namespace PuppetMaster.Client.UI.Messages
{
    public class VoteMapMessage
    {
        public VoteMapMessage(VoteMapViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public VoteMapViewModel ViewModel { get; set; }
    }
}
