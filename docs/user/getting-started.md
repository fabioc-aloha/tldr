# Getting Started with TLDR

TLDR is a local-first document summarizer for Windows. It uses Phi-4 Mini running entirely on your machine to distill long documents into concise summaries, then reads them aloud with sentence-by-sentence highlighting.

No data leaves your computer. No API keys required.

## System Requirements

| Requirement | Minimum                                                               |
| ----------- | --------------------------------------------------------------------- |
| OS          | Windows 10 21H2 or later                                              |
| RAM         | 8 GB (16 GB recommended)                                              |
| Disk        | ~4 GB for model download                                              |
| Dependency  | [Microsoft Foundry Local](https://github.com/microsoft/foundry-local) |
| Hardware    | CPU required; NPU/GPU optional for acceleration                       |

## Installation

### Option A: Download release (recommended)

1. Install [Microsoft Foundry Local](https://github.com/microsoft/foundry-local):

   ```
   winget install Microsoft.FoundryLocal
   ```

2. Download the latest ZIP from [Releases](https://github.com/fabioc-aloha/tldr/releases/latest).
3. Extract to a folder of your choice and run `Tldr.exe`.

The release is self-contained: no .NET install needed.

### Option B: Build from source

Requires [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0).

```
winget install Microsoft.FoundryLocal
dotnet build src\Tldr\Tldr.csproj
dotnet run --project src\Tldr\Tldr.csproj
```

On first launch the app downloads Phi-4 Mini (~2.4 GB). After that it works fully offline.

## First Summarization

1. **Paste text**: Press Ctrl+V in the TLDR window, or copy text first and press Ctrl+Shift+S from anywhere.
2. **Or drop a file**: Drag a `.pdf`, `.docx`, `.txt`, or `.md` file onto the window.
3. **Pick a style**: Choose Bullets, List, Table, Prose, or Same.
4. **Set detail level**: Slide between Brief (~20%), Standard (~35%), and Detailed (~50%).
5. **Click Distill**: The model processes your text locally and produces a summary.

## What's Next

- [User Guide](user-guide.md) for full feature documentation
- [Keyboard Shortcuts](keyboard-shortcuts.md) for quick reference
