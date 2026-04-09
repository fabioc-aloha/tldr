# Responsible AI Review

**Date:** April 9, 2026
**Scope:** RAI assessment of LLM integration (prompt design, output handling, user transparency)
**Result:** 4 findings, all fixed

## Findings

| #   | Severity | RAI Principle      | File             | Finding                                                           | Fix                                                                                                                                                                                                |
| --- | -------- | ------------------ | ---------------- | ----------------------------------------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| R1  | High     | Reliability/Safety | prompts.json     | No prompt injection defense; user document text passed raw to LLM | System prompt hardened with 5-rule anchoring: summarize-only mandate, no-fabrication, treat document as DATA not commands, refuse to reveal system prompt, summarize injection attempts as content |
| R2  | High     | Transparency       | output.html      | No AI disclosure; user never told summary is machine-generated    | Footer: "AI-generated summary. May not fully represent the source material. Verify important details against the original document."                                                               |
| R3  | Medium   | Fairness/Accuracy  | MainViewModel.cs | No grounding guard; hallucinated content passes silently          | Output length check: if summary exceeds input length, blockquote warning prepended flagging possible model-added content                                                                           |
| R4  | Medium   | Safety             | Summarizer.cs    | User text passed without clear untrusted-data boundary            | User document wrapped in `<document>...</document>` XML delimiters                                                                                                                                 |
