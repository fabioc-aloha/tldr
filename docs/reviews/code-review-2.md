# Code Review #2

**Date:** April 9, 2026
**Scope:** Re-review after Code Review #1 fixes applied
**Result:** 4 findings, all fixed

## Findings

| #   | Severity | File             | Finding                                                                      | Fix                                                                                |
| --- | -------- | ---------------- | ---------------------------------------------------------------------------- | ---------------------------------------------------------------------------------- |
| M1  | Medium   | appsettings.json | TTS config still referenced `"edge"` engine                                  | Updated both JSON files to `Engine: "sapi"`                                        |
| M2  | Medium   | MainViewModel.cs | `OperationCanceledException` caught as generic `Exception`                   | Added dedicated `catch (OperationCanceledException)` block                         |
| M3  | Medium   | Summarizer.cs    | `InitializeAsync` didn't forward `CancellationToken` to download/load stages | Added `ct.ThrowIfCancellationRequested()` calls at each async boundary             |
| L1  | Low      | MainViewModel.cs | `_saveTimer` not disposed on application shutdown                            | Implemented `IDisposable` on `MainViewModel`, dispose timer/TTS/CTS in `Dispose()` |
