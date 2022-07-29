using Newtonsoft.Json;
using PuppetMaster.Client.Valorant.Api.Models.Events;
using PuppetMaster.Client.Valorant.Api.Models.Internal;
using RestSharp;

namespace PuppetMaster.Client.Valorant.Api.Services
{
    internal class GameService : IDisposable, IGameService
    {
        private readonly LockfileData _lockfileData;
        private readonly string _gameVersion;
        private readonly IGameLogService _gameLogService;
        private readonly PlayerAffinity _playerAffinity;
        private EntitlementTokens? _entitlementTokensValue;
        private DateTime? _tokenCreatedDate;

        public GameService()
        {
            _lockfileData = GetLockfileData();
            _gameVersion = GetGameVersion();
            _gameLogService = GetGameLogService();
            _playerAffinity = GetPlayerAffinity();
        }

        public event EventHandler<LogMessageEventArgs>? LogMessageEvent;

        public PlayerAffinity PlayerAffinity => _playerAffinity;

        public EntitlementTokens EntitlementTokens
        {
            get
            {
                if (_entitlementTokensValue == null || _tokenCreatedDate?.AddMinutes(50) <= DateTime.Now)
                {
                    _entitlementTokensValue = LocalCall<EntitlementTokens>("/entitlements/v1/token", Method.Get);
                    if (_entitlementTokensValue == null)
                    {
                        throw new InvalidOperationException("EntitlementTokens not found");
                    }

                    _tokenCreatedDate = DateTime.Now;
                }

                return _entitlementTokensValue;
            }
        }

        public PlayerInformation GetPlayerInformation()
        {
            var playerInformation = LocalCall<PlayerInformation>("/chat/v1/session", Method.Get);
            if (playerInformation == null)
            {
                throw new InvalidOperationException("Player UUID not found");
            }

            return playerInformation;
        }

        public T? GlzCall<T>(string endpoint, Method method, object? body = null)
        {
            var response = GlzCall(endpoint, method, body);
            if (string.IsNullOrEmpty(response))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(response);
        }

        public string? GlzCall(string endpoint, Method method, object? body = null)
        {
            var url = $"https://glz-{_playerAffinity.Shard}-1.{_playerAffinity.Region}.a.pvp.net{endpoint}";
            return RemoteCall(url, method, body);
        }

        public T? PdCall<T>(string endpoint, Method method, object? body = null)
        {
            var response = PdCall(endpoint, method, body);
            if (string.IsNullOrEmpty(response))
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(response);
        }

        public string? PdCall(string endpoint, Method method, object? body = null)
        {
            var url = $"https://pd.{_playerAffinity.Region}.a.pvp.net{endpoint}";
            return RemoteCall(url, method, body);
        }

        public void Dispose()
        {
            if (_gameLogService != null)
            {
                _gameLogService.LogMessageEvent -= Log_OnMessage;
                _gameLogService.Dispose();
            }
        }

        private static LockfileData GetLockfileData()
        {
            LockfileData? lockfileData = null;
            while (lockfileData == null)
            {
                if (!File.Exists(Constants.LockfileDataPath))
                {
                    Thread.Sleep(1000);
                    continue;
                }

                using var fileStream = new FileStream(Constants.LockfileDataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var sr = new StreamReader(fileStream);
                string[] parts = sr.ReadToEnd().Split(":");
                lockfileData = new LockfileData()
                {
                    ProcessName = parts[0],
                    ProcessId = Convert.ToInt32(parts[1]),
                    Port = Convert.ToInt32(parts[2]),
                    Password = parts[3],
                    Protocol = parts[4],
                };
            }

            return lockfileData;
        }

        private static string GetGameVersion()
        {
            string? gameVersion = null;
            while (gameVersion == null)
            {
                if (!File.Exists(Constants.ShooterGameLogPath))
                {
                    Thread.Sleep(1000);
                    continue;
                }

                using (var fileStream = new FileStream(Constants.ShooterGameLogPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (var sr = new StreamReader(fileStream))
                {
                    string? line;
                    do
                    {
                        line = sr.ReadLine();
                        if (line != null && line.Contains(Constants.GameVersionLineInfo))
                        {
                            var data = line.Split(Constants.GameVersionLineInfo, StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
                            gameVersion = data.Last();
                            break;
                        }
                    }
                    while (line != null);
                }

                if (gameVersion == null)
                {
                    Thread.Sleep(1000);
                }
            }

            return gameVersion;
        }

        private PlayerAffinity GetPlayerAffinity()
        {
            var playerAffinity = LocalCall<PlayerAffinity>("/player-affinity/product/v1/token", Method.Post, new { product = "valorant" });
            if (playerAffinity == null)
            {
                throw new InvalidOperationException("Player affinity not found");
            }

            return playerAffinity;
        }

        private T? LocalCall<T>(string endpoint, Method method, object? body = null)
        {
            var response = LocalCall(endpoint, method, body);
            if (response == null)
            {
                return default;
            }

            return JsonConvert.DeserializeObject<T>(response);
        }

        private string? LocalCall(string endpoint, Method method, object? body = null)
        {
            var url = $"https://127.0.0.1:{_lockfileData.Port}{endpoint}";
            var restClient = new RestClient(new RestClientOptions()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            });

            var request = new RestRequest(new Uri(url), method);
            if (body != null)
            {
                request.AddJsonBody(JsonConvert.SerializeObject(body));
            }

            request.AddHeader("Authorization", $"Basic {Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes($"riot:{_lockfileData.Password}"))}");
            request.AddHeader("X-Riot-ClientPlatform", Constants.ClientPlatform);
            request.AddHeader("X-Riot-ClientVersion", _gameVersion);

            var response = restClient.Execute(request);

            if (response.IsSuccessful)
            {
                return response.Content;
            }

            return null;
        }

        private IGameLogService GetGameLogService()
        {
            var gameLogService = new GameLogService(Constants.ShooterGameLogPath, TimeSpan.FromMilliseconds(500));
            gameLogService.LogMessageEvent += Log_OnMessage;
            gameLogService.Start();
            return gameLogService;
        }

        private void Log_OnMessage(object? sender, LogMessageEventArgs e)
        {
            var handler = LogMessageEvent;
            handler?.Invoke(sender, e);
        }

        private string? RemoteCall(string url, Method method, object? body = null)
        {
            var restClient = new RestClient(new RestClientOptions()
            {
                RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true
            });

            var request = new RestRequest(new Uri(url), method);
            if (body != null)
            {
                request.AddJsonBody(JsonConvert.SerializeObject(body));
            }

            request.AddHeader("Authorization", $"Bearer {EntitlementTokens.AccessToken}");
            request.AddHeader("X-Riot-Entitlements-JWT", EntitlementTokens.EntitlementToken ?? string.Empty);
            request.AddHeader("X-Riot-ClientPlatform", Constants.ClientPlatform);
            request.AddHeader("X-Riot-ClientVersion", _gameVersion);

            var response = restClient.Execute(request);

            if (response.IsSuccessful)
            {
                return response.Content;
            }

            return null;
        }
    }
}
