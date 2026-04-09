# Code Review #1

**Date:** April 9, 2026
**Scope:** All source files post-implementation (Phases 1-9 complete)
**Result:** 13 findings, all fixed

## Findings

| #   | Severity | File                | Finding                                                                                           | Fix                                                                                       |
| --- | -------- | ------------------- | ------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------- |
| C1  | Critical | MainWindow.xaml     | WebView2 invisible during TTS reading; Result and Reading used separate visibility                | Shared WebView2 grid with `IsWebViewVisible` (Result or Reading)                          |
| C2  | Critical | MarkdownRenderer.cs | Heading elements missed by sentence marker regex (only li/p/tr matched)                           | Added `SentenceHRegex` for h1-h6                                                          |
| C3  | Critical | SapiTtsEngine.cs    | `PlaybackFinished` fires after cancel due to race between cancellation and loop exit              | Guard: only invoke `PlaybackFinished` when `!_cts.Token.IsCancellationRequested`          |
| C4  | Critical | MainViewModel.cs    | `FormatHtmlClipboard` used `string.Length` (char count) instead of UTF-8 byte offsets for CF_HTML | Switched to `Encoding.UTF8.GetByteCount()` for all offset calculations                    |
| H1  | High     | Summarizer.cs       | `CancellationToken` not forwarded through `InitializeAsync` stages                                | Added `ct.ThrowIfCancellationRequested()` at each async boundary                          |
| H2  | High     | PromptBuilder.cs    | `JsonDocument` held open for lifetime of `PromptBuilder`                                          | Extract all data into `Dictionary` during constructor, dispose `JsonDocument` immediately |
| H3  | High     | MainViewModel.cs    | Settings saved on every property change (disk thrash)                                             | Debounced with 500ms `System.Threading.Timer`                                             |
| H4  | High     | MainViewModel.cs    | TTS reads raw markdown syntax (hashes, bullets, pipes)                                            | Line-by-line markdown stripping before passing to SAPI                                    |
| H5  | High     | Config.cs           | `TtsConfig` defaulted to `"edge"` but Edge TTS was removed; startup would fail                    | Changed default to `"sapi"` with empty voice                                              |
| M1  | Medium   | MainWindow.xaml.cs  | `async void` on hotkey handler caused compiler warning                                            | Changed to fire-and-forget with `_ = Task.Run(...)` pattern                               |
| M2  | Medium   | MainWindow.xaml.cs  | `NavigationCompleted` handler never unsubscribed (leak on repeated loads)                         | Self-unsubscribing handler pattern                                                        |
| M3  | Medium   | Summarizer.cs       | Unused `using Microsoft.Extensions.Logging.Abstractions` after refactor                           | Removed                                                                                   |
| M4  | Medium   | appsettings.json    | TTS engine still set to `"edge"` in both JSON files                                               | Updated to `"sapi"`                                                                       |
