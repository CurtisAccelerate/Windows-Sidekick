// path: WindowsSidekick/GemmaClient.cs
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WindowsSidekick.Settings;

namespace WindowsSidekick
{
    public class GemmaClient
    {
        private readonly HttpClient _http;
        private readonly AssistantSettings _settings;

        public GemmaClient(AssistantSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _http = new HttpClient { BaseAddress = new Uri(_settings.BaseUrl) };
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private async Task<string> SendChatRequestAsync(object[] messages)
        {
            var payload = new
            {
                model = _settings.ModelId,
                messages,
                stream = _settings.Stream
            };

            var jsonRequest = JsonSerializer.Serialize(payload);
            using var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");
            using var resp = await _http.PostAsync("/v1/chat/completions", content);
            resp.EnsureSuccessStatusCode();

            var jsonResponse = await resp.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(jsonResponse);

            // Extract the assistant's content field
            var root = doc.RootElement;
            var choice = root.GetProperty("choices")[0];
            var message = choice.GetProperty("message");
            var text = message.GetProperty("content").GetString() ?? string.Empty;
            return text.Trim();
        }

        public Task<string> ChatCompletionAsync(string systemPrompt, string userPrompt)
        {
            object[] messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userPrompt }
            };
            return SendChatRequestAsync(messages);
        }

        public Task<string> ChatWithImageAsync(string systemPrompt, string userPrompt, Image screenshot)
        {
            string dataUri;
            using (var ms = new MemoryStream())
            {
                screenshot.Save(ms, ImageFormat.Png);
                string base64 = Convert.ToBase64String(ms.ToArray());
                dataUri = "data:image/png;base64," + base64;
            }

            object[] userContent = new object[]
            {
                new { type = "text",      text      = userPrompt },
                new { type = "image_url", image_url = new { url = dataUri } }
            };

            object[] messages = new object[]
            {
                new { role = "system", content = systemPrompt },
                new { role = "user",   content = userContent }
            };

            return SendChatRequestAsync(messages);
        }
    }
}
