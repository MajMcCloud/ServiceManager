using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("SmtpNotifier")]
[assembly: AssemblyDescription("ServiceManager notification provider sending mails over a SMTP server")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("SmtpNotifier")]
[assembly: AssemblyCopyright("Copyright ©  2026")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("b9f79424-8bb1-4502-a11d-63d16a7f9942")]

// The tests read the mail of an event and the throttle guard without a mail server in between.
[assembly: InternalsVisibleTo("ServiceManager.Tests")]

// AssemblyVersion, AssemblyFileVersion and AssemblyInformationalVersion are injected by the build:
// see Directory.Build.targets and version.props in the solution root.
