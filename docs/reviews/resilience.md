# Resilience and Error Recovery Review

**Date:** April 9, 2026
**Scope:** Failure mode analysis for all external dependencies and I/O boundaries
**Result:** 4 findings, all fixed

## Findings

| #   | Severity | File             | Finding                                                                                                                                                             | Fix                                                                                                                                |
| --- | -------- | ---------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| E1  | Critical | MainViewModel.cs | `InitializeAsync` has no try/catch around Foundry Local init; if Foundry Local isn't installed or model download fails, the app crashes with an unhandled exception | Wrapped in try/catch; shows "Model failed to load. Ensure Foundry Local is installed." in StatusText; debug trace for diagnostics  |
| E2  | High     | MainViewModel.cs | `LoadFile` calls `FileExtractor.Extract` without catching; corrupt PDF/DOCX or locked file throws to the UI thread                                                  | Added try/catch; `InvalidOperationException` (file-too-large) shows its message; generic catch shows "Failed to read file: {name}" |
| E3  | Medium   | UserSettings.cs  | `Load()` deserializes JSON without catching; corrupt or hand-edited settings.json crashes the app on startup                                                        | Wrapped in try/catch; returns default `UserSettings` on any deserialization failure                                                |
| E4  | Medium   | UserSettings.cs  | `Save()` has no error handling; disk full, permission denied, or antivirus lock crashes the background timer thread                                                 | Wrapped in try/catch; non-fatal, debug trace output                                                                                |
