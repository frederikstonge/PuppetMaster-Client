using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Facades;
using PuppetMaster.Client.UI.Messages;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class LoginViewModel : ValidatingScreen
    {
        private readonly IBackendFacade _backendFacade;
        private readonly IEventAggregator _eventAggregator;
        private string? _userName;
        private string? _password;
        private bool _isLoading;

        public LoginViewModel(IEventAggregator eventAggregator, IBackendFacade backendFacade)
        {
            _eventAggregator = eventAggregator;
            _backendFacade = backendFacade;
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
                NotifyOfPropertyChange(() => CanLogin);
            }
        }

        [Required]
        [Display(Name = "Username")]
        public string? UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                NotifyOfPropertyChange(() => UserName);
                NotifyOfPropertyChange(() => CanLogin);
            }
        }

        [Required]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string? Password
        {
            get => _password;
            set
            {
                _password = value;
                NotifyOfPropertyChange(() => Password);
                NotifyOfPropertyChange(() => CanLogin);
            }
        }

        public bool CanLogin => !HasErrors && !IsLoading;

        public async Task Login()
        {
            Validate(this);
            NotifyOfPropertyChange(() => CanLogin);
            if (CanLogin)
            {
                IsLoading = true;

                await ValidateAsync(
                    async () =>
                    {
                        await _backendFacade.LogInAsync(UserName!, Password!);
                        await _eventAggregator.PublishOnUIThreadAsync(new StartAppMessage());
                    });

                IsLoading = false;
            }
        }

        public Task Register()
        {
            return _eventAggregator.PublishOnUIThreadAsync(new RegisterMessage());
        }

        public void Cancel()
        {
            Application.Current.Shutdown();
        }

        public void OnPasswordChanged(PasswordBox source)
        {
            Password = source.Password;
        }
    }
}
