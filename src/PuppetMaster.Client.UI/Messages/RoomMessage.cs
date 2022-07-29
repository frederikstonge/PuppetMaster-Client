using PuppetMaster.Client.UI.ViewModels;

namespace PuppetMaster.Client.UI.Messages
{
    public class RoomMessage
    {
        public RoomMessage(RoomViewModel viewModel)
        {
            ViewModel = viewModel;
        }

        public RoomViewModel ViewModel { get; set; }
    }
}
