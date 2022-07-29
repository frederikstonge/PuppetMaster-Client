using PuppetMaster.Client.Valorant.Api.Models.Events;

namespace PuppetMaster.Client.Valorant.Api.Services
{
    public interface IProcessService
    {
        event EventHandler<ProcessStateEventArgs>? ProcessChangeEvent;

        void Dispose();

        bool IsRunning();

        void SetGameToForeground();
    }
}