using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Security.Cryptography.X509Certificates;

namespace AikidoLive.Services
{
    public class VimeoAPI
    {
        private string? _clientId;
        private string? _accessToken;
        private string? _accessTokenReturn;
        private string? _clientSecret;
        private string? _acceptHeader;
        private string? _versionAPI;
        private string? _userId;

        public VimeoAPI()
        {
            _clientId = " "!;
            _accessToken = " "!;
            _accessTokenReturn = " "!;
            _clientSecret = " "!;
            _acceptHeader = " "!;
            _versionAPI = " "!;
            _userId = " "!;
        }
        public VimeoAPI(IConfiguration configuration)
        {
            var vimeoSettings = configuration.GetSection("VimeoAPI");
            if (vimeoSettings == null)
            {
                throw new Exception("VimeoAPI section not found in appsettings.json");
            }
            else
            {
                _clientId = vimeoSettings["clientId"] ?? "";
                _accessToken = vimeoSettings["AccessToken"] ?? "";
                _clientSecret = vimeoSettings["clientSecret"] ?? "";
                _acceptHeader = vimeoSettings["acceptHdr"] ?? "";
                _versionAPI = vimeoSettings["versionAPI"] ?? "";
                _userId = vimeoSettings["userId"] ?? "";
            }
        }

        public async Task<string> Authorization()
        {
            var url = "https://api.vimeo.com/oauth/authorize";
            using (var client = new HttpClient())
            {
                var requestBody = new FormUrlEncodedContent(new[]
                {
                    new KeyValuePair<string, string>("response_type", "code"),
                    new KeyValuePair<string, string>("client_id", _clientId ?? ""),
                    new KeyValuePair<string, string>("redirect_uri", "redirectUri"),
                    new KeyValuePair<string, string>("state", "1234567890"),
                    new KeyValuePair<string, string>("scope", "public private unlisted")
                });

                var response = await client.PostAsync(url, requestBody);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var json = JObject.Parse(content);
                    string code = json["code"].ToString();
                    string state = json["state"].ToString();

                    url = "https://api.vimeo.com/oauth/access_token";
                    using (var client1 = new HttpClient())
                    {
                        var requestBody1 = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("grant_type", "authorization_code"),
                            new KeyValuePair<string, string>("code", code ?? ""),
                            new KeyValuePair<string, string>("redirect_uri", "redirectUri"),
                            new KeyValuePair<string, string>("client_id", _clientId ?? ""),
                            new KeyValuePair<string, string>("client_secret", _clientSecret ?? "")
                        });

                        var response1 = await client1.PostAsync(url, requestBody);

                        if (response1.IsSuccessStatusCode)
                        {
                            var content1 = await response.Content.ReadAsStringAsync();
                            var json1 = JObject.Parse(content);
                            _accessTokenReturn = json1["access_token"].ToString();
                            return _accessTokenReturn;
                        }
                        else
                        {
                            throw new HttpRequestException($"POST request to {url} failed with status code {response.StatusCode}");
                        }
                    }
                }
                else
                {
                    throw new HttpRequestException($"POST request to {url} failed with status code {response.StatusCode}");
                }
            }
        }

        public async Task<string> GetFolders()
        {
            var url = "https://api.vimeo.com/users/" + _userId +"/folders";
            return await MakeGetRequest(url);
        }

        public async Task<string> MakeGetRequest(string url)
        {
            using (var client = new HttpClient())
            {
                // Set the Accept header
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_acceptHeader ?? ""));
                client.DefaultRequestHeaders.Add("Accept", _acceptHeader + ";" + _versionAPI);
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);

                // Set other headers
                client.DefaultRequestHeaders.Add("HeaderName", "HeaderValue");
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return content;
                }
                else
                {
                    throw new HttpRequestException($"GET request to {url} failed with status code {response.StatusCode}");
                }
            }
        }
    }
}