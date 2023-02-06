using System;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Services;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class MatchViewModel : Screen, IShellItem, IDisposable
    {
        private readonly IGameService _gameService;        

        public MatchViewModel(IGameService gameService)
        {
            _gameService = gameService;
            DisplayName = "Match";
            _gameService.MatchEndedEvent += OnMatchEndedEvent;
        }

        public void Dispose()
        {
            _gameService.MatchEndedEvent -= OnMatchEndedEvent;
        }

        private async void OnMatchEndedEvent(object? sender, Models.Events.MatchEndedEventArgs e)
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                await TryCloseAsync();
            });
        }
    }
}
