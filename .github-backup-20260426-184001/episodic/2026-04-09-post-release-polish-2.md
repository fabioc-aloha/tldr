# Episodic: Post-Release Polish Session 2

**Date:** 2026-04-09
**Branch:** fix/post-release-polish (ahead of v0.1.0 by 1 commit, 20 uncommitted files)

## What Happened

### Edge Neural TTS Integration
- Added EdgeTtsEngine.cs using Edge_tts_sharp 1.1.7 (free neural voices via WebSocket)
- Volume must be 100f (percentage scale), not 1.0f
- Voice selection uses FirstOrDefault with null safety (not First which throws)
- TTS fallback chain: Edge (Neural) > WinRT > SAPI5
- **Critical fix:** Edge_tts.GetVoice() and Edge_tts.Invoke() are blocking network calls. Wrapping in Task.Run was required to prevent UI thread deadlock. Without this, Read Aloud appeared to do nothing.

### Simple Summary Style
- Added 6th SummaryStyle: Simple ("Explain the key points in plain, everyday language")
- Updated across: Enums.cs, prompts.json, MainWindow.xaml (chip), MainViewModel.cs (binding)

### NASA Power of 10 Hardening (14 changes, 6 files)
- Precondition assertions on SummarizeAsync, DistillAsync, FileExtractor.Extract
- LLM response validation (empty/null check)
- Bounded SAPI pause loop (30 min max)
- Log file rotation at 512 KB
- All 8 bare catch{} blocks now log exception details
- OperationCanceledException propagation in EP registration

### UI Restructure
- Tone chips moved from Settings popup to main window (Row 3 in Loaded panel)
- Settings popup redesigned: Voice picker (ComboBox), Theme chips, Machine Config info box
- Settings popup width 220px > 240px for voice dropdown
- Vertical spacing tightened: section margins 16>12px, label gaps 8>6px, Distill button 20>16px
- Chip padding refined: 14,6 > 12,5 with margin 2,0 > 3,0

### Chip Toggle Multi-Select Bug (Critical)
- All chip groups (Style, Detail, Tone, Theme) had same bug: Unchecked handler blindly re-checked the button, preventing the ViewModel binding from unchecking it when a different chip was selected
- Result: multiple chips highlighted simultaneously (visible in screenshots)
- Fix: Unchecked handlers now only prevent uncheck if the chip IS the active selection
- Theme chips needed special handling: no ViewModel binding, so added _activeTheme field + explicit sibling uncheck in ThemeChip_Checked

### Crash Prevention
- NavigationStarting handler was stacking on every distill (added _webViewInitialized flag)
- LoadHtmlIntoWebView wrapped in try/catch in Distill_Click
- Global exception handlers added to App.xaml.cs: DispatcherUnhandledException, AppDomain.UnhandledException, TaskScheduler.UnobservedTaskException
- All unhandled exceptions now log to errors.log + show MessageBox instead of silent crash

### ViewModel Additions
- SelectedVoice, AvailableVoices (ObservableCollection), TtsEngineName, MachineInfo properties
- Voice populated async from Edge TTS on startup, saved/restored via UserSettings
- PopulateMachineInfo(): model name, context window, CPU cores, RAM, TTS engine
- ReadAloudAsync uses _selectedVoice

### Documentation
- CHANGELOG.md: Full [Unreleased] section with Added/Changed/Fixed/Security
- README.md: 6 styles, tone row, voice picker, updated project structure
- user-guide.md: Simple style, tone in main window, Voice section, Machine Info section
- getting-started.md: Updated first summarization steps
- plan.md: v0.4 Phi Silica integration plan (from prior session)

## Files Changed (20)
Core: Enums.cs, FileExtractor.cs, Summarizer.cs, UserSettings.cs, Config.cs
Platform: App.xaml.cs, MainViewModel.cs, MainWindow.xaml, MainWindow.xaml.cs, EdgeTtsEngine.cs (new), SapiTtsEngine.cs, WinRtTtsEngine.cs
Config: Tldr.csproj, prompts.json
Docs: CHANGELOG.md, README.md, user-guide.md, getting-started.md, plan.md

## Lessons Learned
- Edge_tts_sharp blocking calls on UI thread: always Task.Run for network I/O
- ToggleButton Unchecked handlers must check if the button is still the active value before re-checking; blind re-check creates multi-select bugs
- WebView2 NavigationStarting handler stacks if registered in a method called multiple times; use a flag
- WPF ambiguous types: System.Windows.MessageBox vs System.Windows.Forms.MessageBox; fully qualify when UseWindowsForms is enabled
- Global exception handlers (Dispatcher + AppDomain + TaskScheduler) are essential for desktop apps; without them, crashes vanish with no trace

## Open Items
- Git commit needed: 20 uncommitted files on fix/post-release-polish
- Read Aloud: confirmed working after Task.Run fix but needs user re-test
- Error log only has stale Foundry Local init error from prior session
