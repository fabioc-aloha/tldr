# Performance Review

**Date:** April 9, 2026
**Scope:** Memory usage, large input handling, tray-resident behavior
**Result:** 2 findings fixed, 1 accepted risk

## Findings

| #   | Severity | File             | Finding                                                                                                                                                 | Fix                                                                                                                                                         |
| --- | -------- | ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------- |
| F1  | Medium   | MainViewModel.cs | `BackToReady()` used property setters which trigger WordCount split on empty string; large strings (InputText, SummaryHtml) kept alive until next paste | Set backing fields directly, then fire PropertyChanged; avoids unnecessary WordCount recalculation and ensures immediate GC eligibility                     |
| F2  | Low      | MainViewModel.cs | Model stays loaded in RAM (~1.5-2GB) while app is minimized to tray                                                                                     | Accepted risk: model load takes 10-30s, unloading on minimize would create poor UX on re-open. Future consideration: unload after configurable idle timeout |

## No Issues Found

- **File extraction**: 10 MB limit (from security review) prevents OOM from large files
- **Input length**: 2M char limit prevents oversized string allocation
- **Settings save**: Timer-debounced at 500ms prevents disk thrash
- **TTS**: Sentence-by-sentence streaming, no full audio buffer in memory
- **WebView2**: Single instance reused across Result/Reading states
