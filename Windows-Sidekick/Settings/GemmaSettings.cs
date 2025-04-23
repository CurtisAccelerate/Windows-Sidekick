// path: WindowsSidekick/Settings/GemmaSettings.cs
namespace WindowsSidekick.Settings
{
    public class GemmaSettings
    {
        /// <summary>
        /// Base URL for the local Gemma server (include port, e.g. http://127.0.0.1:1234)
        /// </summary>
        public string BaseUrl { get; set; } = "http://127.0.0.1:1234";

        /// <summary>
        /// Model identifier as reported by LM Studio (e.g. "gemma-3-27b-it-qat")
        /// </summary>
        public string ModelId { get; set; } = "gemma-3-27b-it-qat";

        /// <summary>
        /// Whether to request streaming responses
        /// </summary>
        public bool Stream { get; set; } = false;
    }
}
