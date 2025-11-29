using System.Net.Http.Json;
using System.Text.Json;
using VtmCharacterGenerator.Core.Models;
using System.Text;
using System.Net.Http.Headers;

namespace VtmCharacterGenerator.Core.Services
{
    public class PdfServiceClient
    {
        private readonly HttpClient _httpClient;

        public PdfServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<Stream> GeneratePdfStreamAsync(Character character)
        {
            var jsonContent = JsonSerializer.Serialize(character, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var request = new HttpRequestMessage(HttpMethod.Post, "/generate-pdf");
            request.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");


            var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"PDF Service Error ({response.StatusCode}): {errorContent}");
            }

            return await response.Content.ReadAsStreamAsync();
        }
    }
}