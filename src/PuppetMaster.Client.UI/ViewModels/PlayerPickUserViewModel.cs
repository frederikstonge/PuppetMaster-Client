using System;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Properties;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class PlayerPickUserViewModel : Screen
    {
        private readonly User _user;

        public PlayerPickUserViewModel(User user)
        {
            _user = user;
        }

        public User User => _user;

        public string? AvatarUrl
        {
            get
            {
                var avatarUrl = _user.AvatarUrl;
                var imageUrl = new Uri(new Uri(Settings.Default.BackendUrl), avatarUrl);
                return imageUrl.ToString();
            }
        } 

        public string? UserName => _user.UserName;

        public string? FullName => 
            $"{_user.FirstName} " + 
            _user.LastName;
    }
}
