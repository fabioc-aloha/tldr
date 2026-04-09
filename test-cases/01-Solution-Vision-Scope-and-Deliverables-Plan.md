# 1ES AI-First Dev Starter Pack
## Solution Vision, Scope & Deliverables Plan

**Version:** 1.1  
**Creation Date:** February 3, 2026
**Updated:** February 20, 2026  
**Status:** `[DRAFT]`  
**Authors:** 1ES AI-First Dev Team

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Vision & Strategic Goals](#2-vision--strategic-goals)
3. [Scope Definition](#3-scope-definition)
4. [Target Personas](#4-target-personas)
5. [Supported Environments](#5-supported-environments)
6. [User Scenarios](#6-user-scenarios)
7. [Features & Capabilities Overview](#7-features--capabilities-overview)
8. [Release Plan & Milestones](#8-release-plan--milestones)
9. [Success Metrics & KPIs](#9-success-metrics--kpis)
10. [Risks & Mitigations](#10-risks--mitigations)
11. [Dependencies & Assumptions](#11-dependencies--assumptions)
12. [Team & Collaboration Model](#12-team--collaboration-model)
13. [Open Questions](#13-open-questions)
14. [Appendix: Key References](#appendix-key-references)

---

## 1. Executive Summary

The **1ES AI-First Dev Starter Pack** is a centrally maintained initiative to make AI agents effective in Microsoft production codebases. It provides high-quality context, instructions, and tooling that works consistently across supported environments—giving developers a working AI-assisted environment **configured once and maintained automatically** without carrying the setup or maintenance burden themselves.

**Core Problem:** GitHub Copilot is not production-ready out-of-the-box for Microsoft codebases. Teams face workflow ambiguity, stateless context between sessions, trust deficits, and extensive manual configuration requirements.

**Solution:** A standardized, extensible starter pack that provides:
- **Agent Instructions** — Repository-specific rules and patterns (AGENTS.md)
- **MCP Servers** — Connections to external systems (GitHub, ADO, Kusto)
- **Custom Agents & Skills** — Pre-built specialized workflows

**Evidence:** Teams using structured AI workflows report **3.8x PR velocity improvement** and "PR-ready on first attempt" code generation (1JS internal study, Azure NW-SDN teams).

---

## 2. Vision & Strategic Goals

### Vision Statement

> *Make AI agents effective in production codebases by providing high-quality context, instructions, and tooling that works consistently across supported environments. Teams get a working AI-assisted environment configured once and maintained automatically—without carrying the setup or maintenance burden themselves.*

### Experience Themes

| Theme | Description |
|-------|-------------|
| **Managed Environments** | Configured once, maintained automatically what's generic. Manual customization by the team for what's specific for them. |
| **Contextual AI** | AI that knows your codebase and configures instructions and tools based on it. |
| **Standardized Tooling** | Right tools and guardrails, ready to use by Microsoft engineering teams. |

### Strategic Goals

| Goal | Description | Target |
|------|-------------|--------|
| **G1: Day-One Productivity** | Developers get working AI context immediately upon repo access | All 1ES teams enabled |
| **G2: Layered Customization** | 1ES provides baseline; orgs/teams extend without breaking foundation | 3-tier config model |
| **G3: Measurable Impact** | Demonstrate quantifiable improvements in dev productivity | 2.5x PR velocity |
| **G4: Broad Adoption** | Expand beyond 1ES to E+D, CoreAI, C+AI organizations | 1+ repo per major org |

### Principles

- **Platform-agnostic** — All scenarios work across supported agentic environments (VSCode, GHCP CLI, Agency)
- **Layered customization** — 1ES provides quality baseline; organizations extend for their context
- **Don't duplicate Agency** — Leverage Agency's existing capabilities (MCP servers, custom agents, Entra auth)
- **Maintainable & versioned** — Not a bootstrap; must support updates of generic/reusable assets.

---

## 3. Scope Definition

### In Scope

**Inner-loop development activities involving a repository:**

| Area | Included Activities |
|------|---------------------|
| **Environment Setup** | AI-assisted dev environments setup and maintenance |
| **Code Work** | Writing, reading, understanding, debugging code |
| **Documentation** | Specs, architecture decisions, technical docs |
| **Onboarding** | Navigating and understanding unfamiliar codebases |
| **Quality** | Code review assistance, reducing technical debt |
| **Context Engineering** | Several levels-specific AI instructions (repo, nested/scoped per folder/projects) and memory persistence across agent's chat sessions |

### Out of Scope (v1)

| Exclusion | Rationale |
|-----------|-----------|
| CI/CD pipelines and release operations | Post-commit workflow; different tooling needs |
| Production operations (monitoring, incident response) | SRE domain; covered by other initiatives |
| Post-merge activities | Outside inner-loop focus |
| Visual Studio support (TBD) | AGENTS.md not yet supported in VS (roadmap item for release on late Q3 or Q4, confirmed by Andy Sterland) - Fallback would be to also support GHCP instruction files by the starter pack. |
| Generic tool guidance | Covered by GHCP docs, Anthropic docs, Agency docs, etc. |

---

## 4. Target Personas

### Primary user Personas

> *"Developers" includes anyone who works in repositories to build products — software engineers, dev leads, architects, designers contributing prototypes and PMs creating code-POCs from specs.*

| Priority | Persona | Description | Primary Focus | Key Environments |
|----------|---------|-------------|---------------|------------------|
| **P1** | **Developers** | Software engineers, dev leads, architects building products | Code generation, debugging, PR velocity, code review | GHCP CLI (Agency), VSCode + Copilot |
| **P1** | **PMs** | PMs who create specs in repos alongside code | Spec writing, documentation, requirements, unified/atomic specs+code evolution | VSCode + Copilot |

### Operational Personas

| Persona | Description | Role |
|---------|-------------|------|
| **1ES Engineers** | Engineers who maintain the central 1ES agentic development baseline (this repo: '1ES AI-First Dev starter Pack') | Ensure AI configuration stays current, works across supported environments, provides foundation orgs can extend |
| **Org Engineers** | Engineers in organization-level Engineering Systems teams (i.e. C+AI, E+D, CoreAI, etc.) | Customize and maintain agentic tooling for their own organization |

---

## 5. Supported Environments

### Primary Environments (P0)

| Environment | Availability | Starter Pack Support |
|-------------|--------------|---------------------|
| **VSCode + GitHub Copilot** | All Microsoft | Full: AGENTS.md, MCP, agents, skills, etc. |
| **GitHub Copilot CLI** (Agency preferred) | All Microsoft | Full: AGENTS.md, MCP, agents, skills, etc. |
| **Claude Code** (E+D only - Agency preferred) | E+D pilot | Partial/Investigate: requires CLAUDE.md symlink/import |

> **Note:** Agency is the preferred CLI wrapper for Microsoft engineers due to auto-configured MCPs, Entra auth, and internal systems access. Agency for VSCode is in Agency's roadmap, though.

### Environment Selection Guidance

| Use Case | Recommended Environment |
|----------|------------------------|
| Daily IDE-based coding | VSCode + GitHub Copilot |
| High-productivity parallel tasks workflows | GHCP CLI (via Agency) |
| CI/CD automation integration | GHCP CLI (programmatic mode) |
| E+D teams with Claude preference | Claude Code (via `Agency on Claude`) |

---

## 6. User Scenarios

> *Scenarios define the experiences we want to enable. Each describes a desired outcome from the user's perspective. The "Deliverable In" column indicates which phase delivers the required features.*

### Day-One AI Context

*These scenarios enable developers to get productive AI assistance immediately when accessing a repo, with AI that understands their codebase context and remembers work across sessions.*

#### Scenarios for User-Personas

| ID | Scenario | Persona | Deliverable In | Requires |
|----|----------|---------|----------------|----------|
| S1a | **Apply 1ES Default AI Configuration (Manual):** As an Org Engineer, I want to manually apply the 1ES default AI configuration to my organization's repos so developers get a working AI-assisted environment. | Dev Lead, Repo Owner, Org Engineer/Developer | Phase 1 (Early Mar) | F1 |
| S1b | **Apply 1ES Default AI Configuration (Automated):** As an Org Engineer, I want to run a bootstrap script that automatically applies the 1ES default AI configuration, extracting repo-specific patterns. | Dev Lead, Repo Owner, Org Engineer/Developer | Phase 2 (Early Apr) | F6 |
| S2 | **Extend AI with Repo-Specific Context:** As a repo owner or dev lead, I want to add my repo's specific patterns and workflows so AI understands my codebase conventions. | Dev Lead, Repo Owner, Org Engineer/Developer | Phase 1 (Early Mar) | F1, F4, F5 |
| S3 | **Keep Repo-Specific AI Context Current:** As a repo owner or dev lead, I want AI to detect stale AI context and prompt for updates so instructions stay accurate. | Dev Lead, Repo Owner | Phase 3 (May+) | F15 |
| S4 | **Validate Org-Level AI Instructions:** As a repo owner or dev lead, I want AI to validate org-level custom instructions so they don't conflict with the 1ES baseline. | Dev Lead, Repo Owner | Phase 3 (May+) | F16, F17 |
| S5a | **Focus AI on My Domain in a Monorepo (Manual):** As an Org Engineer/Developer in a large monorepo, I want to use nested AGENTS.md files to scope AI to my domain so it doesn't get confused by unrelated code. | Org Engineer, Dev Lead | Phase 1 (Early Mar) | F1 |
| S5b | **Focus AI on My Domain in a Monorepo (Auto-Scoping):** As an Org Engineer/Developer in a large monorepo, I want AI to automatically scope itself to my domain based on context. | Org Engineer, Dev Lead | Phase 2 (Early Apr) | F7 |
| S6a | **Search Code Beyond My Working Context (MCP):** As an Org Engineer/Developer, I want AI to use connected tools (GitHub, ADO) to explore beyond my current code scope. | Org Engineer/Developer | Phase 1 (Early Mar) | F3 |
| S6b | **Search Code Beyond My Working Context (Live Search):** As an Org Engineer/Developer, I want AI to dynamically search and discover code patterns across the entire codebase during active sessions. | Org Engineer/Developer | Phase 3 (May+) | F10 |
| S7 | **Remember My Work Across Sessions:** As an Org Engineer/Developer, I want AI memory to persist so I don't have to re-explain context when starting a new chat session. | Org Engineer/Developer | Phase 1 (Early Mar) | F2 |
| S8 | **Spec-Driven Development:** As a PM and developer, I want to use AI assistance for writing specs (functional and technical) in the same repo where code lives. Additionally, I want to use AI assistance for syncing the specs when code derives, due to solid reasons, from the original specs, so specs and implementation stay as atomic units. | PM and Org engineer/developer | Phase 1 (Early Mar) | F1, F4, F5 |

#### Scenarios for 1ES Maintenance Personas

| ID | Scenario | Persona | Deliverable In | Requires |
|----|----------|---------|----------------|----------|
| S9a | **Maintain 1ES AI Configuration Baseline (Core Assets):** As a 1ES engineer, I want to maintain and update the standard AI configuration baseline so it stays current across supported environments. | 1ES Engineer | Phase 1 (Early Mar) | F1–F5 |
| S9b | **Maintain 1ES AI Configuration Baseline (Versioned Updates):** As a 1ES engineer, I want to push iterative updates to repos that have adopted the starter pack without requiring manual intervention. | 1ES Engineer | Phase 2 (Early Apr) | F8 |
| S9c | **Track Starter Pack Adoption Across Orgs:** As a 1ES engineer, I want to see adoption dashboards showing which repos have AI instructions so I can track rollout progress and identify gaps. | 1ES Engineer | Phase 3 (May+) | F14 |

### External System Integration

*These scenarios extend AI's reach beyond local code to external systems like work items, pipelines, and cross-team dependencies.*

| ID | Scenario | Persona | Deliverable In | Requires |
|----|----------|---------|----------------|----------|
| S10a | **Start Work from My Assigned Items (Basic MCP):** As an Org Engineer/Developer, I want AI to help me view and understand assigned work items (ADO) or issues (GH) using MCP connections. | Org Engineer/Developer | Phase 1 (Early Mar) | F3 |
| S10b | **Start Work from My Assigned Items (Full Integration):** As an Org Engineer/Developer, I want AI to help me begin work directly from assigned items, auto-creating branches and linking PRs. | Org Engineer/Developer | Phase 3 (May+) | F12 |
| S11a | **Debug Build or Pipeline Failures (Basic MCP):** As an Org Engineer/Developer, I want AI to access build logs and pipeline status via MCP to help understand failures. | Org Engineer/Developer | Phase 1 (Early Mar) | F3 |
| S11b | **Debug Build or Pipeline Failures (Correlation):** As an Org Engineer/Developer, I want AI to correlate failures with recent changes and known issues so I can fix problems faster. | Org Engineer/Developer | Phase 3 (May+) | F13 |
| S12 | **Get AI Help with Dependencies from Other Teams:** As a developer, I want AI to have access to context about external dependencies so it can help me understand APIs and integration points. | Org Engineer/Developer | Phase 3 (May+) | F11 |
| S13 | **Maximize Productivity with Parallel CLI AI Sessions:** As a developer, I want guidance on running multiple AI tasks in parallel using CLI so I can implement features, run tests, and review code simultaneously. | Org Engineer/Developer | Phase 2 (Early Apr) | F9 |

---

## 7. Features & Capabilities Overview

> *Each feature includes WHAT (the deliverable) and WHY (the value it provides).*

### Core Assets (P0 — Work: Feb | Release: Early March)

#### F1: AGENTS.md Instructions
| Attribute | Details |
|-----------|--------|
| **What** | Hierarchical repo-specific rules, patterns, and build commands using AGENTS.md format |
| **Why** | Provides AI agents with project-specific context so they generate code that follows team conventions and understands the codebase structure |
| **Enables** | S1a, S2, S5a, S8, S9a |
| **Investigate** | Should we provide GHCP `copilot-instructions.md` files as fallback for tools that don't support AGENTS.md (e.g., Visual Studio)? Evaluate dual-format maintenance burden vs. broader tool coverage. Andy Sterland mentioned on Feb/3/2026 that "At the moment we're prototyping swapping out our 'agent' mode infra with the Copilot SDK. With the hope of being able to pickup things like agents.md support from them. Roadmap item for release on late Q3 or Q4, confirmed by Andy Sterland".  |
| **Status** | `[IN PROGRESS]` `[EXISTING PRELIMINARY VERSION TO REVIEW]` |

#### F3: MCP Server Configuration
| Attribute | Details |
|-----------|--------|
| **What** | Pre-configured access to GitHub, ADO, Kusto, Bluebird via 1ES MCP Registry |
| **Why** | Extends AI's reach beyond local code to external systems (work items, telemetry, pipelines), enabling richer context and automation |
| **Enables** | S6a, S10a, S11a |
| **Investigate** | Evaluate which MCP servers should be "1ES baseline" (included by default) vs "add-on" (recommended but opt-in). Assess Agency plugin marketplace coverage vs need for custom MCP servers. |
| **Status** | `[IN PROGRESS]` `[EXISTING PRELIMINARY VERSION TO REVIEW]` |

**MCP Servers (via 1ES Registry):**
- GitHub — Repository context, PR operations, code search
- Azure DevOps — Work items, pipelines, builds
- Kusto — Log and telemetry queries
- Bluebird — Microsoft-specific tooling
- *Additional servers available via Agency plugin marketplace*

#### F4: Custom Agents
| Attribute | Details |
|-----------|--------|
| **What** | Specialized chat modes (coder, code-reviewer, trio-dev-agents, PM, etc.) as `.agent.md` files |
| **Why** | Provides consistent, role-specific AI behavior codified in files that can be shared across teams |
| **Enables** | S2, S8, S9a |
| **Investigate** | Evaluate agent discoverability and activation UX across VSCode and CLI, if that's possible. Should agents auto-activate based on context, or only on explicit selection? Benchmark agent effectiveness metrics. |
| **Status** | `[IN PROGRESS]` `[EXISTING PRELIMINARY VERSION TO REVIEW]` |

#### F5: Agent Skills
| Attribute | Details |
|-----------|--------|
| **What** | On-demand task-specific and domain-specific capabilities loaded when needed (agentic-eval, coding-impl-azure-aks, etc.) |
| **Why** | Keeps context efficient by loading specialized instructions only when relevant, avoiding context window bloat |
| **Enables** | S2, S8, S9a |
| **Investigate** | Assess skill runtime behavior differences across VSCode, CLI, and Coding Agent. Evaluate whether some skills are better delivered as MCP servers (see R4). |
| **Status** | `[IN PROGRESS]` `[EXISTING PRELIMINARY VERSION TO REVIEW]` |

> **Important:** Don't duplicate Agency capabilities. Agency already provides MCP servers, custom agents, and Entra auth. The starter pack focuses on repo-level configuration that complements Agency.

---

### Research & Prototypes (P0 — Work: Feb | Release: Late Feb)

> *These investigations run in parallel with core asset development to prioritize additional features if validated. Targets are completion dates, not start dates.*

#### R1: Dynamic 1ES Content Delivery
| Attribute | Details |
|-----------|--------|
| **What** | Investigate options for delivering 1ES content dynamically via main AI agent launch points (Agency & VSCode) |
| **Why** | Ability to update 1ES Default AI configuration frequently on user machines; greater agility & control vs pushing updates to thousands of customer repos |
| **Options to Explore** | Agency plugin distribution, VSCode extension settings, remote instruction fetch |
| **Enables** | S9 (Maintain baseline) |
| **Target** | Findings by end of February |

#### R2: Config Injection into Repo Instructions
| Attribute | Details |
|-----------|--------|
| **What** | Investigate options for injecting 1ES Default AI configuration into repo instructions without modifying repo files |
| **Why** | Ensures agents can discover 1ES config even if not checked into repo; enables central updates without PRs to every repo |
| **Options to Explore** | Directory junctions, VSCode workspace configs, include/import mechanisms, insights from repo analysis into agent instructions |
| **Enables** | S1 (Apply config), S9 (Maintain baseline) |
| **Feeds Into** | **P0-Proto (Bootstrap Prototype)** for early testing → **F6 (Bootstrap Script)** for full feature |
| **Target** | Prototype by end of February |

#### R3: Repo-Specific Project Context Extraction
| Attribute | Details |
|-----------|--------|
| **What** | Prototype tooling that scans an existing codebase (including large monorepos) to automatically extract project context: coding patterns, architectural rules, build commands, dependencies, conventions, policies, and restrictions. This extracted context is then intelligently distributed across the appropriate nested/scoped AGENTS.md files based on directory structure and code ownership. |
| **Why** | Writing AGENTS.md from scratch is time-consuming and error-prone. Teams shouldn't have to manually document what already exists in their code. Automated extraction reduces onboarding friction and ensures instructions reflect actual codebase reality. For monorepos, context must be scoped correctly—global patterns go to root AGENTS.md, domain-specific patterns go to nested AGENTS.md in subdirectories. |
| **Scope** | Auto-extract: build/test commands, dependency patterns, code style conventions, API patterns, error handling approaches, architectural boundaries (CODEOWNERS, directory structure), technology constraints |
| **Enables** | S2 (Extend with repo context), S3 (Keep context current) |
| **Feeds Into** | **P0-Proto (Bootstrap Prototype)** for basic extraction → **F6 (Bootstrap Script)** for intelligent pattern distribution across nested AGENTS.md files |
| **Target** | Prototype complete by **end of February** (critical dependency for P0-Proto and F6 development in March) |

#### R4: Auto-configuring Default Skills
| Attribute | Details |
|-----------|--------|
| **What** | Prototype auto-configuring the right default set of skills for 1ES Default AI config |
| **Why** | Ensure AI can search repo & answer telemetry questions easily without manual skill setup; evaluate best approach |
| **Key Decision** | Evaluate MCP servers vs SKILLS.md wrappers — which provides better UX and capability coverage? |
| **Enables** | S6 (Search beyond context), S2 (Repo-specific patterns) |
| **Feeds Into** | **F5 (Skills)** for initial skill definitions; **F8 (Versioning & Iterative Updates)** for skill versioning/distribution; **F10 (Live Code Search)** for implementation approach (MCP vs. skill wrapper) |
| **Target** | Evaluation complete by end of February |

#### R5: Behavioral Instructions Testing
| Attribute | Details |
|-----------|--------|
| **What** | Test AI agent default *behavioral* instructions effectiveness |
| **Why** | Gather & test effectiveness of behavioral instructions from multiple sources; ensure instructions actually improve AI output quality |
| **Approach** | Collect behavioral instructions from 1JS, ESChat, community; A/B test against baseline; measure PR quality, code review feedback |
| **Enables** | S9 (Maintain baseline), S4 (Validate instructions) |
| **Feeds Into** | **F16 (Instruction Effectiveness Testing)** in P2/Phase 3 (May+) — lower priority research that informs later tooling |
| **Target** | Initial test results by end of February |

#### R6: CLI Parallel Workflows Patterns
| Attribute | Details |
|-----------|--------|
| **What** | Investigate and document effective patterns for running multiple AI tasks in parallel using GHCP CLI (with Agency preferred) |
| **Why** | CLI enables higher productivity through parallel task execution—something not easily achieved in VSCode's single-session model. Need to validate which patterns actually improve velocity before documenting as guidance. |
| **Options to Explore** | Plan Mode usage patterns, `/review` command workflows, background task delegation (`&`), memory/context coordination across parallel sessions, shell scripts or aliases for common parallel patterns, Agency-specific enhancements (auto-MCPs, Entra auth) |
| **Feeds Into** | **F9 (CLI Parallel Workflows Guide)** |
| **Target** | Patterns validated by mid-March |

---

### Prototype Deliverables (P0 — Work: Feb–Early Mar | Release: Early Mar)

> *These prototypes emerge from research findings and provide early testable versions of P1 features. Target is M1.3S (stretch) milestone.*

#### P0-Proto: Bootstrap Script Prototype
| Attribute | Details |
|-----------|--------|
| **What** | Basic working prototype of the repo bootstrap script — minimal viable version that applies starter pack templates and performs initial context extraction. Not yet "intelligent" but functional enough for early testing and feedback. |
| **Why** | Enables 3+ weeks of real-world testing (Mar 7 → Mar 28) before full F6 release. Early feedback shapes the full implementation and validates R2+R3 findings in practice. |
| **Scope** | Copy core templates (AGENTS.md), basic build command detection, simple directory structure analysis. Does NOT yet include: CODEOWNERS integration, advanced pattern extraction, nested AGENTS.md distribution. |
| **Foundation For** | S1b (full enablement via F6) |
| **Depends On** | R2 (config injection) + R3 (context extraction) findings |
| **Evolves Into** | **F6 (Repo Bootstrap Engine/Script)** — full feature in P1 with intelligent onboarding |
| **Target** | **M1.3S (Stretch): Mar 7** |
| **Status** | `[NOT STARTED]` `[DEPENDS ON R2, R3]` |

---

### P1 Capabilities (Work: Feb–Mar | Release: Early Apr)

> *Work begins in February; release target is early April.*

#### F6: Repo Bootstrap Engine/Script (Intelligent Onboarding)
| Attribute | Details |
|-----------|--------|
| **What** | Production-ready (v1.0 to keep evolving/improving) automated engine/script to apply starter pack to any repo (release target: early April). The "intelligent" part: scans the existing codebase to extract patterns, conventions, and rules (using R3 findings), then generates appropriate AGENTS.md files—distributing context intelligently across root and nested/scoped locations based on directory structure and code ownership. Not just copying templates, but creating customized instructions that reflect the actual codebase. |
| **Why** | Enables rapid onboarding of repos to AI-first workflow without manual copy-paste or manual documentation. For large monorepos, automatically scopes instructions to the right level (global vs. domain-specific) so AI doesn't get overwhelmed or confused. |
| **Enables** | S1b |
| **Milestones** | **P0-Proto (M1.3S Stretch):** Basic prototype by Mar 7 for testing → **March testing:** Validate with 1ES teams, gather feedback, iterate → **M1.4:** Polished, production-ready script by Mar 28 → **M2.1:** Broad deployment across all 1ES PROD repos Apr 11 |
| **Evolves From** | **P0-Proto (Bootstrap Script Prototype)** — basic version tested and validated during March; F6 is the polished, production-ready evolution |
| **Investigate** | Evaluate script delivery mechanism: standalone CLI, npm package, or integrated into Agency. Should script detect existing AI configs and offer merge vs. replace? How to handle CODEOWNERS for intelligent scoping? Can we use AI-assisted analysis (LLM) to summarize extracted patterns? |
| **Depends On** | R2 (config injection), R3 (context extraction) findings |
| **Status** | `[NOT STARTED]` `[DEPENDS ON R2, R3]` |

**Key Deliverable (Jacek — early April):** *"A script that takes a repo that has not seen any this material to a repo that is reasonably set up for an AI-first workflow."*

#### F7: Enterprise Monorepo Scoping (Advanced)
| Attribute | Details |
|-----------|--------|
| **What** | Advanced AI scoping support for Microsoft's largest enterprise monorepos with deep hierarchies (multiple levels of products, domains, projects, and subprojects). Extends F6's basic monorepo support to handle complex organizational structures, cross-cutting concerns, and multi-team boundaries that F6 alone cannot address. |
| **Why** | F6 (Bootstrap Script) works well for small/medium repos and simple monorepos, but Microsoft's largest codebases (e.g., Office, Windows) have complexity that requires specialized scoping: multiple CODEOWNERS files, deeply nested domain boundaries, shared infrastructure layers, and cross-product dependencies. Without advanced scoping, AI gets overwhelmed or generates incorrect context. |
| **Enables** | S5b (auto-scoping for enterprise monorepos) |
| **Scope** | Multi-level CODEOWNERS integration, domain boundary detection across 5+ folder levels, shared/common library handling, cross-product context isolation, intelligent context pruning for massive codebases |
| **Investigate** | How to detect product vs. domain vs. project boundaries automatically? How to handle shared infrastructure that spans multiple products? Performance implications of deep hierarchy analysis. |
| **Related To** | F6 (Bootstrap Script — handles simple/medium cases), F1 (nested AGENTS.md hierarchy) |
| **Status** | `[NOT STARTED]` |

#### F8: Versioning & Iterative Update System (Work: Feb–Mar | Release: Early Apr)
| Attribute | Details |
|-----------|--------|
| **What** | Version management for low-customization assets with iterative deployment capability. **Must be ready for M2.1 (1ES PROD Deployment)** so the initial deployment includes the update mechanism from day one. |
| **Why** | Enable teams to receive updates without losing customizations; support templates vs versioned assets strategy. **Critical principle: NOT a one-time dump.** The starter pack must support iterative updates to repos over time, avoiding excessive upfront polishing while enabling continuous improvement. |
| **Enables** | S9b |
| **Iterative Deployment** | Repos receive multiple updates over time rather than a single deployment. **Update cadence: stable releases every 2 months** after initial deployment. This approach: (1) gets impact sooner with a "decent" initial pack, (2) establishes the update mechanism from the start, (3) allows refinement based on real-world feedback. |
| **Investigate** | Evaluate versioning/distribution strategies: semantic versioning of templates, git submodules, package-based distribution (npm/nuget), or **Agency plugin distribution** (leverage Agency's existing update infrastructure for CLI users). Define update detection and notification mechanism for repos. |
| **Depends On** | R1 (dynamic delivery) findings; R4 (skills auto-config) findings for skill versioning and distribution |
| **Status** | `[NOT STARTED]` `[DEPENDS ON R1, R4]` `[WORK STARTS FEB]` |

#### F9: CLI Parallel Workflows Guide (GHCP CLI + Agency)
| Attribute | Details |
|-----------|--------|
| **What** | Documented recommended workflows for running multiple AI tasks in parallel using GHCP CLI (with Agency preferred). Includes: Plan Mode usage, `/review` command, background task delegation (`&`), orchestrating multiple parallel CLI sessions, and best practices for context management across parallel tasks. |
| **Why** | CLI enables higher productivity through parallel task execution—something not easily achieved in VSCode's single-session model. Teams report significant velocity gains when running multiple AI agents simultaneously (e.g., one implementing feature A, another writing tests for feature B). Without guidance, developers underutilize this capability. |
| **Enables** | S13 |
| **Depends On** | R6 (CLI parallel workflows patterns) findings |
| **Status** | `[NOT STARTED]` `[DEPENDS ON R6]` |

---

### P2 Capabilities (Work: Apr–May | Release: May+)

> *Work begins in April; release targets are May and beyond.*

#### F10: Live Code Search Beyond Context
| Attribute | Details |
|-----------|--------|
| **What** | Enable AI to dynamically search and discover code patterns across the entire codebase during active work sessions—not just open files or immediate context. By default, AI only "sees" files explicitly in context, missing relevant patterns, utilities, and examples elsewhere in the repo. With live search, AI can query the codebase on-demand as questions arise. |
| **Why** | Large codebases contain reusable patterns, utility functions, API usage examples, and similar implementations that AI should discover and learn from in real-time. Without this, AI may reinvent existing patterns or generate inconsistent code because it doesn't know what already exists. Live search means AI can answer "how is this done elsewhere?" whenever needed during a session. |
| **Enables** | S6b |
| **Investigate** | Evaluate live code search approaches: GitHub code search MCP (remote search via API), local indexing (embedded search), or hybrid. Assess context window impact of including expanded search results. Consider how search results are summarized vs. included verbatim. |
| **Depends On** | R4 (auto-configuring skills) findings |
| **Status** | `[NOT STARTED]` `[DEPENDS ON R4]` |

#### F11: External Dependency Context
| Attribute | Details |
|-----------|--------|
| **What** | AI help with dependencies from other teams |
| **Why** | Cross-team dependencies are common; AI needs access to external API docs and patterns |
| **Enables** | S12 |
| **Investigate** | Evaluate context sources: auto-fetch package docs, Service Tree integration for internal service discovery, or curated dependency context files. |
| **Status** | `[NOT STARTED]` |

#### F12: ADO/Work Item Integration
| Attribute | Details |
|-----------|--------|
| **What** | Start work directly from assigned items in Azure DevOps |
| **Why** | Reduces context switching; AI understands requirements from the source |
| **Enables** | S10b |
| **Investigate** | Evaluate ADO MCP server capabilities. Should AI auto-create branches and link PRs to work items? Security implications of AI reading/writing ADO data. |
| **Status** | `[NOT STARTED]` |

#### F13: Build Failure Correlation
| Attribute | Details |
|-----------|--------|
| **What** | AI correlates pipeline failures with recent changes and known issues |
| **Why** | Accelerates debugging by connecting symptoms to likely causes |
| **Enables** | S11b |
| **Investigate** | Evaluate failure data sources: ADO build logs, Kusto telemetry, or dedicated failure database. Can AI suggest fixes based on historical resolution patterns? |
| **Status** | `[NOT STARTED]` |

#### F14: Repo Inventory Analysis & Adoption Telemetry
| Attribute | Details |
|-----------|--------|
| **What** | Scan repos for existing AI instructions using CodeAsData; provide dashboards and statistics on adoption maturity |
| **Why** | Understand current state across orgs: what repos have AGENTS.md/CLAUDE.md/copilot-instructions, skills in root or subfolders; inform rollout strategy and measure adoption progress |
| **Enables** | S9c |
| **Supports** | S9a (ongoing maintenance — provides visibility into adoption for baseline maintenance) |
| **Investigate** | — |
| **Status** | `[NOT STARTED]` |

#### F15: Stale Context Detection
| Attribute | Details |
|-----------|--------|
| **What** | AI detects outdated instructions and prompts for updates |
| **Why** | Instructions drift from reality as code evolves; automated detection prevents AI from following stale patterns |
| **Enables** | S3 |
| **Investigate** | Evaluate detection approaches: timestamp-based, content hash comparison, or AI semantic analysis of code vs. instructions mismatch. |
| **Status** | `[NOT STARTED]` |

#### F16: Instruction Effectiveness Testing
| Attribute | Details |
|-----------|--------|
| **What** | Evaluate instruction quality using 1JS/ESChat tooling; includes behavioral instruction testing (R5 findings) |
| **Why** | Objectively measure whether instructions improve AI output; enables data-driven iteration on templates |
| **Enables** | S4 |
| **Supports** | S9a (ongoing maintenance — validates instruction quality for continuous baseline improvement) |
| **Investigate** | Evaluate integration approach with 1JS/ESChat testing infrastructure. Define metrics: PR acceptance rate, code review iterations, test coverage of generated code. |
| **Depends On** | R5 (behavioral testing) findings |
| **Status** | `[NOT STARTED]` `[DEPENDS ON R5]` |

#### F17: Org-Level Instruction Validation
| Attribute | Details |
|-----------|--------|
| **What** | Validate org customizations don't break 1ES baseline |
| **Why** | Layered customization requires guardrails; orgs shouldn't accidentally override critical 1ES behaviors |
| **Enables** | S4 |
| **Investigate** | Define "critical 1ES behaviors" that cannot be overridden. Evaluate validation approach: schema validation, linting rules, or runtime checks. |
| **Status** | `[NOT STARTED]` |

---

## 8. Release Plan & Milestones

### Phase 1: Starter Pack v1 + Bootstrap Script/Engine (Work: Feb–Mar 2026 | Release: Early April 2026)

**Goal:** Deliver first version of starter pack AND bootstrap script by early April, with parallel research to inform architecture.

| Milestone | Target Date | Deliverables | Status |
|-----------|-------------|--------------|--------|
| **M1.1: Core Assets Complete** | Feb 28 | F1-F5: AGENTS.md templates, MCP config, agents, skills | `[]` |
| **M1.1R: Research Findings** | Feb 28 | R1-R6: Dynamic delivery, config injection, context tooling, skills auto-config, behavioral testing, CLI parallel patterns | `[]` |
| **M1.2: Manual Onboarding Begins** | Mar 7 | 2-3 1ES teams onboarded, feedback collected | `[]` |
| **M1.3: Iteration & Refinement** | Mar 21 | Incorporate feedback, improve templates based on R1-R6 findings | `[]` |
| **M1.3S: Bootstrap Prototype** ⚡ | Mar 7 | **(Stretch)** P0-Proto: Basic bootstrap script using R2+R3 findings; early testing before full F6 | `[]` |
| **M1.4: Bootstrap Script Ready** | Mar 28 | F6: Full script to transform any repo to AI-first workflow (evolved from P0-Proto) | `[]` |
| **M1.5: 1ES Rollout Complete** | Apr 4 | All 1ES teams using starter pack | `[]` |

**Research Track (Parallel with M1.1–M1.3):**

| Research | Target | Key Question |
|----------|--------|-------------|
| R1: Dynamic Content Delivery | Feb 28 (research) → Mar 28 (F8 ready) | Can we update 1ES config without PRs to repos? **(Critical for F8 iterative updates)** — Includes F8 Versioning System development; must be ready for M2.1 deployment |
| R2: Config Injection | Feb 28 | How do agents discover config not in repo? **(Feeds into P0-Proto → F6 Bootstrap)** |
| R3: Project Context Extraction | Feb 28 | Can we auto-extract patterns and distribute to nested AGENTS.md? **(Critical for P0-Proto → F6 Bootstrap)** |
| R4: Skills Auto-config | Feb 28 | MCP servers vs SKILLS.md — which is better? **(Feeds into F5 Skills + F8 iterative updates + F10 Live Search)** |
| R5: Behavioral Instructions | Feb 28 | Which behavioral patterns improve AI output? **(Lower priority; feeds F16 in P2)** |
| R6: CLI Parallel Workflows | Mar 21 | Which CLI parallel patterns actually improve velocity? **(Feeds into F9 guide)** |

**Acceptance Criteria:**
- [ ] All 1ES teams have starter pack applied to at least one repo
- [ ] At least 1 repo from outside 1ES for each E+D/CoreAI/C+AI deployed (Karl)
- [ ] Bootstrap script functional and documented
- [ ] Research findings documented and incorporated into assets
- [ ] Measurable improvements in code quality & productivity documented
- [ ] Project board created to track features & goals

**Initial Feature Focus:**
- Repo-specific instructions (AGENTS.md hierarchy)
- MCP servers: GitHub, ADO, Bluebird (via 1ES Registry)
- Agent modes for common workflows

### Phase 2: Automation & Tooling (Work: Mar–Apr 2026 | Release: Early April 2026)

**Goal:** Deploy at scale leveraging Phase 1 research. **M2.1 is the first major deployment milestone.** P2 feature work begins in parallel.

| Milestone | Target Date | Deliverables | Status |
|-----------|-------------|--------------|--------|
| **M2.1: 1ES PROD Deployment** ⭐ | Apr 11 | Deploy F6 (Bootstrap) + F7 (Enterprise Monorepo Scoping) + F8 (Versioning) across all 1ES PROD repos — **First major deployment milestone** | `[]` |

**Iterative Deployment Cadence:** After M2.1 initial deployment, stable updates pushed to repos **every 2 months** (June, August, October, etc.). This avoids one-time dump and establishes sustainable update rhythm.

**P2 Feature Work Begins:** Starting April, development begins on P2 capabilities (F10–F17) in parallel with deployment activities. See Phase 3 for release targets.

### Phase 3: Advanced Features & Scalability Improvements (Work: Apr–May 2026 | Release: May+ 2026)

**Goal:** Expand to larger teams, monorepos, and advanced scenarios.

| Capability | Description | Features |
|------------|-------------|----------|
| Live code search | AI discovers patterns across entire codebase during sessions | F10 |
| Advanced context automation | Stale detection and instruction updates | F15 |
| Instruction quality tooling | Effectiveness testing and org-level validation | F16, F17 |
| External system integration | ADO work items, build pipeline correlation | F12, F13 |
| Repo inventory & telemetry | Adoption dashboards and statistics | F14 |
| External dependency context | AI help with cross-team dependencies | F11 |
| Partner organization expansion | Broader E+D, CoreAI, C+AI adoption | — |

---

## 9. Success Metrics & KPIs

### Primary Metrics (Executive Dashboard)

| Metric | Target | Measurement Method |
|--------|--------|-------------------|
| **PR Velocity Improvement** | 2.5x baseline average in teams using this asset | Compare AI-assisted vs. non-AI PRs |
| **Adoption Rate** | 100% 1ES teams by early April 2026 | Repo inventory scan |
| **Developer NSAT** | 4.2/5.0 | Post-pilot surveys |
| **Time-to-First-PR** (new devs) | 50% reduction | Onboarding tracking |

### Secondary Metrics

| Metric | Target | Notes |
|--------|--------|-------|
| PR Cycle Time | 15% reduction | Time from PR open to merge |
| Code Review Iterations | 20% reduction | Fewer back-and-forth cycles |
| Instruction Effectiveness | 80%+ score | Via 1JS/ESChat testing tooling |
| Training Completion | 60% enrolled | Workshop/module completion |

### Quarterly Review Checkpoints

- **End of Q1 (March):** All 1ES teams onboarded, baseline metrics established
- **End of Q2 (June):** Automation deployed, 10+ external repos enabled, impact measured

---

## 10. Risks & Mitigations

| Risk | Probability | Impact | Mitigation |
|------|-------------|--------|------------|
| **Low pilot adoption** | Medium | High | Start with engaged teams; iterate on feedback quickly |
| **Automation blocks early adoption** | Medium | Medium | Parallel tracks: manual onboarding + automation development |
| **Customization vs. updatability tension** | High | Medium | Clear layering model: 1ES base → Org extensions → Team overrides |
| **Multiple agent platform support** | Medium | Medium | Focus on GHCP + Agency; document Claude Code workarounds |
| **Quality/trust concerns** | Medium | High | Explicit guardrails; senior engineer involvement in templates |
| **Resource constraints** | Medium | Medium | Prioritize P0 features; defer P2 to later phases |
| **Stale instructions over time** | High | Medium | Build stale detection; establish update cadence |

---

## 11. Dependencies & Assumptions

### Dependencies

| Dependency | Owner | Impact if Not Met |
|------------|-------|-------------------|
| **Agency CLI** | Agency team | Cannot leverage auto-MCPs, Entra auth; fallback to manual config |
| **1ES MCP Registry** | 1ES MCP team | Teams must configure MCP servers manually |
| **1JS/ESChat testing tooling** | 1JS, ESChat teams | Cannot validate instruction effectiveness objectively |

### Assumptions

| Assumption | Risk if Invalid |
|------------|----------------|
| Manual onboarding generates sufficient feedback | Automation may not address real pain points |
| 1ES teams are representative of broader Microsoft | Expansion to other orgs may reveal new requirements |
| VSCode remains primary IDE for target personas | Visual Studio also for .NET/C# as primary platform. |

---

## 12. Team & Collaboration Model

### Team Structure

| Role | Responsibility | Dedication |
|------|----------------|------------|
| **PM(s)** | Vision, prioritization, stakeholder alignment, hands-on enablement (Cesar embedded) | Cesar: 100% |
| **Engineering Lead** | Technical direction, quality oversight | TBD |
| **SWE(s)** | Asset development, automation tooling | TBD |

**Core Team (100% dedicated):** Cesar, [TBD — Engineering Lead], [TBD — SWE(s)]

### Collaboration Cadence

| Cadence | Participants | Purpose | Schedule |
|---------|--------------|---------|----------|
| **Daily Standup** | Core team (100% dedicated) | Progress sync, blockers, daily priorities | Starting Feb |
| **Weekly Experiments** | Core team | Deliver 1–2 quick features, iterate, measure | Wednesdays (TBD) |
| **Weekly Sync** | Core team + stakeholders | Status sync, experiment reviews, blocker resolution | TBD |
| **Bi-weekly Demo** | Extended stakeholders | Feature demos, stakeholder feedback | TBD |
| **Monthly Review** | Leadership | Metrics review, roadmap adjustment | TBD |

### Project Tracking

- **Project Board:** Create and maintain project board to track features & goals (Action: TBD)
- **Feature Tracking:** Use GitHub Issues or ADO work items
- **Progress Visibility:** Weekly status updates to stakeholders

### Partner Engagement

- **Agency team** — Alignment on MCP, auth, custom agents
- **1JS / ESChat teams** — Instruction effectiveness testing tooling
- **E+D / CoreAI / C+AI orgs** — External pilot repos

---

## 13. Open Questions

| ID | Question | Owner | Status | Resolution |
|----|----------|-------|--------|------------|
| Q1 | Who will be the Engineering Lead and SWE(s) on the core team? | Leadership | `[OPEN]` | — |
| Q2 | Which specific 1ES teams should be first for manual onboarding? | TBD | `[OPEN]` | — |
| Q3 | How will instruction effectiveness testing integrate with starter pack? | 1JS/ESChat | `[OPEN]` | — |
| Q4 | What's the versioning strategy for templates vs. versioned assets? | TBD | `[OPEN]` | — |
| Q5 | **R1:** What's the best mechanism for dynamic 1ES content delivery — Agency plugin, VSCode extension, or remote fetch? | Dmitry, Agency | `[OPEN]` | — |
| Q6 | **R2:** Which config injection approach works across all environments — directory junctions, workspace configs, or includes? | Dmitry | `[OPEN]` | — |


---

## Appendix: Key References

| Document | Purpose |
|----------|---------|
| [1ES Agentic Development Scenario (Dmitry)](./resources/1ES%20Agentic%20Development%20Scenario%20(Dmitry-Feb-3-2026).md) | Experience scenarios and personas |
| [Potential Prototypes To Do (Dmitry)](./resources/potential-prototypes-to-do.md) | Research investigations and architecture decisions |
| [Kick-Off Meeting Notes (Feb 2)](./resources/Kick-Off-Meeting-Feb-2-2026.md) | Priorities and timeline decisions |
| [Multiple Folks Requirements](./resources/multiple-folks-requirements.md) | Stakeholder requirements and acceptance criteria |
| [Target Scenarios & Environments](../specs-target-scenarios-personas-environments/specs-funct-and-tech.md) | Environment comparison and persona details |
| [Original Global Plan](../00-original-global-plan-ai-first-dev-pack.md) | Historical context and extended feature list |
| [AI-first dev Kick-off Loop table](https://microsoft-my.sharepoint.com/:fl:/p/dgonch/IQDpF0_fAv2HTZj838-2l-_dAS7xgN8yk0CRFIpl3IB206k) | Live tracking of prototypes and experiments |

---

