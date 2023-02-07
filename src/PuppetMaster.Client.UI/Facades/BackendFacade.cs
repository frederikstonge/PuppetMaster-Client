using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel;
using IdentityModel.Client;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PuppetMaster.Client.UI.Models;
using PuppetMaster.Client.UI.Models.Events;
using PuppetMaster.Client.UI.Models.Messages;
using PuppetMaster.Client.UI.Models.Requests;
using PuppetMaster.Client.UI.Properties;

namespace PuppetMaster.Client.UI.Facades
{
    public class BackendFacade : IBackendFacade, IAsyncDisposable
    {
        private readonly string _baseUrl;
        private readonly HttpClient _httpClient;
        private readonly HubConnection _roomHubConnection;
        private readonly List<IDisposable> _activeSubscriptions;

        public BackendFacade()
        {
            _baseUrl = Settings.Default.BackendUrl;
            _httpClient = new HttpClient();
            _activeSubscriptions = new List<IDisposable>();
            _roomHubConnection = ConfigureRoomSignalRHub();
        }

        public event EventHandler<ChatMessageEventArgs>? ChatMessageEvent;

        public event EventHandler<RoomChangedEventArgs>? RoomChangedEvent;

        public event EventHandler<MatchChangedEventArgs>? MatchChangedEvent;

        public event EventHandler<MatchEndedEventArgs>? MatchEndedEvent;

        public event EventHandler<CreateLobbyEventArgs>? CreateLobbyEvent;

        public event EventHandler<JoinLobbyEventArgs>? JoinLobbyEvent;

        public event EventHandler<SetupLobbyEventArgs>? SetupLobbyEvent;

        protected static Token? Token
        {
            get
            {
                return JsonConvert.DeserializeObject<Token>(Settings.Default.BackendTokenResponse);
            }

            set
            {
                if (value == null)
                {
                    return;
                }

                var serializedValue = JsonConvert.SerializeObject(value);
                Settings.Default[nameof(Settings.Default.BackendTokenResponse)] = serializedValue;
                Settings.Default[nameof(Settings.Default.TokenExpiresAt)] = DateTime.Now.AddSeconds(value.ExpiresIn).AddMinutes(-5);
                Settings.Default.Save();
            }
        }

        protected static DateTime? ExpiresAt
        {
            get
            {
                return Settings.Default.TokenExpiresAt;
            }
        }

        public Task StartHubAsync()
        {
            return _roomHubConnection.StartAsync();
        }

        public Task StopHubAsync()
        {
            return _roomHubConnection.StopAsync();
        }

        public Task SendChatMessageAsync(ChatMessage message)
        {
            return _roomHubConnection.SendAsync(SignalRMethods.ChatMessage, message);
        }

        public async Task<List<Game>> GetGamesAsync()
        {
            var endpoint = "/api/games";
            var games = await MakeRequestAsync<List<Game>>(endpoint, HttpMethod.Get);
            if (games == null)
            {
                return new List<Game>();
            }

            return games;
        }

        public Task<User?> GetUserAsync()
        {
            var endpoint = "/api/account";
            return MakeRequestAsync<User>(endpoint, HttpMethod.Get);
        }

        public async Task<List<GameUser>> GetGameUsersAsync()
        {
            var endpoint = "/api/gameusers";
            var gameUsers = await MakeRequestAsync<List<GameUser>>(endpoint, HttpMethod.Get);
            if (gameUsers == null)
            {
                return new List<GameUser>();
            }

            return gameUsers;
        }

        public Task<GameUser?> CreateGameUserAsync(CreateGameUserRequest request)
        {
            var endpoint = "/api/gameusers";
            return MakeRequestAsync<GameUser>(endpoint, HttpMethod.Post, request);
        }

        public async Task<List<Room>> GetRoomsAsync(Guid gameId, string region)
        {
            var endpoint = $"/api/rooms?gameId={gameId}&region={region}";
            var rooms = await MakeRequestAsync<List<Room>>(endpoint, HttpMethod.Get);
            if (rooms == null)
            {
                return new List<Room>();
            }

            return rooms;
        }

        public Task<Room?> GetRoomAsync(Guid roomId)
        {
            var endpoint = $"/api/rooms/{roomId}";
            return MakeRequestAsync<Room>(endpoint, HttpMethod.Get);
        }

        public Task<Room?> CreateRoomAsync(CreateRoomRequest request)
        {
            var endpoint = $"/api/rooms";
            return MakeRequestAsync<Room>(endpoint, HttpMethod.Post, request);
        }

        public Task JoinRoomAsync(Guid id, string? password)
        {
            var endpoint = $"/api/rooms/{id}?password={password}";
            return MakeRequestAsync(endpoint, HttpMethod.Put);
        }

        public Task LeaveRoomAsync(Guid id)
        {
            var endpoint = $"/api/rooms/{id}";
            return MakeRequestAsync(endpoint, HttpMethod.Delete);
        }

        public Task ReadyRoomAsync(Guid id, bool isReady)
        {
            var endpoint = $"/api/rooms/{id}/ready?isReady={isReady}";
            return MakeRequestAsync(endpoint, HttpMethod.Put);
        }

        public Task<Match?> GetMatchAsync(Guid matchId)
        {
            var endpoint = $"/api/matches/{matchId}";
            return MakeRequestAsync<Match>(endpoint, HttpMethod.Get);
        }

        public Task HasJoinedAsync(Guid id)
        {
            var endpoint = $"api/matches/{id}/join";
            return MakeRequestAsync(endpoint, HttpMethod.Post);
        }

        public Task PickPlayerAsync(Guid id, PickPlayerRequest request)
        {
            var endpoint = $"api/matches/{id}/pick";
            return MakeRequestAsync(endpoint, HttpMethod.Post, request);
        }

        public Task SetLobbyIdAsync(Guid id, SetLobbyIdRequest request)
        {
            var endpoint = $"api/matches/{id}/lobby-id";
            return MakeRequestAsync(endpoint, HttpMethod.Post, request);
        }

        public Task VoteMapAsync(Guid id, VoteMapRequest request)
        {
            var endpoint = $"api/matches/{id}/vote-map";
            return MakeRequestAsync(endpoint, HttpMethod.Post, request);
        }

        public Task MatchEndedAsync(Guid id, MatchEndedRequest request)
        {
            var endpoint = $"api/matches/{id}/stats";
            return MakeRequestAsync(endpoint, HttpMethod.Post, request);
        }

        public async Task<bool> LoginIsNeededAsync()
        {
            if (Token == null ||
                string.IsNullOrEmpty(Token.AccessToken) ||
                string.IsNullOrEmpty(Token.RefreshToken))
            {
                return true;
            }

            try
            {
                await RefreshAsync();
            }
            catch
            {
                return true;
            }

            return false;
        }

        public async ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            foreach (var subscription in _activeSubscriptions)
            {
                subscription.Dispose();
            }

            _activeSubscriptions.Clear();

            await _roomHubConnection.DisposeAsync();
            GC.SuppressFinalize(this);
        }

        public Task RegisterAsync(RegisterRequest request)
        {
            var endpoint = $"/api/account";
            return MakeAnonymousRequestAsync(endpoint, HttpMethod.Post, request);
        }

        public Task ChangePasswordAsync(ChangePasswordRequest request)
        {
            var endpoint = $"/api/account/password";
            return MakeRequestAsync(endpoint, HttpMethod.Put, request);
        }

        public Task UpdateUserAsync(UpdateUserRequest request)
        {
            var endpoint = $"/api/account";
            return MakeRequestAsync(endpoint, HttpMethod.Put, request);
        }

        public async Task LogInAsync(string userName, string password)
        {
            var configuration = await _httpClient.GetDiscoveryDocumentAsync(_baseUrl);
            if (configuration.IsError)
            {
                throw new HttpRequestException($"An error occurred while retrieving the configuration document: {configuration.Error}");
            }

            var scope = $"{OidcConstants.StandardScopes.OpenId} " +
                $"{OidcConstants.StandardScopes.Profile} " +
                $"{OidcConstants.StandardScopes.OfflineAccess} " +
                $"{OidcConstants.StandardScopes.Email} " +
                $"roles";

            var response = await _httpClient.RequestPasswordTokenAsync(new PasswordTokenRequest
            {
                Address = configuration.TokenEndpoint,
                UserName = userName,
                Password = password,
                Scope = scope,
            });

            if (!response.IsError)
            {
                Token = new Token()
                {
                    IdentityToken = response­.IdentityToken,
                    AccessToken = response.AccessToken,
                    ExpiresIn = response.ExpiresIn,
                    RefreshToken = response.RefreshToken
                };
            }
            else
            {
                throw new HttpRequestException(response.Error);
            }
        }

        private async Task<T?> MakeRequestAsync<T>(string endpoint, HttpMethod method, object? body = null)
        {
            var accessToken = await GetAccessTokenAsync();

            var url = new Uri(new Uri(_baseUrl), endpoint);
            using var request = new HttpRequestMessage(method, url.ToString());
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            if (body != null)
            {
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    System.Text.Encoding.UTF8,
                    "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            var responseBody = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(responseBody);
            }

            return JsonConvert.DeserializeObject<T>(responseBody);
        }

        private async Task MakeRequestAsync(string endpoint, HttpMethod method, object? body = null)
        {
            var accessToken = await GetAccessTokenAsync();

            var url = new Uri(new Uri(_baseUrl), endpoint);
            using var request = new HttpRequestMessage(method, url.ToString());
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            if (body != null)
            {
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    System.Text.Encoding.UTF8,
                    "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(responseBody);
            }
        }

        private async Task MakeAnonymousRequestAsync(string endpoint, HttpMethod method, object? body = null)
        {
            var url = new Uri(new Uri(_baseUrl), endpoint);
            using var request = new HttpRequestMessage(method, url.ToString());
            if (body != null)
            {
                request.Content = new StringContent(
                    JsonConvert.SerializeObject(body),
                    System.Text.Encoding.UTF8,
                    "application/json");
            }

            using var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException(responseBody);
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            if (Token == null ||
                string.IsNullOrEmpty(Token.AccessToken) ||
                string.IsNullOrEmpty(Token.RefreshToken))
            {
                throw new InvalidOperationException("User is not logged in");
            }

            if (DateTime.Now >= ExpiresAt)
            {
                await RefreshAsync();
            }

            return Token.AccessToken;
        }

        private async Task RefreshAsync()
        {
            if (Token == null ||
                string.IsNullOrEmpty(Token.AccessToken) ||
                string.IsNullOrEmpty(Token.RefreshToken))
            {
                throw new InvalidOperationException("User is not logged in");
            }

            var configuration = await _httpClient.GetDiscoveryDocumentAsync(_baseUrl);
            if (configuration.IsError)
            {
                throw new Exception($"An error occurred while retrieving the configuration document: {configuration.Error}");
            }

            var request = new RefreshTokenRequest
            {
                Address = configuration.TokenEndpoint,
                RefreshToken = Token.RefreshToken,
            };

            var response = await _httpClient.RequestRefreshTokenAsync(request);

            if (response.IsError)
            {
                throw new InvalidOperationException("Cannot refresh token");
            }

            Token = new Token()
            {
                IdentityToken = response­.IdentityToken,
                AccessToken = response.AccessToken,
                ExpiresIn = response.ExpiresIn,
                RefreshToken = response.RefreshToken
            };
        }

        private void ChatMessageReceived(ChatMessage message)
        {
            var handler = ChatMessageEvent;
            handler?.Invoke(this, new ChatMessageEventArgs(message));
        }

        private void RoomChanged(Room room)
        {
            var handler = RoomChangedEvent;
            handler?.Invoke(this, new RoomChangedEventArgs(room));
        }

        private void MatchChanged(RoomMatchMessage match)
        {
            var handler = MatchChangedEvent;
            handler?.Invoke(this, new MatchChangedEventArgs(match));
        }

        private void CreateLobby(CreateLobbyMessage createLobby)
        {
            var handler = CreateLobbyEvent;
            handler?.Invoke(this, new CreateLobbyEventArgs(createLobby.MatchId));
        }

        private void JoinLobby(JoinLobbyMessage joinLobby)
        {
            var handler = JoinLobbyEvent;
            handler?.Invoke(this, new JoinLobbyEventArgs(joinLobby.LobbyId, joinLobby.MatchId, joinLobby.TeamIndex));
        }

        private void SetupLobby(SetupLobbyMessage setupLobby)
        {
            var handler = SetupLobbyEvent;
            handler?.Invoke(this, new SetupLobbyEventArgs(setupLobby.Map!, setupLobby.MatchId));
        }

        private void MatchEnded(MatchEndedMessage matchEnded)
        {
            var handler = MatchEndedEvent;
            handler?.Invoke(this, new MatchEndedEventArgs());
        }

        private HubConnection ConfigureRoomSignalRHub()
        {
            var url = new Uri(new Uri(_baseUrl), "/hubs/room");
            var hubConnection = new HubConnectionBuilder()
            .WithUrl(url, options =>
            {
                options.AccessTokenProvider = async () =>
                {
                    try
                    {
                        return await GetAccessTokenAsync();
                    }
                    catch
                    {
                        return null;
                    }
                };
            })
            .AddMessagePackProtocol()
            .Build();

            _activeSubscriptions.Add(hubConnection.On<ChatMessage>(SignalRMethods.ChatMessage, ChatMessageReceived));
            _activeSubscriptions.Add(hubConnection.On<Room>(SignalRMethods.RoomChanged, RoomChanged));
            _activeSubscriptions.Add(hubConnection.On<RoomMatchMessage>(SignalRMethods.MatchChanged, MatchChanged));
            _activeSubscriptions.Add(hubConnection.On<CreateLobbyMessage>(SignalRMethods.CreateLobby, CreateLobby));
            _activeSubscriptions.Add(hubConnection.On<JoinLobbyMessage>(SignalRMethods.JoinLobby, JoinLobby));
            _activeSubscriptions.Add(hubConnection.On<SetupLobbyMessage>(SignalRMethods.SetupLobby, SetupLobby));
            _activeSubscriptions.Add(hubConnection.On<MatchEndedMessage>(SignalRMethods.MatchEnded, MatchEnded));

            return hubConnection;
        }
    }
}
