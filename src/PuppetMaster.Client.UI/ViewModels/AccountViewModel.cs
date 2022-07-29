using System;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Facades;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Properties;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class AccountViewModel : Conductor<Screen>.Collection.OneActive, IMainPaneItem
    {
        private readonly IBackendFacade _backendFacade;
        private User? _user;

        public AccountViewModel(IBackendFacade backendFacade)
        {
            _backendFacade = backendFacade;
            DisplayName = "Account";
        }

        public string? Icon { get; set; }

        public object? Tag { get; set; }

        public async Task InitializeAsync()
        {
            _user = await _backendFacade.GetUserAsync();
            var iconUrl = new Uri(new Uri(Settings.Default.BackendUrl), _user!.AvatarUrl);
            Icon = iconUrl.ToString();
            NotifyOfPropertyChange(() => Icon);

            var updateUserViewModel = IoC.Get<UpdateUserViewModel>();
            Items.Add(updateUserViewModel);

            var changePasswordViewModel = IoC.Get<ChangePasswordViewModel>();
            Items.Add(changePasswordViewModel);

            if (Items.Any())
            {
                ActiveItem = Items.First();
            }

            NotifyOfPropertyChange(() => Items);
            NotifyOfPropertyChange(() => ActiveItem);
        }
    }
}
