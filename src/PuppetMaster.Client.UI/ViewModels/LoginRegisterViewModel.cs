using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Messages;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class LoginRegisterViewModel : Conductor<ValidatingScreen>.Collection.OneActive, IShellItem, IHandle<RegisterMessage>
    {
        private readonly IEventAggregator _eventAggregator;

        public LoginRegisterViewModel(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
            DisplayName = "Login / Register";
            _eventAggregator.SubscribeOnPublishedThread(this);
        }

        public Task HandleAsync(RegisterMessage message, CancellationToken cancellationToken)
        {
            var loginViewModel = IoC.Get<RegisterViewModel>();
            Items.Add(loginViewModel);
            ActiveItem = loginViewModel;

            NotifyOfPropertyChange(() => Items);
            NotifyOfPropertyChange(() => ActiveItem);
            return Task.CompletedTask;
        }

        protected override Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var loginViewModel = IoC.Get<LoginViewModel>();
            Items.Add(loginViewModel);
            ActiveItem = loginViewModel;

            NotifyOfPropertyChange(() => Items);
            NotifyOfPropertyChange(() => ActiveItem);

            return base.OnInitializeAsync(cancellationToken);
        }

        protected override Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            _eventAggregator.Unsubscribe(this);
            return base.OnDeactivateAsync(close, cancellationToken);
        }
    }
}
