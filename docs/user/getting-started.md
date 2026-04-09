# Getting Started with TLDR

TLDR is a local-first document summarizer for Windows. It uses a small language model (Phi-4 Mini) running entirely on your machine to distill long documents into concise summaries, then reads them aloud with sentence-by-sentence highlighting.

No data leaves your computer. No API keys required.

## System Requirements

| Requirement | Minimum                                         |
| ----------- | ----------------------------------------------- |
| OS          | Windows 10 21H2 or later                        |
| RAM         | 8 GB (16 GB recommended)                        |
| Disk        | ~4 GB for model download                        |
| Runtime     | .NET 9 Desktop Runtime                          |
| Hardware    | CPU required; NPU/GPU optional for acceleration |

## Installation

1. Install [Microsoft Foundry Local](https://github.com/microsoft/foundry-local) if you don't already have it:

   ```
   winget install Microsoft.FoundryLocal
   ```

2. Download the latest TLDR release and extract it to a folder of your choice.
3. Run `Tldr.exe`. On first launch the app downloads the Phi-4 Mini model (~2.4 GB). A progress bar shows download status.

## First Summarization

1. **Paste text**: Press `Ctrl+V` in the TLDR window, or copy text first and press `Ctrl+Shift+S` from anywhere.
2. **Or drop a file**: Drag a `.pdf`, `.docx`, `.txt`, or `.md` file onto the window.
3. **Pick a style**: Choose Bullets, List, Table, Prose, or Same.
4. **Set detail level**: Slide between Brief (~20%), Standard (~35%), and Detailed (~50%).
5. **Click Distill**: The model processes your text locally and produces a summary.

## What's Next

- [User Guide](user-guide.md) for full feature documentation
- [Keyboard Shortcuts](keyboard-shortcuts.md) for quick reference
