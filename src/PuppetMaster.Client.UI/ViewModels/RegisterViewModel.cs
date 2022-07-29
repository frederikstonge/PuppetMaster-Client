using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Controls;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Facades;
using PuppetMaster.Client.UI.Messages;
using PuppetMaster.Client.UI.Models.Requests;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class RegisterViewModel : ValidatingScreen
    {
        private readonly IBackendFacade _backendFacade;
        private readonly IEventAggregator _eventAggregator;
        private string? _userName;
        private string? _password;
        private string? _confirmPassword;
        private string? _email;
        private string? _firstName;
        private string? _lastName;
        private bool _isLoading;

        public RegisterViewModel(IEventAggregator eventAggregator, IBackendFacade backendFacade)
        {
            _backendFacade = backendFacade;
            _eventAggregator = eventAggregator;
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
                NotifyOfPropertyChange(() => CanRegister);
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
                NotifyOfPropertyChange(() => CanRegister);
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
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password))]
        public string? ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                NotifyOfPropertyChange(() => ConfirmPassword);
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        [Required]
        [Display(Name = "Email")]
        [DataType(DataType.EmailAddress)]
        public string? Email
        {
            get => _email;
            set
            {
                _email = value;
                NotifyOfPropertyChange(() => Email);
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        [Required]
        [Display(Name = "First Name")]
        public string? FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                NotifyOfPropertyChange(() => FirstName);
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        [Required]
        [Display(Name = "Last Name")]
        public string? LastName
        {
            get => _lastName;
            set
            {
                _lastName = value;
                NotifyOfPropertyChange(() => LastName);
                NotifyOfPropertyChange(() => CanRegister);
            }
        }

        public bool CanRegister => !HasErrors && !IsLoading;

        public async Task Register()
        {
            Validate(this);
            NotifyOfPropertyChange(() => CanRegister);
            if (CanRegister)
            {
                IsLoading = true;

                await ValidateAsync(
                    async () =>
                    {
                        var request = new RegisterRequest()
                        {
                            Email = Email!,
                            FirstName = FirstName!,
                            LastName = LastName!,
                            UserName = UserName!,
                            Password = Password!
                        };

                        await _backendFacade.RegisterAsync(request);
                        await _backendFacade.LogInAsync(UserName!, Password!);
                        await _eventAggregator.PublishOnUIThreadAsync(new StartAppMessage());
                    });

                IsLoading = false;
            }
        }

        public Task Cancel()
        {
            return TryCloseAsync();
        }

        public void OnPasswordChanged(PasswordBox source)
        {
            Password = source.Password;
        }

        public void OnConfirmPasswordChanged(PasswordBox source)
        {
            ConfirmPassword = source.Password;
        }
    }
}
