using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Messages;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Models.Messages;
using PuppetMaster.Client.UI.Services;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class RoomViewModel : Screen, IShellItem
    {
        private readonly IGameService _gameService;
        private readonly IEventAggregator _eventAggregator;
        private readonly SimpleContainer _container;
        private Room? _room;
        private User? _user;
        private MatchViewModel? _matchViewModel;
        private PlayerPickViewModel? _playerPickViewModel;
        private bool _isReady;
        private string _inputMessage = string.Empty;

        public RoomViewModel(IGameService gameService, IEventAggregator eventAggregator, SimpleContainer container)
        {
            DisplayName = "Loading room data...";
            _gameService = gameService;
            _eventAggregator = eventAggregator;
            _container = container;
            Messages = new FlowDocument();
            RoomUsers = new ObservableCollection<RoomUserViewModel>();
        }

        public string InputMessage
        {
            get => _inputMessage;
            set
            {
                if (_inputMessage != value)
                {
                    _inputMessage = value;
                    NotifyOfPropertyChange(() => InputMessage);
                }
            }
        }

        public bool IsPrivate => _room?.IsPrivate ?? false;

        public string ReadyText => IsReady ? "Not Ready" : "Ready Up!";

        public bool IsReady
        {
            get => _isReady;
            set
            {
                if (_isReady != value)
                {
                    _isReady = value;
                    NotifyOfPropertyChange(() => IsReady);
                    NotifyOfPropertyChange(() => ReadyText);
                }
            }
        }

        public FlowDocument Messages { get; }

        public ObservableCollection<RoomUserViewModel> RoomUsers { get; set; }

        public Task LeaveRoomAsync()
        {
            return TryCloseAsync();
        }

        public async Task ReadyAsync()
        {
            if (_gameService.IsRunning)
            {
                if (_gameService.UserIsIngame())
                {
                    var errorMessage = new ErrorMessage()
                    {
                        Title = "User in game",
                        Message = "Cannot ready up when user is already in a game"
                    };

                    await _eventAggregator.PublishOnUIThreadAsync(errorMessage);
                }
                else
                {
                    await _gameService.ReadyRoomAsync(_room!.Id, !IsReady);
                }
            }
            else
            {
                var errorMessage = new ErrorMessage()
                {
                    Title = "Game not running",
                    Message = "Game must be running"
                };

                await _eventAggregator.PublishOnUIThreadAsync(errorMessage);
            }
        }

        public async Task SendAsync()
        {
            if (!string.IsNullOrEmpty(InputMessage))
            {
                var message = new ChatMessage()
                {
                    From = _user!.UserName,
                    Message = InputMessage
                };

                await _gameService.SendChatMessageAsync(message);
                InputMessage = string.Empty;
            }
        }

        protected override async Task OnDeactivateAsync(bool close, CancellationToken cancellationToken)
        {
            if (close)
            {
                await _gameService.StopHubAsync();
                _gameService.ChatMessageEvent -= OnChatMessageEvent;
                _gameService.RoomChangedEvent -= OnRoomChangedEvent;
                _gameService.MatchChangedEvent -= OnMatchChangedEvent;
                if (_room != null)
                {
                    var room = await _gameService.GetRoomAsync(_room!.Id);
                    if (room != null && room.MatchId == null)
                    {
                        try
                        {
                            await _gameService.LeaveRoomAsync(room.Id);
                        }
                        catch
                        {
                            // Already left from StopHubAsync();
                        }
                    }
                }
            }

            await base.OnDeactivateAsync(close, cancellationToken);
        }

        protected override async Task OnInitializeAsync(CancellationToken cancellationToken)
        {
            _user = await _gameService.GetUserAsync();
            var room = await _gameService.GetRoomAsync(_user!.RoomId.GetValueOrDefault());
            if (room == null)
            {
                await TryCloseAsync();
            }
            else
            {
                await SetRoom(room);
                if (room.MatchId.HasValue)
                {
                    await OpenMatchViewAsync();
                }
            }

            _gameService.ChatMessageEvent += OnChatMessageEvent;
            _gameService.RoomChangedEvent += OnRoomChangedEvent;
            _gameService.MatchChangedEvent += OnMatchChangedEvent;
            await _gameService.StartHubAsync();

            await base.OnInitializeAsync(cancellationToken);
        }

        private async void OnMatchChangedEvent(object? sender, Models.Events.MatchChangedEventArgs e)
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                if (_matchViewModel == null)
                {
                    await OpenMatchViewAsync();
                }

                if (e.Match.AvailablePlayers.Any())
                {
                    if (_playerPickViewModel == null)
                    {
                        _playerPickViewModel = _container.GetInstance<PlayerPickViewModel>();
                        await _playerPickViewModel.InitializeAsync(e.Match);
                        var message = new PlayerPickMessage(_playerPickViewModel);
                        await _eventAggregator.PublishOnUIThreadAsync(message);
                    }
                }
                else
                {
                    _playerPickViewModel = null;
                    var votemapViewModel = _container.GetInstance<VoteMapViewModel>();
                    await votemapViewModel.InitializeAsync(e.Match);
                    var message = new VoteMapMessage(votemapViewModel);
                    await _eventAggregator.PublishOnUIThreadAsync(message);
                }
            });
        }

        private void OnChatMessageEvent(object? sender, Models.Events.ChatMessageEventArgs e)
        {
            Execute.OnUIThread(() =>
            {
                var paragraph = new Paragraph
                {
                    Margin = new Thickness(0)
                };
                var fromRun = new Run($"{e.ChatMessage.From}: ")
                {
                    FontWeight = FontWeights.Bold
                };
                paragraph.Inlines.Add(fromRun);
                paragraph.Inlines.Add(new Run(e.ChatMessage.Message));
                Messages.Blocks.Add(paragraph);
                NotifyOfPropertyChange(() => Messages);
            });
        }

        private async void OnRoomChangedEvent(object? sender, Models.Events.RoomChangedEventArgs e)
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                await SetRoom(e.Room);
            });
        }

        private async Task SetRoom(Room room)
        {
            if (!room.RoomUsers!.Any(ru => ru.ApplicationUserId == _user!.Id))
            {
                await TryCloseAsync();
            }

            _room = room;
            DisplayName = room.Name;

            RoomUsers.Clear();
            foreach (var roomUser in _room.RoomUsers!)
            {
                RoomUsers.Add(new RoomUserViewModel(roomUser));
                if (roomUser.ApplicationUserId == _user!.Id)
                {
                    IsReady = roomUser!.IsReady;
                }
            }

            NotifyOfPropertyChange(() => IsPrivate);
            NotifyOfPropertyChange(() => RoomUsers);
        }

        private Task OpenMatchViewAsync()
        {
            _matchViewModel = _container.GetInstance<MatchViewModel>();
            var message = new MatchMessage(_matchViewModel);
            return _eventAggregator.PublishOnUIThreadAsync(message);
        }
    }
}
