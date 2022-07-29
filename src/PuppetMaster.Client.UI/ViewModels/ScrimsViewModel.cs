using Caliburn.Micro;
using PuppetMaster.Client.UI.Services;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class ScrimsViewModel : Screen, IGameShellTabItem
    {
        private readonly IGameService _gameService;        

        public ScrimsViewModel(IGameService gameService)
        {
            _gameService = gameService;
            DisplayName = "Scrims";
        }
    }
}
