# TLDR User Guide

## Overview

TLDR takes long text and produces a focused summary using a local AI model. Everything runs on your machine with no cloud dependency.

## Input Methods

### Paste from Clipboard

Press `Ctrl+V` at any time. If you're viewing a result or listening to a reading, the current operation stops and TLDR loads the new clipboard text.

### Drag and Drop

Drag any supported file onto the window:

| Format     | Extension |
| ---------- | --------- |
| PDF        | `.pdf`    |
| Word       | `.docx`   |
| Plain text | `.txt`    |
| Markdown   | `.md`     |

### Global Hotkey

Press `Ctrl+Shift+S` from any application. TLDR opens (or comes to the foreground), reads the clipboard, and loads the text automatically.

## Summary Options

### Style

Select one of five output formats before distilling:

| Style       | Description                                          |
| ----------- | ---------------------------------------------------- |
| **Bullets** | Markdown bullet list of key points                   |
| **List**    | Numbered list ordered by importance                  |
| **Table**   | Three-column table: Topic, Key Points, Details       |
| **Prose**   | Flowing paragraph summary                            |
| **Same**    | Condensed version preserving the original formatting |

### Detail Level

A slider with three positions controls how much of the original content is retained:

- **Brief**: ~20% of original length, critical points only
- **Standard**: ~35%, all main points covered
- **Detailed**: ~50%, supporting details and examples preserved

### Tone

Found in the Settings popup (gear icon in the status bar):

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

Click **Read Aloud** to hear the summary spoken sentence by sentence. The current sentence highlights in the output view as it's read.

Controls during playback:

| Button    | Action                                      |
| --------- | ------------------------------------------- |
| **Pause** | Pauses speech; click again to resume        |
| **Stop**  | Stops speech and returns to the result view |

Press `Ctrl+Shift+X` from any application to stop reading immediately.

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

## Settings Persistence

Your last-used Style, Detail, Tone, and Theme choices are saved automatically to `%APPDATA%\Tldr\settings.json` and restored on next launch.

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

- **File size**: Maximum 10 MB per file. Larger files are rejected with a size message.
- **Input length**: Maximum 2,000,000 characters. The model has a 128k token context window. Inputs exceeding the limit (after reserving space for the prompt and output) are rejected with a message showing the token counts.
- **Languages**: The model works best with English text. Other languages may produce lower quality summaries.
- **First launch**: Model download requires an internet connection. After that, TLDR works fully offline.
- **AI-generated content**: Every summary shows an AI disclosure footer. If the summary is longer than the original text, a warning banner flags possible model-added content.
