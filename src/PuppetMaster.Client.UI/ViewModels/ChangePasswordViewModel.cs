using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Controls;
using PuppetMaster.Client.UI.Facades;
using PuppetMaster.Client.UI.Models.Requests;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class ChangePasswordViewModel : ValidatingScreen
    {
        private readonly IBackendFacade _backendFacade;
        private string? _currentPassword;
        private string? _newPassword;
        private string? _confirmPassword;
        private bool _isLoading;
        private string? _successMessage;

        public ChangePasswordViewModel(IBackendFacade backendFacade)
        {
            DisplayName = "Change Password";
            _backendFacade = backendFacade;
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
                NotifyOfPropertyChange(() => CanChangePassword);
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
        [Display(Name = "Current Password")]
        [DataType(DataType.Password)]
        public string? CurrentPassword
        {
            get => _currentPassword;
            set
            {
                _currentPassword = value;
                NotifyOfPropertyChange(() => CurrentPassword);
                NotifyOfPropertyChange(() => CanChangePassword);
            }
        }

        [Required]
        [Display(Name = "New Password")]
        [DataType(DataType.Password)]
        public string? NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value;
                NotifyOfPropertyChange(() => NewPassword);
                NotifyOfPropertyChange(() => CanChangePassword);
            }
        }

        [Required]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare(nameof(NewPassword))]
        public string? ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value;
                NotifyOfPropertyChange(() => ConfirmPassword);
                NotifyOfPropertyChange(() => CanChangePassword);
            }
        }

        public bool CanChangePassword => !HasErrors && !IsLoading;

        public async Task ChangePassword()
        {
            SuccessMessage = null;
            Validate(this);
            NotifyOfPropertyChange(() => CanChangePassword);
            if (CanChangePassword)
            {
                IsLoading = true;

                await ValidateAsync(
                    async () =>
                    {
                        var request = new ChangePasswordRequest()
                        {
                            CurrentPassword = CurrentPassword!,
                            NewPassword = NewPassword!
                        };
                        await _backendFacade.ChangePasswordAsync(request);
                        var user = await _backendFacade.GetUserAsync();
                        await _backendFacade.LogInAsync(user!.UserName, NewPassword!);
                    });

                IsLoading = false;
                if (!HasGlobalErrorMessage)
                {
                    SuccessMessage = "Password changed successfully";
                }
            }
        }

        public void OnCurrentPasswordChanged(PasswordBox source)
        {
            CurrentPassword = source.Password;
        }
        
        public void OnNewPasswordChanged(PasswordBox source)
        {
            NewPassword = source.Password;
        }

        public void OnConfirmPasswordChanged(PasswordBox source)
        {
            ConfirmPassword = source.Password;
        }
    }
}
