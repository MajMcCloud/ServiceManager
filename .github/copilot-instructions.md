# Copilot instructions

Guidance for Copilot when working on this repository. It applies to the whole project.

## Project

ServiceManager is a Windows service that watches services and processes and raises events
(start, stop, crash, high memory, no response). Events can be forwarded to notification
providers. Built on .NET Framework 4.8.1, a WinForms UI, a Windows service, and extensions
that are loaded at runtime from the `extensions` folder.

Build with MSBuild (not `dotnet build`: the projects are `packages.config` style, the
restore target is `msbuild /t:Restore`). Tests run through VSTest, either in Visual Studio
or with `vstest.console.exe` against `ServiceManager.Tests\bin\Debug\net481\ServiceManager.Tests.dll`.

## Language

* Code, comments, XML documentation, commit messages and documentation are English. No
  exceptions: the scripts (including `publish.ps1`) and the CI workflow are English too.
* The developer is answered in German.
* The UI is English: labels, messages and notification texts. The resource files hold no
  localized text, the strings live in the Designer files and in the providers' message
  templates, and they are English as well.

## Comments and documentation

* Comments explain why something happens, never what the code already says.
* No separators or banner lines inside methods.
* XML documentation on public members: short, plain sentences.
* Keep the style of the surrounding file. The core projects are older than the extensions
  and are formatted differently; do not reformat existing code.

## Code style

* `if`, `for`, `foreach` and `using` are always written with braces.
* Null checks use `if (x == null)` or `if (x != null)`, not pattern matching.
* `var` only when the type is obvious, otherwise the type is written out.
* New extension projects are named `<Name>Notifier` (the folder and the project share the
  name). The assembly name is set explicitly when it differs from the project name, as
  `TelegramBotNotifier.dll` shows.
* New extension projects follow the structure of the existing ones: `Notifier.cs`,
  `Config\Config.cs`, `forms\`, `Properties\`, `app.config`, `packages.config`.
* Anything that lists providers is sorted alphabetically, ignoring case: the menu, the table
  and the sections of the README, the provider dropdown and the active provider list in the
  settings form.

## Tests

* MSTest, one test class per production class, in `ServiceManager.Tests`.
* Test names are lower case snake case: `short_name_is_false_for_a_full_name`.
* Arrange/Act/Assert blocks are separated by a blank line, no `//` comments marking them.
* Each test asserts one behaviour.
* Network, file system and time are replaced by interfaces in the production code
  (`ISyslogTransport`, `IMailSender`, `IHookProcessRunner`), so tests can use fakes.
  Existing classes such as `SyslogSender` or `ScriptRunner` keep their parameterless
  constructors.
* Tests that need a working directory use `TestContextHolder.GetTestDirectory()`; they must
  not write into the Visual Studio installation folder.

## New notification provider

1. Project `Extensions\<Name>Notifier` with a reference to `ServiceManager.Base`.
2. `Config\Config.cs` deriving from `ConfigBase`, with `Save()` and `Load()`.
3. A form deriving from `ExtensionConfigForm` (constructor takes the config) for the
   settings dialog.
4. A class deriving from `NotificationProviderBase` and implementing `INotificationProvider`
   (Name, Description, Config, CreateConfigForm, Send).
5. `AssemblyInfo.cs` with `[assembly: ExtensionInfo]` and `[assembly: ExtensionType]`.
6. The project is added to the solution under the "Extensions" solution folder and to the
   "Notification Extensions" folder in `ServiceManager.Tests`.
7. `publish.ps1` finds the extension on its own; a different assembly name is registered in
   its `$FolderNames` table.

## Git

* Commit messages are English and describe what the commit changes, for example
  "Add syslog notification provider".
* One commit per topic. Application changes that are independent of the new feature go into
  their own commit first.
* No AI or co-author trailers in commit messages.
* `session-state` is ignored and stays out of commits.
* Commits are created locally and never pushed on your own. Pushing is upload: ask first and
  wait for a clear yes, the same way as for a release.

## Releases

* Never create a release on your own - not the tag, not the push of the tag, not the GitHub
  release. Ask first and wait for a clear yes.
* `publish.ps1` builds the package and writes `publish\RELEASE-NOTES.md`. The version is a
  build parameter, `version.props` stays one step ahead of the last tag.
* The notes are English and grouped in three sections, empty ones are left out: "New features",
  "Changes" and "Bug fixes and corrections".
* The script reads the sections from the commit subjects: "Fix", "Correct" or "Restore" at the
  start means a bug fix, subjects about tests, CI, documentation or screenshots are changes, and
  "Add", "Support" or "Introduce" means a new feature. Keep product features and test-only or
  documentation-only work in separate commits, that is what keeps the notes readable.
* The release title names what the release brings ("Multiple notification providers") instead of
  repeating the version number.

## Working together

* Propose options and agree on the approach before larger changes.
* Ask rather than guess.
* After changes: build the solution and run the affected tests.
