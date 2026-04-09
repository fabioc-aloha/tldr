<p align="center">
  <img src="docs/assets/banner.png" alt="TLDR — Local AI Document Summarizer" />
</p>

<h1 align="center">TLDR</h1>

<p align="center">
  Local AI document summarizer for Windows.<br/>
  Paste or drop text, get a concise summary. No cloud, no API keys, no data leaves your machine.
</p>

<p align="center">
  <a href="../../releases/latest"><img src="https://img.shields.io/github/v/release/fabioc-aloha/tldr?style=flat-square" alt="Latest Release" /></a>
  <img src="https://img.shields.io/badge/platform-Windows%2010%2B-blue?style=flat-square" alt="Platform" />
  <img src="https://img.shields.io/github/license/fabioc-aloha/tldr?style=flat-square" alt="License" />
</p>

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
| **Fluent UI**        | WPF-UI Mica window with dark, light, and system themes       |

## Install

**Option A: Download release** (recommended)

1. Install [Microsoft Foundry Local](https://github.com/microsoft/foundry-local): `winget install Microsoft.FoundryLocal`
2. Download the latest ZIP from [Releases](../../releases/latest)
3. Extract and run `Tldr.exe`

The release is self-contained: no .NET install needed.

**Option B: Build from source**

```powershell
winget install Microsoft.FoundryLocal
dotnet build src\Tldr\Tldr.csproj
dotnet run --project src\Tldr\Tldr.csproj
```

On first launch the app downloads Phi-4 Mini (~2.4 GB). After that it works fully offline.

## Usage

1. **Paste** text with Ctrl+V, or press Ctrl+Shift+S from any app
2. **Or drop** a PDF, DOCX, TXT, or Markdown file onto the window
3. **Pick** a style and detail level
4. **Click Distill** to summarize locally
5. **Read Aloud** to hear it spoken with sentence highlighting

## Documentation

| Document                                              | Description                       |
| ----------------------------------------------------- | --------------------------------- |
| [Getting Started](docs/user/getting-started.md)       | Installation, first summarization |
| [User Guide](docs/user/user-guide.md)                 | Full feature reference            |
| [Keyboard Shortcuts](docs/user/keyboard-shortcuts.md) | Hotkey quick reference            |

## Project Structure

```
src/Tldr/
  Core/             Summarizer, PromptBuilder, FileExtractor, MarkdownRenderer, Config
  Platform/         MainWindow, ViewModel, SapiTtsEngine, HotkeyManager, TrayIcon
  Assets/           output.html (WebView2 template), icon
  prompts.json      LLM prompt fragments
  appsettings.json  Runtime configuration
docs/
  assets/           Banner and branding images
  plans/            Implementation plan and solution vision
  reviews/          Code, UI/UX, security, RAI, accessibility, resilience, performance, license
  user/             Getting started, user guide, keyboard shortcuts
scripts/            Replicate image generation (banner, icon)
test-cases/         Sample documents for testing
```

## Release

Pushing a version tag triggers the GitHub Actions pipeline, which builds a self-contained single-file exe and publishes a GitHub Release with the ZIP attached.

```powershell
git tag v0.1.0
git push origin v0.1.0
```

<details>
<summary>Manual publish</summary>

```powershell
dotnet publish src\Tldr\Tldr.csproj -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

</details>

## License

[MIT](LICENSE)
