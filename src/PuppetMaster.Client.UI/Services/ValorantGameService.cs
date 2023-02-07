using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using PuppetMaster.Client.UI.Facades;
using PuppetMaster.Client.UI.Messages;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Models.Events;
using PuppetMaster.Client.UI.Models.Messages;
using PuppetMaster.Client.UI.Models.Requests;
using PuppetMaster.Client.Valorant.Api;
using PuppetMaster.Client.Valorant.Api.Models.Events;
using PuppetMaster.Client.Valorant.Api.Models.Requests;

namespace PuppetMaster.Client.UI.Services
{
    public class ValorantGameService : IGameService, IDisposable
    {
        public const string GameName = "Valorant";
        private const string MatchFoundEventName = "Pregame_GetMatch";
        private const string MatchStartedEventName = "CoreGame_FetchMatch";
        private const string MatchEndedEventName = "LogShooterGameState: Match Ended:";
        private const string MatchLeftEventName = "CoreGame_DisassociatePlayer";

        private readonly IBackendFacade _backEndFacade;
        private readonly IValorantClient _valorantClient;
        private readonly IEventAggregator _eventAggregator;
        private bool _valorantMatchStarted;
        private string? _valorantMatchId;

        public ValorantGameService(IBackendFacade backEndFacade, IValorantClient valorantClient, IEventAggregator eventAggregator)
        {
            _backEndFacade = backEndFacade;
            _valorantClient = valorantClient;
            _eventAggregator = eventAggregator;
            _valorantClient.ProcessStateChangeEvent += ProcessStateChangeEvent;
            _valorantClient.LogMessageEvent += LogMessageEvent;
            _backEndFacade.ChatMessageEvent += OnChatMessageEvent;
            _backEndFacade.RoomChangedEvent += OnRoomChangedEvent;
            _backEndFacade.MatchChangedEvent += OnMatchChangedEvent;
            _backEndFacade.MatchEndedEvent += OnMatchEndedEvent;
            _backEndFacade.CreateLobbyEvent += CreateLobbyEvent;
            _backEndFacade.JoinLobbyEvent += JoinLobbyEvent;
            _backEndFacade.SetupLobbyEvent += SetupLobbyEvent;
        }

        public event EventHandler<ChatMessageEventArgs>? ChatMessageEvent;

        public event EventHandler<RoomChangedEventArgs>? RoomChangedEvent;

        public event EventHandler<MatchChangedEventArgs>? MatchChangedEvent;

        public event EventHandler<MatchEndedEventArgs>? MatchEndedEvent;

        public event EventHandler<GameStateEventArgs>? GameStateEvent;

        public Game? Game { get; private set; }

        public bool IsRunning => _valorantClient.IsRunning;

        public Task StartHubAsync()
        {
            return _backEndFacade.StartHubAsync();
        }

        public Task StopHubAsync()
        {
            return _backEndFacade.StopHubAsync();
        }

        public Task SendChatMessageAsync(ChatMessage message)
        {
            return _backEndFacade.SendChatMessageAsync(message);
        }

        public async Task<GameUser?> GetGameUserAsync()
        {
            var gameUsers = await _backEndFacade.GetGameUsersAsync();
            return gameUsers.FirstOrDefault(g => g.GameId == Game?.Id);
        }

        public Task<User?> GetUserAsync()
        {
            return _backEndFacade.GetUserAsync();
        }

        public PlayerInfo GetPlayerInfo()
        {
            var valorantPlayerInfo = _valorantClient.GetPlayerInformation();
            return new PlayerInfo()
            {
                GameName = $"{valorantPlayerInfo.GameName}#{valorantPlayerInfo.GameTag}",
                PlayerUniqueId = valorantPlayerInfo.PlayerUniqueId,
                Region = valorantPlayerInfo.Region
            };
        }

        public async Task<bool> CreateGameUserAsync(CreateGameUserRequest request)
        {
            var gameUser = await _backEndFacade.CreateGameUserAsync(request);
            if (gameUser == null)
            {
                return false;
            }

            return true;
        }

        public Task<List<Room>> GetRoomsAsync(GameUser gameUser)
        {
            return _backEndFacade.GetRoomsAsync(gameUser.GameId, gameUser.Region);
        }

        public Task<Room?> GetRoomAsync(Guid roomId)
        {
            return _backEndFacade.GetRoomAsync(roomId);
        }

        public Task<Room?> CreateRoomAsync(CreateRoomRequest request)
        {
            return _backEndFacade.CreateRoomAsync(request);
        }

        public Task JoinRoomAsync(Guid id, string? password)
        {
            return _backEndFacade.JoinRoomAsync(id, password);
        }

        public Task LeaveRoomAsync(Guid id)
        {
            return _backEndFacade.LeaveRoomAsync(id);
        }

        public Task<Match?> GetMatchAsync(Guid matchId)
        {
            return _backEndFacade.GetMatchAsync(matchId);
        }

        public Task ReadyRoomAsync(Guid id, bool isReady)
        {
            return _backEndFacade.ReadyRoomAsync(id, isReady);
        }

        public Task PickPlayerAsync(Guid id, PickPlayerRequest request)
        {
            return _backEndFacade.PickPlayerAsync(id, request);
        }

        public Task VoteMapAsync(Guid id, VoteMapRequest request)
        {
            return _backEndFacade.VoteMapAsync(id, request);
        }

        public async Task InitializeAsync()
        {
            var games = await _backEndFacade.GetGamesAsync();
            Game = games?.FirstOrDefault(g => g.Name == GameName);
        }

        public IEnumerable<Map> GetMaps()
        {
            return Game!.Maps;
        }

        public bool UserIsIngame()
        {
            try
            {
                var playerInfo = _valorantClient.GetPlayerInformation();
                var coreGamePlayer = _valorantClient.CoreGameFetchPlayer(playerInfo.PlayerUniqueId);
                return !string.IsNullOrEmpty(coreGamePlayer?.MatchId);
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            _valorantClient.ProcessStateChangeEvent -= ProcessStateChangeEvent;
            _valorantClient.LogMessageEvent -= LogMessageEvent;
            _backEndFacade.ChatMessageEvent -= OnChatMessageEvent;
            _backEndFacade.RoomChangedEvent -= OnRoomChangedEvent;
            _backEndFacade.MatchChangedEvent -= OnMatchChangedEvent;
            _backEndFacade.MatchEndedEvent -= OnMatchEndedEvent;
            _backEndFacade.CreateLobbyEvent -= CreateLobbyEvent;
            _backEndFacade.JoinLobbyEvent -= JoinLobbyEvent;
            _backEndFacade.SetupLobbyEvent -= SetupLobbyEvent;
            GC.SuppressFinalize(this);
        }

        private void OnChatMessageEvent(object? sender, ChatMessageEventArgs e)
        {
            var handler = ChatMessageEvent;
            handler?.Invoke(sender, e);
        }

        private void OnRoomChangedEvent(object? sender, RoomChangedEventArgs e)
        {
            var handler = RoomChangedEvent;
            handler?.Invoke(sender, e);
        }

        private void OnMatchChangedEvent(object? sender, MatchChangedEventArgs e)
        {
            var handler = MatchChangedEvent;
            handler?.Invoke(sender, e);
        }

        private void OnMatchEndedEvent(object? sender, MatchEndedEventArgs e)
        {
            var handler = MatchEndedEvent;
            handler?.Invoke(sender, e);
        }

        private async void ProcessStateChangeEvent(object? sender, ProcessStateEventArgs e)
        {
            var user = await GetUserAsync();
            await UserGameNotOpenedAsync(user);

            var handler = GameStateEvent;
            handler?.Invoke(sender, new GameStateEventArgs(e.IsRunning));
        }

        private void SetupLobbyEvent(object? sender, SetupLobbyEventArgs e)
        {
            var party = _valorantClient.GetParty();
            _valorantClient.SetCustomGameSettings(party.CurrentPartyId, e.Map);
            _valorantClient.StartCustomGame(party.CurrentPartyId);
        }

        private async void JoinLobbyEvent(object? sender, JoinLobbyEventArgs e)
        {
            _valorantClient.JoinParty(e.LobbyId);
            _valorantClient.ChangeTeam(e.LobbyId, (GameTeam)(e.TeamIndex - 1));

            await _backEndFacade.HasJoinedAsync(e.MatchId);
        }

        private async void CreateLobbyEvent(object? sender, CreateLobbyEventArgs e)
        {
            var party = _valorantClient.GetParty();
            party = _valorantClient.LeaveCurrentParty(party.CurrentPartyId);
            _valorantClient.SetPartyOpen(party.CurrentPartyId);
            _valorantClient.SetPartyToCustomGame(party.CurrentPartyId);

            await _backEndFacade.SetLobbyIdAsync(e.MatchId, new SetLobbyIdRequest()
            {
                LobbyId = party.CurrentPartyId,
            });
        }

        private async void LogMessageEvent(object? sender, LogMessageEventArgs e)
        {
            if (e.Message.Contains(MatchFoundEventName))
            {
                await MatchFoundAsync();
            }
            else if (e.Message.Contains(MatchStartedEventName))
            {
                await MatchStartedAsync();
            }
            else if (e.Message.Contains(MatchEndedEventName))
            {
                await MatchEndedAsync();
            }
            else if (e.Message.Contains(MatchLeftEventName))
            {
                MatchLeft();
            }
        }

        private async Task MatchFoundAsync()
        {
            try
            {
                var playerInfo = _valorantClient.GetPlayerInformation();
                var coreGamePlayer = _valorantClient.PreGameGetPlayer(playerInfo.PlayerUniqueId);
                var user = await GetUserAsync();
                if (user?.RoomId != null)
                {
                    var room = await GetRoomAsync(user.RoomId.Value);
                    if (room?.MatchId != null)
                    {
                        _valorantClient.SetGameToForeground();
                        return;
                    }

                    await ValidateUserInGameAsync(user, room!);
                }
            }
            catch
            {
                // Escape
            }
        }

        private async Task MatchStartedAsync()
        {
            try
            {
                var user = await GetUserAsync();
                if (user?.RoomId != null)
                {
                    var playerInfo = _valorantClient.GetPlayerInformation();
                    var coreGamePlayer = _valorantClient.CoreGameFetchPlayer(playerInfo.PlayerUniqueId);
                    var room = await GetRoomAsync(user.RoomId.Value);
                    if (room?.MatchId != null)
                    {
                        var match = await GetMatchAsync(room.MatchId.Value);
                        if (match!.MatchTeams!.SelectMany(mt => mt.MatchTeamUsers!).All(mtu => mtu.HasJoined))
                        {
                            _valorantMatchId = coreGamePlayer.MatchId;
                            _valorantMatchStarted = true;
                            return;
                        }
                    }

                    await ValidateUserInGameAsync(user, room!);
                }
            }
            catch
            {
                // Escape
            }
        }

        private async Task MatchEndedAsync()
        {
            if (_valorantMatchStarted)
            {
                var user = await GetUserAsync();
                if (user?.RoomId != null)
                {
                    var room = await GetRoomAsync(user.RoomId.Value);
                    if (room?.MatchId != null)
                    {
                        var matchDetails = _valorantClient.FetchMatchDetails(_valorantMatchId!);
                        var request = new MatchEndedRequest()
                        {
                            MatchData = matchDetails!
                        };

                        await _backEndFacade.MatchEndedAsync(room.MatchId.Value, request);
                    }
                }
            }

            _valorantMatchStarted = false;
            _valorantMatchId = null;
        }

        private void MatchLeft()
        {
            if (_valorantMatchStarted)
            {
                // Player left before end of match
            }

            _valorantMatchStarted = false;
            _valorantMatchId = null;
        }

        private async Task ValidateUserInGameAsync(User user, Room room)
        {
            var roomUser = room.RoomUsers?.FirstOrDefault(ru => ru.ApplicationUserId == user.Id);
            if (user.RoomId.HasValue && roomUser != null && roomUser.IsReady)
            {
                // Unready a player that joins a game
                await _backEndFacade.ReadyRoomAsync(user.RoomId.Value, false);
                var errorMessage = new ErrorMessage()
                {
                    Title = "User in game",
                    Message = "Cannot ready up when user is already in a game"
                };

                await _eventAggregator.PublishOnUIThreadAsync(errorMessage);
            }
        }
        
        private async Task UserGameNotOpenedAsync(User? user)
        {
            if (user?.RoomId != null)
            {
                await _backEndFacade.ReadyRoomAsync(user.RoomId.Value, false);
                var errorMessage = new ErrorMessage()
                {
                    Title = "Game not running",
                    Message = "Game must be running"
                };

                await _eventAggregator.PublishOnUIThreadAsync(errorMessage);
            }
        }
    }
}
