using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// Allgemeine Informationen über eine Assembly werden über die folgenden
// Attribute gesteuert. Ändern Sie diese Attributwerte, um die Informationen zu ändern,
// die einer Assembly zugeordnet sind.
[assembly: AssemblyTitle("SmtpNotifier")]
[assembly: AssemblyDescription("ServiceManager notification provider sending mails over a SMTP server")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SmtpNotifier")]
[assembly: AssemblyCopyright("Copyright ©  2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Durch Festlegen von ComVisible auf FALSE werden die Typen in dieser Assembly
// für COM-Komponenten unsichtbar.  Wenn Sie auf einen Typ in dieser Assembly von
// COM aus zugreifen müssen, sollten Sie das ComVisible-Attribut für diesen Typ auf "True" festlegen.
[assembly: ComVisible(false)]

// Die folgende GUID bestimmt die ID der Typbibliothek, wenn dieses Projekt für COM verfügbar gemacht wird
[assembly: Guid("b9f79424-8bb1-4502-a11d-63d16a7f9942")]

// The tests read the mail of an event and the throttle guard without a mail server in between.
[assembly: InternalsVisibleTo("ServiceManager.Tests")]

// AssemblyVersion, AssemblyFileVersion and AssemblyInformationalVersion are injected by the build:
// see Directory.Build.targets and version.props in the solution root.
