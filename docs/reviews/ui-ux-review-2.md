# UI/UX Review #2

**Date:** April 9, 2026
**Scope:** Second polish pass after UI/UX Review #1 fixes
**Result:** 6 findings, all fixed

## Findings

| #   | Severity | File               | Finding                                                                                         | Fix                                                                                 |
| --- | -------- | ------------------ | ----------------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------- |
| P1  | High     | MainWindow.xaml    | Detail slider hardcoded `Value="1"`, ignores persisted Detail setting                           | Created `DetailLevelToDoubleConverter`; bound slider to `Detail` property (OneWay)  |
| P2  | High     | MainWindow.xaml    | No loading feedback in Loaded state when model is downloading                                   | Replaced bare `ProgressRing` with `StackPanel` containing ring + `StatusText` label |
| P3  | High     | MainWindow.xaml.cs | Ctrl+V only worked in Ready state; pasting from Result/Reading did nothing                      | Removed state guard; `LoadText` now stops TTS and cancels distill on re-entry       |
| P4  | Medium   | output.html        | CSS highlight comma selector (`[data-theme="light"] .highlight, .highlight`) matched all themes | Separated into distinct dark/light rules                                            |
| P5  | Medium   | MainWindow.xaml.cs | No keyboard shortcut to dismiss settings popup                                                  | Added Escape key handler to close `SettingsPopup`                                   |
| P6  | Medium   | output.html        | No scrollbar on long summaries; content clipped                                                 | Added `overflow-y: auto` to body CSS                                                |
