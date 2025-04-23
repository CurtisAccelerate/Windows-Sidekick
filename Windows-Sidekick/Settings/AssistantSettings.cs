// path: WindowsSidekick/Settings/AssistantSettings.cs
using WindowsSidekick.Settings;

namespace WindowsSidekick.Settings
{
    public class AssistantSettings
    {
        /// <summary>
        /// Base URL for the local Gemma server (include port, e.g. http://127.0.0.1:1234)
        /// </summary>
        public string BaseUrl { get; set; } = "http://127.0.0.1:1234";

        /// <summary>
        /// Model identifier as reported by LM Studio (e.g. "gemma-3-27b-it-qat")
        /// </summary>
        public string ModelId { get; set; } = "gemma-3-27b-it-qat";

        /// <summary>
        /// Whether to request streaming responses
        /// </summary>
        public bool Stream { get; set; } = false;

        /// <summary>
        /// System prompt instructing the assistant’s overall behavior and format requirements
        /// </summary>
        public string SystemPrompt { get; set; } =
    @"**Role:** Windows AI Assistant analyzing screen image & optional user text.
        **Instructions:**
        1. Carefully analyze image for context (app, state).
        2. If user text: Read carefully the ask and use the image context to help answer the question if relevant.
        3. If no text & image has errors: Explain error & suggest fix.

        Provide a helpful answer and then emit all commands/code/actions in a code block.
        **Requirement:** ALL commands/code/actions MUST be in ```markdown blocks```. BE CONCISE";
        /// <summary>
        /// Hotkey configuration for opening the prompt dialog
        /// </summary>
        public HotkeySettings Hotkey { get; set; } = new HotkeySettings();
    }
}
