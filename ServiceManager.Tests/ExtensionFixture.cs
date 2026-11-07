using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ServiceManager.Tests
{
    /// <summary>
    /// Compiles a throwaway extension assembly into <test output>\extensions\<name>\ so the loader
    /// can be exercised against real DLLs. Runtime compilation instead of fixture projects keeps
    /// every test isolated: each one gets its own assembly, its own Id and its own folder.
    ///
    /// A loaded assembly cannot be deleted while the process lives (the CLR locks the file), so the
    /// folders stay behind and the next run wipes them at the start. Assertions therefore look for a
    /// specific Id instead of counting the whole folder.
    /// </summary>
    internal sealed class ExtensionFixture
    {
        public String Folder { get; }

        public String AssemblyPath { get; }

        public String CtorLog { get; }

        public String SentLog { get; }

        public Guid Id { get; }

        public String Name { get; }

        private static int m_Counter;

        private static readonly Type HostInterface = typeof(global::ServiceManager.Extensions.Interfaces.INotificationExtension);

        public static String RootFolder
        {
            get { return global::ServiceManager.Extensions.Extensions.ExtensionFolder; }
        }

        private static Assembly HostExtensionsAssembly
        {
            get { return HostInterface.Assembly; }
        }

        private ExtensionFixture(String folder, String assemblyPath, Guid id, String name)
        {
            Folder = folder;
            AssemblyPath = assemblyPath;
            CtorLog = Path.Combine(folder, "ctor.log");
            SentLog = Path.Combine(folder, "sent.log");
            Id = id;
            Name = name;
        }

        /// <summary>
        /// How often the fixture constructor ran. The real TelegramNotifier does its work in the
        /// constructor too: it reads its settings and writes them straight back.
        /// </summary>
        public int ConstructorCalls
        {
            get { return File.Exists(CtorLog) ? File.ReadAllLines(CtorLog).Length : 0; }
        }

        /// <summary>
        /// What the fixture received, one line per Send call. The fixture writes it next to its own
        /// assembly because the CLR gives the fixture its own copy of the interface types.
        /// </summary>
        public String[] SentLines
        {
            get { return File.Exists(SentLog) ? File.ReadAllLines(SentLog) : new String[0]; }
        }

        /// <summary>
        /// The source factory receives the identity, so a test can compile a twin that shares the Id
        /// of another fixture.
        /// </summary>
        public static ExtensionFixture Compile(String prefix, Func<Guid, String, String> sourceFactory, Guid? id = null, String name = null, bool copyHostExtensionsAssembly = false)
        {
            var identity = id ?? Guid.NewGuid();
            var extensionName = name ?? prefix + "-" + identity.ToString("N").Substring(0, 8);

            var folder = Path.Combine(RootFolder, String.Format("{0}_{1}_{2}", prefix, Interlocked.Increment(ref m_Counter), Guid.NewGuid().ToString("N").Substring(0, 8)));

            Directory.CreateDirectory(folder);

            var assemblyPath = Path.Combine(folder, extensionName + ".dll");

            Emit(sourceFactory(identity, extensionName), assemblyPath);

            if (copyHostExtensionsAssembly)
            {
                // A real extension folder ships its own copy of the host assembly, and that copy is
                // exactly what the loader's identity guard exists for.
                File.Copy(HostExtensionsAssembly.Location, Path.Combine(folder, "ServiceManager.Extensions.dll"), overwrite: true);
            }

            return new ExtensionFixture(folder, assemblyPath, identity, extensionName);
        }

        private static void Emit(String source, String assemblyPath)
        {
            var references = new List<MetadataReference>
            {
                MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location),
                MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location),
                MetadataReference.CreateFromFile(HostExtensionsAssembly.Location),
            };

            var compilation = CSharpCompilation.Create(
                Path.GetFileNameWithoutExtension(assemblyPath),
                new[] { CSharpSyntaxTree.ParseText(source) },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

            var result = compilation.Emit(assemblyPath);

            if (!result.Success)
            {
                var errors = result.Diagnostics
                    .Where(a => a.Severity == DiagnosticSeverity.Error)
                    .Select(a => a.ToString());

                throw new InvalidOperationException("Fixture could not be compiled:" + Environment.NewLine +
                    String.Join(Environment.NewLine, errors) + Environment.NewLine + source);
            }
        }

        /// <summary>
        /// Removes everything below the extensions folder. Only works for assemblies that are not
        /// loaded in this process, so it runs at the start of a run rather than after every test.
        /// </summary>
        public static void WipeExtensionFolder()
        {
            var root = RootFolder;

            if (!Directory.Exists(root))
                return;

            try
            {
                Directory.Delete(root, true);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }

        private const String Header =
@"using System;
using System.IO;
using System.Threading.Tasks;
using ServiceManager.Extensions.Interfaces;

namespace Fixtures
{
";

        private const String Members =
@"        public String Description { get { return ""test fixture""; } }

        public Task Send(String message) { return Task.CompletedTask; }

        public void Configure() { }
";

        private static String Identity(Guid id, String name)
        {
            return String.Format(
@"        public Guid Id {{ get {{ return new Guid(""{0:N}""); }} }}

        public String Name {{ get {{ return ""{1}""; }} }}

", id, name);
        }

        /// <summary>
        /// The marker is written next to the fixture assembly, which is how a test can tell how often
        /// a constructor ran without reaching into the fixture.
        /// </summary>
        private static String CtorMarker(String className)
        {
            return String.Format(
@"            File.AppendAllText(Path.Combine(Path.GetDirectoryName(typeof({0}).Assembly.Location), ""ctor.log""), ""ctor"" + Environment.NewLine);
", className);
        }

        /// <summary>A normal extension: public class with a public parameterless constructor.</summary>
        public static String ValidSource(String className, Guid id, String name, String extraCtorBody = null)
        {
            return Header +
                String.Format("    public class {0} : INotificationExtension\r\n    {{\r\n\r\n", className) +
                Identity(id, name) +
                String.Format("        public {0}()\r\n        {{\r\n", className) +
                CtorMarker(className) +
                (extraCtorBody == null ? "" : "            " + extraCtorBody + "\r\n") +
                "        }\r\n\r\n" +
                Members +
                "    }\r\n}\r\n";
        }

        /// <summary>Constructor throws, the way a broken settings file makes TelegramNotifier fail.</summary>
        public static String ThrowingSource(String className, Guid id, String name)
        {
            return ValidSource(className, id, name, "throw new InvalidOperationException(\"fixture constructor failed\");");
        }

        /// <summary>No public parameterless constructor.</summary>
        public static String NoParameterlessCtorSource(String className, Guid id, String name)
        {
            return Header +
                String.Format("    public class {0} : INotificationExtension\r\n    {{\r\n\r\n", className) +
                Identity(id, name) +
                String.Format("        public {0}(int required) {{ }}\r\n\r\n", className) +
                Members +
                "    }\r\n}\r\n";
        }

        /// <summary>Abstract class implementing the interface - can never be instantiated.</summary>
        public static String AbstractSource(String className, Guid id, String name)
        {
            return Header +
                String.Format("    public abstract class {0} : INotificationExtension\r\n    {{\r\n\r\n", className) +
                Identity(id, name) +
                Members +
                "    }\r\n}\r\n";
        }

        /// <summary>Concrete and instantiable, but not public.</summary>
        public static String NonPublicSource(String className, Guid id, String name)
        {
            return Header +
                String.Format("    class {0} : INotificationExtension\r\n    {{\r\n\r\n", className) +
                Identity(id, name) +
                String.Format("        public {0}()\r\n        {{\r\n", className) +
                CtorMarker(className) +
                "        }\r\n\r\n" +
                Members +
                "    }\r\n}\r\n";
        }

        /// <summary>
        /// A provider written against the structured contract. It logs both entry points, so a test
        /// can see which overload the dispatcher chose and what arrived in it.
        /// </summary>
        public static String StructuredSource(String className, Guid id, String name)
        {
            return StructuredTemplate
                .Replace("$CLASS$", className)
                .Replace("$ID$", id.ToString("N"))
                .Replace("$NAME$", name);
        }

        private const String StructuredTemplate =
@"using System;
using System.IO;
using System.Threading.Tasks;
using ServiceManager.Extensions.Interfaces;

namespace Fixtures
{
    public class $CLASS$ : INotificationMessageExtension
    {
        public Guid Id { get { return new Guid(""$ID$""); } }

        public String Name { get { return ""$NAME$""; } }

        public String Description { get { return ""test fixture""; } }

        public $CLASS$()
        {
            File.AppendAllText(Path.Combine(Path.GetDirectoryName(typeof($CLASS$).Assembly.Location), ""ctor.log""), ""ctor"" + Environment.NewLine);
        }

        public Task Send(String message)
        {
            return Record(""text"", message, null, null, null);
        }

        public Task Send(NotificationMessage message)
        {
            return Record(""message"", message.Text, message.Kind.ToString(), message.ServiceTitle, message.Details);
        }

        public void Configure() { }

        private static Task Record(String entry, String text, String kind, String service, String details)
        {
            File.AppendAllText(
                Path.Combine(Path.GetDirectoryName(typeof($CLASS$).Assembly.Location), ""sent.log""),
                String.Join(""|"", entry, text, kind, service, details) + Environment.NewLine);

            return Task.CompletedTask;
        }
    }
}
";

        /// <summary>
        /// A provider written against the old contract only. It records its text, so a test can see
        /// that such a provider still gets exactly what it used to get.
        /// </summary>
        public static String LegacyRecordSource(String className, Guid id, String name)
        {
            return Fill(LegacyTemplate, className, id, name);
        }

        /// <summary>Structured provider whose Send throws, the way a provider without internet fails.</summary>
        public static String FailingStructuredSource(String className, Guid id, String name)
        {
            return Fill(FailingTemplate, className, id, name);
        }

        private static String Fill(String template, String className, Guid id, String name)
        {
            return template
                .Replace("$CLASS$", className)
                .Replace("$ID$", id.ToString("N"))
                .Replace("$NAME$", name);
        }

        private const String LegacyTemplate =
@"using System;
using System.IO;
using System.Threading.Tasks;
using ServiceManager.Extensions.Interfaces;

namespace Fixtures
{
    public class $CLASS$ : INotificationExtension
    {
        public Guid Id { get { return new Guid(""$ID$""); } }

        public String Name { get { return ""$NAME$""; } }

        public String Description { get { return ""test fixture""; } }

        public Task Send(String message)
        {
            File.AppendAllText(
                Path.Combine(Path.GetDirectoryName(typeof($CLASS$).Assembly.Location), ""sent.log""),
                message + Environment.NewLine);

            return Task.CompletedTask;
        }

        public void Configure() { }
    }
}
";

        private const String FailingTemplate =
@"using System;
using System.IO;
using System.Threading.Tasks;
using ServiceManager.Extensions.Interfaces;

namespace Fixtures
{
    public class $CLASS$ : INotificationMessageExtension
    {
        public Guid Id { get { return new Guid(""$ID$""); } }

        public String Name { get { return ""$NAME$""; } }

        public String Description { get { return ""test fixture""; } }

        public Task Send(String message) { return Task.CompletedTask; }

        public Task Send(NotificationMessage message)
        {
            throw new InvalidOperationException(""fixture provider failed"");
        }

        public void Configure() { }
    }
}
";

        /// <summary>An assembly without any extension at all, e.g. a library an extension ships with.</summary>
        public static String PlainLibrarySource(String className)
        {
            return Header +
                String.Format("    public class {0}\r\n    {{\r\n        public String Name {{ get {{ return \"not an extension\"; }} }}\r\n    }}\r\n}}\r\n", className);
        }
    }
}
