using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Facades;
using PuppetMaster.Client.UI.Properties;
using PuppetMaster.Client.UI.Services;
using PuppetMaster.Client.UI.ViewModels.Internal;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class MainViewModel : Conductor<IMainPaneItem>.Collection.OneActive, IShellItem
    {
        private readonly IBackendFacade _backendFacade;
        private readonly Bootstrapper _bootstrapper;
        private readonly Dictionary<string, SimpleContainer> _containers;

        public MainViewModel(IBackendFacade backendFacade, Bootstrapper bootstrapper)
        {
            _backendFacade = backendFacade;
            _bootstrapper = bootstrapper;
            _containers = new Dictionary<string, SimpleContainer>();
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            if (!Settings.Default.StandaloneTool)
            {
                var games = await _backendFacade.GetGamesAsync();

                foreach (var game in games)
                {
                    var container = _bootstrapper.Container.CreateChildContainer();
                    if (container == null)
                    {
                        throw new InvalidOperationException("Container not found");
                    }

                    var gameService = IoC.Get<IGameService>(game.Name);
                    if (gameService == null)
                    {
                        continue;
                    }

                    await gameService.InitializeAsync();

                    games.ForEach(g => container.UnregisterHandler<IGameService>(g.Name));
                    container.Instance(gameService);

                    container.Singleton<IMainPaneItem, GameShellViewModel>();
                    container.Singleton<IGameShellTabItem, RoomsViewModel>();
                    container.Singleton<IGameShellTabItem, ScrimsViewModel>();
                    container.PerRequest<RoomViewModel>();
                    container.PerRequest<MatchViewModel>();
                    container.PerRequest<PlayerPickViewModel>();
                    container.PerRequest<VoteMapViewModel>();

                    container.Instance(container);

                    var viewModel = container.GetInstance<IMainPaneItem>();
                    if (viewModel == null)
                    {
                        throw new InvalidOperationException("GameShellViewModel not found in child container");
                    }

                    _containers.Add(game.Name, container);
                    Items.Add(viewModel);
                }

                var accountViewModel = IoC.Get<AccountViewModel>();
                await accountViewModel.InitializeAsync();
                Items.Add(accountViewModel);
            }
            else
            {
                var internalViewModel = IoC.Get<InternalShellViewModel>();
                Items.Add(internalViewModel);
            }

            if (Items.Any())
            {
                ActiveItem = Items.First();
            }

            NotifyOfPropertyChange(() => Items);
            NotifyOfPropertyChange(() => ActiveItem);
            await base.OnInitializeAsync(cancellationToken);
        }
    }
}
