using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Controls;
using PuppetMaster.Client.UI.Services;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class JoinRoomViewModel : ValidatingScreen
    {
        private readonly IGameService _gameService;
        private readonly Guid _roomId;

        private string? _roomPassword;

        private bool _isLoading;

        public JoinRoomViewModel(IGameService gameService, Guid roomId)
        {
            _gameService = gameService;
            _roomId = roomId;
            DisplayName = "Join room";
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
                NotifyOfPropertyChange(() => CanJoin);
            }
        }

        [Required]
        [Display(Name = "Room Password")]
        [DataType(DataType.Password)]
        public string? RoomPassword
        {
            get => _roomPassword;
            set
            {
                _roomPassword = value;
                NotifyOfPropertyChange(() => RoomPassword);
                NotifyOfPropertyChange(() => CanJoin);
            }
        }

        public bool CanJoin => !HasErrors && !IsLoading;

        public async Task Join()
        {
            Validate(this);
            NotifyOfPropertyChange(() => CanJoin);
            if (CanJoin)
            {
                IsLoading = true;

                await ValidateAsync(
                    async () =>
                    {
                        await _gameService.JoinRoomAsync(_roomId, RoomPassword);
                        await TryCloseAsync(true);
                    });

                IsLoading = false;
            }
        }

        public Task Cancel()
        {
            return TryCloseAsync(false);
        }

        public void OnPasswordChanged(PasswordBox source)
        {
            RoomPassword = source.Password;
        }
    }
}
