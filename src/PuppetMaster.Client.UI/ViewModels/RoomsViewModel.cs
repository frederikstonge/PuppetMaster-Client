using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Messages;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Models.Requests;
using PuppetMaster.Client.UI.Services;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class RoomsViewModel : Screen, IGameShellTabItem
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IWindowManager _windowManager;
        private readonly IGameService _gameService;
        private readonly SimpleContainer _container;
        private Room? _selectedRoom;

        public RoomsViewModel(
            IEventAggregator eventAggregator,
            IWindowManager windowManager,
            IGameService gameService,
            SimpleContainer container)
        {
            _eventAggregator = eventAggregator;
            _windowManager = windowManager;
            _gameService = gameService;
            _container = container;
            DisplayName = "Rooms";
            Rooms = new ObservableCollection<Room>();
        }

        public ObservableCollection<Room> Rooms { get; set; }

        public Room? SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                if (_selectedRoom != value)
                {
                    _selectedRoom = value;
                    NotifyOfPropertyChange(() => SelectedRoom);
                    NotifyOfPropertyChange(() => CanJoin);
                }
            }
        }

        public bool CanJoin => _selectedRoom != null;

        public Task Create()
        {
            return ValidateAndExecuteAsync(async () =>
             {
                 var viewModel = new CreateRoomViewModel(_gameService, _gameService.Game!.Id);
                 var result = await _windowManager.ShowDialogAsync(viewModel);
                 if (!result.GetValueOrDefault())
                 {
                     return;
                 }

                 var roomViewModel = _container.GetInstance<RoomViewModel>();
                 if (roomViewModel == null)
                 {
                     return;
                 }

                 var roomMessage = new RoomMessage(roomViewModel);
                 await _eventAggregator.PublishOnUIThreadAsync(roomMessage);
             });
        }

        public async Task Join()
        {
            if (!CanJoin)
            {
                return;
            }

            await ValidateAndExecuteAsync(async () =>
            {
                if (SelectedRoom!.IsPrivate)
                {
                    var viewModel = new JoinRoomViewModel(_gameService, SelectedRoom.Id);
                    var result = await _windowManager.ShowDialogAsync(viewModel);
                    if (!result.GetValueOrDefault())
                    {
                        return;
                    }
                }
                else
                {
                    await _gameService.JoinRoomAsync(SelectedRoom.Id, null);
                }

                var roomViewModel = _container.GetInstance<RoomViewModel>();
                if (roomViewModel == null)
                {
                    return;
                }

                var roomMessage = new RoomMessage(roomViewModel);
                await _eventAggregator.PublishOnUIThreadAsync(roomMessage);
            });
        }

        public Task RefreshRooms()
        {
            return ValidateAndExecuteAsync(async () =>
             {
                 var gameUser = await _gameService.GetGameUserAsync();
                 var rooms = await _gameService.GetRoomsAsync(gameUser!);
                 Rooms.Clear();
                 foreach (var room in rooms)
                 {
                     Rooms.Add(room);
                 }

                 SelectedRoom = Rooms.FirstOrDefault();
             });
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            var user = await _gameService.GetUserAsync();
            if (user!.RoomId.HasValue)
            {
                var valid = await ValidateAndExecuteAsync(async () =>
                {
                    var roomViewModel = _container.GetInstance<RoomViewModel>();
                    if (roomViewModel == null)
                    {
                        return;
                    }

                    var roomMessage = new RoomMessage(roomViewModel);
                    await _eventAggregator.PublishOnUIThreadAsync(roomMessage);
                });

                if (!valid)
                {
                    await _gameService.LeaveRoomAsync(user.RoomId.Value);
                }
            }

            await base.OnInitializeAsync(cancellationToken);
        }

        private async Task<bool> ValidateAndExecuteAsync(Func<Task> onSuccess)
        {
            if (!_gameService.IsRunning)
            {
                var errorMessage = new ErrorMessage()
                {
                    Title = "Game not running",
                    Message = "Game must be running"
                };
                
                await _eventAggregator.PublishOnUIThreadAsync(errorMessage);
                return false;
            }

            var playerInfo = _gameService.GetPlayerInfo();

            var gameUser = await _gameService.GetGameUserAsync();
            if (gameUser == null)
            {
                var linkAccountMessage = new LinkGameAccountMessage()
                {
                    Title = "Link account",
                    Message = $"Do you want to link your account {playerInfo.GameName}?",
                    OnSuccess = async () =>
                    {
                        var creationResult = await _gameService.CreateGameUserAsync(new CreateGameUserRequest()
                        {
                            GameId = _gameService.Game!.Id,
                            UniqueGameId = playerInfo.PlayerUniqueId,
                            Region = playerInfo.Region
                        });

                        if (!creationResult)
                        {
                            var errorMessage = new ErrorMessage()
                            {
                                Title = "Link account error",
                                Message = "An error occurred while linking your account"
                            };

                            await _eventAggregator.PublishOnUIThreadAsync(errorMessage);
                            return;
                        }

                        await onSuccess.Invoke();
                    }
                };

                await _eventAggregator.PublishOnUIThreadAsync(linkAccountMessage);
                return false;
            }
            else
            {
                if (playerInfo.PlayerUniqueId != gameUser.UniqueGameId)
                {
                    var errorMessage = new ErrorMessage()
                    {
                        Title = "Wrong account",
                        Message = "You must login to your right game account"
                    };

                    await _eventAggregator.PublishOnUIThreadAsync(errorMessage);
                    return false;
                }                
            }

            await onSuccess.Invoke();
            return true;
        }
    }
}
