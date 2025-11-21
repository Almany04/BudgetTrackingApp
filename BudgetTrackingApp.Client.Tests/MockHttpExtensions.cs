using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;
using System.Net.Http.Headers;

namespace BudgetTrackingApp.Client.Tests
{
    public static class MockHttpExtensions
    {
        // Ez a metódus lehetővé teszi, hogy így írd: .RespondJson(new { ... })
        public static void RespondJson<T>(this MockedRequest request, T content, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var json = JsonSerializer.Serialize(content);

            request.Respond(req =>
            {
                var response = new HttpResponseMessage(statusCode);
                response.Content = new StringContent(json);
                response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                return response;
            });
        }
    }
}