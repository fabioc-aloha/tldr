# Contributing to TLDR

Thanks for your interest in contributing. Here's how to get started.

## Prerequisites

- Windows 10 21H2 or later
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Microsoft Foundry Local](https://github.com/microsoft/foundry-local)

## Setup

```powershell
git clone https://github.com/fabioc-aloha/tldr.git
cd tldr
dotnet build src\Tldr\Tldr.csproj
dotnet run --project src\Tldr\Tldr.csproj
```

## Making Changes

1. Fork the repository and create a branch from `main`.
2. Make your changes.
3. Test locally: build succeeds with zero warnings and zero errors.
4. Commit with a descriptive message using conventional prefixes (`fix:`, `feat:`, `docs:`, `chore:`).
5. Open a pull request against `main`.

## Code Style

- Follow existing patterns in the codebase.
- Keep changes focused: one concern per PR.
- Pin NuGet package versions to exact resolved versions.

## Reporting Bugs

Use the [Bug Report](https://github.com/fabioc-aloha/tldr/issues/new?template=bug_report.md) issue template. Include:

- Steps to reproduce
- Expected vs actual behavior
- Windows version and hardware (CPU/GPU/NPU)

## Suggesting Features

Use the [Feature Request](https://github.com/fabioc-aloha/tldr/issues/new?template=feature_request.md) issue template. Describe the problem you're solving and your proposed solution.

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE).
