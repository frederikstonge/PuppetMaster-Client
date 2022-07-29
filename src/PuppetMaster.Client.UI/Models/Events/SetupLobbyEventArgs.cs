using System;
using PuppetMaster.Client.Valorant.Api.Models.Requests;

namespace PuppetMaster.Client.UI.Models.Events
{
    public class SetupLobbyEventArgs : EventArgs
    {
        public SetupLobbyEventArgs(GameMap gameMap, Guid matchId)
        {
            GameMap = gameMap;
            MatchId = matchId;
        }

        public GameMap GameMap { get; set; }

        public Guid MatchId { get; set; }
    }
}
