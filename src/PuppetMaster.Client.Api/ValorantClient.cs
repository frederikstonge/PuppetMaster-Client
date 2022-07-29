using PuppetMaster.Client.Valorant.Api.Extensions;
using PuppetMaster.Client.Valorant.Api.Models.Events;
using PuppetMaster.Client.Valorant.Api.Models.Internal;
using PuppetMaster.Client.Valorant.Api.Models.Requests;
using PuppetMaster.Client.Valorant.Api.Models.Responses;
using PuppetMaster.Client.Valorant.Api.Services;
using RestSharp;

namespace PuppetMaster.Client.Valorant.Api
{
    public sealed class ValorantClient : IDisposable, IValorantClient
    {
        private const string ValorantProcessName = "VALORANT-Win64-Shipping";
        private IProcessService? _processService;
        private IGameService? _gameService;
        private bool _isStarted;

        public event EventHandler<ProcessStateEventArgs>? ProcessStateChangeEvent;

        public event EventHandler<LogMessageEventArgs>? LogMessageEvent;

        public bool IsRunning { get; private set; }

        public void Start()
        {
            if (!_isStarted)
            {
                _processService = new ProcessService(ValorantProcessName);
                _processService.ProcessChangeEvent += ProcessChangeEventHandler;

                if (_processService.IsRunning())
                {
                    _gameService = new GameService();
                    _gameService.LogMessageEvent += On_LogMessageEvent;
                    IsRunning = true;
                }

                _isStarted = true;
            }
        }

        public PlayerInformationResponse GetPlayerInformation()
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var playerInformation = _gameService.GetPlayerInformation();
            var playerAffinity = _gameService.PlayerAffinity;

            return new PlayerInformationResponse()
            {
                GameName = playerInformation.GameName,
                GameTag = playerInformation.GameTag,
                PlayerUniqueId = playerInformation.PlayerUniqueId,
                Region = playerAffinity.Shard
            };
        }

        public PartyResponse GetParty()
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/parties/v1/players/{_gameService.EntitlementTokens!.UserPlayerId}";
            var party = _gameService.GlzCall<PartyResponse>(endpoint, Method.Get);
            if (party == null)
            {
                throw new InvalidOperationException("Unable to retrieve party");
            }

            return party;
        }

        public PartyResponse LeaveCurrentParty(string partyId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/parties/v1/players/{_gameService.EntitlementTokens!.UserPlayerId}/leaveparty/{partyId}";
            var party = _gameService.GlzCall<PartyResponse>(endpoint, Method.Post);
            if (party == null)
            {
                throw new InvalidOperationException("Unable to retrieve party");
            }

            return party;
        }

        public PartyResponse JoinParty(string partyId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/parties/v1/players/{_gameService.EntitlementTokens!.UserPlayerId}/joinparty/{partyId}";
            var party = _gameService.GlzCall<PartyResponse>(endpoint, Method.Post);
            if (party == null)
            {
                throw new InvalidOperationException("Unable to retrieve party");
            }

            return party;
        }

        public void SetPartyOpen(string partyId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/parties/v1/parties/{partyId}/accessibility";
            var body = new
            {
                accessibility = "OPEN"
            };
            _gameService.GlzCall(endpoint, Method.Post, body);
        }

        public void SetPartyToCustomGame(string partyId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/parties/v1/parties/{partyId}/makecustomgame";
            _gameService.GlzCall(endpoint, Method.Post);
        }
        
        public void SetCustomGameSettings(string partyId, GameMap map)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var bestServer = GetBestServerForParty(partyId);
            var mapString = map.ToString("G");
            var body = new CustomGameSettings
            {
                Map = $"/Game/Maps/{mapString}/{mapString}",
                Mode = "/Game/GameModes/Bomb/BombGameMode.BombGameMode_C",
                UseBots = false,
                GamePod = bestServer,
                GameRules = new GameRules
                {
                    AllowGameModifiers = "false",
                    PlayOutAllRounds = "false",
                    SkipMatchHistory = "false",
                    TournamentMode = "false",
                    IsOvertimeWinByTwo = "true"
                }
            };

            var endpoint = $"/parties/v1/parties/{partyId}/customgamesettings";
            _gameService.GlzCall(endpoint, Method.Post, body);
        }

        public void ChangeTeam(string partyId, GameTeam team)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var userId = _gameService.EntitlementTokens.UserPlayerId;

            var endpoint = $"/parties/v1/parties/{partyId}/customgamemembership/{team:G}";
            var request = new ChangeTeamRequest() { Subject = userId };
            _gameService.GlzCall(endpoint, Method.Post, request);
        }

        public void StartCustomGame(string partyId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/parties/v1/parties/{partyId}/startcustomgame";
            _gameService.GlzCall(endpoint, Method.Post);
        }

        public PreGamePlayerResponse PreGameGetPlayer(string playerId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/pregame/v1/players/{playerId}";
            var playerGameInfo = _gameService.GlzCall<PreGamePlayerResponse>(endpoint, Method.Get);
            if (playerGameInfo == null)
            {
                throw new InvalidOperationException("Unable to retrieve core game player information");
            }

            return playerGameInfo;
        }

        public CoreGamePlayerResponse CoreGameFetchPlayer(string playerId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/core-game/v1/players/{playerId}";
            var playerGameInfo = _gameService.GlzCall<CoreGamePlayerResponse>(endpoint, Method.Get);
            if (playerGameInfo == null)
            {
                throw new InvalidOperationException("Unable to retrieve core game player information");
            }

            return playerGameInfo;
        }

        public string? CoreGameFetchMatch(string matchId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/core-game/v1/matches/{matchId}";
            var matchInfo = _gameService.GlzCall(endpoint, Method.Get);
            if (matchInfo == null)
            {
                throw new InvalidOperationException("Unable to retrieve core game match information");
            }

            return matchInfo;
        }

        public string? FetchMatchDetails(string matchId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/match-details/v1/matches/{matchId}";
            var matchDetails = _gameService.PdCall(endpoint, Method.Get);
            if (matchDetails == null)
            {
                throw new InvalidOperationException("Unable to retrieve match details information");
            }

            return matchDetails;
        }

        public void SetGameToForeground()
        {
            if (_processService != null)
            {
                _processService.SetGameToForeground();
            }
        }

        public void Dispose()
        {
            if (_processService != null)
            {
                _processService.ProcessChangeEvent -= ProcessChangeEventHandler;
                _processService.Dispose();
            }

            if (_gameService != null)
            {
                _gameService.LogMessageEvent -= On_LogMessageEvent;
                _gameService.Dispose();
                _gameService = null;
            }
        }

        private PartyExpandedResponse GetPartyExpanded(string partyId)
        {
            if (_gameService == null)
            {
                throw new InvalidOperationException("Client not started");
            }

            var endpoint = $"/parties/v1/parties/{partyId}";
            var party = _gameService.GlzCall<PartyExpandedResponse>(endpoint, Method.Get);
            if (party == null)
            {
                throw new InvalidOperationException("Unable to retrieve party information");
            }

            return party;
        }

        private string GetBestServerForParty(string partyId)
        {
            var party = GetPartyExpanded(partyId);

            return party.Members
                .SelectMany(m => m.Pings)
                .GroupBy(p => p.GamePodId)
                .ToDictionary(k => k.Key, p => p.Sum(s => s.Ping))
                .OrderBy(o => o.Value)
                .First()
                .Key;
        }

        private void ProcessChangeEventHandler(object? sender, ProcessStateEventArgs e)
        {
            if (e.IsRunning)
            {
                _gameService = new GameService();
                _gameService.LogMessageEvent += On_LogMessageEvent;
            }
            else
            {
                if (_gameService != null)
                {
                    _gameService.LogMessageEvent -= On_LogMessageEvent;
                    _gameService.Dispose();
                    _gameService = null;
                }
            }

            IsRunning = e.IsRunning;
            var handler = ProcessStateChangeEvent;
            handler?.Invoke(sender, e);
        }

        private void On_LogMessageEvent(object? sender, LogMessageEventArgs e)
        {
            var handler = LogMessageEvent;
            handler?.Invoke(sender, e);
        }
    }
}
