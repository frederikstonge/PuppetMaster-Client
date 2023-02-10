using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using PuppetMaster.Client.UI.Facades;
using PuppetMaster.Client.UI.Models.Requests;
using PuppetMaster.Client.UI.Properties;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class UpdateUserViewModel : ValidatingScreen
    {
        private readonly IBackendFacade _backendFacade;
        private string? _firstName;
        private string? _lastName;
        private bool _isLoading;
        private string? _successMessage;

        public UpdateUserViewModel(IBackendFacade backendFacade)
        {
            DisplayName = "Update User";
            _backendFacade = backendFacade;
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
                NotifyOfPropertyChange(() => CanUpdateUser);
            }
        }

        public string? SuccessMessage
        {
            get => _successMessage;
            set
            {
                _successMessage = value;
                NotifyOfPropertyChange(() => SuccessMessage);
                NotifyOfPropertyChange(() => HasSuccessMessage);
            }
        }

        public bool HasSuccessMessage => !string.IsNullOrEmpty(SuccessMessage);

        [Required]
        [Display(Name = "First Name")]
        public string? FirstName
        {
            get => _firstName;
            set
            {
                _firstName = value;
                NotifyOfPropertyChange(() => FirstName);
                NotifyOfPropertyChange(() => CanUpdateUser);
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
                NotifyOfPropertyChange(() => CanUpdateUser);
            }
        }

        public bool CanUpdateUser => !HasErrors && !IsLoading;

        public void Logout()
        {
            Settings.Default.BackendTokenResponse = null;
            Settings.Default.Save();
            Application.Current.Shutdown();
        }

        public async Task UpdateUser()
        {
            SuccessMessage = null;
            Validate(this);
            NotifyOfPropertyChange(() => CanUpdateUser);
            if (CanUpdateUser)
            {
                IsLoading = true;

                await ValidateAsync(
                    async () =>
                    {
                        var request = new UpdateUserRequest()
                        {
                            FirstName = FirstName!,
                            LastName = LastName!
                        };
                        await _backendFacade.UpdateUserAsync(request);
                    });

                IsLoading = false;

                if (!HasGlobalErrorMessage)
                {
                    SuccessMessage = "User updated successfully";
                }
            }
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var user = await _backendFacade.GetUserAsync();
            FirstName = user!.FirstName;
            LastName = user!.LastName;
            await base.OnInitializeAsync(cancellationToken);
        }
    }
}
