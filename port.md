# Porting TLDR to Other Platforms

## Platform Feasibility

| Platform | Effort   | LLM Runtime                                    | UI Framework                 | Feasibility            |
| -------- | -------- | ---------------------------------------------- | ---------------------------- | ---------------------- |
| macOS    | Moderate | Foundry Local (native Apple Silicon support)   | Avalonia UI + FluentAvalonia | Realistic v2 goal      |
| Linux    | Moderate | Foundry Local (native support)                 | Avalonia UI + FluentAvalonia | Same effort as macOS   |
| iOS      | High     | Core ML (ONNX conversion) or llama.cpp (Metal) | SwiftUI (full rewrite)       | Different app entirely |
| Android  | High     | llama.cpp (Vulkan) or ONNX Runtime Mobile      | .NET MAUI or Kotlin          | Different app entirely |

## macOS / Linux: What Changes

| Component              | Windows (current)             | macOS / Linux equivalent                         |
| ---------------------- | ----------------------------- | ------------------------------------------------ |
| UI framework           | WPF + WPF-UI (Fluent Design)  | Avalonia UI + FluentAvalonia                     |
| Markdown panel         | WebView2 (Chromium)           | Avalonia WebView or CefGlue                      |
| TTS (neural)           | Edge speech service WebSocket | Same (works cross-platform)                      |
| TTS (offline fallback) | System.Speech (SAPI5)         | AVSpeechSynthesizer (macOS), espeak (Linux)      |
| Audio playback         | NAudio (Windows audio APIs)   | NAudio.Vorbis or platform invoke                 |
| Global hotkeys         | RegisterHotKey Win32 P/Invoke | CGEvent tap (macOS), X11/DBus (Linux)            |
| System tray            | NotifyIcon (WinForms)         | NSStatusItem (macOS), StatusNotifierItem (Linux) |
| Clipboard              | WPF Clipboard.GetText()       | Avalonia Clipboard API                           |
| File drop              | WPF DragDrop                  | Avalonia DragDrop                                |
| Single instance        | Mutex                         | Named pipe or file lock                          |
| Mica backdrop          | WPF-UI Mica                   | Platform-native translucency or solid fallback   |

### What Ports Unchanged (zero effort)

- `Summarizer.cs`: Foundry Local SDK is cross-platform
- `PromptBuilder.cs`: pure C# string assembly
- `FileExtractor.cs`: PdfPig and OpenXml are cross-platform .NET libraries
- `MarkdownRenderer.cs`: Markdig is cross-platform .NET
- `appsettings.json` and `prompts.json`: configuration files
- All prompt engineering and chunking logic

### Estimated Rewrite Scope

- ~40% of codebase: all UI (XAML, code-behind, state machine) and platform services (tray, hotkeys, audio)
- ~0% of business logic: summarizer, prompts, file extraction, markdown rendering
- ~10% adapter layer: TTS offline fallback, clipboard, single-instance

## iOS: Why It's a Different App

1. **Model size**: Phi-4 Mini (3.6 GB) exceeds practical iOS RAM budgets. Qwen 0.5B (500 MB) is the realistic ceiling.
2. **Runtime**: Foundry Local doesn't support iOS. Options are Core ML (convert ONNX via `coremltools`) or `llama.cpp` with Metal GPU acceleration.
3. **UI**: WPF has no iOS path. SwiftUI is the standard; .NET MAUI is possible but weaker on iOS.
4. **TTS**: AVSpeechSynthesizer is excellent on iOS (neural voices built in). This is the one component that gets easier.
5. **Distribution**: App Store review, sandboxing, background processing limits.
6. **Shared assets**: Only `prompts.json` content and the prompt engineering approach transfer directly.

## Architecture Decisions to Facilitate Porting

### 1. Clean Separation of Concerns

Keep platform-agnostic logic in standalone classes with no UI or Windows dependencies:

```
src/Tldr/
  Core/                    # Portable (zero platform deps)
    Summarizer.cs          # Foundry Local SDK calls
    PromptBuilder.cs       # Prompt assembly from fragments
    FileExtractor.cs       # PDF, DOCX, TXT parsing
    MarkdownRenderer.cs    # Markdig HTML conversion
    Config.cs              # Strongly-typed settings
    TokenEstimator.cs      # Text length heuristics
  Platform/                # Windows-specific
    MainWindow.xaml        # WPF-UI window
    TrayIconManager.cs     # NotifyIcon
    HotkeyManager.cs       # Win32 RegisterHotKey
    TextToSpeech.cs        # Edge WebSocket + SAPI5
    AudioPlayer.cs         # NAudio playback
```

### 2. Interface Abstractions for Platform Services

Define interfaces in Core that Platform classes implement:

```csharp
// Core/ITtsEngine.cs
public interface ITtsEngine
{
    Task SpeakAsync(string text, string voice, CancellationToken ct);
    void Stop();
}

// Core/IClipboardService.cs
public interface IClipboardService
{
    string? GetText();
    void SetText(string plainText, string? htmlText = null);
}

// Core/IHotkeyService.cs
public interface IHotkeyService
{
    bool Register(string id, string keys, Action callback);
    void UnregisterAll();
}
```

### 3. No Direct WPF References in Core

- Core classes must not reference `System.Windows`, `PresentationCore`, or `WindowsBase`
- Use events or callbacks for progress reporting (not WPF Dispatcher)
- Return plain strings or DTOs, not WPF-specific types

### 4. Configuration as Plain JSON

- `appsettings.json` and `prompts.json` are already portable
- Avoid Windows registry or AppData paths in Core; pass paths from Platform layer

### 5. Avalonia Migration Path

If porting to macOS/Linux, Avalonia is the lowest-friction path:

- XAML syntax is 90% compatible with WPF
- DataBinding, styles, and templates transfer with minor adjustments
- FluentAvalonia provides Fluent Design controls similar to WPF-UI
- `dotnet publish -r osx-arm64` produces a native macOS bundle
