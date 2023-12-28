using System.Net;
using System.Net.Http.Json;
using System.Text;
using PBL6.Application.Contract.ExternalServices.Meetings.Dtos;

namespace PBL6.Application.ExternalServices
{
    public interface IMeetingServiceEx
    {
        Task<string> CreateSession(Session session);
        Task<string> CreateToken(string sessionId);
        Task CloseSession(string sessionId);
    }

    public class MeetingServiceEx : IMeetingServiceEx
    {
        private readonly IHttpClientFactory _clientFactory;

        public MeetingServiceEx(IHttpClientFactory clientFactory)
        {
            _clientFactory =
                clientFactory ?? throw new ArgumentNullException(nameof(clientFactory));
        }

        public async Task CloseSession(string sessionId)
        {
            var client = _clientFactory.CreateClient("Meeting");
            var response = await client.DeleteAsync(
                "openvidu/api/sessions/" + sessionId.Trim('"')
            );
            response.EnsureSuccessStatusCode();
            return;
        }

        public async Task<string> CreateSession(Session session)
        {
            var client = _clientFactory.CreateClient("Meeting");
            var content = JsonContent.Create(session);
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
