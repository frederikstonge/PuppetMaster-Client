using PuppetMaster.Client.Valorant.Api.Models.Events;

namespace PuppetMaster.Client.Valorant.Api.Services
{
    internal interface IGameLogService
    {
        event EventHandler<LogMessageEventArgs>? LogMessageEvent;

        void Dispose();

        void Start();
    }
}