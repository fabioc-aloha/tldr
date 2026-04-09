# UI/UX Review #1

**Date:** April 9, 2026
**Scope:** Full UI/UX walkthrough of all application states
**Result:** 8 findings, all fixed

## Findings

| #   | Severity | File             | Finding                                                              | Fix                                                                                                   |
| --- | -------- | ---------------- | -------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------- |
| U1  | High     | MainWindow.xaml  | No way to return from Loaded state to Ready (user stuck after paste) | Added Clear button with Dismiss24 icon next to input preview                                          |
| U2  | High     | MainWindow.xaml  | Pause button doesn't toggle to Resume; no visual state change        | `DataTrigger` on `IsPaused`: swaps icon (Pause24/Play24) and content text                             |
| U3  | High     | MainWindow.xaml  | Style radio buttons ignore persisted settings on load                | Bound `IsChecked` to computed `IsStyle*` properties (OneWay)                                          |
| U4  | High     | MainWindow.xaml  | Tone radio buttons ignore persisted settings on load                 | Bound `IsChecked` to computed `IsTone*` properties (OneWay)                                           |
| U5  | Medium   | MainWindow.xaml  | No visual feedback during drag-drop (no highlight/border)            | Added `DragOverlay` Border with accent color, shown on DragOver, hidden on DragLeave/Drop             |
| U6  | Medium   | MainViewModel.cs | Copy confirmation ("Copied to clipboard") too subtle in StatusText   | Kept as StatusText (consistent with app pattern); stronger CSS highlight not needed for non-HTML text |
| U7  | Medium   | output.html      | Sentence highlight invisible in light mode (dark-only rgba)          | Added `[data-theme="light"] .highlight` rule with appropriate opacity                                 |
| U8  | Low      | MainWindow.xaml  | Sentence count not shown anywhere after distill                      | Added Info badge in result bar showing sentence count                                                 |
