using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Models.Messages;
using PuppetMaster.Client.UI.Models.Requests;
using PuppetMaster.Client.UI.Services;

namespace PuppetMaster.Client.UI.ViewModels
{
    public class VoteMapViewModel : Screen, IShellItem, IDisposable
    {
        private readonly IGameService _gameService;
        private Timer? _timer;
        private int _elaspedTime;
        private Map? _selectedAvailableMap;
        private Match? _match;

        public VoteMapViewModel(IGameService gameService)
        {
            DisplayName = "Map Vote";
            _gameService = gameService;
            AvailableMaps = new ObservableCollection<Map>();
        }

        public ObservableCollection<Map> AvailableMaps { get; set; }

        public bool CanVote => SelectedAvailableMap != null;

        public Map? SelectedAvailableMap
        {
            get => _selectedAvailableMap;
            set
            {
                if (value != null)
                {
                    _selectedAvailableMap = value;
                    NotifyOfPropertyChange(() => SelectedAvailableMap);
                    NotifyOfPropertyChange(() => CanVote);
                }
            }
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

        public async Task Vote()
        {
            if (!CanVote)
            {
                return;
            }

            await _gameService.VoteMapAsync(_match!.Id, new VoteMapRequest()
            {
                VoteMap = _selectedAvailableMap!.Name
            });

            await TryCloseAsync();
        }

        public async Task InitializeAsync(RoomMatchMessage roomMatchMessage)
        {
            _match = roomMatchMessage.Match;
            AvailableMaps.Clear();
            foreach (var map in _gameService.GetMaps())
            {
                AvailableMaps.Add(map);
            }

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
            GC.SuppressFinalize(this);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("AsyncUsage", "AsyncFixer03:Fire-and-forget async-void methods or delegates", Justification = "Needed")]
        private async void DecreaseTime(object? state)
        {
            ElaspedTime--;
            if (ElaspedTime <= 0)
            {
                _timer?.Dispose();
                _timer = null;
                await TryCloseAsync();
            }
        }
    }
}
