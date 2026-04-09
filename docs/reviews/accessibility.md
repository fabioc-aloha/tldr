# Accessibility (WCAG) Review

**Date:** April 9, 2026
**Scope:** Full UI audit against WCAG 2.1 AA, screen reader, and keyboard navigation
**Result:** 5 findings, all fixed

## Findings

| #   | Severity | File            | Finding                                                                                                             | Fix                                                                                                                                                                   |
| --- | -------- | --------------- | ------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| A1  | High     | MainWindow.xaml | Style radio buttons have no group-level accessible name; screen readers announce individual buttons without context | Added `AutomationProperties.Name` to each radio and `KeyboardNavigation.TabNavigation="Local"` + group label `AutomationProperties.Name="Summary style"` on container |
| A2  | High     | MainWindow.xaml | Tone and Theme radio buttons in settings popup lack accessible names                                                | Added `AutomationProperties.Name` to all 6 radios, `AutomationProperties.HeadingLevel` to section headers, Tab navigation on container                                |
| A3  | Medium   | output.html     | WebView2 HTML body has no ARIA role or label; screen readers have no context for the summary content region         | Added `role="document" aria-label="AI-generated summary"` to body, `role="article"` to content div                                                                    |
| A4  | Medium   | MainWindow.xaml | Settings popup container lacks keyboard Tab containment                                                             | Added `KeyboardNavigation.TabNavigation="Local"` to popup `StackPanel`                                                                                                |
| A5  | Low      | MainWindow.xaml | All interactive controls already had `AutomationProperties.Name`; no other missing names found                      | No fix needed (confirmed existing coverage)                                                                                                                           |

## Existing Coverage (No Issues)

- Distill, Copy, Re-distill, Read Aloud, Stop, Pause/Resume, Clear, Settings buttons all have `AutomationProperties.Name`
- ProgressRing elements have `AutomationProperties.Name`
- WebView2 has `AutomationProperties.Name="Summary output"`
- Detail slider has `AutomationProperties.Name="Detail level"`
