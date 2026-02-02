// credits:
// https://github.com/Kno2/Kno2.ApiTestClient/blob/de2cc748e43691bef44b80747128b9b722d3b071/src/Kno2.ApiTestClient.Core/MessageHandlers/AccessTokenHandler.cs

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SutureHealth.Patients.Helpers;
using SutureHealth.Patients.Resources;

namespace SutureHealth.Patients.MessageHandlers
{
    public class AccessTokenHandler : DelegatingHandler
    {
        private readonly Uri _authUri;
        private readonly string _clientId;
        private readonly string _clientSecret;
        private readonly string _appId;

        public AccessTokenHandler(Uri authUri, string clientId, string clientSecret, string appId)
        {
            _authUri = authUri;
            _clientId = clientId;
            _clientSecret = clientSecret;
            _appId = appId;
        }

        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            IToken token = TokenHelpers.GetRefreshToken();

            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, _authUri);
            httpRequestMessage.Headers.Add("AppId", _appId);
            HttpResponseMessage httpResponseMessage;

            // Determine what type of token request to make (if needed) based on tracking
            // the timestamps of the access token and the state of the local token
            switch (token.TokenRequestType)
            {
                case TokenRequestType.None:
                    break;

                case TokenRequestType.ClientCredential:

                    httpResponseMessage = await PrimaryTokenRequest(httpRequestMessage, cancellationToken);
                    token = JsonConvert.DeserializeObject<AuthResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
                    token.Save();
                    break;

                case TokenRequestType.RefreshToken:

                    httpResponseMessage = await RefreshTokenRequest(httpRequestMessage, token.RefreshToken, cancellationToken);
                    string jsonResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                    token = JsonConvert.DeserializeObject<AuthResponse>(jsonResponse);

                    // The refresh token request failed - try the primary token
                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        httpResponseMessage = await PrimaryTokenRequest(httpRequestMessage, cancellationToken);
                        token = JsonConvert.DeserializeObject<AuthResponse>(await httpResponseMessage.Content.ReadAsStringAsync());
                    }

                    token.Save();
                    break;
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.AccessToken);
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);

            return response;
        }

        private Task<HttpResponseMessage> PrimaryTokenRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "client_credentials")
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            request.Headers.Authorization = new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.ASCII.GetBytes(string.Format("{0}:{1}", _clientId, _clientSecret))));

            return base.SendAsync(request, cancellationToken);
        }

        private Task<HttpResponseMessage> RefreshTokenRequest(HttpRequestMessage request, string refreshToken, CancellationToken cancellationToken)
        {
            var nameValueCollection = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("refresh_token", refreshToken)
            };

            request.Content = new FormUrlEncodedContent(nameValueCollection);

            return base.SendAsync(request, cancellationToken);
        }
    }
}
