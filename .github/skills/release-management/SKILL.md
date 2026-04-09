---
name: release-management
description: GitHub Flow branching, semantic versioning, and tag-triggered release procedure for solo/small-team projects
tier: project
applyTo: '**/CHANGELOG*,**/.github/workflows/release*,**/version*'
---

# Release Management Skill

> Ship confidently: branch, tag, release, repeat.

## Branching Model: GitHub Flow

```
main ──●──●── v0.1.0 ──●──●──●── v0.2.0
              \       /    \     /
               fix/a ┘      feat/b
```

- `main` is always releasable.
- All work happens on short-lived branches off `main`.
- Merge back to `main` via PR (or direct push for solo repos).
- Tags on `main` trigger the release pipeline.

## Branch Naming

| Prefix | Use |
| --- | --- |
| `feat/` | New feature |
| `fix/` | Bug fix |
| `docs/` | Documentation only |
| `chore/` | Build, CI, tooling |
| `refactor/` | Code restructure, no behavior change |

Examples: `feat/export-pdf`, `fix/tts-pause-resume`, `docs/api-reference`

## Semantic Versioning

Format: `MAJOR.MINOR.PATCH`

| Bump | When |
| --- | --- |
| PATCH (0.1.0 -> 0.1.1) | Bug fixes, docs, minor tweaks |
| MINOR (0.1.0 -> 0.2.0) | New features, backward-compatible |
| MAJOR (0.2.0 -> 1.0.0) | Breaking changes, public API shifts |

Pre-1.0: anything goes. Post-1.0: follow SemVer strictly.

## Release Procedure

### 1. Prepare

```powershell
# Ensure main is clean
git checkout main
git pull
git status  # should be clean
```

### 2. Update version and changelog

- Set `<Version>` in .csproj (or equivalent)
- Add release entry to CHANGELOG.md under a new `## [x.y.z] - YYYY-MM-DD` heading
- Move items from `[Unreleased]` to the new version section
- Add comparison link at bottom: `[x.y.z]: https://github.com/owner/repo/releases/tag/vx.y.z`

### 3. Commit the release

```powershell
git add -A
git commit -m "chore: release v0.1.0"
git push
```

### 4. Tag and push

```powershell
git tag v0.1.0
git push origin v0.1.0
```

The GitHub Actions workflow builds and publishes the release automatically.

### 5. Verify

```powershell
gh release view v0.1.0
```

## Post-Release: Start Next Work

```powershell
# Create a feature branch
git checkout -b feat/my-feature

# ... make changes, commit ...

# Push and open a PR (or merge directly for solo repos)
git push -u origin feat/my-feature
gh pr create --fill

# After merge, clean up
git checkout main
git pull
git branch -d feat/my-feature
```

## Hotfix Procedure

For urgent fixes on a released version:

```powershell
git checkout main
git checkout -b fix/critical-bug
# ... fix, test, commit ...
git push -u origin fix/critical-bug
# merge to main, then tag
git checkout main
git pull
git tag v0.1.1
git push origin v0.1.1
```

## Checklist: Before Tagging

- [ ] Version in .csproj matches the tag
- [ ] CHANGELOG.md updated with release date
- [ ] Build succeeds with zero warnings/errors
- [ ] No secrets in tracked files
- [ ] README reflects current features
