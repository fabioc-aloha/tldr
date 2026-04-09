# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [0.1.0] - 2026-04-09

### Added

- Local AI summarization powered by Phi-4 Mini via Microsoft Foundry Local
- Five summary styles: Bullets, Numbered List, Table, Prose, Same Format
- Three detail levels: Brief (~20%), Standard (~35%), Detailed (~50%)
- Tone selection: Neutral, Formal, Casual
- Read Aloud with sentence-by-sentence TTS and synchronized highlighting
- Pause, resume, and stop controls during playback
- PDF, DOCX, TXT, and Markdown file drag-and-drop
- Ctrl+V paste from clipboard in any app state
- Global hotkeys: Ctrl+Shift+S (open and paste), Ctrl+Shift+X (stop reading)
- System tray with minimize-on-close behavior
- Dark, Light, and System theme support (WPF-UI Mica backdrop)
- Copy to clipboard with rich HTML and plain Markdown formats
- Re-distill: change options and summarize again without re-pasting
- Settings popup with tone and theme controls
- Persistent user preferences saved to %APPDATA%\Tldr\settings.json
- Detail level slider with continuous adjustment
- Sentence count badge on summary results
- Smoke test mode via --smoke-test flag
- GitHub Actions release pipeline (self-contained single-file build)

### Security

- XSS sanitization on all HTML output (strips event handler attributes)
- WebView2 navigation guard blocks non-file:// URIs
- Content Security Policy on the output HTML template
- Input size limits: 10 MB file size, 2M character maximum
- Generic error messages for unexpected exceptions (no stack trace leakage)
- All 10 NuGet packages pinned to exact versions

### Responsible AI

- 5-rule injection-resistant system prompt (summarize-only, no fabrication)
- User input wrapped in XML delimiters to separate data from instructions
- AI disclosure footer on every summary
- Grounding length guard: warning banner if output exceeds input length

### Accessibility

- AutomationProperties.Name on all interactive controls
- ARIA role="document" on the summary output region
- Keyboard tab navigation on radio button groups
- Heading levels on settings popup sections

[Unreleased]: https://github.com/fabioc-aloha/tldr/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/fabioc-aloha/tldr/releases/tag/v0.1.0
