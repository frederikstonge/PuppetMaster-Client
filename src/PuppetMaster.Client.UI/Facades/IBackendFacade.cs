using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Models.Events;
using PuppetMaster.Client.UI.Models.Messages;
using PuppetMaster.Client.UI.Models.Requests;

namespace PuppetMaster.Client.UI.Facades
{
    public interface IBackendFacade
    {
        event EventHandler<ChatMessageEventArgs>? ChatMessageEvent;

        event EventHandler<RoomChangedEventArgs>? RoomChangedEvent;

        event EventHandler<MatchChangedEventArgs>? MatchChangedEvent;

        event EventHandler<MatchEndedEventArgs>? MatchEndedEvent;

        event EventHandler<CreateLobbyEventArgs>? CreateLobbyEvent;

        event EventHandler<JoinLobbyEventArgs>? JoinLobbyEvent;

        event EventHandler<SetupLobbyEventArgs>? SetupLobbyEvent;

        Task StartHubAsync();

        Task StopHubAsync();

        Task SendChatMessageAsync(ChatMessage message);

        Task<List<Game>> GetGamesAsync();

        Task<User?> GetUserAsync();

        Task<List<GameUser>> GetGameUsersAsync();

        Task<GameUser?> CreateGameUserAsync(CreateGameUserRequest request);

        Task<List<Room>> GetRoomsAsync(Guid gameId, string region);

        Task<Room?> GetRoomAsync(Guid roomId);

        Task<Room?> CreateRoomAsync(CreateRoomRequest request);

        Task JoinRoomAsync(Guid id, string? password);

        Task LeaveRoomAsync(Guid id);

        Task ReadyRoomAsync(Guid id, bool isReady);

        Task<Match?> GetMatchAsync(Guid matchId);

        Task HasJoinedAsync(Guid id);

        Task PickPlayerAsync(Guid id, PickPlayerRequest request);

        Task SetLobbyIdAsync(Guid id, SetLobbyIdRequest request);

        Task VoteMapAsync(Guid id, VoteMapRequest request);

        Task MatchEndedAsync(Guid id, MatchEndedRequest request);

        Task<bool> LoginIsNeededAsync();

        Task LogInAsync(string userName, string password);

        Task RegisterAsync(RegisterRequest request);

        Task ChangePasswordAsync(ChangePasswordRequest request);

        Task UpdateUserAsync(UpdateUserRequest request);
    }
}