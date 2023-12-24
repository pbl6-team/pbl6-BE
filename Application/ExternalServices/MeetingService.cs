using System.Net;
using System.Net.Http.Json;
using System.Text;

namespace PBL6.Application.ExternalServices
{
    public interface IMeetingService
    {
        Task<string> CreateSession(string session);
        Task<string> CreateToken(string sessionId);
    }

    public class    MeetingService : IMeetingService
    {
        private readonly IHttpClientFactory _clientFactory;

        public MeetingService(IHttpClientFactory clientFactory)
        {
            _clientFactory =
                clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task<string> CreateSession(string session)
        {
            var client = _clientFactory.CreateClient("Meeting");
            var content = new StringContent(session, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("openvidu/api/sessions", content);
            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return "";
            }
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadFromJsonAsync<
                Dictionary<string, object>
            >();
            var sessionId = responseBody["sessionId"].ToString().Trim('"');

            return sessionId;
        }

        public async Task<string> CreateToken(string sessionId)
        {
            var client = _clientFactory.CreateClient("Meeting");
            var content = new StringContent("", Encoding.UTF8, "application/json");
            var response = await client.PostAsync(
                "openvidu/api/sessions/" + sessionId.Trim('"') + "/connection",
                content
            );
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadFromJsonAsync<
                Dictionary<string, object>
            >();
            var token = responseBody["token"].ToString().Trim('"');
            return token;
        }
    }
}
