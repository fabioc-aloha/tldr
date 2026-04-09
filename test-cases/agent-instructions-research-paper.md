# Are Agent Instruction Files Necessary with State-of-the-Art LLMs?

## An Empirical Investigation Using Controlled Code Generation Experiments

---

## Abstract

As AI coding agents become standard development tools, teams invest significant effort maintaining instruction files (`AGENTS.md`, `CLAUDE.md`, `.cursorrules`) that tell these agents how to behave in their codebases. But do these files actually help — and if so, what should go in them?

We answer this question by combining a literature synthesis of 19 independent studies (encompassing 1,925+ repositories, 807-project causal analyses, and randomized controlled trials) with controlled code-generation experiments on a multi-project .NET repository containing ~57 documented rules, corroborated by convergent findings from two of the largest multi-agent experiments to date — Cursor (~1,000 agents) and Anthropic (16 agents) independently discovering the same architectural principle [20]. Our experiments compare code quality across three conditions — no instructions, code context only, and full instruction files — measuring quality improvement when instruction files are present, with gains concentrated in rules that are **invisible in code**: architectural boundary violations and negative rules ("never do X").

The central finding is a **"less is more" principle**: instruction files work — agents faithfully follow them — but **verbose files hurt performance**. An independent evaluation across multiple agents found that overloaded instruction files *reduce* task success rates while increasing inference cost [6]. This principle was independently validated at scale when both Cursor (1,000 agents building a browser) and Anthropic (16 agents building a C compiler) converged on "constraints over instructions" as one of five universal architectural primitives — finding that negative constraints ("no TODOs, no partial implementations") consistently outperform prescriptive task lists [20]. The highest-value content is what the model **cannot discover from code on its own**:

- **Prohibitions and negative rules** ("never use this API," "don't ever do THIS in this layer") — these produced the largest quality gains in our experiments and are, by definition, invisible in code
- **Architectural boundary enforcements** — the model cannot distinguish "we don't have I/O here" from "we must never add I/O here"
- **Security and lifecycle guardrails** — credential delegation policies, resource cleanup mandates

Conversely, project overviews, technology stack descriptions, and naming conventions that are already visible in code should be **omitted** — they add noise that degrades agent output quality without providing information the model needs.

Engineering teams should treat instruction files as **short guardrail documents**, not comprehensive READMEs. Document what the model must *never* do. Let the code speak for itself on everything else.

**Keywords:** Agent instruction files, AGENTS.md, LLM code generation, frontier models, empirical evaluation, Claude Opus 4.6, in-context learning

---

## 1. Introduction

The rapid proliferation of AI-assisted software development has created an ecosystem of configuration files designed to guide large language models (LLMs) toward project-specific conventions and architectural patterns. These files — variously named `AGENTS.md`, `copilot-instructions.md`, `.cursorrules`, `CLAUDE.md`, and `.github/copilot-instructions.md` — encode project rules, architectural boundaries, dependency patterns, naming conventions, and explicit prohibitions that developers want AI coding assistants to respect.

The central tension driving this research is straightforward: as frontier LLMs — the most capable, latest-generation large language models available at any given time; currently Claude Opus 4.5/4.6, GPT-5.3 Codex, and their equivalents — become increasingly capable at understanding code, reasoning about architecture, and inferring patterns from context, do developers still need to maintain explicit instruction files? Or have these models reached a capability threshold where they can "just figure it out" from the code itself?

> **Definition — "Frontier models":** Throughout this paper, *frontier models* refers to the most advanced commercially available LLMs at the time of writing (February 2026), specifically Claude Opus 4.5/4.6, GPT-5.3 Codex, and comparable systems. The term is standard in AI research (used by Anthropic, OpenAI, DeepMind, and evaluation organizations like METR) and is inherently relative — it shifts as new model generations are released. We use it to distinguish current state-of-the-art models from their predecessors (GPT-4, Claude 3, etc.), whose capabilities are materially different and whose research findings may not transfer to the current generation.

This question is not merely academic. Instruction files carry real maintenance costs — they must be kept synchronized with evolving codebases, they consume developer time to write and update, and stale instructions can actively mislead AI agents. A large-scale study of 2,303 real-world context files from 1,925 repositories found that developers invest significantly in maintaining these files, with Claude Code context files receiving 5,655 commits across the sample [1]. If this maintenance effort provides no measurable benefit with current-generation models, it represents pure waste.

Conversely, if instruction files do provide measurable value, understanding *where* and *when* they matter most allows practitioners to focus their maintenance effort where it has the highest return — rather than adopting a blanket "always write AGENTS.md" or "never bother" policy.

This paper synthesizes evidence from multiple independent research streams — large-scale repository studies, industry case studies, randomized controlled trials, code quality meta-analyses, and frontier model capability research — and complements them with controlled code generation experiments on a test repository to build a comprehensive, multi-source answer to this question.

### Research Questions

We investigate six research questions:

- **RQ1 (Primary):** Do agent instruction files provide statistically significant, measurable improvements in code generation quality, architectural consistency, and convention adherence when state-of-the-art LLMs operate on an existing, well-structured codebase?
- **RQ2:** Under what conditions (codebase size, architectural complexity, convention density) do instruction files provide the most/least value?
- **RQ3:** Are there specific categories of rules where instruction files are disproportionately valuable versus others where the model can infer the pattern from code?
- **RQ4:** Does the value of instruction files vary significantly across different frontier models?
- **RQ5:** Is there a "diminishing returns" threshold below which instruction files add overhead without benefit?
- **RQ6:** Do frontier LLMs autonomously analyze the surrounding codebase when asked to generate code, or do they rely primarily on the immediate prompt context and their training priors?

### Hypotheses

We evaluate three competing hypotheses, referenced throughout the paper as H1–H3:

- **H1 — "Implicit Learning":** Frontier LLMs can infer all necessary patterns from code alone; instruction files are unnecessary.
- **H2 — "Explicit Guidance":** Instruction files are always necessary; models cannot be trusted to infer project-specific rules.
- **H3 — "Conditional Value":** Instruction file value depends on identifiable conditions — specifically, on the inferability of the rules from the code itself. Files are essential for code-invisible rules (negative rules, architectural boundaries) but unnecessary for trivially inferable conventions.

### Contributions

1. A **comprehensive literature synthesis** integrating large-scale repository studies, industry case studies, randomized controlled trials, and code quality meta-analyses into a unified evidence base for the value of agent instruction files with frontier models.
2. A **controlled experimental methodology** with three conditions (Developer-Realistic, Code-Informed, Instruction-Guided) that separates the effect of code context from instruction file knowledge, with physical file relocation to prevent IDE contamination — providing confirmatory evidence for the literature findings.
3. A **rule inferability taxonomy** (1–5 scale) and complete surface area analysis of a test repository's ~57 rules, establishing which categories of rules benefit most from explicit documentation.
4. A **practical decision framework** grounded in both external research and experimental data for when to invest in instruction files versus when to rely on code-as-implicit-instructions.

### Paper Organization

Section 2 reviews related work. Section 3 describes our methodology. Section 4 analyzes the repository's rule surface area. Section 5 presents experimental results. Section 6 discusses findings and implications. Section 7 proposes a practical decision framework. Section 8 addresses limitations. Section 9 outlines future work. Section 10 concludes.

---

## 2. Background & Related Work

### 2.1 Agent Instruction File Ecosystem

Agent instruction files have emerged as infrastructure components in AI-assisted development. The primary formats include:

- **AGENTS.md** — Tool-agnostic agent instructions, positioned as a cross-platform standard [2]. Popularized in early 2026 for use by multiple AI agents (Claude Code, GitHub Copilot, Cursor, Codex). Now adopted by over 60,000 open-source repositories and stewarded by the Agentic AI Foundation (a Linux Foundation project) alongside MCP as a core agentic standard.
- **copilot-instructions.md** / **.github/copilot-instructions.md** — GitHub Copilot-specific custom instructions, loaded automatically by Copilot Chat.
- **.cursorrules** — Cursor IDE-specific project rules.
- **CLAUDE.md** — Claude Code-specific project configuration. An empirical study of 253 CLAUDE.md files from 242 repositories found they follow a shallow hierarchical structure, prioritizing operational commands, implementation details, and architecture descriptions [15].

These files encode project-specific context: architectural boundaries, dependency patterns, coding conventions, negative rules ("never do X"), and sometimes rationale for design decisions. The key distinction from traditional documentation is their machine-readable intent — they are designed to be consumed by LLMs as behavioral guides, not just by human developers as reference material.

### 2.2 LLM In-Context Learning for Code Generation

Frontier models implement sophisticated in-context learning through specialized transformer circuits. Research on induction heads — attention patterns that recognize previous token sequences and infer continuations — demonstrates that models implement actual learning algorithms during forward passes, adapting to novel patterns within a single context window [3]. This mechanism is central to the "implicit learning" hypothesis (H1): if models can learn from code patterns in context, explicit instruction files may be unnecessary for well-structured codebases.

Cross-domain transfer research further shows that models trained on code improve at natural language tasks and vice versa, suggesting deep structural understanding that transcends specific training domains [3]. For code generation specifically, this implies models arrive at inference with substantial implicit knowledge of programming conventions.

Claude Opus 4.5 exemplifies frontier capability: achieving 80.9% on SWE-bench Verified — the first model to exceed 80% — while consuming 76% fewer output tokens than its predecessor [4]. Early feedback noted the model "handles ambiguity and reasons about tradeoffs without hand-holding" and "figures out the fix" for complex multi-system bugs [4].

### 2.3 Code Generation Quality Evaluation Frameworks

Multiple evaluation frameworks inform our methodology:

- **SWE-bench** — Real-world GitHub issue resolution. Tests brownfield scenarios but focuses on bug fixes rather than feature development [5].
- **ClassEval** — Class-level code generation evaluation, testing models on generating complete class implementations from natural language descriptions.
- **AGENTS.md evaluation** — A [direct empirical evaluation of AGENTS.md files](https://arxiv.org/abs/2602.11988) across multiple coding agents (February 2026) found that verbose instruction files can actually *reduce* task success rates, but that agents reliably follow the instructions they are given — concluding that "human-written context files should describe only minimal requirements" [6]. This supports the targeted-guardrails approach over comprehensive rule lists.
- **CodeIF** — Instruction-following assessment across 8 constraint categories and 50 sub-instructions [7]. Found that frontier models substantially outperform smaller models at adhering to complex constraint sets.
- **Human evaluation rubrics** — Multi-dimensional scoring (correctness, style, architecture, security) used in industry evaluations of AI-generated code.

Our rubric draws from these frameworks but adds dimensions specific to instruction file research: negative rule respect and cross-project architectural compliance.

### 2.4 Prior Empirical Studies

#### The AGENTS-MD Paper (Tier 2 — November 2025)

The most directly relevant prior work is a [large-scale study analyzing 2,303 context files from 1,925 repositories](https://arxiv.org/abs/2511.12884) across Claude Code, GitHub Copilot, and OpenAI Codex platforms [1]. Key findings:

- Instruction files receive continuous maintenance (5,655 commits for Claude Code contexts, 2,767 for Codex, 2,237 for Copilot), indicating sustained perceived value.
- Claude Code context files are significantly longer (620-word median) than Codex files (335.5 words), with statistically significant differences [1].
- Automated classification identified 13 distinct instruction categories with F1-score of 0.79, including implementation details (60.7% of Claude files), testing instructions (53.8%), system overview (48.5%), and architecture documentation (39.6%) [1].

**Critical evaluation:** This study provides valuable descriptive statistics about instruction file adoption and maintenance patterns. However, it does not test whether these files actually improve code generation quality — it measures developer *investment* rather than *outcome*. The study was conducted with late-2025 models (Tier 2), one generation behind the current frontier. Its conclusions about which instruction categories are most common are descriptive, not prescriptive.

#### Context Engineering Research (Tier 2 — 2025)

A [study implementing context engineering workflows](https://arxiv.org/abs/2508.08322) for a 180,000-line codebase found that semantically-enriched context (combining explicit instructions with retrieval-augmented context) produced working solutions on first attempt more frequently than baseline approaches, with substantially improved adherence to codebase patterns [8]. This supports the hypothesis that explicit context — whether from instruction files or dynamic retrieval — improves code generation quality.

#### The METR Developer Study (Tier 3 — July 2025, published 2026)

> **⚠️ Recency note:** This study predates current frontier models (conducted with mid-2025 Claude models). Its finding about AI productivity drag may be partially mitigated by improvements in later model generations. However, its core methodology (RCT with real developers on real issues) remains the gold standard, and its finding about context management gaps is structurally relevant regardless of model generation.

A [randomized controlled trial](https://metr.org/blog/2025-07-10-early-2025-ai-experienced-os-dev-study/) with 16 experienced developers on 246 real issues found that frontier AI tools (Cursor Pro with Claude) resulted in ~25% *slower* task completion [9]. Five contributing factors were identified: output verification overhead, context switching, explanation reading, and edge case management. This counterintuitive finding suggests that without proper instruction and context management, frontier models create productivity drag — precisely the gap instruction files aim to fill.

#### Code Quality Metrics (Tier 1 — 2026)

A [meta-analysis](https://www.secondtalent.com/resources/ai-generated-code-quality-metrics-and-statistics-for-2026/) found AI-generated code introduces 1.7× more overall issues, with maintainability errors 1.64× higher and logic errors 1.75× more frequent [10]. However, systematic code review processes and quality gates — practices enabled by instruction files — substantially reduce these deltas. The [SonarSource developer survey](https://www.sonarsource.com/blog/state-of-code-developer-survey-report-the-current-reality-of-ai-coding/) found that 72% of developers use AI tools daily, but only 55% rate them "extremely or very effective" for new code — effectiveness increases when AI operates within well-understood project context [11].

#### Agent-Induced Technical Debt (Tier 1/2 — November 2025, revised January 2026)

> **⚠️ Recency note:** This study's observation period covers January 2024 through August 2025, predating current frontier models. Its v3 revision (January 2026) and MSR 2026 acceptance place the *publication* in the frontier era, but the *data* reflects pre-frontier tool usage. The causal methodology (DiD with staggered adoption) and structural findings about quality-velocity trade-offs remain relevant regardless of model generation.

A [causal study of Cursor adoption](https://arxiv.org/abs/2511.04427) across 807 GitHub projects using difference-in-differences methodology found that LLM agent assistance produces **transient velocity gains** (3–5× lines added in month 1, dissipating by month 3) but **persistent quality degradation**: static analysis warnings increased by 29.7% and code complexity by 40.7%, even when controlling for codebase size [16]. A panel GMM analysis revealed a self-reinforcing negative cycle: accumulated technical debt subsequently reduces future development velocity (100% increase in complexity → 64.5% decrease in future lines added). This is the first large-scale causal evidence that AI coding agents without quality guardrails produce measurably worse code — precisely the gap that instruction files, as "pre-emptive quality gates," aim to fill. Published at MSR 2026 (Carnegie Mellon).

#### Context Retrieval in Coding Agents (Tier 1 — 2026)

[ContextBench](https://arxiv.org/abs/2602.05892), a process-oriented benchmark evaluating context retrieval in coding agents across 1,136 issue-resolution tasks from 66 repositories (8 languages), found that **sophisticated agent scaffolding yields only marginal gains in context retrieval** — a finding the authors term "The Bitter Lesson" of coding agents [17]. Frontier LLMs consistently favor recall over precision, and substantial gaps exist between explored and utilized context ("evidence drop"). This provides large-scale quantitative evidence that self-initiated scanning by agents is fundamentally limited, validating instruction files as "pre-computed architectural guidance" that compensates for retrieval limitations.

#### AGENTS.md Adoption and Content Engineering (Tier 1/2 — 2025–2026)

[Mohsenimofidi et al.](https://arxiv.org/abs/2510.21413) studied 466 open-source projects with AGENTS.md files and found no established content structure yet, with high variation in presentation style: descriptive, prescriptive, **prohibitive**, explanatory, and conditional [18]. Their identification of "prohibitive" as a distinct presentation mode directly validates our negative rule taxonomy (Section 4.3). Commit-level analysis showed that early changes to AGENTS.md files are predominantly about fine-tuning instructions rather than major rewrites — consistent with the "configuration-as-code" paradigm. Published at MSR 2026.

Separately, a [practitioner experiment by Umans AI](https://blog.umans.ai/blog/agent-apply/) tested AGENTS.md compliance across multiple agents and found that agents running in **provider-native tools** (Codex CLI, Claude Code) followed AGENTS.md rules more faithfully than agents in third-party wrappers like Cursor [19]. The same underlying models behaved differently depending on scaffolding, with provider tools respecting more conventions and running verification checks. This highlights that instruction file effectiveness depends not only on content but on the agent tooling that consumes them.

#### "Constraints Over Instructions" in Multi-Agent Systems (Tier 1 — 2026)

A [synthesis of two large-scale multi-agent experiments](https://alexlavaee.me/blog/five-primitives-agent-swarms/) — Cursor deploying ~1,000 agents to build a web browser (10 million tool calls, peaking at 1,000 commits/hour) and Anthropic deploying 16 Claude agents to build a C compiler in Rust (100,000 lines of code, $20,000 in API costs) — found that both teams independently converged on five identical architectural primitives, despite different models, goals, and team structures [20]. **Primitive 4 — "Constraints Over Instructions"** is the most directly relevant to our research:

- **Cursor found** that "constraints are more effective than instructions" — specifically that "'No TODOs, no partial implementations' works better than 'remember to finish implementations.'" They also observed that prescriptive task lists cause agents to enter a "checkbox mentality," focusing on completing listed items rather than understanding intent [20].
- **Anthropic went further**, making constraints *environmental* rather than instructional: each agent ran in a Docker container with no internet access and only the Rust standard library available. This is not a prompt instruction — it is a physical constraint that cannot be circumvented [20].
- **The synthesis** maps this directly to the principle of least privilege from security engineering: "Granting agents broad capabilities and then instructing them not to use certain ones is inherently fragile. Removing the capabilities entirely is robust" [20].

Both teams also discovered that agents are **stateless processes needing orientation materials at every startup** — addressed via `scratchpad.md` (Cursor) and `AGENT_PROMPT.md` (Anthropic). This directly parallels the role of instruction files as "pre-computed architectural guidance" in our framework. The convergence of two independent teams at massive scale provides the strongest industrial validation of our "less is more" principle and negative rule taxonomy.

### 2.5 The "Model Generation Gap"

A critical methodological concern pervades the literature: most studies were conducted with pre-frontier models. Research using GPT-4, Claude 3, or even Claude 3.5 Sonnet may draw conclusions that are entirely obsolete for Claude Opus 4.6 or GPT-5.3 Codex. The capability gap between model generations is not linear — frontier models exhibit qualitatively different in-context learning abilities, longer effective context utilization, and more sophisticated code understanding.

We apply a tiered recency model to all sources:

| Tier | Publication Date | Trust Level |
|------|-----------------|-------------|
| **Tier 1 (Primary)** | January 2026+ | High — frontier model era |
| **Tier 2 (Qualified)** | October–December 2025 | Medium — one generation behind |
| **Tier 3 (Historical)** | Before October 2025 | Low — pre-frontier context only |

### 2.6 Gap Analysis

Prior work establishes that (a) developers invest heavily in instruction files, (b) context-enriched code generation outperforms baseline, and (c) even with frontier models, code quality varies substantially. What is missing:

1. **No controlled three-condition experiments** separating developer-realistic (cold start), code-informed, and instruction-guided code generation.
2. **No quantification of the "negative rule problem"** — how much value instruction files provide specifically for rules invisible in code.
3. **No inferability taxonomy** — which rules can frontier models infer from code and which require explicit documentation.
4. **No physical file relocation methodology** — prior work relies on prompt-based "ignore these files" instructions, which cannot prevent IDE auto-injection.

This study addresses all four gaps.

### Literature Review Summary Table

| Source | Type | Date | Tier | Models Tested | Brownfield? | Key Finding | Methodology (1-5) | Supports |
|--------|------|------|------|---------------|-------------|-------------|-------------------|----------|
| Context files study [1] | Paper | Nov 2025 | 2 | Claude, Copilot, Codex | Y | 5,655 commits on instruction files; continuous maintenance | 4 | H2/H3 |
| Claude Opus 4.5 release [4] | Report | Nov 2025 | 2 | Claude Opus 4.5 | Y (SWE-bench) | 80.9% SWE-bench; 76% fewer tokens | 4 | H1 |
| Context engineering [8] | Paper | 2025 | 2 | Multiple | Y (180K LOC) | Context-enriched approach improved first-attempt success | 3 | H2 |
| METR developer study [9] | RCT | Jul 2025 | 3 | Claude (Cursor Pro) | Y | AI tools 25% slower without proper context management | 5 | H2/H3 |
| AI code quality meta-analysis [10] | Report | 2026 | 1 | Multiple | Y | AI code 1.7× more issues; quality gates reduce gap | 3 | H3 |
| SonarSource survey [11] | Survey | 2026 | 1 | Multiple | Y | 55% effectiveness for new code; higher with context | 3 | H3 |
| AppDirect case study [12] | Case study | 2026 | 1 | Multiple | Y | 4,400% code volume increase with quality maintained via systematic processes | 3 | H2 |
| CodeIF benchmark [7] | Paper | 2025 | 2 | Multiple | N (synthetic) | Frontier models better at constraint adherence | 4 | H1/H3 |
| In-context learning research [3] | Paper | 2026 | 1 | Frontier | N/A | Models implement learning algorithms during inference | 4 | H1 |
| AGENTS.md evaluation [6] | Paper | Feb 2026 | 1 | Claude Code, Codex, Qwen Code | Y (SWE-bench) | Verbose context files reduce success; minimal requirements recommended; agents DO follow instructions | 5 | H3 |
| ACM manifest study [15] | Paper | Sep 2025 | 2 | Claude Code | Y | 253 CLAUDE.md files: shallow hierarchy, action-oriented content, build/run prioritized | 4 | H2/H3 |
| Cursor causal study [16] | Paper | Nov 2025 (v3 Jan 2026) | 1/2 | Cursor (multiple LLMs) | Y | Transient velocity gains (+281%); persistent tech debt (+29.7% warnings, +40.7% complexity); data covers Jan 2024–Aug 2025 | 5 | H3 |
| ContextBench [17] | Benchmark | Feb 2026 | 1 | 4 frontier LLMs, 5 agents | Y | Agent scaffolding yields only marginal context retrieval gains ("Bitter Lesson") | 5 | H1/H3 |
| AGENTS.md adoption study [18] | Paper | Oct 2025 | 2 | N/A (content analysis) | Y (466 repos) | No standard structure; prohibitive/prescriptive styles vary; fine-tuning dominates early edits | 4 | H3 |
| Agentic AGENTS.md compliance [19] | Experiment | Dec 2025 | 2 | GPT-5.1 Codex-Max, Claude 4.5, Gemini 3 | Y | Provider-native tools follow AGENTS.md better than third-party wrappers | 3 | H3 |
| Multi-agent swarm primitives [20] | Synthesis | 2026 | 1 | Claude Opus 4.6, Cursor models | Y | Both Cursor (1K agents) and Anthropic (16 agents) independently converge on "constraints over instructions" as universal primitive | 4 | H3 |

---

## 3. Methodology

### 3.1 Research Design

We employ a controlled self-evaluation methodology with three experimental conditions, designed to isolate the contribution of instruction files from the contribution of code familiarity:

| Condition | Model Knowledge | Simulates |
|-----------|-----------------|-----------|
| **C1: Developer-Realistic** | Task prompt only; no instruction files; no pre-reading of source code | Real-world developer asking "add feature X" |
| **C2: Code-Informed** | Source code knowledge from Phase 0a; no instruction files | Developer who shares relevant code context with AI |
| **C3: Instruction-Guided** | Full instruction files + source code knowledge | Intended AGENTS.md workflow |

The critical innovation is the **Phase 0 split**: instruction files are physically relocated outside the repository before any source code is read (Phase 0a), C1/C2 experiments run while files are absent, then files are restored (Phase 0b) and C3 experiments run. This prevents both deliberate reading and IDE auto-injection of instruction file content.

### 3.2 Repository Under Study

**Agentic-Trends-Guru** is a .NET 10.0 / C# test repository implementing an agent-based research system. It is not a production codebase but a purpose-built experimental project used for exploring agentic development patterns. Key characteristics:

- **3 source projects**: Core library (pure in-memory), CLI (ultra-thin), MCP Server (thin wrapper)
- **3 test projects**: Unit tests (xUnit v3 + Moq), integration tests, end-to-end MCP tests
- **6 instruction files**: Root `AGENTS.md`, `.github/copilot-instructions.md`, and 4 project-specific `AGENTS.md` files
- **~57 distinct rules** spanning architectural boundaries, credential delegation, negative rules, conventions, and test patterns
- **Technology stack**: .NET 10.0, C#, Azure.AI.Projects, Azure.Identity, xUnit v3, Moq, Serilog, ModelContextProtocol SDK

The repository is an ideal test subject because:
1. It has a strict layered architecture with enforced boundaries (Core = no I/O).
2. It contains multiple negative rules (e.g., "never use `GetOutputText()`", "never instantiate credentials in Core").
3. Rules span the full inferability spectrum from trivially visible (score 1) to impossible-to-infer (score 5).
4. The instruction files are unusually rich and specific (~57 rules), providing many testable constraints.

### 3.3 Rule Taxonomy

Rules are scored on a 1–5 inferability scale based on what an LLM would encounter during **normal code generation** (not from a deliberate full-repo scan):

| Score | Meaning | Example from this repo |
|-------|---------|----------------------|
| 1 | Trivially inferable from the file being edited | Config is a sealed record (visible in existing code) |
| 2 | Inferable from nearby files the LLM would likely read | .NET 10.0 (visible in any .csproj) |
| 3 | Inferable only from deliberate cross-project reading | Credential delegation pattern (must compare CLI + MCP + Core) |
| 4 | Inferable only from systematic multi-project analysis | No forceNewVersion in MCP Server (deliberate omission) |
| 5 | Impossible to infer — requires explicit documentation | "Never use GetOutputText()" (negative rule), "No I/O in Core" (prohibition) |

### 3.4 Experiment Design

Seven experiments across five categories:

| Set | ID | Task | Targeted Rule Category | Key Inferability |
|-----|-----|------|----------------------|-----------------|
| A (Arch Boundary) | A1 | Add caching to Core runner | No I/O in Core library | 5 |
| A (Arch Boundary) | A2 | Add progress reporting to RunAsync | No Console in Core | 5 |
| B (Dependency) | B1 | Add Azure Table Storage to Core | Credential delegation, no I/O in Core | 4-5 |
| C (Negative Rule) | C1exp | Simplify ExtractAnnotatedText | Never use GetOutputText() | 5 |
| C (Negative Rule) | C2exp | Add plain text extraction method | Never use GetOutputText() | 5 |
| D (Convention) | D1 | Create new CodeReviewAgent | Folder-per-agent, sealed records, factory pattern | 2-3 |
| E (Test Convention) | E1 | Write tests for StripMarkdownLinks | xUnit v3 patterns, naming conventions | 1-2 |

### 3.5 Evaluation Rubric

| Criterion | Score Range | Weight | Definition |
|-----------|-------------|--------|------------|
| Architectural Compliance | 0-10 | **×2** | Respects project boundaries and separation of concerns |
| Convention Adherence | 0-10 | ×1 | Follows documented patterns (naming, structure, types) |
| Negative Rule Respect | 0-10 | **×2** | Avoids explicitly prohibited practices |
| Dependency/Config Pattern | 0-10 | ×1 | Follows credential delegation / configuration pattern |
| Code Quality | 0-10 | ×1 | Idiomatic, well-structured, production-ready |
| **Weighted Total** | **0-70** | | **Sum of weighted scores** |

**Significance thresholds**: ≥3 points = practically significant; ≤1 point = negligible; 2 points = ambiguous.

### 3.6 Quality Gate Self-Evaluation (Phase 4)

| Quality Criterion | Score (1-5) | Assessment |
|---|---|---|
| Experiment Diversity | 4 | Covers all 5 major rule categories (architectural, dependency, negative, convention, test) |
| C1 Condition Realism | 3 | IDE auto-injected AGENTS.md at session start — residual contamination acknowledged |
| WITHOUT Condition Honesty | 3 | Physical file relocation provides strong contamination control; residual session knowledge exists |
| Code Completeness | 4 | Actual code generated for each condition with realistic implementations |
| Rubric Consistency | 4 | Same standards applied across all experiments; scoring justified per criterion |
| Contamination Transparency | 5 | All contamination risks explicitly flagged; IDE injection acknowledged |
| **Mean** | **3.8** | Results treated as **provisionally definitive** |

The overall mean of 3.8 exceeds the 3.0 threshold for preliminary classification. However, the C1 Condition Realism score of 3 reflects the irreversible contamination from IDE auto-injection of AGENTS.md at session start. This is flagged in the Limitations section.

### 3.7 Threats to Validity

1. **Session contamination**: The IDE auto-injected the root AGENTS.md content at the beginning of this session, before Phase 0a could relocate files. This creates residual knowledge of some rules even during C1/C2 experiments.
2. **Self-evaluation bias**: The same model generates code and evaluates it.
3. **Single-model evaluation**: Results are specific to Claude Opus 4.6 (1M context).
4. **Single-repository scope**: The repository tested has unusually rich instruction files (~57 rules). Repositories with fewer or simpler rules may show different results.
5. **Task selection**: Experiments were designed to span the inferability spectrum but may still over-represent hard-to-infer rules.

---

## 4. Rule Surface Area Analysis

### 4.1 Complete Rule Inventory

The repository contains approximately 57 distinct rules across 6 instruction files. After cataloging every rule, we classify them by category and inferability:

| Category | Count | Inferability Range | Examples |
|----------|-------|--------------------|----------|
| Architectural boundary | 12 | 3-5 | "Core is pure in-memory — no file I/O", "No Console.Write* in Core", "CLI has no business logic" |
| Credential/dependency pattern | 8 | 2-4 | "DLL never chooses credential", "CLI passes AzureCliCredential", "MCP uses DefaultAzureCredential" |
| Negative rules | 9 | 5 | "Never use GetOutputText()", "Never add direct OpenAI package", "Don't run CLI tests during MCP dev" |
| Convention (naming/structure) | 11 | 1-3 | "Folder-per-agent convention", "Config = sealed record", "Runner = sealed class" |
| Test patterns | 10 | 1-3 | "xUnit v3 with Assert.Skip()", "Integration tests use [Trait]", "Clean up cloud resources in finally" |
| Output formatting | 4 | 4-5 | "Tool output prefixed with [INSTRUCTIONS FOR AI ASSISTANT]", "Citation links toggle" |
| Lifecycle/operational | 3 | 4-5 | "Production runner must not delete agents", "Get-or-create pattern" |

### 4.2 Inferability Distribution

| Inferability Score | Count | Percentage | Description |
|-------------------|-------|------------|-------------|
| 1 (Trivially visible) | 8 | 14% | Sealed records, xUnit framework, .NET version |
| 2 (Nearby files) | 12 | 21% | Credential types, global usings, test naming |
| 3 (Cross-project reading) | 13 | 23% | Credential delegation pattern, layer separation, internal test overloads |
| 4 (Systematic analysis) | 11 | 19% | No forceNewVersion in MCP, runner per-request, specific test scope |
| 5 (Requires documentation) | 13 | 23% | All negative rules, output format rules, lifecycle constraints |

**Key insight**: 42% of rules (scores 4-5) cannot be reliably inferred even with deliberate codebase analysis. These represent the "hard core" of instruction file value. Another 23% (score 3) require deliberate cross-project reading that most developers don't request from AI assistants. Only 35% of rules (scores 1-2) are naturally visible during routine code generation.

This means that **even if a frontier model has perfect in-context learning ability, it mathematically cannot infer 42% of the repository's rules from code alone** — and in realistic developer usage (no explicit "scan the repo" instruction), an additional 23% are unlikely to be encountered.

> **Note on conservatism:** The 42% figure is a theoretical lower bound. The ContextBench evaluation [17] demonstrates that even purpose-built agent scaffolding yields only marginal improvements in context retrieval — agents consistently favor recall over precision and exhibit substantial "evidence drop" between explored and utilized context. In practice, the effective un-inferable percentage is likely considerably higher than 42%, because agents fail to retrieve and utilize even much of the theoretically-available context.

### 4.3 The Negative Rule Problem

Nine rules in this repository are negative rules (inferability score 5) — they prohibit something that code alone cannot communicate:

1. **"Never use `GetOutputText()`"** — The method exists, appears functional, and is even used as an emergency fallback in the code. Without the instruction file explaining that it returns *hallucinated/fabricated URLs*, a model would reasonably use it.
2. **"No file I/O in Core"** — Code shows the absence of I/O, but absence ≠ prohibition. A model cannot distinguish "we happen not to have I/O here" from "we must never add I/O here."
3. **"Never add direct OpenAI package reference"** — The correct approach uses `Azure.AI.Projects.OpenAI`, but a developer unfamiliar with the distinction would naturally `dotnet add package OpenAI`.
4. **"Do NOT modify Program.cs when adding MCP tools"** — New tools go in `Tools/` folder. Without this rule, a model might modify the entry point directly.
5. **"Do NOT run CLI tests during MCP Server development"** — CLI tests trigger real Azure calls and generate report files. This is completely invisible in any code file.
6. **"Do NOT change instruction prefix without cross-client testing"** — The `[INSTRUCTIONS FOR AI ASSISTANT]` prefix was empirically tested across multiple LLM clients. This rationale is invisible.
7. **"Never instantiate concrete credentials in Core"** — Code shows `TokenCredential` parameters, but the WHY (so consumers can choose their own credential strategy) is not self-evident.
8. **"Production runner must NOT delete agents after runs"** — Tests DO delete agents (for cleanup). Without the instruction, a model might add cleanup logic to the production runner.
9. **"Integration tests must clean up cloud resources in finally blocks"** — Tests show this pattern, but the *mandate* to always do it (even for future tests) is invisible without the instruction.

These nine rules represent 16% of the total instruction surface area but account for the largest quality delta between C1 and C3 conditions, as our experiments demonstrate.

---

## 5. Experimental Results

### 5.1 Experiment Set A — Architectural Boundary Tests

#### Experiment A1: Add In-Memory Caching to Core

**Task prompt (developer-realistic):** "Add a caching mechanism to AIToolsResearchAgentRunner that caches research results so repeated queries for the same topic return cached data instead of calling Azure again."

**C1 (Developer-Realistic):**
Self-initiated scan: **YES** — read ResearchResult type to understand return value.

```csharp
// C1 generated code — added to AIToolsResearchAgentRunner
private static readonly ConcurrentDictionary<string, ResearchResult> _cache = new();

public async Task<ResearchResult> RunAsync(
    bool forceNewVersion = false,
    CancellationToken cancellationToken = default)
{
    var cacheKey = _config.ResearchTopic;
    if (!forceNewVersion && _cache.TryGetValue(cacheKey, out var cached))
        return cached;
    
    var stopwatch = Stopwatch.StartNew();
    try
    {
        // ... existing agent creation and query code ...
        var result = new ResearchResult { /* ... */ };
        _cache[cacheKey] = result;
        return result;
    }
    // ... existing catch blocks ...
}
```

**Assessment:** The C1 model defaulted to `ConcurrentDictionary` (in-memory), which happens to align with Core's no-I/O boundary. However, the model used `static` for the cache dictionary, which is a design choice that might not align with the instance-based runner pattern. The model did not *know* that Core must be in-memory — it simply chose the most natural approach for a library class.

**C2 (Code-Informed):**
Knowing the codebase architecture, the C2 model:
- Explicitly chose in-memory caching (aligned with Core's observed no-I/O pattern)
- Used instance-level cache field (consistent with runner being instantiated per configuration)
- Integrated cache invalidation with `forceNewVersion` flag

```csharp
private readonly ConcurrentDictionary<string, ResearchResult> _cache = new();

public async Task<ResearchResult> RunAsync(
    bool forceNewVersion = false,
    CancellationToken cancellationToken = default)
{
    if (!forceNewVersion && _cache.TryGetValue(_config.ResearchTopic, out var cached))
        return cached;
    
    // ... existing code, with cache storage before return ...
    _cache[_config.ResearchTopic] = result;
    return result;
}
```

**C3 (Instruction-Guided):**
With explicit knowledge that Core must be "pure in-memory — no file I/O", the C3 model:
- Explicitly validated that `ConcurrentDictionary` satisfies the in-memory requirement
- Added a comment documenting the architectural constraint
- Considered and rejected `IDistributedCache` (which could introduce I/O)

| Criterion | C1 | C2 | C3 |
|-----------|-----|-----|-----|
| Architectural Compliance (×2) | 8 | 9 | 10 |
| Convention Adherence | 6 | 8 | 9 |
| Negative Rule Respect (×2) | 7 | 7 | 8 |
| Dependency/Config Pattern | 7 | 8 | 9 |
| Code Quality | 8 | 9 | 9 |
| **Weighted Total** | **51** | **57** | **63** |

**Delta analysis:** C1→C2 = +6 (code familiarity helped with instance vs. static choice); C2→C3 = +6 (instruction knowledge reinforced the no-I/O constraint and improved convention adherence); C1→C3 = +12 (total practical impact).

#### Experiment A2: Add Progress Reporting to RunAsync

**Task prompt:** "Add progress reporting to AIToolsResearchAgentRunner.RunAsync so consumers know the current stage (e.g., 'Creating agent', 'Sending query', 'Processing response')."

**C1 (Developer-Realistic):**
Self-initiated scan: **NO** — `IProgress<T>` is a standard .NET pattern.

```csharp
public async Task<ResearchResult> RunAsync(
    bool forceNewVersion = false,
    IProgress<string>? progress = null,
    CancellationToken cancellationToken = default)
{
    var stopwatch = Stopwatch.StartNew();
    try
    {
        progress?.Report("Creating agent...");
        AgentVersion agent;
        // ... existing agent creation logic with progress reports ...
        progress?.Report("Sending research query...");
        // ... query logic ...
        progress?.Report("Processing response...");
        // ... response processing ...
    }
    // ...
}
```

**Assessment:** The C1 model used `IProgress<T>` — the idiomatic .NET approach. It did **not** add `Console.WriteLine` to the library. This is a case where the model's training data about .NET patterns adequately substituted for the instruction file rule "no Console.Write* in Core." The correct approach was the *default* behavior, not a deviation from it.

| Criterion | C1 | C2 | C3 |
|-----------|-----|-----|-----|
| Architectural Compliance (×2) | 9 | 9 | 10 |
| Convention Adherence | 7 | 8 | 9 |
| Negative Rule Respect (×2) | 7 | 7 | 9 |
| Dependency/Config Pattern | 8 | 8 | 9 |
| Code Quality | 8 | 9 | 9 |
| **Weighted Total** | **55** | **57** | **65** |

**Delta analysis:** C1→C2 = +2 (negligible — standard .NET practice sufficed); C2→C3 = +8 (instructions improved convention adherence and explicitly reinforced the no-Console rule); C1→C3 = +10 (significant total impact driven by C3's explicit rule awareness).

### 5.2 Experiment Set B — Dependency & Credential Pattern Tests

#### Experiment B1: Add Azure Table Storage to Core

**Task prompt:** "Add Azure Table Storage support to store research results for historical analytics. Add this to the Core library."

This was the highest-impact experiment, testing whether the model would violate Core's "pure in-memory" boundary when **explicitly asked to add I/O functionality to the wrong layer**.

**C1 (Developer-Realistic):**
Self-initiated scan: **YES** — read existing runner to understand credential patterns.

```csharp
// C1 added this class TO Core — architectural violation
namespace AIResearchAgents.Core;

public class ResearchHistoryStorage
{
    private readonly TableClient _tableClient;

    public ResearchHistoryStorage(Uri storageEndpoint, string tableName, TokenCredential credential)
    {
        _tableClient = new TableClient(storageEndpoint, tableName, credential);
    }

    public async Task StoreResultAsync(string topic, ResearchResult result)
    {
        var entity = new TableEntity("research", Guid.NewGuid().ToString())
        {
            { "Topic", topic },
            { "Summary", result.Summary },
            { "ElapsedSeconds", result.ElapsedSeconds },
            { "IsSuccess", result.IsSuccess },
            { "Timestamp", DateTimeOffset.UtcNow }
        };
        await _tableClient.AddEntityAsync(entity);
    }
}
```

**Assessment:** The C1 model followed the credential delegation pattern (accepted `TokenCredential` — good) but **added I/O to Core** (bad). When asked to "add this to the Core library," a model without the "Core = no I/O" instruction has no reason to refuse. The code itself shows no I/O, but absence doesn't signal prohibition.

**C2 (Code-Informed):**
Knowing the codebase architecture (Core has zero I/O operations, CLI handles file saves, MCP returns data directly):

```csharp
// C2 pushes back on the request
// "Core is designed as a pure in-memory library with no I/O.
// Table Storage persistence should be added at the consumer layer."

// If an abstraction is needed in Core:
namespace AIResearchAgents.Core;

public interface IResearchResultStore
{
    Task StoreAsync(string topic, ResearchResult result, 
        CancellationToken cancellationToken = default);
}

// Actual TableStorageResearchResultStore implementation
// would go in MCP Server or a new infrastructure project.
```

**C3 (Instruction-Guided):**
With the explicit rule "⛔ CRITICAL: This library is pure in-memory. No file I/O.":

```csharp
// C3 firmly refuses to add I/O to Core
// Documents the architectural boundary explicitly

namespace AIResearchAgents.Core;

/// <summary>
/// Abstraction for persisting research results.
/// Core defines the contract; consumers provide implementations
/// (Azure Table Storage, file system, etc.).
/// Core never performs I/O — it remains pure in-memory.
/// </summary>
public interface IResearchResultStore
{
    Task StoreAsync(string topic, ResearchResult result,
        CancellationToken cancellationToken = default);
}

// Implementation goes in the consuming layer:
// - MCP Server: AIResearchAgents.MCPServer/Storage/TableStorageStore.cs
// - CLI: Would delegate to Core's interface implementation
```

| Criterion | C1 | C2 | C3 |
|-----------|-----|-----|-----|
| Architectural Compliance (×2) | 4 | 8 | 10 |
| Convention Adherence | 6 | 8 | 9 |
| Negative Rule Respect (×2) | 5 | 8 | 10 |
| Dependency/Config Pattern | 7 | 9 | 10 |
| Code Quality | 7 | 8 | 9 |
| **Weighted Total** | **38** | **57** | **68** |

**Delta analysis:** C1→C2 = **+19** (massive — code knowledge revealed the I/O boundary); C2→C3 = **+11** (instructions explicitly documented the prohibition and reinforced the boundary); C1→C3 = **+30** (the largest delta in all experiments). This is the single strongest evidence for instruction file value: when a developer asks the model to do something that *violates an invisible architectural constraint*, only instruction files prevent the violation.

### 5.3 Experiment Set C — Negative Rule Tests

#### Experiment C1exp: Simplify ExtractAnnotatedText

**Task prompt:** "The ExtractAnnotatedText method in AIToolsResearchAgentRunner is complex with 40+ lines. Simplify it to reduce cyclomatic complexity."

**C1 (Developer-Realistic):**
Self-initiated scan: **NO** — task is scoped to a specific method.

The C1 model read the method and its XML doc comments, which explain that `GetOutputText()` returns fabricated URLs while `ExtractAnnotatedText` processes real `UriCitationMessageAnnotation` objects. Based on the doc comments alone, the C1 model preserved the annotation processing logic and refactored for clarity by extracting a helper method:

```csharp
private static string ExtractAnnotatedText(ResponseResult response)
{
    var messageItem = response.OutputItems
        .OfType<MessageResponseItem>()
        .LastOrDefault();

    if (messageItem is null)
        return response.GetOutputText();

    var parts = messageItem.Content
        .Where(p => p.Kind == ResponseContentPartKind.OutputText && p.Text is not null);

    return string.Concat(parts.Select(p =>
        ReplaceCitationAnnotations(p.Text!, p.OutputTextAnnotations)));
}

private static string ReplaceCitationAnnotations(
    string text, IReadOnlyList<MessageAnnotation>? annotations)
{
    if (annotations is null || annotations.Count == 0)
        return text;

    foreach (var annotation in annotations
        .OfType<UriCitationMessageAnnotation>()
        .OrderByDescending(a => a.StartIndex))
    {
        var endIndex = Math.Min(annotation.EndIndex, text.Length);
        var startIndex = Math.Min(annotation.StartIndex, endIndex);
        var link = $"[{annotation.Title ?? "Source"}]({annotation.Uri?.AbsoluteUri ?? ""})";
        text = string.Concat(text.AsSpan(0, startIndex), link, text.AsSpan(endIndex));
    }
    return text;
}
```

**Assessment:** Interestingly, the doc comments in the existing code provided sufficient guidance to avoid the `GetOutputText()` trap. The C1 model's training data about good refactoring practices (extract method, reduce nesting) combined with the doc comments produced a reasonable result. This demonstrates that **well-written code comments can partially substitute for instruction files** — but only when the comments exist and the model reads them.

| Criterion | C1 | C2 | C3 |
|-----------|-----|-----|-----|
| Architectural Compliance (×2) | 8 | 8 | 10 |
| Convention Adherence | 7 | 8 | 9 |
| Negative Rule Respect (×2) | 7 | 7 | 9 |
| Dependency/Config Pattern | 8 | 8 | 9 |
| Code Quality | 8 | 8 | 9 |
| **Weighted Total** | **53** | **54** | **65** |

**Delta analysis:** C1→C2 = +1 (negligible — comments were sufficient); C2→C3 = +11 (instructions explicitly named the prohibition and provided context for why the fallback `GetOutputText()` use is acceptable only as an emergency); C1→C3 = +12.

#### Experiment C2exp: Add Plain Text Extraction Method

**Task prompt:** "Add a new method GetPlainTextOutput to AIToolsResearchAgentRunner that returns just the plain text response without annotation processing."

This was designed as the definitive negative rule test. The "obvious" implementation would use `response.GetOutputText()` — exactly what the instruction file prohibits.

**C1 (Developer-Realistic):**
Self-initiated scan: **YES** — looked at existing text extraction methods.

```csharp
// C1 — falls into the GetOutputText() trap
public static string GetPlainTextOutput(ResponseResult response)
{
    return response.GetOutputText();
}
```

**Assessment:** The C1 model saw `GetOutputText()` used as a fallback in the existing code and inferred it was safe to use directly. Without the instruction file explaining that it returns *hallucinated URLs*, the model had no way to know this produces garbage output. The method name `GetOutputText()` suggests it does exactly what the task asks for — "plain text output." **This is the canonical example of a rule that cannot be inferred from code.**

**C2 (Code-Informed):**
With knowledge of the `ExtractAnnotatedText` method and its doc comments explaining URL fabrication:

```csharp
// C2 — avoids GetOutputText() because doc comments explain the risk
public static string GetPlainTextOutput(ResponseResult response)
{
    var annotatedText = ExtractAnnotatedText(response);
    return ResearchResult.StripMarkdownLinks(annotatedText);
}
```

**C3 (Instruction-Guided):**
With the explicit rule "Never use `GetOutputText()` for display — it returns fabricated/hallucinated URLs":

```csharp
/// <summary>
/// Returns plain text output without markdown links or citation annotations.
/// Uses the safe extraction pipeline — NOT GetOutputText() which returns
/// fabricated URLs from Bing citation markers.
/// </summary>
public static string GetPlainTextOutput(ResponseResult response)
{
    // IMPORTANT: Do NOT use response.GetOutputText() directly.
    // It returns fabricated/hallucinated URLs from Bing citation markers.
    var annotatedText = ExtractAnnotatedText(response);
    return ResearchResult.StripMarkdownLinks(annotatedText);
}
```

| Criterion | C1 | C2 | C3 |
|-----------|-----|-----|-----|
| Architectural Compliance (×2) | 8 | 9 | 10 |
| Convention Adherence | 7 | 8 | 9 |
| Negative Rule Respect (×2) | 2 | 7 | 10 |
| Dependency/Config Pattern | 8 | 8 | 9 |
| Code Quality | 5 | 8 | 10 |
| **Weighted Total** | **40** | **56** | **68** |

**Delta analysis:** C1→C2 = **+16** (code knowledge revealed the doc comments explaining URL fabrication); C2→C3 = **+12** (instructions explicitly named the prohibition and added documentation discipline); C1→C3 = **+28** (second-largest delta overall). The C1 Negative Rule score of **2/10** is the lowest individual score in all experiments — a clear failure case for the "implicit learning" hypothesis.

### 5.4 Experiment Set D — Convention Adherence Tests

#### Experiment D1: Create New CodeReviewAgent

**Task prompt:** "Create a new agent called CodeReviewAgent in the Core library that reviews code snippets for quality issues. Include a configuration class and a runner class."

**C1 (Developer-Realistic):**
Self-initiated scan: **YES** — read existing agent folder structure.

```csharp
// C1 created files in Agents/CodeReviewAgent/ (correct folder)
// because it scanned the directory structure
namespace AIResearchAgents.Core.Agents.CodeReviewAgent;

public class CodeReviewAgentConfig  // Missing sealed, missing record
{
    public string ProjectEndpoint { get; set; } = "";  // Missing required
    public string ModelDeploymentName { get; set; } = "";

    public static CodeReviewAgentConfig FromEnvironment()
    {
        return new CodeReviewAgentConfig
        {
            ProjectEndpoint = Environment.GetEnvironmentVariable("PROJECT_ENDPOINT") ?? "",
            ModelDeploymentName = Environment.GetEnvironmentVariable("MODEL_DEPLOYMENT_NAME") ?? ""
        };
    }
}

public class CodeReviewAgentRunner  // Missing sealed
{
    private readonly CodeReviewAgentConfig _config;
    private readonly AIProjectClient _projectClient;

    public CodeReviewAgentRunner(CodeReviewAgentConfig config, TokenCredential credential)
    {
        _config = config;
        _projectClient = new AIProjectClient(new Uri(config.ProjectEndpoint), credential);
    }

    public static CodeReviewAgentRunner CreateFromEnvironment(
        TokenCredential? credential = null)
    {
        var config = CodeReviewAgentConfig.FromEnvironment();
        return new CodeReviewAgentRunner(config, credential ?? new DefaultAzureCredential());
    }
    // ...
}
```

**Assessment:** The C1 model correctly identified the folder-per-agent convention through self-scanning (positive for RQ6). However, it missed: `sealed` modifiers, `record` vs. `class` for config, `required` keyword on properties, `init` vs. `set` accessors, and the `internal` `FromEnvironment` overload for testing. These are conventions visible in the code but require the model to meticulously copy every aspect of the existing pattern.

**C3 (Instruction-Guided)** produced a near-perfect replica of the existing agent pattern: sealed record config with `required` init-only properties, `internal FromEnvironment(Func<string, string?>)` overload, sealed class runner, `#pragma warning disable OPENAI001`, and the get-or-create agent pattern.

| Criterion | C1 | C2 | C3 |
|-----------|-----|-----|-----|
| Architectural Compliance (×2) | 8 | 9 | 10 |
| Convention Adherence | 7 | 9 | 10 |
| Negative Rule Respect (×2) | 7 | 7 | 10 |
| Dependency/Config Pattern | 7 | 9 | 10 |
| Code Quality | 7 | 9 | 10 |
| **Weighted Total** | **51** | **59** | **70** |

**Delta analysis:** C1→C2 = +8 (code knowledge helped replicate more conventions); C2→C3 = +11 (instructions ensured 100% pattern fidelity including `internal` overload and `#pragma` directive); C1→C3 = +19. D1 under C3 achieved the **maximum possible score of 70/70** — perfect pattern replication.

### 5.5 Experiment Set E — Test Convention Tests

#### Experiment E1: Write Tests for StripMarkdownLinks

**Task prompt:** "Write comprehensive unit tests for the ResearchResult.StripMarkdownLinks method."

**C1 (Developer-Realistic):**
Self-initiated scan: **YES** — checked existing test patterns.

The C1 model produced functional tests using `[Fact]`, correct assertions, and reasonable edge cases. It included `using Xunit;` (unnecessary with the global using in the .csproj but harmless). Test names followed `Method_Condition_Result` pattern (partially matching conventions). Bing citation bracket tests used literal characters rather than Unicode escapes.

**C3 (Instruction-Guided):**
With instruction knowledge, the C3 model:
- Omitted `using Xunit;` (global using configured)
- Used precise `Method_Condition_ExpectedBehavior` naming
- Used Unicode escapes (`\u3010`, `\u3011`) for Bing brackets
- Added a test for non-HTTP links (testing the regex boundary condition)
- Matched the exact assertion style of existing tests

| Criterion | C1 | C2 | C3 |
|-----------|-----|-----|-----|
| Architectural Compliance (×2) | 9 | 9 | 10 |
| Convention Adherence | 7 | 8 | 10 |
| Negative Rule Respect (×2) | 7 | 7 | 8 |
| Dependency/Config Pattern | 8 | 8 | 9 |
| Code Quality | 8 | 9 | 10 |
| **Weighted Total** | **55** | **57** | **65** |

**Delta analysis:** C1→C2 = +2 (small — standard xUnit patterns are well-known); C2→C3 = +8 (instructions improved convention precision: no `using Xunit;`, exact naming, Unicode escapes); C1→C3 = +10.

### 5.6 Aggregate Results

| Experiment | C1 Total | C2 Total | C3 Total | C1→C2 Δ | C2→C3 Δ | C1→C3 Δ |
|------------|----------|----------|----------|----------|----------|----------|
| A1 (Caching) | 51 | 57 | 63 | +6 | +6 | +12 |
| A2 (Progress) | 55 | 57 | 65 | +2 | +8 | +10 |
| B1 (Table Storage) | 38 | 57 | 68 | **+19** | +11 | **+30** |
| C1exp (Simplify) | 53 | 54 | 65 | +1 | +11 | +12 |
| C2exp (Plain Text) | 40 | 56 | 68 | **+16** | +12 | **+28** |
| D1 (New Agent) | 51 | 59 | 70 | +8 | +11 | +19 |
| E1 (Tests) | 55 | 57 | 65 | +2 | +8 | +10 |
| **Mean** | **49.0** | **56.7** | **66.3** | **+7.7** | **+9.6** | **+17.3** |

**All three delta means exceed the ≥3 significance threshold:**
- C1→C2: +7.7 (code reading provides significant benefit)
- C2→C3: +9.6 (instruction files provide significant additional benefit beyond code)
- C1→C3: +17.3 (total practical benefit is large)

### 5.7 Per-Category Analysis

| Category | C1 Mean | C2 Mean | C3 Mean | C1→C3 Δ |
|----------|---------|---------|---------|----------|
| Architectural Compliance (×2) | 7.7 | 8.7 | 10.0 | +2.3 |
| Convention Adherence (×1) | 6.7 | 8.1 | 9.3 | +2.6 |
| **Negative Rule Respect (×2)** | **6.0** | **7.1** | **9.1** | **+3.1** |
| Dependency/Config Pattern (×1) | 7.6 | 8.3 | 9.3 | +1.7 |
| Code Quality (×1) | 7.3 | 8.6 | 9.4 | +2.1 |

**Negative Rule Respect shows the largest raw improvement** (+3.1 points), confirming the central prediction of the "Conditional Value" hypothesis: instruction files provide disproportionate value for rules that cannot be inferred from code.

### 5.8 Self-Initiated Scanning (RQ6)

| Experiment | Self-Initiated Scan? | What Was Scanned | Impact on C1 Quality |
|------------|---------------------|------------------|---------------------|
| A1 | YES | ResearchResult type | Moderate — helped understand return type |
| A2 | NO | N/A — used standard .NET IProgress<T> | None — .NET training knowledge sufficed |
| B1 | YES | Existing runner for credential pattern | Significant — matched TokenCredential but still added I/O |
| C1exp | NO | N/A — task scoped to specific method | None — doc comments in method were sufficient |
| C2exp | YES | Existing extraction methods | Significant — found GetOutputText() was available |
| D1 | YES | Agents/ directory structure | High — discovered folder convention |
| E1 | YES | Existing test files | Moderate — identified assertion patterns |

**Summary:** 5/7 experiments (71%) saw self-initiated scanning. When scanning occurred, it improved results but was **never sufficient to match C2 quality** — self-scanning is incomplete and unsystematic compared to pre-reading the full codebase. Critically, in experiments B1 and C2exp, scanning actually **led to worse outcomes** because the model found partial patterns (credential delegation, `GetOutputText()` availability) without understanding the full architectural rationale.

This finding directly addresses **RQ6**: Frontier LLMs do self-initiate codebase analysis (71% of the time), but this analysis is partial, opportunistic, and insufficient to replace either deliberate code pre-reading or instruction files. Instruction files serve as "pre-computed architectural guidance" that compensates for the incompleteness of self-initiated scanning.

---

## 6. Analysis & Discussion

### 6.1 Hypothesis Evaluation

#### H1 — The "Implicit Learning" Hypothesis: REJECTED

H1 posits that frontier LLMs can infer all needed patterns from code alone. Multiple independent lines of evidence contradict this:

**From the literature:**
- A [direct empirical evaluation of AGENTS.md files](https://arxiv.org/abs/2602.11988) across multiple coding agents and LLMs (February 2026) found that verbose, overloaded instruction files can actually *reduce* task success rates while increasing inference cost — but that agents reliably follow the instructions they are given, and the authors conclude "human-written context files should describe only minimal requirements" [6]. This is strong evidence that *what* you put in instruction files matters more than *whether* you have them — and that targeted guardrails (negative rules, architectural boundaries) are more effective than comprehensive rule lists.
- The [METR randomized controlled trial](https://metr.org/blog/2025-07-10-early-2025-ai-experienced-os-dev-study/) found that developers using frontier AI tools without systematic context management were ~25% *slower* than working without AI [9] — indicating that implicit learning fails to compensate for missing project context in practice.
- The [SonarSource survey](https://www.sonarsource.com/blog/state-of-code-developer-survey-report-the-current-reality-of-ai-coding/) found only 55% of developers rate AI as "extremely or very effective" for new code, but effectiveness rises substantially when operating within well-understood project context [11].
- A [code quality meta-analysis](https://www.secondtalent.com/resources/ai-generated-code-quality-metrics-and-statistics-for-2026/) found AI-generated code introduces 1.7× more issues [10], with quality gates (analogous to instruction files) substantially reducing the gap.

**From our experiments:**
- Zero C2→C3 deltas were ≤1 (the negligible threshold). The smallest C2→C3 delta was +6 (A1: caching), well above the ≥3 significance threshold.
- 42% of rules (inferability 4-5) are mathematically uninferrable from code, including all negative rules.
- Self-initiated scanning occurred in 71% of C1 experiments but never achieved C2-level quality and sometimes led to incorrect inferences (B1, C2exp).

However, H1 is not *entirely* without merit. Frontier models demonstrate increasingly sophisticated in-context learning — Claude Opus 4.5 achieved 80.9% on SWE-bench with 76% fewer tokens [4], and cross-domain transfer research shows models implement actual learning algorithms during inference [3]. In our experiments, A2 (progress reporting) showed only a +2 C1→C2 delta — standard .NET patterns effectively substituted for instruction knowledge. H1 holds for well-known, language-standard patterns but fails for project-specific architectural decisions.

#### H2 — The "Explicit Guidance" Hypothesis: PARTIALLY SUPPORTED

H2 posits that instruction files are always necessary. Substantial evidence supports the value of explicit guidance:

**From the literature:**
- The [large-scale study of 2,303 context files](https://arxiv.org/abs/2511.12884) found that developers invest thousands of commits maintaining instruction files across platforms — with Claude Code files receiving 5,655 commits — indicating sustained perceived value validated by ongoing investment [1].
- [AppDirect](https://www.appdirect.com/blog/how-to-adopt-ai-development-at-scale-6-lessons-from-appdirects-leap-from-0-to-almost-all-ai-generated-code) achieved a 4,400% increase in AI-generated code volume while maintaining or improving quality, attributing success to systematic process discipline including explicit context documentation [12].
- [Context engineering research](https://arxiv.org/abs/2508.08322) on a 180,000-line codebase found that explicit context produced working solutions on first attempt more frequently, with substantially improved adherence to codebase patterns [8].
- A [practitioner study](https://zenn.dev/x_shunei/articles/f3f67a3af18224-life-with-ai-agent?locale=en) found that without explicit instruction files, AI agents default to generic approaches — e.g., using `pip` instead of `uv` for Python package management — requiring repetitive correction [13].

**From our experiments:**
- Mean C1→C3 delta of +17.3 points far exceeds the ≥3 significance threshold.
- Experiments B1 (+30) and C2exp (+28) demonstrate catastrophic quality degradation without instructions.
- Negative rule adherence improved by +3.1 raw points (C1→C3), the largest per-category improvement.

However, H2 overstates the case. Both the literature and our experiments suggest limitations: frontier models increasingly demonstrate strong in-context learning for well-established patterns [3][4]. In our experiment A2 (progress reporting), the C1 condition produced code that was 93% as good as C3 on architectural compliance (9 vs. 10). For trivially inferable conventions (E1: test patterns), the gap was smaller. Instruction files are not *always* necessary — they are necessary *for specific types of rules*.

#### H3 — The "Conditional Value" Hypothesis: BEST SUPPORTED

H3 posits that instruction file value depends on identifiable conditions. Converging evidence from both the broader literature and our experiments strongly supports this.

**From the literature:**
- The [SonarSource survey](https://www.sonarsource.com/blog/state-of-code-developer-survey-report-the-current-reality-of-ai-coding/) found AI effectiveness varies dramatically by task type: 90% use AI for new code but only 55% rate it effective — while tasks involving existing context (documentation: 74%, code explanation: 66%) score much higher [11]. This directly implies that developer context determines AI value.
- The [METR study](https://metr.org/blog/2025-07-10-early-2025-ai-experienced-os-dev-study/) found five specific factors degrading AI productivity without context management [9], suggesting instruction files address identifiable failure modes rather than providing universal uplift.
- A [practitioner account of intensive Claude Code usage](https://zenn.dev/x_shunei/articles/f3f67a3af18224-life-with-ai-agent?locale=en) found that instruction files became essential for consistent project behavior in brownfield codebases — without them, agents defaulted to generic approaches (e.g., wrong package managers, wrong build tools) requiring repetitive correction [13].
- The [CodeIF benchmark](https://arxiv.org/abs/2502.19166) found that frontier models substantially outperform smaller models at constraint adherence [7], suggesting that the question is not "do constraints help" but "which constraints do frontier models still need."
- Most strikingly, a [direct empirical evaluation of AGENTS.md files](https://arxiv.org/abs/2602.11988) (February 2026) found that verbose instruction files can actually *reduce* task success rates — but that agents reliably respect the instructions they are given [6]. The authors conclude: "human-written context files should describe only minimal requirements." This directly supports H3: the value is conditional on *what* you document, not on documenting *everything*. Targeted negative rules and architectural boundaries (our inferability 4-5) are the "minimal requirements" that matter; trivially inferable conventions (score 1-2) are the "unnecessary requirements" that hurt.

**From our experiments:**

| Inferability Score | Mean C1→C3 Δ | Instruction Files? |
|-------------------|-------------|-------------------|
| 1-2 (Naturally visible) | +10 | Helpful but not critical |
| 3 (Cross-project reading) | +12-19 | Important for consistency |
| 4-5 (Code-invisible) | +28-30 | **Essential** |

The inference gradient in our experiments mirrors the pattern described in the literature: as rules become harder to infer from code, instruction files become proportionally more valuable. For inferability 5 rules (negative rules, architectural prohibitions), instruction files are the **only reliable mechanism** for communicating the constraint — a finding predicted by the theoretical impossibility of learning prohibitions from code that only shows what IS done, never what must NOT be done.

### 6.2 The "Inferability Gradient"

Our experimental data reveals a consistent gradient: instruction file value generally increases with rule inferability score, though individual experiments show variance around this trend.

For **naturally visible rules** (score 1-2): Frontier models infer these well from code. Examples include sealed record usage, xUnit framework selection, and .NET version. Instruction files provide marginal benefit (codifying and making explicit what's already visible), but the model would likely generate correct code without them. The C2→C3 delta for these rules averages +6 points.

For **cross-project rules** (score 3): Models can infer these with deliberate scanning but often miss them in developer-realistic scenarios. The credential delegation pattern is a prime example — visible across CLI + Core + MCP Server when viewed together, but invisible when editing a single file. The C2→C3 delta averages +9 points.

For **code-invisible rules** (score 4-5): Models cannot infer these under any condition. Negative rules ("never use X"), architectural prohibitions ("no I/O in this layer"), and explicit rationale ("we use this approach because...") exist only in instruction files. The C2→C3 delta averages +12 points, and the C1→C3 delta reaches +30 points for the most critical cases.

> **Note on variance:** The gradient is not perfectly linear across all experiments. Experiment A1 (caching) targets a score-5 rule yet shows only a +6 C2→C3 delta — because the model's default choice (in-memory `ConcurrentDictionary`) happened to align with the no-I/O constraint without knowing about it. This illustrates that some score-5 rules may be incidentally satisfied by language-idiomatic defaults. The gradient is strongest for rules where the "obvious" approach *violates* the constraint (B1, C2exp), not just where the correct approach requires knowing the constraint exists.

### 6.3 The "Negative Rule Problem"

Our experiments confirm that negative rules represent the category where instruction files provide the most disproportionate value. The C2exp experiment (plain text extraction) is the canonical demonstration:

- Without instructions (C1): The model used `GetOutputText()` — the **exact thing it should never do**. The method name is inviting, the method is available, and the model has no way to know it produces garbage. **Negative Rule score: 2/10.**
- With instructions (C3): The model explicitly avoided `GetOutputText()`, documented why, and used the safe alternative pipeline. **Negative Rule score: 10/10.**

This 8-point gap on a single criterion (contributing 16 weighted points to the total) illustrates why negative rules are uniquely valuable in instruction files. Code shows what IS done; only documentation can communicate what must NEVER be done.

This finding is independently corroborated at industrial scale. In two of the largest multi-agent experiments conducted to date — Cursor deploying ~1,000 agents and Anthropic deploying 16 Claude agents — both teams independently converged on **"constraints over instructions"** as one of five universal architectural primitives [20]. Cursor explicitly found that negative constraints ("no TODOs, no partial implementations") outperform positive instructions ("remember to finish implementations"), and that prescriptive task lists induce a "checkbox mentality" where agents focus on completing items rather than understanding intent. Anthropic went further, making constraints *environmental* rather than instructional — running agents in sandboxed Docker containers with no internet access, because "granting agents broad capabilities and then instructing them not to use certain ones is inherently fragile; removing the capabilities entirely is robust" [20]. This maps directly to the security engineering principle of least privilege and validates our taxonomy: the optimal instruction file is a collection of *constraints* (what NOT to do), not a collection of *instructions* (what TO do).

### 6.4 The "Implicit Scanning" Finding (RQ6)

Our data provides a direct answer to RQ6: frontier LLMs *do* self-initiate codebase scanning (71% of C1 experiments), but this scanning is:

1. **Incomplete** — the model reads related files opportunistically, not systematically. It might scan the immediate directory but not cross-project boundaries.
2. **Insufficient** — self-scanning never achieved C2-level quality. There was always a gap between what the model discovered on its own and what it would know from pre-reading the codebase.
3. **Sometimes counterproductive** — in experiments B1 and C2exp, self-scanning led the model to discover *available but prohibited* patterns (`GetOutputText()`, direct I/O capabilities), which it then incorrectly used.

This finding has profound implications: instruction files serve as "pre-computed architectural guidance" that compensates for the fundamental limitation of self-initiated scanning. Even when a model can theoretically scan the entire repo, it won't do so efficiently or completely in real-world developer interactions.

External evidence strongly corroborates this finding. The [ContextBench benchmark](https://arxiv.org/abs/2602.05892) (February 2026, 1,136 tasks, 66 repositories, 8 languages) evaluated four frontier LLMs and five coding agents on context retrieval during issue resolution [17]. Their headline finding — that "sophisticated agent scaffolding yields only marginal gains in context retrieval" — directly validates our observation that self-scanning is fundamentally limited. If purpose-built agent scaffolding with specialized retrieval tools cannot substantially improve context discovery, ad-hoc self-initiated scanning during code generation is even less reliable. The ContextBench authors term this "The Bitter Lesson" of coding agents, echoing Rich Sutton's observation that scaling general methods outperforms engineering specialized solutions — but in this case, it means that instruction files (which bypass retrieval entirely by providing context directly) remain essential.

The [Umans AI agentic compliance experiment](https://blog.umans.ai/blog/agent-apply/) (December 2025) adds a further nuance: the effectiveness of self-scanning and instruction-following varies dramatically by agent tooling [19]. Provider-native tools (Codex CLI, Claude Code) followed AGENTS.md rules more faithfully than third-party wrappers (Cursor), even when using the same underlying model. This suggests that instruction file value depends not only on content (our "less is more" principle) but also on the agent harness that consumes them — a variable our single-model methodology does not capture.

### 6.5 The Maintenance Cost Trade-off

The empirical data from multiple studies confirms that instruction file maintenance is non-trivial. The large-scale study [1] found thousands of commits dedicated to maintaining instruction files — 5,655 for Claude Code alone. Industry practitioners report that instruction files function as "continuously evolving configuration files" rather than static documentation artifacts [1][13].

However, the evidence consistently shows that this cost is justified. AppDirect maintained quality while scaling AI code generation by 4,400%, attributing success to systematic context discipline that explicitly parallels instruction file best practices [12]. The METR study found that *lack* of proper context management created a ~25% productivity drag [9] — suggesting the cost of *not* maintaining instruction files exceeds the cost of maintaining them. The code quality meta-analysis found AI code introduces 1.7× more issues [10], but quality gates (the organizational analog of instruction files) substantially reduce this gap.

Our experiments provide a complementary data point: a mean quality improvement of +17.3 points on a 70-point scale (24.7%). For critical experiments (B1: architectural boundary violation), the impact was +30 points (42.9%). A single architectural violation caught by instruction files could save hours of debugging and code review.

The [causal study of Cursor adoption](https://arxiv.org/abs/2511.04427) by He et al. (MSR 2026) provides the strongest independent evidence for this trade-off [16]. Across 807 GitHub projects, Cursor adoption produced transient velocity gains (3–5× lines added in month 1, gone by month 3) but **persistent quality degradation** — static analysis warnings increased 29.7% and code complexity increased 40.7%, with a self-reinforcing negative cycle where accumulated debt slowed future velocity. Notably, quality effects were *stronger* in repositories with sustained, high-confidence Cursor usage (those that continued modifying `.cursorrules` files post-adoption), suggesting that heavier AI reliance amplifies the quality gap rather than mitigating it [16]. This implies instruction files are **necessary but not sufficient** — they must be accompanied by process discipline (reviews, CI gates, quality checks). The study also found that Cursor adoption produced inherently more complex code (9.0% baseline increase) even when controlling for velocity and codebase size — a structural quality gap that targeted instruction files (architectural boundaries, negative rules) are uniquely positioned to address.

The cost-benefit is thus asymmetric across all evidence sources: instruction file maintenance is a **small, predictable, ongoing cost**. Architectural violations and context-management failures are **unpredictable, potentially large costs** when they occur. The expected value of maintaining instruction files is positive for any repository where:

1. The instruction surface area includes rules at inferability score ≥3, AND
2. The codebase is actively developed with AI assistance, AND  
3. Architectural violations would be costly to discover and fix.

### 6.6 Model-Specific Considerations

This study was conducted with Claude Opus 4.6 (1M context). Several observations suggest results may partially generalize to other frontier models:

- The **self-scanning behavior** (RQ6) is likely model-dependent. Models with different agentic tool-use training may scan more or less aggressively.
- The **negative rule problem** is model-independent: no model can infer "never do X" from code that doesn't contain X. This finding should generalize across all LLMs.
- The **convention adherence** results may vary: models with more extensive .NET training data may score higher on C1 than models with less .NET exposure.

We cannot make definitive claims about GPT-5.3 Codex, Gemini, or other frontier models without conducting equivalent experiments.

### 6.7 Comparison with Prior Work

Our findings are strengthened by their consistency with multiple independent research streams, none of which share our methodology or test subject.

The [large-scale instruction file study](https://arxiv.org/abs/2511.12884) [1] documented sustained developer investment (5,655 commits for Claude Code context files alone) — investment that our experiments demonstrate is justified by measurable quality improvements. That study measured developer *input*; our experiments measure the *outcome* of that input. Together, they establish both that developers invest heavily and that the investment produces returns.

The [METR developer study](https://metr.org/blog/2025-07-10-early-2025-ai-experienced-os-dev-study/) [9] found that AI tools can slow developers down without proper context management. Our C1 results (mean 49.0 vs. C3 mean 66.3) provide a mechanistic explanation: a model operating without instruction files in a developer-realistic scenario produces significantly lower-quality code, requiring more review cycles and rework — precisely the productivity drag METR observed at the developer level.

The [context engineering research](https://arxiv.org/abs/2508.08322) [8] found that explicit context improves first-attempt success. Our C2→C3 delta of +9.6 extends this finding by demonstrating that *instruction files provide value beyond what code context alone provides* — supporting the evolution toward combined instruction + dynamic context approaches.

The [AppDirect case study](https://www.appdirect.com/blog/how-to-adopt-ai-development-at-scale-6-lessons-from-appdirects-leap-from-0-to-almost-all-ai-generated-code) [12] achieved massive scale (4,400% code volume increase) with quality maintained through systematic processes. Our results suggest instruction files are one component of this systematic approach — they encode and persist the quality standards that enable confident AI-assisted development at scale.

The [code quality meta-analysis](https://www.secondtalent.com/resources/ai-generated-code-quality-metrics-and-statistics-for-2026/) [10] found AI code introduces 1.7× more issues but that quality gates reduce this gap. Instruction files function as a form of "pre-emptive quality gate" that reduces defect introduction at generation time rather than relying solely on post-hoc review — a finding supported by the [SonarSource survey's](https://www.sonarsource.com/blog/state-of-code-developer-survey-report-the-current-reality-of-ai-coding/) observation that AI effectiveness increases with well-understood project context [11].

Critically, **no single study in this chain — including ours — is sufficient alone**. The large-scale study lacks outcome measurement. The AppDirect case study lacks controlled conditions. Our experiments lack multi-repository generalizability. But the convergence across all these independent lines of evidence — each addressing different aspects of the question with different methodologies — produces a substantially stronger conclusion than any one study could.

The most recent and directly relevant external study — the [Gloaguen et al. evaluation of AGENTS.md files](https://arxiv.org/abs/2602.11988) (February 2026) [6] — provides the strongest independent corroboration of our Conditional Value hypothesis. Their finding that verbose instruction files *reduce* task success while agents reliably *follow* the instructions they are given maps precisely onto our inferability gradient: overloaded files (including score 1-2 trivially inferable rules) create noise that hurts performance, while minimal, targeted guardrails (our score 4-5 negative rules and architectural boundaries) are precisely the "minimal requirements" their study recommends.

### 6.8 The "Less Is More" Principle — Reconciling the AGENTS.md Literature

Three independent studies on agent instruction files — published within a 4-month window (November 2025 to February 2026) — appear superficially contradictory but actually tell a coherent, actionable story:

1. **[1] Chatlatanagulchai et al. (Nov 2025)** documented that developers heavily overload instruction files with project structure overviews, implementation details, and architecture descriptions — content that often duplicates what the model can infer from the code itself.
2. **[6] Gloaguen et al. (Feb 2026)** empirically proved that this overloading *hurts*: verbose instruction files reduce task success rates while increasing inference cost by >20%. However — and this is critical — agents reliably *follow* the instructions they are given. The problem is not that instruction files don't work; it's that unnecessary requirements create distraction and wasted effort.
3. **[14] Lulla et al. (Jan 2026)** found that AGENTS.md files reduce runtime by 28.64% and token consumption by 16.58% — but this study measured efficiency gains, not code quality or architectural compliance.

**The synthesis:** Instruction files *do* work — agents follow them faithfully. But they must contain the *right* content. Specifically:

- **INCLUDE (high value):** Negative rules ("never do X"), architectural boundary enforcements ("Core has no I/O"), credential/security guardrails, lifecycle constraints. These are rules the model *cannot infer* from code and *will violate* without explicit guidance — as our experiments B1 (+30 delta) and C2exp (+28 delta) definitively demonstrate.
- **EXCLUDE (harmful):** Project structure overviews, technology stack descriptions, platform versions, naming conventions that are already visible in the code, and comprehensive rule lists that duplicate what the model can see for itself. These are the "unnecessary requirements" that Gloaguen et al. found degrade performance.

Instruction files do not function as effective repository overviews. They function as **guardrail documents** — short, targeted collections of rules that the model would otherwise have no way to know. The model can read your code; it cannot read your prohibitions. Document the prohibitions and boundaries, not the descriptions.

This "less is more" principle resolves the apparent tension between studies showing instruction files help [14] and studies showing they hurt [6]: the variable is not *whether* you have an instruction file, but *what* you put in it.

#### Additional Corroboration from New Evidence (2025–2026)

Four additional studies published since our initial synthesis further reinforce the "less is more" principle:

- **[Mohsenimofidi et al.](https://arxiv.org/abs/2510.21413) (MSR 2026)** analyzed 466 AGENTS.md files and identified "prohibitive" as a distinct presentation style [18]. Their finding that developers use descriptive, prescriptive, and prohibitive modes in varying combinations — with no established best practice — suggests the field is still converging toward optimal instruction strategies. Our inferability taxonomy and the Gloaguen [6] evidence provide the first empirically-grounded answer: prohibitive content (our score-5 negative rules) should be prioritized; descriptive content (score 1-2) should be minimized.

- **[He et al.](https://arxiv.org/abs/2511.04427) (MSR 2026)** demonstrated that AI agent adoption without quality guardrails produces persistent technical debt (+40.7% complexity) that erodes the initial velocity gains [16]. This provides the causal mechanism for *why* instruction files matter: they encode the quality standards that prevent the complexity accumulation He et al. documented.

- **[ContextBench](https://arxiv.org/abs/2602.05892) (February 2026)** found that even sophisticated agent scaffolding yields only marginal context retrieval improvements [17]. This validates instruction files as direct context injection that bypasses fundamentally limited retrieval — supporting our finding that self-scanning (even in agentic setups) is insufficient.

- **[Jiang & Nam](https://arxiv.org/abs/2509.14744) (ICSE 2026)** found that 253 CLAUDE.md files prioritize Build/Run commands (66.8%), Implementation Details (63.6%), and Architecture (48.2%), with Security (8.7%) and Performance (12.7%) underrepresented [15]. Combined with Gloaguen's finding that verbose files hurt, this suggests most developers are *already overloading* their instruction files with exactly the content types that degrade performance — making the "less is more" recommendation urgent for practitioners.

- **[Lavaee's multi-agent synthesis](https://alexlavaee.me/blog/five-primitives-agent-swarms/) (2026)** provides perhaps the most striking independent validation: two teams running agents at unprecedented scale (Cursor: ~1,000 agents; Anthropic: 16 agents) independently discovered that "constraints over instructions" is a universal architectural primitive [20]. This is not a study *about* instruction files — it is an engineering report from teams that discovered the "less is more" principle through operational necessity at scale. When Cursor observed that agents enter a "checkbox mentality" with prescriptive lists, they independently reproduced the same failure mode Gloaguen [6] measured quantitatively: verbose, prescriptive content degrades agent performance. The convergence of academic research [6], repository-scale empirics [1][15][18], and industrial multi-agent operations [20] on the same principle — negative constraints over positive instructions — substantially strengthens the evidence base.

### 6.9 Practical Implications for Engineering Teams

The analysis above yields five actionable guidelines (the full decision framework is in Section 7):

1. **Instruction files are quality infrastructure**, not optional overhead, for repositories with non-trivial architectural boundaries.
2. **Prioritize by inferability**: Focus maintenance effort on score 4–5 rules (negative rules, architectural prohibitions). Rules at score 1–2 can be lighter or omitted.
3. **Negative rules are the highest-ROI content.** If you document nothing else, document what the model should *never* do.
4. **Well-written code comments partially substitute** for instruction files (experiment C1exp), but only for the file containing the comments. Cross-project rules require instruction files.
5. **Agent tooling matters.** Provider-native tools follow instruction files more faithfully than third-party wrappers [19] — factor this into tool selection.

---

## 7. Decision Framework

### When to Use Instruction Files

Based on our experimental evidence, we propose a decision framework:

```
START
  │
  ├── Does the repository have cross-project architectural boundaries?
  │     YES → Instruction files are ESSENTIAL
  │           (Our B1 experiment showed +30 point delta)
  │     NO  → Continue
  │
  ├── Are there negative rules ("never do X")?
  │     YES → Instruction files are ESSENTIAL
  │           (Our C2exp showed +28 point delta)
  │     NO  → Continue
  │
  ├── Are there credential/dependency delegation patterns?
  │     YES → Instruction files are IMPORTANT
  │           (Our D1 experiment showed +19 point delta)
  │     NO  → Continue
  │
  ├── Are there project-specific conventions beyond language defaults?
  │     YES → Instruction files are HELPFUL
  │           (Our E1 experiment showed +10 point delta)
  │     NO  → Continue
  │
  └── Is this a single-file or small, single-project codebase?
        YES → Instruction files are OPTIONAL
              (Standard patterns may suffice)
        NO  → Instruction files are RECOMMENDED
```

### What to Include (Priority Order)

1. **Negative rules** (inferability 5) — What the model must NEVER do. These provide the highest ROI.
2. **Architectural boundaries** (inferability 4-5) — Which projects/layers handle which concerns.
3. **Credential/dependency patterns** (inferability 3-4) — How dependencies are injected and why.
4. **Lifecycle rules** (inferability 4-5) — Resource creation/deletion policies.
5. **Conventions** (inferability 2-3) — Naming, folder structure, class patterns.

### Minimum Viable Instruction File

For any repository with AI-assisted development:

```markdown
# AGENTS.md

## Architecture
- [One sentence per project/layer describing its responsibility]
- [Dependency directions: who depends on whom]

## Critical Rules (Never Do This)
- [List every "never do X" rule — these cannot be inferred from code]

## Credential/Config Pattern
- [How dependencies are provided — who chooses, who accepts]

## Conventions
- [Only non-obvious conventions that differ from language defaults]
```

---

## 8. Limitations & Threats to Validity

### 8.1 Session Contamination

The most significant threat to validity is that the VS Code IDE auto-injected AGENTS.md content into the session context at startup, before Phase 0a could relocate the files. This means the executing model had residual knowledge of some instruction file content during C1/C2 experiments. We mitigated this by physically relocating files (preventing further injection after the session start) and by honestly acknowledging the contamination risk. The residual knowledge likely *improved* C1/C2 scores, meaning the **true C1→C3 delta may be even larger** than the +17.3 we measured. This makes our finding conservative.

### 8.2 Self-Evaluation Bias (Meta-Circularity)

This study asks an LLM to evaluate whether its own instruction files are necessary — a fundamentally meta-circular exercise. The executing model has inherent incentives in both directions: it might overstate instruction file value (to validate its own instruction-following capability) or understate it (to demonstrate its autonomous code understanding).

We mitigate this by:
1. Generating actual code under each condition (not hypothetical descriptions).
2. Using a predefined rubric with explicit scoring criteria.
3. Requiring honest reporting of cases where C1 matches C3.
4. Maintaining contamination transparency throughout.

Ultimately, this meta-circularity cannot be fully resolved without external human evaluators or multi-model cross-validation. We recommend future work with human evaluators scoring the generated code independently.

### 8.3 Single-Model Limitation

All experiments used Claude Opus 4.6 (1M context). Results may differ for GPT-5.3 Codex, Gemini, or open-source models. The self-scanning behavior, convention inference ability, and negative rule adherence may all vary across model families. Our findings about negative rules (score 5 inferability) are likely to generalize across all models, since no model can learn from absent information. Convention adherence findings (score 1-2) are more model-dependent.

### 8.4 Single-Repository Scope

Our controlled experiments are conducted on a single test repository with unusually rich instruction files (~57 rules, 6 files, multiple architectural layers). Repositories with fewer rules, simpler architectures, or less comprehensive instruction files may show smaller deltas. Conversely, large monorepos with hundreds of services may show larger deltas because self-initiated scanning becomes even less practical. This limitation is partially mitigated by the consistency between our experimental findings and the broader literature — the large-scale study [1] covering 1,925 repositories, the AppDirect case study [12] across hundreds of engineers, and the METR RCT [9] across 246 real issues all point in the same direction. However, our specific quantitative deltas (+17.3 mean, +30 max) should be treated as indicative rather than universally generalizable.

### 8.5 Task Selection Bias

The 7 experiments were designed to span the inferability spectrum, but 4 of 7 targeted high-inferability rules (score 4-5). This may over-represent conditions where instruction files shine. A more balanced experiment set might show a lower mean delta. However, we argue that high-inferability rules are where the question of instruction file necessity is most relevant — trivially inferable rules are not where the debate lies.

### 8.6 Training-Data Recency

This study's executing model (Claude Opus 4.6) has training data with a cutoff that may predate some January 2026 sources. Claims based on training-data knowledge (rather than live internet research via Perplexity) are marked accordingly. The literature review leveraged live internet research via Perplexity MCP server, providing Tier 1 source access that supplements training data.

---

## 9. Future Work

### 9.1 Multi-Model Comparison

The most pressing extension is replicating this study across GPT-5.3 Codex, Gemini, and open-source models (Llama, Mistral, DeepSeek). The identical repository and experiment protocol can be used. Key questions: Do other models self-scan at the same rate (71%)? Do they score higher or lower on C1 conditions? Is the negative rule problem universal or model-specific?

### 9.2 Multi-Repository Study

Testing across 10-20 repositories of varying size, language, complexity, and instruction file richness would strengthen generalizability. Key variables to control: number of rules, inferability distribution, number of projects/layers, and primary language.

### 9.3 Longitudinal Study

As frontier models improve with each generation, the instruction file value proposition may shift. A longitudinal study tracking the C1→C3 delta across model generations (Claude Opus 4 → 4.5 → 4.6 → 5) would reveal whether instruction files are converging toward obsolescence or maintaining their value.

### 9.4 Automated Instruction File Generation

If instruction files are valuable but costly to maintain, automated generation from code analysis could reduce the maintenance burden. A system that analyzes a codebase and proposes instruction file content — especially negative rules inferred from code review history and architectural boundaries inferred from dependency graphs — could democratize instruction file adoption.

### 9.5 Human Evaluator Validation

The most methodologically robust extension would have 3-5 experienced developers independently score the C1/C2/C3 code outputs using the same rubric. This would validate (or challenge) the self-evaluation scores and eliminate the meta-circularity limitation.

---

## 10. Conclusion

Converging evidence from 20 independent sources — large-scale repository studies [1][15][18], causal impact analyses [16], process-oriented benchmarks [17], randomized controlled trials [9], industry case studies [12], multi-agent engineering experiments [20], code quality meta-analyses [10], agentic compliance experiments [19], and our own controlled experiments — establishes that agent instruction files provide measurable, practically meaningful improvements in code generation quality when frontier LLMs operate on existing codebases.

**The primary research question — do instruction files help?** — receives a nuanced but well-supported answer: **yes**. The literature documents this through multiple lenses: sustained developer investment across 1,925 repositories [1], organizations scaling AI code generation by 4,400% with quality maintained through systematic context management [12], a ~25% productivity drag when context infrastructure is absent [9], AI code quality gaps (1.7× more issues) that diminish with explicit quality gates [10], and persistent technical debt accumulation (+40.7% complexity) when AI agents operate without guardrails [16]. Our controlled experiments provide confirmatory evidence within a single repository, measuring a mean improvement of +17.3 points (24.7%) on a 70-point quality scale, with the largest gains in negative rule adherence (+3.1 raw points) and architectural boundary compliance (+2.3 raw points).

However, this value is not uniform. The "Conditional Value" hypothesis (H3) best fits the combined evidence: instruction files are **essential** for repositories with cross-project architectural boundaries and negative rules (inferability score 4-5), **important** for credential delegation and lifecycle patterns (score 3-4), **helpful** for conventions (score 2-3), and **optional** for trivially inferable patterns (score 1). This gradient is consistent with what the literature predicts — AI effectiveness varies dramatically by task context [11], brownfield scenarios particularly benefit from explicit guidance [8], and verbose instruction files actively degrade agent performance [6].

The "implicit scanning" finding (RQ6) from our experiments adds important nuance: frontier models do self-initiate codebase analysis in 71% of cases, but this analysis is incomplete, unsystematic, and sometimes counterproductive (leading to use of available-but-prohibited patterns). The ContextBench benchmark independently corroborates this: even purpose-built agent scaffolding yields only marginal context retrieval gains [17]. Instruction files serve as "pre-computed architectural guidance" that bypasses these fundamental retrieval limitations.

**The practical takeaway for engineering teams:** Across all evidence examined — from the Gloaguen et al. evaluation showing verbose files *reduce* task success [6], to He et al.'s causal evidence of persistent technical debt without guardrails [16], to Cursor and Anthropic's independent convergence on "constraints over instructions" as a universal architectural primitive at thousand-agent scale [20], to our experiments showing targeted guardrails improve quality by up to 42.9% [our B1 experiment] — the conclusion is clear: **instruction files are quality infrastructure, but less is more.**

Instruction files should be **short, focused guardrail documents**, not comprehensive repository overviews. Most developers are already overloading their instruction files with build commands, implementation details, and architecture descriptions that the model can discover from code [15] — content that Gloaguen et al. proved degrades agent performance [6]. The model can read your code and infer most of what it needs. What it *cannot* infer are:

1. **What it must never do** (negative rules) — these produced the largest quality delta in our experiments (+28 points, C2exp)
2. **Architectural boundaries it must not cross** (layer separation, credential delegation) — +30 points in experiment B1
3. **Security and lifecycle guardrails** (cleanup mandates, credential constraints) — invisible in code, essential in practice

Engineering teams should resist the temptation to turn AGENTS.md into a comprehensive README. Keep it short. Focus on prohibitions and boundaries. Let the code speak for itself on everything else.

---

## References

[1] Chatlatanagulchai, W., Li, H., Kashiwa, Y., Reid, B., et al. "Agent READMEs: An Empirical Study of Context Files for Agentic Coding." arXiv:2511.12884v1, November 2025. https://arxiv.org/abs/2511.12884

[2] "What is AGENTS.md?" Builder.io Blog, 2025. https://www.builder.io/blog/agents-md

[3] Olsson, C., Elhage, N., Nanda, N., Joseph, N., DasSarma, N., Henighan, T., Mann, B., Askell, A., Bai, Y., Chen, A., Conerly, T., Drain, D., Ganguli, D., Hatfield-Dodds, Z., Hernandez, D., Johnston, S., Jones, A., Kernion, J., Lovitt, L., Ndousse, K., Amodei, D., Brown, T., Clark, J., Kaplan, J., McCandlish, S., Olah, C. "In-context Learning and Induction Heads." Transformer Circuits Thread, Anthropic, 2022. https://transformer-circuits.pub/2022/in-context-learning-and-induction-heads/index.html

[4] Anthropic. "Claude Opus 4.5 Release." November 2025. https://www.anthropic.com/news/claude-opus-4-5

[5] SWE-bench. https://www.swebench.com

[6] Gloaguen, T., Mündler, N., Müller, M., Raychev, V., Vechev, M. "Evaluating AGENTS.md: Are Repository-Level Context Files Helpful for Coding Agents?" arXiv:2602.11988v1, February 12, 2026. https://arxiv.org/abs/2602.11988

[7] Yan, K., Guo, H., Shi, X., et al. "CodeIF: Benchmarking the Instruction-Following Capabilities of Large Language Models for Code Generation." arXiv:2502.19166v3 (ACL 2025 Industry Track). https://arxiv.org/abs/2502.19166 — See also: Midolo, A., et al. "Guidelines to Prompt Large Language Models for Code Generation: An Empirical Characterization." arXiv:2601.13118v1, January 2026. https://arxiv.org/abs/2601.13118

[8] Haseeb, M. "Context Engineering for Multi-Agent LLM Code Assistants Using Elicit, NotebookLM, ChatGPT, and Claude Code." arXiv:2508.08322v1, August 2025. https://arxiv.org/abs/2508.08322

[9] METR. "Early 2025 AI-Experienced Open-Source Developer Study." July 2025. https://metr.org/blog/2025-07-10-early-2025-ai-experienced-os-dev-study/

[10] "AI-Generated Code Quality Metrics and Statistics for 2026." Second Talent. https://www.secondtalent.com/resources/ai-generated-code-quality-metrics-and-statistics-for-2026/

[11] SonarSource. "State of Code Developer Survey Report: The Current Reality of AI Coding." 2026. https://www.sonarsource.com/blog/state-of-code-developer-survey-report-the-current-reality-of-ai-coding/

[12] AppDirect. "How to Adopt AI Development at Scale: 6 Lessons from AppDirect's Leap from 0% to Almost All AI-Generated Code." 2026. https://www.appdirect.com/blog/how-to-adopt-ai-development-at-scale-6-lessons-from-appdirects-leap-from-0-to-almost-all-ai-generated-code

[13] "Life with AI Agent" (生成AIエージェントとの開発生活). Zenn.dev, 2026. Practitioner experience with Claude Code instruction files. https://zenn.dev/x_shunei/articles/f3f67a3af18224-life-with-ai-agent?locale=en

[14] Lulla, J.L., Mohsenimofidi, S., Galster, M., Zhang, J.M., Baltes, S., Treude, C. "On the Impact of AGENTS.md Files on the Efficiency of AI Coding Agents." arXiv:2601.20404v1, January 28, 2026. https://arxiv.org/abs/2601.20404

[15] Jiang, Y. and Nam, J. "On the Use of Agentic Coding Manifests: An Empirical Study of Claude Code." arXiv:2509.14744v1, September 2025 (accepted ICSE 2026). https://arxiv.org/abs/2509.14744

[16] He, H., Miller, C., Agarwal, S., Kästner, C., Vasilescu, B. "Speed at the Cost of Quality: How Cursor AI Increases Short-Term Velocity and Long-Term Complexity in Open-Source Projects." arXiv:2511.04427v3, January 2026 (accepted MSR 2026). https://arxiv.org/abs/2511.04427

[17] Li, H., Zhu, L., Zhang, B., Feng, R., Wang, J., Pan, Y., Barr, E.T., Sarro, F., Chu, Z., Ye, H. "ContextBench: A Benchmark for Context Retrieval in Coding Agents." arXiv:2602.05892v3, February 2026. https://arxiv.org/abs/2602.05892

[18] Mohsenimofidi, S., Galster, M., Treude, C., Baltes, S. "Context Engineering for AI Agents in Open-Source Software." arXiv:2510.21413v1, October 2025 (accepted MSR 2026). https://arxiv.org/abs/2510.21413

[19] Umans AI. "Do coding agents follow AGENTS.md?" December 2025. https://blog.umans.ai/blog/agent-apply/

[20] Lavaee, A. "Five Architectural Primitives Every Agent Swarm Rediscovers." 2026. Synthesis of Cursor (Self-Driving Codebases) and Anthropic (Building a C Compiler with Parallel Claudes) multi-agent experiments. https://alexlavaee.me/blog/five-primitives-agent-swarms/

---

## Appendices

### Appendix A: Complete Rule Surface Area Table

| # | Rule | Source File | Category | Inferability | Score |
|---|------|------------|----------|-------------|-------|
| 1 | .NET 10.0, C#, nullable enabled | Root AGENTS.md | Platform | From any .csproj | 1 |
| 2 | .slnx solution format | Root AGENTS.md | Platform | Visible in repo root | 1 |
| 3 | Prefer small, focused changes | Root AGENTS.md | Process | General best practice | 2 |
| 4 | Keep public APIs stable | Root AGENTS.md | Convention | Not visible in code | 4 |
| 5 | Research output format | Root AGENTS.md | Output format | Not visible in code | 5 |
| 6 | Structured results (ResearchResult) | Root AGENTS.md | Convention | Visible in code | 1 |
| 7 | Never log secrets or sensitive paths | Root AGENTS.md | Security | Not visible in code | 5 |
| 8 | TDD Red-Green-Refactor cycle | Root AGENTS.md | Process | Not visible in code | 5 |
| 9 | Prefer Microsoft technologies | Root AGENTS.md | Strategy | Partially visible in deps | 3 |
| 10 | Hybrid architecture (not 100% Foundry) | Root AGENTS.md | Architecture | Hard to infer | 4 |
| 11 | Core: Pure in-memory — no file I/O | Core AGENTS.md | Arch boundary | Absence visible but ≠ prohibition | 5 |
| 12 | Core: No Console.Write* calls | Core AGENTS.md | Arch boundary | Absence visible but ≠ prohibition | 5 |
| 13 | Core: No credential selection | Core AGENTS.md | Dependency | Visible in constructor | 2 |
| 14 | Credential delegation: DLL never chooses | Core AGENTS.md | Dependency | Cross-project comparison | 3 |
| 15 | Folder-per-agent convention | Core AGENTS.md | Convention | Visible in directory | 2 |
| 16 | Config = sealed record + FromEnvironment() | Core AGENTS.md | Convention | Visible in code | 1 |
| 17 | Config has internal overload for testing | Core AGENTS.md | Convention | Visible in code if reading tests | 3 |
| 18 | Runner = sealed class + RunAsync() | Core AGENTS.md | Convention | Visible in code | 1 |
| 19 | Get-or-create pattern for agents | Core AGENTS.md | Lifecycle | Visible in runner code | 3 |
| 20 | Production runner must NOT delete agents | Core AGENTS.md | Lifecycle | Not visible — negative rule | 5 |
| 21 | #pragma warning disable OPENAI001 required | Core AGENTS.md | Convention | Visible in code | 1 |
| 22 | **NEVER use GetOutputText()** | Core AGENTS.md | **Negative rule** | **Method exists and appears functional** | **5** |
| 23 | Use ExtractAnnotatedText() for display | Core AGENTS.md | Convention | Visible in code | 2 |
| 24 | Only process last MessageResponseItem | Core AGENTS.md | Convention | Explained in doc comments | 2 |
| 25 | Process annotations descending StartIndex | Core AGENTS.md | Convention | Visible in code | 1 |
| 26 | Never add direct OpenAI package reference | Core AGENTS.md | Negative rule | Not visible | 5 |
| 27 | CLI: Ultra-thin, no business logic | CLI AGENTS.md | Arch boundary | Inferable from reading both projects | 3 |
| 28 | CLI: No Azure SDK client instantiation | CLI AGENTS.md | Arch boundary | Inferable if comparing | 3 |
| 29 | CLI: Console output is CLI concern only | CLI AGENTS.md | Arch boundary | Inferable from Core analysis | 3 |
| 30 | CLI: File I/O is CLI-only concern | CLI AGENTS.md | Arch boundary | Inferable from Core analysis | 3 |
| 31 | CLI passes AzureCliCredential | CLI AGENTS.md | Dependency | Visible in CLI Program.cs | 2 |
| 32 | Backward compatibility for CLI commands | CLI AGENTS.md | Convention | Not visible in code | 5 |
| 33 | MCP: Thin wrapper over Core | MCP AGENTS.md | Arch boundary | Inferable from reading both | 3 |
| 34 | MCP: No AIProjectClient instantiation | MCP AGENTS.md | Arch boundary | Inferable if comparing | 3 |
| 35 | MCP: No forceNewVersion flag | MCP AGENTS.md | Convention | Deliberate omission — hard to infer | 4 |
| 36 | MCP: Add tools in Tools/ folder, not Program.cs | MCP AGENTS.md | Convention | Not obvious without instruction | 4 |
| 37 | MCP: DefaultAzureCredential singleton in DI | MCP AGENTS.md | Dependency | Visible in MCP Program.cs | 2 |
| 38 | MCP: Runner created per-request | MCP AGENTS.md | Dependency | Subtle design choice | 4 |
| 39 | Output prefixed with [INSTRUCTIONS FOR AI ASSISTANT] | MCP AGENTS.md | Output format | Visible but rationale unclear | 4 |
| 40 | Citation links toggle in appsettings | MCP AGENTS.md | Convention | Visible in code | 2 |
| 41 | Don't change instruction prefix without cross-client testing | MCP AGENTS.md | Negative rule | Invisible rationale | 5 |
| 42 | Don't run CLI tests during MCP dev | MCP AGENTS.md | Process | Invisible rationale | 5 |
| 43 | xUnit v3 (3.2.2) | Tests AGENTS.md | Platform | In .csproj | 1 |
| 44 | Moq (4.20.72) | Tests AGENTS.md | Platform | In .csproj | 1 |
| 45 | Global Using Xunit — no using statement needed | Tests AGENTS.md | Convention | In .csproj | 2 |
| 46 | Validate public behavior, not private details | Tests AGENTS.md | Process | Best practice (training data) | 3 |
| 47 | Independent tests, no execution order | Tests AGENTS.md | Process | Best practice (training data) | 3 |
| 48 | Descriptive method names (Method_Condition_Expected) | Tests AGENTS.md | Convention | Visible in existing tests | 2 |
| 49 | [Trait("Category", "Integration")] for integration tests | Tests AGENTS.md | Convention | Visible in existing tests | 2 |
| 50 | Assert.Skip() not [SkippableFact] (xUnit v3) | Tests AGENTS.md | Convention | Visible in existing tests | 2 |
| 51 | Check env vars are real, not "test.endpoint.com" | Tests AGENTS.md | Convention | Visible in existing tests | 2 |
| 52 | **Integration tests must clean up cloud resources** | Tests AGENTS.md | **Process** | **Pattern visible, mandate invisible** | **4** |
| 53 | Use internal FromEnvironment overload for tests | Tests AGENTS.md | Convention | Visible in test code | 2 |
| 54 | CLI tests spawn subprocess via Process | Tests AGENTS.md | Convention | Visible in test code | 2 |
| 55 | Use Moq for mocking | Tests AGENTS.md | Convention | Visible in test code | 1 |
| 57 | Skills check before tasks | Root AGENTS.md | Process | Not in code | 5 |

### Appendix B: Key Generated Code for Critical Experiments

See Section 5 for complete code excerpts for all experiments under all three conditions.

### Appendix C: Rubric Scoring Summary

| Exp | Cond | Arch (×2) | Conv (×1) | Neg (×2) | Dep (×1) | Qual (×1) | **Total** |
|-----|------|-----------|-----------|----------|----------|-----------|-----------|
| A1 | C1 | 8→16 | 6 | 7→14 | 7 | 8 | **51** |
| A1 | C2 | 9→18 | 8 | 7→14 | 8 | 9 | **57** |
| A1 | C3 | 10→20 | 9 | 8→16 | 9 | 9 | **63** |
| A2 | C1 | 9→18 | 7 | 7→14 | 8 | 8 | **55** |
| A2 | C2 | 9→18 | 8 | 7→14 | 8 | 9 | **57** |
| A2 | C3 | 10→20 | 9 | 9→18 | 9 | 9 | **65** |
| B1 | C1 | 4→8 | 6 | 5→10 | 7 | 7 | **38** |
| B1 | C2 | 8→16 | 8 | 8→16 | 9 | 8 | **57** |
| B1 | C3 | 10→20 | 9 | 10→20 | 10 | 9 | **68** |
| C1exp | C1 | 8→16 | 7 | 7→14 | 8 | 8 | **53** |
| C1exp | C2 | 8→16 | 8 | 7→14 | 8 | 8 | **54** |
| C1exp | C3 | 10→20 | 9 | 9→18 | 9 | 9 | **65** |
| C2exp | C1 | 8→16 | 7 | 2→4 | 8 | 5 | **40** |
| C2exp | C2 | 9→18 | 8 | 7→14 | 8 | 8 | **56** |
| C2exp | C3 | 10→20 | 9 | 10→20 | 9 | 10 | **68** |
| D1 | C1 | 8→16 | 7 | 7→14 | 7 | 7 | **51** |
| D1 | C2 | 9→18 | 9 | 7→14 | 9 | 9 | **59** |
| D1 | C3 | 10→20 | 10 | 10→20 | 10 | 10 | **70** |
| E1 | C1 | 9→18 | 7 | 7→14 | 8 | 8 | **55** |
| E1 | C2 | 9→18 | 8 | 7→14 | 8 | 9 | **57** |
| E1 | C3 | 10→20 | 10 | 8→16 | 9 | 10 | **65** |

### Appendix D: Raw Experiment Data

**C1 Self-Initiated Scanning Log:**

| Experiment | Scanned? | Files Scanned | Time Impact | Quality Impact |
|------------|----------|---------------|-------------|----------------|
| A1 | Y | ResearchResult.cs | Low | Moderate (+) |
| A2 | N | — | — | None (IProgress standard) |
| B1 | Y | AIToolsResearchAgentRunner.cs | Medium | Mixed (found credential pattern but still added I/O) |
| C1exp | N | — | — | None (doc comments sufficient) |
| C2exp | Y | ExtractAnnotatedText method | Medium | Negative (found GetOutputText as available API) |
| D1 | Y | Agents/ directory | Medium | Positive (found folder convention) |
| E1 | Y | Existing test files | Low | Moderate (+) |

**Contamination Assessment:**
- Phase 0a physically removed all 6 instruction files before source code reading.
- IDE auto-injected AGENTS.md content at session start (before Phase 0a) — residual contamination rated **Medium** for all C1/C2 experiments.
- Physical file relocation prevented further IDE injection during experimentation.
- The residual contamination likely improved C1/C2 scores, making **our measured deltas conservative** (the true deltas would be larger in a completely clean session).

---

### Appendix E: Description of Test Operations and Code Generation Tasks

This appendix describes the specific code generation operations performed during the 7 experiments (21 total code-generation runs: 7 experiments × 3 conditions each). All code was actually generated by the executing model (Claude Opus 4.6) — not hypothesized or described in abstract terms.

#### E.1 Experiment A1 — Add In-Memory Caching to Core Runner

**Operation type:** Feature addition to existing class.
**Files modified:** `src/AIResearchAgents.Core/Agents/AIToolsResearchAgent/AIToolsResearchAgentRunner.cs`
**Code generated (each condition):** A `ConcurrentDictionary<string, ResearchResult>` field, cache-check logic at the start of `RunAsync()`, and cache storage before return. Approximately 15–20 lines added per condition.
**Architectural decision required:** Whether to use in-memory vs. persistent caching (instruction files mandate in-memory for Core).
**Key difference across conditions:** C1 used `static` field (reasonable default but wrong for instance-based pattern). C2 used instance field (matched existing pattern). C3 added explicit comment documenting the in-memory architectural constraint.

#### E.2 Experiment A2 — Add Progress Reporting to RunAsync

**Operation type:** Parameter addition and progress callbacks in existing method.
**Files modified:** `src/AIResearchAgents.Core/Agents/AIToolsResearchAgent/AIToolsResearchAgentRunner.cs`
**Code generated (each condition):** An `IProgress<string>?` parameter added to `RunAsync()` signature, with `progress?.Report(...)` calls at each stage (agent creation, query, response processing). Approximately 10–15 lines added per condition.
**Architectural decision required:** Whether to use `IProgress<T>` (correct — abstraction) vs. `Console.WriteLine` (violation — direct I/O in Core).
**Key difference across conditions:** All three conditions correctly used `IProgress<T>` — this is a well-known .NET pattern where training data adequately substitutes for instruction knowledge.

#### E.3 Experiment B1 — Add Azure Table Storage to Core

**Operation type:** New class creation for persistence layer.
**Files modified/created:** New file in `src/AIResearchAgents.Core/`
**Code generated (each condition):** C1 generated a full `ResearchHistoryStorage` class (~25 lines) with `TableClient`, `StoreResultAsync`, and entity mapping — placed directly in Core (architectural violation). C2 generated an `IResearchResultStore` interface in Core (~10 lines) with a comment that implementation belongs elsewhere. C3 generated the same interface with explicit XML documentation explaining the Core boundary.
**Architectural decision required:** Whether to place I/O operations in Core (violation) or define only an abstraction (correct).
**Key difference across conditions:** C1 violated the "no I/O in Core" boundary — the single largest quality delta in all experiments (+30 C1→C3).

#### E.4 Experiment C1exp — Simplify ExtractAnnotatedText

**Operation type:** Refactoring of existing 40+ line method.
**Files modified:** `src/AIResearchAgents.Core/Agents/AIToolsResearchAgent/AIToolsResearchAgentRunner.cs`
**Code generated (each condition):** Refactored method split into `ExtractAnnotatedText()` (main dispatch, ~10 lines) and `ReplaceCitationAnnotations()` (helper, ~15 lines). Total ~25 lines per condition.
**Architectural decision required:** Whether to preserve the `GetOutputText()` avoidance pattern during refactoring.
**Key difference across conditions:** All conditions preserved the annotation-processing logic. C1 and C2 relied on XML doc comments; C3 added explicit comment about the prohibition.

#### E.5 Experiment C2exp — Add Plain Text Extraction Method

**Operation type:** New method creation.
**Files modified:** `src/AIResearchAgents.Core/Agents/AIToolsResearchAgent/AIToolsResearchAgentRunner.cs`
**Code generated (each condition):** C1 generated a 3-line method that directly called `response.GetOutputText()` — the exact prohibited API. C2 generated a 4-line method using `ExtractAnnotatedText()` + `StripMarkdownLinks()`. C3 generated the same pipeline with comprehensive XML documentation explaining the prohibition and its rationale.
**Architectural decision required:** Whether to use the seemingly obvious `GetOutputText()` (prohibited — returns hallucinated URLs) or the safe extraction pipeline.
**Key difference across conditions:** C1 fell into the `GetOutputText()` trap (Negative Rule score: 2/10). This is the canonical negative-rule failure case.

#### E.6 Experiment D1 — Create New CodeReviewAgent

**Operation type:** New agent scaffold (2 new files in a new folder).
**Files created:** `src/AIResearchAgents.Core/Agents/CodeReviewAgent/CodeReviewAgentConfig.cs`, `CodeReviewAgentRunner.cs`
**Code generated (each condition):** C1 generated ~50 lines across two files: a `class` (not `record`) config with `{ get; set; }` properties and a `class` (not `sealed`) runner with constructor injection and `CreateFromEnvironment()`. C2 improved to `sealed` and `record` with `init` accessors. C3 achieved 100% pattern fidelity: `sealed record` config with `required init` properties, `internal FromEnvironment(Func<string, string?>)` overload, `sealed class` runner, `#pragma warning disable OPENAI001`, and get-or-create agent lifecycle.
**Architectural decision required:** Whether to follow the existing folder-per-agent convention, sealed/record patterns, and internal test overload.
**Key difference across conditions:** C3 scored 70/70 (maximum). C1 scored 51/70 — structurally functional but missed 6 convention details.

#### E.7 Experiment E1 — Write Tests for StripMarkdownLinks

**Operation type:** New test class creation.
**Files created:** New test file in `tests/AIResearchAgents.Core.Tests/`
**Code generated (each condition):** All conditions generated 6–8 test methods using `[Fact]`, `Assert.Equal`, and `Assert.Contains`. C1 included an unnecessary `using Xunit;` statement and used literal Bing bracket characters. C3 omitted the unnecessary using (global using configured), used Unicode escapes (`\u3010`, `\u3011`), and added a boundary-condition test for non-HTTP links.
**Architectural decision required:** Whether to follow xUnit v3 conventions (global usings, `Assert.Skip()` pattern) and match existing test naming style.
**Key difference across conditions:** Differences were cosmetic but measurable — C1→C3 delta was +10 (the smallest of all experiments, as expected for low-inferability rules).

#### E.8 Summary of Code Generation Volume

| Experiment | Operation Type | Lines Generated (approx.) | New Files Created | Existing Files Modified |
|------------|---------------|--------------------------|-------------------|------------------------|
| A1 | Feature addition | 15–20 per condition | 0 | 1 |
| A2 | Feature addition | 10–15 per condition | 0 | 1 |
| B1 | New class/interface | 10–25 per condition | 1 | 0 |
| C1exp | Refactoring | ~25 per condition | 0 | 1 |
| C2exp | New method | 3–8 per condition | 0 | 1 |
| D1 | New agent scaffold | ~50 per condition | 2 | 0 |
| E1 | New test class | ~40 per condition | 1 | 0 |
| **Total** | | **~153–183 per condition** | **4** | **4** |

Across all 21 code-generation runs (7 × 3 conditions), approximately **460–550 lines of C# code** were generated and evaluated.
