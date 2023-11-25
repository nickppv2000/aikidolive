using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;

namespace AikidoLive.Services
{
    public class VimeoAPI
    {
        private string _clientId;
        private string _accessToken;
        private string _clientSecret;
        VimeoAPI(IConfiguration configuration)
        {
            var vimeoSettings = configuration.GetSection("VimeoAPI");
            _clientId = vimeoSettings["clientId"];
            _accessToken = vimeoSettings["AccessToken"];
            _clientSecret = vimeoSettings["clientSecret"];
        }

        public async Task<string> MakeGetRequest(string url)
        {
            using (var client = new HttpClient())
            {
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