using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Caliburn.Micro;
using MahApps.Metro.Controls.Dialogs;
using PuppetMaster.Client.UI.Facades;
using PuppetMaster.Client.UI.Messages;
using Squirrel;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class ShellViewModel : 
        Conductor<IShellItem>.Collection.OneActive, 
        IHandle<ErrorMessage>,
        IHandle<LinkGameAccountMessage>,
        IHandle<PlayerPickMessage>,
        IHandle<VoteMapMessage>,
        IHandle<RoomMessage>,
        IHandle<StartAppMessage>,
        IHandle<MatchMessage>
    {
        private readonly IDialogCoordinator _dialogCoordinator;
        private readonly IEventAggregator _eventAggregator;
        private readonly IBackendFacade _backendFacade;
        private LoginRegisterViewModel? _loginRegisterViewModel;

        public ShellViewModel(
            IDialogCoordinator dialogCoordinator,
            IEventAggregator eventAggregator,
            IBackendFacade backendFacade)
        {
            _dialogCoordinator = dialogCoordinator;
            _eventAggregator = eventAggregator;
            _backendFacade = backendFacade;
            DisplayName = "Puppet Master";
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public async Task HandleAsync(ErrorMessage message, CancellationToken cancellationToken)
        {
            var dialog = await _dialogCoordinator.GetCurrentDialogAsync<MessageDialog>(this);
            if (dialog == null)
            {
                await _dialogCoordinator.ShowMessageAsync(this, message.Title, message.Message);
            }
        }

        public async Task HandleAsync(LinkGameAccountMessage message, CancellationToken cancellationToken)
        {
            var result = await _dialogCoordinator.ShowMessageAsync(
            this,
            message.Title,
            message.Message,
            MessageDialogStyle.AffirmativeAndNegative);

            if (result == MessageDialogResult.Affirmative && message.OnSuccess != null)
            {
                await message.OnSuccess.Invoke();
            }
        }

        public Task HandleAsync(RoomMessage message, CancellationToken cancellationToken)
        {
            Items.Add(message.ViewModel);
            ActiveItem = message.ViewModel;
            NotifyOfPropertyChange(() => ActiveItem);
            return Task.CompletedTask;
        }

        public Task HandleAsync(PlayerPickMessage message, CancellationToken cancellationToken)
        {
            Items.Add(message.ViewModel);
            ActiveItem = message.ViewModel;
            NotifyOfPropertyChange(() => ActiveItem);
            return Task.CompletedTask;
        }

        public Task HandleAsync(VoteMapMessage message, CancellationToken cancellationToken)
        {
            Items.Add(message.ViewModel);
            ActiveItem = message.ViewModel;
            NotifyOfPropertyChange(() => ActiveItem);
            return Task.CompletedTask;
        }

        public async Task HandleAsync(StartAppMessage message, CancellationToken cancellationToken)
        {
            await _loginRegisterViewModel!.TryCloseAsync();
            StartApp();
        }

        public Task HandleAsync(MatchMessage message, CancellationToken cancellationToken)
        {
            Items.Add(message.ViewModel);
            ActiveItem = message.ViewModel;
            NotifyOfPropertyChange(() => ActiveItem);
            return Task.CompletedTask;
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                _eventAggregator.Unsubscribe(this);
            }

            return base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var loginMessage = string.Empty;
            var settings = new LoginDialogSettings()
            {
                NegativeButtonVisibility = Visibility.Visible
            };

            if (await _backendFacade.LoginIsNeededAsync())
            {
                _loginRegisterViewModel = IoC.Get<LoginRegisterViewModel>();
                if (_loginRegisterViewModel != null)
                {
                    Items.Add(_loginRegisterViewModel);
                    ActiveItem = Items.First();
                    NotifyOfPropertyChange(() => Items);
                    NotifyOfPropertyChange(() => ActiveItem);
                }
            }
            else
            {
                StartApp();
            }

            await base.OnInitializeAsync(cancellationToken);
        }

        private void StartApp()
        {
            var mainViewModel = IoC.Get<MainViewModel>();
            if (mainViewModel != null)
            {
                Items.Add(mainViewModel);
                ActiveItem = Items.First();
                NotifyOfPropertyChange(() => Items);
                NotifyOfPropertyChange(() => ActiveItem);
            }
        }
    }
}
