# TLDR

Local-first document summarizer for Windows. Paste or drop text and get a concise summary powered by a small language model running entirely on your machine. No cloud, no API keys, no data leaves your computer.

## Features

| Feature              | Description                                                  |
| -------------------- | ------------------------------------------------------------ |
| **Local AI**         | Phi-4 Mini via Microsoft Foundry Local (CPU, GPU, or NPU)    |
| **5 summary styles** | Bullets, Numbered List, Table, Prose, Same Format            |
| **3 detail levels**  | Brief (~20%), Standard (~35%), Detailed (~50%)               |
| **Read Aloud**       | Sentence-by-sentence TTS with synchronized highlighting      |
| **File support**     | PDF, DOCX, TXT, Markdown drag-and-drop                       |
| **Global hotkeys**   | Ctrl+Shift+S to open and paste, Ctrl+Shift+X to stop reading |
| **System tray**      | Runs in background, always one hotkey away                   |
| **Fluent UI**        | WPF-UI Mica window with dark/light/system theme              |

## Requirements

- Windows 10 21H2 or later
- .NET 9 Desktop Runtime
- [Microsoft Foundry Local](https://github.com/microsoft/foundry-local)
- ~4 GB disk space for model download

## Quick Start

```powershell
# Install Foundry Local
winget install Microsoft.FoundryLocal

# Build
dotnet build src\Tldr\Tldr.csproj

# Run
dotnet run --project src\Tldr\Tldr.csproj
```

On first launch the app downloads Phi-4 Mini (~2.4 GB). After that it works fully offline.

## Publish

```powershell
dotnet publish src\Tldr\Tldr.csproj -c Release --self-contained -o publish
```

## Project Structure

```
src/Tldr/
  Core/           Summarizer, PromptBuilder, FileExtractor, MarkdownRenderer, Config, Enums
  Platform/       MainWindow (WPF-UI), ViewModel, SapiTtsEngine, HotkeyManager, TrayIcon, Converters
  Assets/         output.html (WebView2 template), icon.ico
  prompts.json    LLM prompt fragments (style, detail, tone)
  appsettings.json  Runtime configuration
  SmokeTest.cs    Console smoke test (--smoke-test flag)
  GlobalUsings.cs Type alias disambiguation for WPF/WinForms
docs/
  plans/          Implementation plan and solution vision
  port/           Port/migration notes
  reviews/        Code, UI/UX, security, RAI, accessibility, resilience, performance, license reviews
  user/           Getting started, user guide, keyboard shortcuts
test-cases/       Sample documents for testing
```

## Documentation

- [Getting Started](docs/user/getting-started.md)
- [User Guide](docs/user/user-guide.md)
- [Keyboard Shortcuts](docs/user/keyboard-shortcuts.md)
- [Reviews](docs/reviews/) (code, UI/UX, security, RAI, accessibility, resilience, performance, license)

## License

[MIT](LICENSE)
