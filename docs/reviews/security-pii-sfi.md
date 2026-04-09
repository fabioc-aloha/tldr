# Security, PII, and SFI Review

**Date:** April 9, 2026
**Scope:** Full codebase review against OWASP Top 10, STRIDE, Microsoft SFI, PII exposure
**Result:** 6 findings, all fixed

## Findings

| #   | Severity | Category         | File                               | Finding                                                                                                                                                 | Fix                                                                                                               |
| --- | -------- | ---------------- | ---------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------- |
| S1  | Critical | XSS              | output.html                        | `setContent(html)` injects LLM output via `innerHTML` with no sanitization; event-handler vectors (`onerror`, `onload`) bypass innerHTML's script block | Regex sanitizer strips all `on*=` attributes before injection                                                     |
| S2  | High     | Navigation       | MainWindow.xaml.cs                 | WebView2 allows unrestricted navigation; links in LLM output could reach external/malicious URLs                                                        | `NavigationStarting` handler blocks non-`file://` URIs                                                            |
| S3  | High     | SFI/Supply Chain | Tldr.csproj                        | All 10 NuGet packages use wildcard versions (`1.*`, `0.*`); supply-chain attack surface                                                                 | Pinned all packages to resolved versions (e.g., `WPF-UI 4.2.0`, `WebView2 1.0.3856.49`)                           |
| S4  | Medium   | XSS Defense      | output.html                        | No Content Security Policy; no defense-in-depth against injected inline handlers                                                                        | Added restrictive CSP: `default-src 'none'; style-src 'unsafe-inline'; script-src 'unsafe-inline'; img-src data:` |
| S5  | Medium   | DoS              | FileExtractor.cs, MainViewModel.cs | No file size or input length limit; multi-GB file causes OOM                                                                                            | 10 MB file limit in `FileExtractor`, 2M char limit in `LoadText`                                                  |
| S6  | Low      | PII              | MainViewModel.cs                   | Raw `ex.Message` in StatusText could leak document content in error paths                                                                               | Generic error for unexpected exceptions; `InvalidOperationException` preserved; debug trace for diagnostics       |

## Additional Hardening

- `highlightSentence()` validates integer parameter with `parseInt`/`isNaN` guard
- Template literal in `highlightSentence` replaced with string concatenation (no template injection surface)
