using PuppetMaster.Client.Valorant.Api.Models.Events;
using PuppetMaster.Client.Valorant.Api.Models.Requests;
using PuppetMaster.Client.Valorant.Api.Models.Responses;

namespace PuppetMaster.Client.Valorant.Api
{
    public interface IValorantClient
    {
        event EventHandler<LogMessageEventArgs>? LogMessageEvent;

        event EventHandler<ProcessStateEventArgs>? ProcessStateChangeEvent;

        bool IsRunning { get; }

        void Start();

        void Dispose();

        PlayerInformationResponse GetPlayerInformation();

        PartyResponse GetParty();

        PartyResponse JoinParty(string partyId);

        void ChangeTeam(string partyId, GameTeam team);

        PartyResponse LeaveCurrentParty(string partyId);

        void SetCustomGameSettings(string partyId, GameMap map);

        void SetPartyOpen(string partyId);

        void SetPartyToCustomGame(string partyId);

        void StartCustomGame(string partyId);

        PreGamePlayerResponse PreGameGetPlayer(string playerId);

        CoreGamePlayerResponse CoreGameFetchPlayer(string playerId);

        string? CoreGameFetchMatch(string matchId);

        string? FetchMatchDetails(string matchId);

        void SetGameToForeground();
    }
}