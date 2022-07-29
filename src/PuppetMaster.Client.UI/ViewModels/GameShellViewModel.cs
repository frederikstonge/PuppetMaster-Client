using System;
using System.Collections.Generic;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Properties;
using PuppetMaster.Client.UI.Services;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class GameShellViewModel : Conductor<IGameShellTabItem>.Collection.OneActive, IMainPaneItem
    {
        private readonly IGameService _gameService;

        public GameShellViewModel(IEnumerable<IGameShellTabItem> tabs, IGameService gameService)
        {
            _gameService = gameService;
            Items.AddRange(tabs);

            if (_gameService.Game != null)
            {
                DisplayName = _gameService.Game.Name;
                var iconUrl = new Uri(new Uri(Settings.Default.BackendUrl), _gameService.Game.IconUrl);
                Icon = iconUrl.ToString();
            }
        }

        public string? Icon { get; set; }

        public object? Tag { get; set; }
    }
}
