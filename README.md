# Windows Sidekick

*By Curtis White*

Windows Sidekick is a lightweight tray-resident Windows application designed to quickly capture screen contexts and interact with a local large-language model (LLM). This application is entirely local-first, ensuring no cloud connections are required unless explicitly configured.

## 🚀 Quick Start

```bash
git clone https://github.com/CurtisAccelerate/Windows-Sidekick.git
cd Sidekick
dotnet restore
msbuild WindowsSidekick.csproj
```


Press your configured hotkey (`Ctrl + `` by default) anywhere to invoke the assistant.

---

## 📌 Key Features

- **Global Hotkeys**: System-wide shortcuts captured using native Windows hooks.
- **Contextual Screenshots**: Captures active window or entire desktop if window is minimized or invalid.
- **Local LLM Integration**: Communicates via a configurable local HTTP endpoint.
- **Intuitive UI**: Dark-themed, semi-transparent overlays for prompts and responses.

---

## 📁 Project File Structure

```
Sidekick/
│
├── Program.cs                  # Application entry point
├── TrayApplicationContext.cs   # Manages tray icon and hotkeys
├── GemmaClient.cs              # HTTP client for local LLM
├── PromptForm.cs               # UI form for user input prompts
├── ResponseForm.cs             # UI form displaying LLM responses
├── Utils/
│   └── WindowCapture.cs        # Captures screenshots
└── Settings/
    ├── AssistantSettings.cs    # Assistant configuration
    ├── GemmaSettings.cs        # LLM endpoint settings
    └── HotkeySettings.cs       # Hotkey configurations
```

### References:

Webview2
Markdig


---

## 🔧 Configuration

Modify `\WindowsSidekick\Settings\`:

```json
{
  "BaseUrl":      "http://127.0.0.1:1234",
  "ModelId":      "gemma-3-27b-it-qat",
  "SystemPrompt": "**Role:** Windows AI Assistant analyzing screen image & optional user text.\n**Instructions:**\n1. Carefully analyze image for context (app, state).\n2. If user text: Read carefully the ask and use the image context to help answer the question if relevant.\n3. If no text & image has errors: Explain error & suggest fix.\n\nProvide a helpful answer and then emit all commands/code/actions in a code block.\n**Requirement:** ALL commands/code/actions MUST be in ```markdown blocks```. BE CONCISE",
  "Hotkey":       { "Ctrl": true, "Alt": false, "Shift": false, "Win": false, "Key": "Oem3" },
  "ExitHotkey":   { "Ctrl": true, "Alt": false, "Shift": false, "Win": false, "Key": "D1" },
  "Stream":       false
}
```

---

## 📌 Architecture Overview

1. **Hotkey Listener**:

   - Installs native Windows hooks to detect global hotkey presses.
   - Saves the handle of the currently active foreground window.

2. **Window Capture**:

   - Captures screenshots of the active window via `PrintWindow`.
   - Falls back to capturing the entire desktop when the window is minimized or invalid.

3. **Prompt Overlay**:

   - Opens a dark-themed UI for user input.
   - Users can input textual prompts to accompany the captured screenshot.

4. **Local LLM Client**:

   - Sends a JSON payload containing the screenshot and user prompt to a locally configured HTTP endpoint.
   - Default setup targets Gemma 3 27B, but can easily be adapted to other models.

5. **Response Overlay**:

   - Displays the LLM's response in Markdown and code, allowing further follow-up interactions.

---

## 📢 Hotkeys

| Action           | Hotkey     | Description                                   |
| ---------------- | ---------- | --------------------------------------------- |
| Capture & Prompt | `Ctrl + `` | Captures window/desktop, opens prompt overlay |
| Exit Application | `Ctrl + 1` | Quits the Sidekick application                |

- Press `Ctrl + `` to open the prompt overlay.
- Press `Ctrl + 1` to uninstall (exit) Windows Sidekick.

---

## LLM Local

You must be running local LLM at the endpoint. You can do this easily by downloading LM Studio and configuring the service endpoint.

## 📌 Extending and Customizing

- **Hotkeys**: Customize via `settings.json`.
- **LLM Endpoint**: Easily configurable to any local or remote model endpoint* (with configuration).


## 📌 Knwown issues
- It attempts to grab the active application and fall back to the screen grab if that fails: however, there are cases where results where desired screen area is not captured. If complains about blank background, likely the grab failed. There is a diangostic button to view captured image for verification.


---

Windows Sidekick provides immediate contextual assistance directly from your desktop, ensuring rapid, seamless integration with your workflow.

##  Caution
- Caution. No warranty. This was a rapid AI build personal project.
- MIT License
