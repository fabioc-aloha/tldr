# TLDR User Guide

## Overview

TLDR takes long text and produces a focused summary using a local AI model (Phi-4 Mini via Microsoft Foundry Local). Everything runs on your machine with no cloud dependency.

## Input Methods

### Paste from Clipboard

Press Ctrl+V at any time. If you're viewing a result or listening to a reading, the current operation stops and TLDR loads the new clipboard text.

### Drag and Drop

Drag any supported file onto the window:

| Format     | Extension |
| ---------- | --------- |
| PDF        | `.pdf`    |
| Word       | `.docx`   |
| Plain text | `.txt`    |
| Markdown   | `.md`     |

**Limits**: 10 MB per file, 2,000,000 characters maximum.

### Global Hotkey

Press Ctrl+Shift+S from any application. TLDR opens (or comes to the foreground), reads the clipboard, and loads the text automatically.

## Summary Options

### Style

Select one of six output formats before distilling:

| Style       | Description                                           |
| ----------- | ----------------------------------------------------- |
| **Bullets** | Markdown bullet list of key points                    |
| **List**    | Numbered list ordered by importance                   |
| **Table**   | Three-column table: Topic, Key Points, Details        |
| **Prose**   | Flowing paragraph summary                             |
| **Simple**  | Plain everyday language, no jargon or technical terms |
| **Same**    | Condensed version preserving the original formatting  |

### Detail Level

A slider with three positions controls how much of the original content is retained:

- **Brief**: ~20% of original length, critical points only
- **Standard**: ~35%, all main points covered
- **Detailed**: ~50%, supporting details and examples preserved

### Tone

Chip toggles in the main window, below the detail level:

- **Neutral**: No tone modification (default)
- **Formal**: Professional, academic language
- **Casual**: Conversational, approachable language

## Working with Results

### Copy

Click **Copy** to place the summary on the clipboard in two formats:

- **Rich HTML**: Pastes with formatting into Word, Outlook, Teams, etc.
- **Plain Markdown**: Falls back to raw text in editors that don't support HTML paste.

### Re-distill

Click **Re-distill** to return to the Loaded state with your original text intact. Change style, detail, or tone, then Distill again.

### Read Aloud

Click **Read Aloud** to hear the summary spoken sentence by sentence. The current sentence highlights in the output view as it's read. The voice can be changed in Settings.

Controls during playback:

| Button    | Action                                      |
| --------- | ------------------------------------------- |
| **Pause** | Pauses speech; click again to resume        |
| **Stop**  | Stops speech and returns to the result view |

Press Ctrl+Shift+X from any application to stop reading immediately.

## Voice

Open Settings (gear icon) and select a voice from the dropdown. The list shows available English neural voices from the Edge speech service. Your selection is saved and restored on next launch.

The TTS engine falls back automatically: Edge neural voices (best quality, needs network) > WinRT voices > SAPI5 (offline, always available).

## Theme

Open Settings (gear icon) and choose:

- **System**: Follows Windows dark/light preference
- **Dark**: Forces dark mode
- **Light**: Forces light mode

The theme applies to both the app chrome and the summary output view.

## System Tray

Closing the window minimizes TLDR to the system tray instead of exiting. Use the tray icon to:

- **Double-click**: Restore the window
- **Right-click > Open**: Restore the window
- **Right-click > Exit**: Quit the application

## Machine Info

The Settings popup includes a machine configuration box showing:

- AI model name and context window size
- CPU core count and available RAM
- Active TTS engine

This helps verify which hardware resources the app is using.

## Settings Persistence

Your last-used Style, Detail, Tone, Theme, and Voice choices are saved automatically to `%APPDATA%\Tldr\settings.json` and restored on next launch.

## Configuration

Advanced settings live in `appsettings.json` beside the executable:

| Setting                 | Default        | Description                     |
| ----------------------- | -------------- | ------------------------------- |
| `Llm.Model`             | `phi-4-mini`   | Foundry Local model alias       |
| `Llm.MaxOutputTokens`   | `1024`         | Maximum tokens in the summary   |
| `Hotkeys.OpenWindow`    | `Ctrl+Shift+S` | Global hotkey to open and paste |
| `Hotkeys.StopReading`   | `Ctrl+Shift+X` | Global hotkey to stop TTS       |
| `Window.MinimizeToTray` | `true`         | Close button minimizes to tray  |

## Limitations

- **File size**: 10 MB per file maximum.
- **Input length**: 2,000,000 characters maximum. The model's 128k token context window is the effective ceiling; inputs that exceed it (after reserving space for prompt and output) are rejected with a message.
- **Languages**: Best results with English. Other languages may produce lower quality summaries.
- **First launch**: Model download (~2.4 GB) requires an internet connection. After that, TLDR works fully offline.
- **AI disclosure**: Every summary shows an AI-generated content footer. If the summary is longer than the input, a warning banner flags possible hallucinated content.
