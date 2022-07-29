using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Messages;
using PuppetMaster.Client.Valorant.Api;
using PuppetMaster.Client.Valorant.Api.Models.Requests;

namespace PuppetMaster.Client.UI.ViewModels.Internal
{
    public class ControlsViewModel : Screen, IInternalShellTabItem
    {
        private readonly IValorantClient _client;
        private readonly IEventAggregator _eventAggregator;
        private string? _partyId;
        private string? _playerId;
        private string? _matchId;
        private string? _payload;
        private GameMap _selectedMap;

        public ControlsViewModel(IValorantClient client, IEventAggregator eventAggregator)
        {
            _client = client;
            _eventAggregator = eventAggregator;
            DisplayName = "Controls";
            Maps = new ObservableCollection<GameMap>(Enum.GetValues<GameMap>());
            _selectedMap = Maps.First();
        }

        public ObservableCollection<GameMap> Maps { get; set; }

        public GameMap SelectedMap
        {
            get
            {
                return _selectedMap;
            }

            set
            {
                _selectedMap = value;
                NotifyOfPropertyChange(() => SelectedMap);
            }
        }

        public string? PartyId
        {
            get
            {
                return _partyId;
            }

            set
            {
                _partyId = value;
                NotifyOfPropertyChange(() => PartyId);
            }
        }

        public string? PlayerId
        {
            get
            {
                return _playerId;
            }

            set
            {
                _playerId = value;
                NotifyOfPropertyChange(() => PlayerId);
            }
        }

        public string? MatchId
        {
            get
            {
                return _matchId;
            }

            set
            {
                _matchId = value;
                NotifyOfPropertyChange(() => MatchId);
            }
        }

        public string? Payload
        {
            get
            {
                return _payload;
            }

            set
            {
                _payload = value;
                NotifyOfPropertyChange(() => Payload);
            }
        }

        public async Task SetTeam1()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            if (PartyId == null)
            {
                await GetParty();
            }

            _client.ChangeTeam(PartyId!, GameTeam.TeamOne);
        }

        public async Task SetTeam2()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            if (PartyId == null)
            {
                await GetParty();
            }

            _client.ChangeTeam(PartyId!, GameTeam.TeamTwo);
        }

        public async Task GetParty()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            var party = _client.GetParty();
            PartyId = party.CurrentPartyId;
        }

        public async Task GetPlayerId()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            var playerInfo = _client.GetPlayerInformation();
            PlayerId = playerInfo.PlayerUniqueId;
        }

        public async Task CoreGameFetchPlayer()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            var coreGamePlayer = _client.CoreGameFetchPlayer(PlayerId!);
            MatchId = coreGamePlayer?.MatchId;
        }

        public async Task CoreGameFetchMatch()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            var payload = _client.CoreGameFetchMatch(MatchId!);
            Payload = payload;
        }

        public async Task FetchMatchDetails()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            var payload = _client.FetchMatchDetails(MatchId!);
            Payload = payload;
        }

        public async Task LeaveCurrentParty()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            if (PartyId == null)
            {
                await GetParty();
            }

            var party = _client.LeaveCurrentParty(PartyId!);
            PartyId = party.CurrentPartyId;
        }

        public async Task SetPartyOpen()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            if (PartyId == null)
            {
                await GetParty();
            }

            _client.SetPartyOpen(PartyId!);
        }

        public async Task SetPartyToCustomGame()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            if (PartyId == null)
            {
                await GetParty();
            }

            _client.SetPartyToCustomGame(PartyId!);
        }

        public async Task SetCustomGameSettings()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            if (PartyId == null)
            {
                await GetParty();
            }

            _client.SetCustomGameSettings(PartyId!, SelectedMap);
        }

        public async Task ConfigureNewGame()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            var party = _client.GetParty();
            party = _client.LeaveCurrentParty(party.CurrentPartyId);
            PartyId = party.CurrentPartyId;
            _client.SetPartyOpen(party.CurrentPartyId);
            _client.SetPartyToCustomGame(party.CurrentPartyId);
            _client.SetCustomGameSettings(party.CurrentPartyId, SelectedMap);
        }

        public async Task StartCustomGame()
        {
            if (!await ValidateAsync())
            {
                return;
            }

            if (PartyId == null)
            {
                await GetParty();
            }

            _client.StartCustomGame(PartyId!);
        }

        private async Task<bool> ValidateAsync()
        {
            if (!_client.IsRunning)
            {
                var errorMessage = new ErrorMessage()
                {
                    Title = "Game not running",
                    Message = "Game must be running"
                };

                await _eventAggregator.PublishOnUIThreadAsync(errorMessage);
                return false;
            }

            return true;
        }
    }
}
