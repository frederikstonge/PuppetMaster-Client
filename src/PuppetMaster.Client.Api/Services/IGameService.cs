using PuppetMaster.Client.Valorant.Api.Models.Events;
using PuppetMaster.Client.Valorant.Api.Models.Internal;
using RestSharp;

namespace PuppetMaster.Client.Valorant.Api.Services
{
    internal interface IGameService
    {
        event EventHandler<LogMessageEventArgs>? LogMessageEvent;

        EntitlementTokens EntitlementTokens { get; }

        PlayerAffinity PlayerAffinity { get; }

        PlayerInformation GetPlayerInformation();

        void Dispose();

        string? GlzCall(string endpoint, Method method, object? body = null);

        T? GlzCall<T>(string endpoint, Method method, object? body = null);

        string? PdCall(string endpoint, Method method, object? body = null);

        T? PdCall<T>(string endpoint, Method method, object? body = null);
    }
}