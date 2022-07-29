using System;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Properties;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class RoomUserViewModel : Screen
    {
        private readonly RoomUser _roomUser;

        public RoomUserViewModel(RoomUser roomUser)
        {
            _roomUser = roomUser;
        }

        public string? AvatarUrl
        {
            get
            {
                var avatarUrl = _roomUser?.ApplicationUser?.AvatarUrl;
                var imageUrl = new Uri(new Uri(Settings.Default.BackendUrl), avatarUrl);
                return imageUrl.ToString();
            }
        } 

        public string? UserName => _roomUser?.ApplicationUser?.UserName;

        public string? FullName => 
            $"{_roomUser?.ApplicationUser?.FirstName} " + 
            _roomUser?.ApplicationUser?.LastName;

        public bool IsReady => _roomUser?.IsReady ?? false;
    }
}
