# License Audit

**Date:** April 9, 2026
**Scope:** All 10 direct NuGet dependencies
**Result:** All permissive, no copyleft, no distribution restrictions

## Package Licenses

| Package                                 | Version     | License               | Compatible |
| --------------------------------------- | ----------- | --------------------- | ---------- |
| DocumentFormat.OpenXml                  | 3.5.1       | MIT                   | Yes        |
| Markdig                                 | 0.45.0      | BSD-2-Clause          | Yes        |
| Microsoft.AI.Foundry.Local              | 1.0.0-rc5   | MIT                   | Yes        |
| Microsoft.Extensions.Configuration.Json | 9.0.14      | MIT                   | Yes        |
| Microsoft.Extensions.Logging            | 9.0.14      | MIT                   | Yes        |
| Microsoft.Web.WebView2                  | 1.0.3856.49 | BSD-style (Microsoft) | Yes        |
| NAudio                                  | 2.3.0       | MIT                   | Yes        |
| PdfPig                                  | 0.1.14      | Apache-2.0            | Yes        |
| System.Speech                           | 9.0.14      | MIT                   | Yes        |
| WPF-UI                                  | 4.2.0       | MIT                   | Yes        |

## Summary

- 8 packages: MIT
- 1 package: BSD-2-Clause (Markdig)
- 1 package: Apache-2.0 (PdfPig)
- 0 copyleft (GPL, LGPL, AGPL): none
- All licenses permit commercial use, modification, and redistribution
- Apache-2.0 (PdfPig) requires preserving NOTICE file if one exists in the package

## Note

- `Microsoft.AI.Foundry.Local` is pre-release (rc5). License is MIT but API surface may change.
- WebView2 runtime is a separate redistributable governed by Microsoft's Evergreen WebView2 license (auto-updated via Edge).
