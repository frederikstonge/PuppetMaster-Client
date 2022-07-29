using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Models.Events;
using PuppetMaster.Client.UI.Models.Messages;
using PuppetMaster.Client.UI.Models.Requests;

namespace PuppetMaster.Client.UI.Services
{
    public interface IGameService
    {
        event EventHandler<ChatMessageEventArgs>? ChatMessageEvent;

        event EventHandler<RoomChangedEventArgs>? RoomChangedEvent;

        event EventHandler<MatchChangedEventArgs>? MatchChangedEvent;

        event EventHandler<GameStateEventArgs>? GameStateEvent;

        event EventHandler<MatchEndedEventArgs>? MatchEndedEvent;

        public Game? Game { get; }

        public bool IsRunning { get; }

        Task StartHubAsync();

        Task StopHubAsync();

        Task SendChatMessageAsync(ChatMessage message);

        PlayerInfo GetPlayerInfo();

        Task<User?> GetUserAsync();

        Task<GameUser?> GetGameUserAsync();

        Task<bool> CreateGameUserAsync(CreateGameUserRequest request);

        Task<List<Room>> GetRoomsAsync(GameUser gameUser);

        Task<Room?> GetRoomAsync(Guid roomId);

        Task<Room?> CreateRoomAsync(CreateRoomRequest request);

        Task JoinRoomAsync(Guid id, string? password);

        Task LeaveRoomAsync(Guid id);

        Task ReadyRoomAsync(Guid id, bool isReady);

        Task PickPlayerAsync(Guid id, PickPlayerRequest request);

        Task VoteMapAsync(Guid id, VoteMapRequest request);

        IEnumerable<Map> GetMaps();

        bool UserIsIngame();

        Task InitializeAsync();
    }
}
