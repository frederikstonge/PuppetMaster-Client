using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Models.Messages;
using PuppetMaster.Client.UI.Models.Requests;
using PuppetMaster.Client.UI.Services;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class PlayerPickViewModel : Screen, IShellItem, IDisposable
    {
        private readonly IGameService _gameService;
        private Timer? _timer;
        private int _elaspedTime;
        private bool _captainToPickThisTurnIsMe;
        private PlayerPickUserViewModel? _selectedAvailablePlayer;
        private string? _captainToPickThisTurn;
        private Match? _match;

        public PlayerPickViewModel(IGameService gameService)
        {
            DisplayName = "Player Selection";
            _gameService = gameService;
            _gameService.MatchChangedEvent += MatchChangedEvent;
            AvailablePlayers = new ObservableCollection<PlayerPickUserViewModel>();
        }

        public ObservableCollection<PlayerPickUserViewModel> AvailablePlayers { get; set; }

        public PlayerPickUserViewModel? SelectedAvailablePlayer
        {
            get => _selectedAvailablePlayer;
            set
            {
                if (value != null)
                {
                    _selectedAvailablePlayer = value;
                    NotifyOfPropertyChange(() => SelectedAvailablePlayer);
                    NotifyOfPropertyChange(() => CanPick);
                }
            }
        }

        public string? CaptainToPickThisTurn
        {
            get => _captainToPickThisTurn;
            set
            {
                if (value != null)
                {
                    _captainToPickThisTurn = value;
                    NotifyOfPropertyChange(() => CaptainToPickThisTurn);
                }
            }
        }

        public bool CaptainToPickThisTurnIsMe
        {
            get => _captainToPickThisTurnIsMe;
            set
            {
                _captainToPickThisTurnIsMe = value;
                NotifyOfPropertyChange(() => CaptainToPickThisTurnIsMe);
                NotifyOfPropertyChange(() => CanPick);
            }
        }

        public bool CanPick 
        {
            get => SelectedAvailablePlayer != null && CaptainToPickThisTurnIsMe;
        }

        public int ElaspedTime
        {
            get => _elaspedTime;
            set
            {
                _elaspedTime = value;
                NotifyOfPropertyChange(() => ElaspedTime);
            }
        }

        public async Task Pick()
        {
            if (!CanPick)
            {
                return;
            }

            await _gameService.PickPlayerAsync(_match!.Id, new PickPlayerRequest()
            {
                PickedUserId = SelectedAvailablePlayer!.User.Id
            });

            CaptainToPickThisTurnIsMe = false;
        }

        public async Task InitializeAsync(RoomMatchMessage roomMatchMessage)
        {
            _match = roomMatchMessage.Match;
            var user = await _gameService.GetUserAsync();

            AvailablePlayers.Clear();
            foreach (var player in roomMatchMessage.AvailablePlayers)
            {
                AvailablePlayers.Add(new PlayerPickUserViewModel(player));
            }

            CaptainToPickThisTurnIsMe = roomMatchMessage.CaptainToPickThisTurn.HasValue && 
                                        roomMatchMessage.CaptainToPickThisTurn.Value == user!.Id;

            CaptainToPickThisTurn = _match!.MatchTeams!
                .SelectMany(mt => mt.MatchTeamUsers!)
                .Select(mtu => mtu.ApplicationUser!)
                .FirstOrDefault(a => a.Id == roomMatchMessage.CaptainToPickThisTurn)
                ?.UserName;      

            if (_timer != null)
            {
                await _timer.DisposeAsync();
                _timer = null;
            }

            if (roomMatchMessage.Delay.HasValue)
            {
                ElaspedTime = roomMatchMessage.Delay.Value.Seconds;
                _timer = new Timer(DecreaseTime, null, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
            _gameService.MatchChangedEvent -= MatchChangedEvent;
            GC.SuppressFinalize(this);
        }

        private async void MatchChangedEvent(object? sender, Models.Events.MatchChangedEventArgs e)
        {
            await Execute.OnUIThreadAsync(async () =>
            {
                if (e.Match.AvailablePlayers.Any())
                {        
                    await InitializeAsync(e.Match);          
                }
                else
                {
                    await TryCloseAsync();
                }
            });
        }

        private void DecreaseTime(object? state)
        {
            ElaspedTime--;
            if (ElaspedTime <= 0)
            {
                _timer?.Dispose();
                _timer = null;
            }
        }
    }
}
