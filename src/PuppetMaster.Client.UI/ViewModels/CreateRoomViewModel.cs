using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Windows.Controls;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Models.Requests;
using PuppetMaster.Client.UI.Services;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class CreateRoomViewModel : ValidatingScreen
    {
        private readonly IGameService _gameService;
        private readonly Guid _gameId;

        private string? _roomName;
        private string? _roomPassword;

        private bool _isLoading;

        public CreateRoomViewModel(IGameService gameService, Guid gameId)
        {
            _gameService = gameService;
            _gameId = gameId;
            DisplayName = "Create room";
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyOfPropertyChange(() => IsLoading);
                NotifyOfPropertyChange(() => CanCreate);
            }
        }

        [Required]
        [Display(Name = "Room Name")]
        public string? RoomName
        {
            get => _roomName;
            set
            {
                _roomName = value;
                NotifyOfPropertyChange(() => RoomName);
                NotifyOfPropertyChange(() => CanCreate);
            }
        }

        [Display(Name = "Room Password")]
        [DataType(DataType.Password)]
        public string? RoomPassword
        {
            get => _roomPassword;
            set
            {
                _roomPassword = value;
                NotifyOfPropertyChange(() => RoomPassword);
            }
        }

        public bool CanCreate => !HasErrors && !IsLoading;

        public async Task Create()
        {
            Validate(this);
            NotifyOfPropertyChange(() => CanCreate);
            if (CanCreate)
            {
                IsLoading = true;

                await ValidateAsync(
                    async () =>
                    {
                        var request = new CreateRoomRequest()
                        {
                            GameId = _gameId,
                            Name = RoomName!,
                            Password = RoomPassword
                        };

                        await _gameService.CreateRoomAsync(request);
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
