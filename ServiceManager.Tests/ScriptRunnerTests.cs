using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceManager.Extensions.Interfaces;
using ServiceManager.Extensions.ScriptNotifier;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ScriptConfig = ServiceManager.Extensions.ScriptNotifier.Config.Config;

namespace ServiceManager.Tests
{
    /// <summary>
    /// Starting a program and waiting for it. The programs here are the ones every Windows installation
    /// ships, so the tests say what the provider does with an exit code, a timeout and a path that looks
    /// like an attempt to smuggle a second command into the line.
    /// </summary>
    [TestClass]
    public class ScriptRunnerTests
    {
        private static readonly String Cmd = Environment.ExpandEnvironmentVariables("%ComSpec%");

        private static readonly String PowerShell = Environment.ExpandEnvironmentVariables("%SystemRoot%\\System32\\WindowsPowerShell\\v1.0\\powershell.exe");

        private String __temp;


        [TestInitialize]
        public void CreateTempFolder()
        {
            __temp = Path.Combine(Path.GetTempPath(), "scriptnotifier-" + Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(__temp);
        }


        [TestCleanup]
        public void RemoveTempFolder()
        {
            try
            {
                if (Directory.Exists(__temp))
                    Directory.Delete(__temp, true);
            }
            catch (IOException)
            {
            }
        }


        private static NotificationMessage Event(NotificationKind kind, String text = "Service stopped")
        {
            return new NotificationMessage()
            {
                Kind = kind,
                Timestamp = new DateTime(2026, 3, 4, 5, 6, 7),
                InstanceName = "HomeLab",
                ServiceTitle = "Backup",
                Text = text,
                Details = "exit code 3"
            };
        }


        private static ScriptConfig Settings(String path, String arguments)
        {
            return new ScriptConfig()
            {
                ScriptPath = path,
                Arguments = arguments,
                WriteJsonToStdin = false,
                PassJsonAsArgument = false,
                TimeoutSeconds = 30
            };
        }


        /// <summary>
        /// A script of the test in the temporary folder. The name carries a space so that the path of
        /// the script has to be quoted in the arguments, which is what a user would do as well.
        /// </summary>
        private String Write(String name, String content)
        {
            var path = Path.Combine(__temp, name);

            File.WriteAllText(path, content);

            return path;
        }


        /// <summary>
        /// A small program of the test itself. The shells of Windows read a backslash in front of a
        /// quote differently, so a test about the arguments of a program needs a program that follows
        /// the rules of the operating system rather than a shell.
        /// </summary>
        private String CompileHook(String name, String source)
        {
            var path = Path.Combine(__temp, name + ".exe");

            var references = new List<MetadataReference>()
            {
                MetadataReference.CreateFromFile(typeof(Object).Assembly.Location),
            };

            var compilation = CSharpCompilation.Create(
                name,
                new[] { CSharpSyntaxTree.ParseText(source) },
                references,
                new CSharpCompilationOptions(OutputKind.ConsoleApplication));

            var result = compilation.Emit(path);

            if (!result.Success)
            {
                var errors = result.Diagnostics
                    .Where(a => a.Severity == DiagnosticSeverity.Error)
                    .Select(a => a.ToString());

                throw new InvalidOperationException("the hook could not be compiled:" + Environment.NewLine +
                    String.Join(Environment.NewLine, errors) + Environment.NewLine + source);
            }

            return path;
        }


        [TestMethod]
        public async Task a_runner_without_program_names_the_settings_of_the_provider()
        {
            var runner = new ScriptRunner(new ScriptConfig());

            var first = await runner.Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsTrue(first.Contains("no program is configured"), "said: " + first);

            Assert.IsNull(await runner.Deliver(Event(NotificationKind.ConsoleError)), "the same mistake is not repeated per event");
        }


        /// <summary>
        /// A program that was deleted or moved is the usual reason for this provider to fail, so the
        /// path belongs to the answer rather than a bare exception of the framework.
        /// </summary>
        [TestMethod]
        public async Task a_program_that_is_not_there_is_reported_with_its_path()
        {
            var missing = Path.Combine(__temp, "does not exist.exe");

            var answer = await new ScriptRunner(Settings(missing, "")).Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsNotNull(answer, "a program that is not there was reported as success");

            Assert.IsTrue(answer.Contains(missing), "said: " + answer);
        }


        /// <summary>
        /// The exit code is the answer of a script, and what it printed to stderr is the reason behind
        /// it. Both belong in one line of the console of the service.
        /// </summary>
        [TestMethod]
        public async Task a_program_that_refuses_the_event_shows_its_exit_code_and_its_answer()
        {
            var answer = await new ScriptRunner(Settings(Cmd, "/c echo boom 1>&2 & exit 3")).Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsNotNull(answer, "a program with exit code 3 was reported as success");

            Assert.IsTrue(answer.Contains("exited with code 3"), "said: " + answer);

            Assert.IsTrue(answer.Contains("boom"), "the answer of the script is missing: " + answer);
        }


        /// <summary>
        /// A script that never returns stands between every event that follows, so the provider ends it
        /// after the configured seconds rather than waiting for a machine that is already down.
        /// </summary>
        [TestMethod]
        public async Task a_program_that_hangs_is_killed_after_the_configured_seconds()
        {
            var config = Settings(Cmd, "/c ping -n 30 127.0.0.1 >nul");

            config.TimeoutSeconds = 1;

            var started = DateTime.Now;

            var answer = await new ScriptRunner(config).Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsNotNull(answer, "a program that hangs was reported as success");

            Assert.IsTrue(answer.Contains("did not finish within 1 seconds and was killed"), "said: " + answer);

            Assert.IsTrue(DateTime.Now - started < TimeSpan.FromSeconds(20), "the wait for the program was not cut short");
        }


        /// <summary>
        /// The program is the file that is started, not a command line handed to a shell. A folder name
        /// with a space and an ampersand in it is what a shared drive is called in real installations,
        /// and an ampersand is what a shell would read as the start of a second command.
        /// </summary>
        [TestMethod]
        public async Task a_path_with_a_space_and_an_ampersand_stays_one_program()
        {
            var folder = Path.Combine(__temp, "hook scripts & more");

            Directory.CreateDirectory(folder);

            var path = Path.Combine(folder, "run hook & check.exe");

            File.Copy(Cmd, path);

            var answer = await new ScriptRunner(Settings(path, "/c exit 7")).Deliver(Event(NotificationKind.ConsoleError));

            Assert.IsNotNull(answer, "the copy of the interpreter was not started at all");

            Assert.IsTrue(answer.Contains("exited with code 7"), "said: " + answer);
        }


        /// <summary>
        /// The event travels as one json object on the input of the program. The script here reads it
        /// back and refuses the event when a field is missing or wrong, which is the only way to see
        /// what really arrived.
        /// </summary>
        [TestMethod]
        public async Task the_event_arrives_as_one_json_object_on_the_input()
        {
            var config = Settings(PowerShell, "-NoProfile -NonInteractive -Command \"$j=[Console]::In.ReadToEnd()|ConvertFrom-Json; if($j.level -ne 'error' -or $j.event -ne 'ConsoleError' -or $j.instance -ne 'HomeLab'){exit 1}\"");

            config.WriteJsonToStdin = true;

            var runner = new ScriptRunner(config);

            Assert.IsNull(await runner.Deliver(Event(NotificationKind.ConsoleError)), "the script did not recognise the event it was given");

            var answer = await runner.Deliver(Event(NotificationKind.ServiceStop));

            Assert.IsNotNull(answer, "the script accepted an event it was told to refuse");

            Assert.IsTrue(answer.Contains("exited with code 1"), "said: " + answer);
        }


        /// <summary>
        /// A script written for the shell of a router takes its event as one argument instead. The
        /// quotes of that argument follow the rules of the operating system, so a name with a space
        /// and an ampersand in it neither splits the line nor splits the object.
        /// </summary>
        [TestMethod]
        public async Task the_event_can_be_handed_over_as_one_argument()
        {
            var script = Write("check event.ps1", "" +
                "if ($args.Count -ne 1) { exit 2 }\n" +
                "$event = $args[0] | ConvertFrom-Json\n" +
                "if ($event.service -ne 'Backup & extra') { exit 1 }\n" +
                "exit 0\n");

            var config = Settings(PowerShell, "-NoProfile -NonInteractive -File \"" + script + "\"");

            config.PassJsonAsArgument = true;

            var message = Event(NotificationKind.ServiceStop, "stopped & restarted");

            message.ServiceTitle = "Backup & extra";

            var answer = await new ScriptRunner(config).Deliver(message);

            Assert.IsNull(answer, "said: " + answer);
        }


        /// <summary>
        /// The same object with a quote inside it is what a service name from a config file can look
        /// like. The hook writes the argument it was given to a file, so the test compares it with the
        /// object the provider produced instead of trusting how a shell would have read the line.
        /// </summary>
        [TestMethod]
        public async Task an_event_with_a_quote_in_it_stays_one_argument()
        {
            var received = Path.Combine(__temp, "received.txt");

            var hook = CompileHook("argument hook", "" +
                "using System;\n" +
                "using System.IO;\n" +
                "class Hook\n" +
                "{\n" +
                "    static int Main(string[] args)\n" +
                "    {\n" +
                "        if (args.Length != 3) { return 2; }\n" +
                "        File.WriteAllText(args[1], args[2]);\n" +
                "        return 0;\n" +
                "    }\n" +
                "}\n");

            var config = Settings(hook, "/out \"" + received + "\"");

            config.PassJsonAsArgument = true;

            var message = Event(NotificationKind.ServiceStop);

            message.ServiceTitle = "Back\"up";

            var answer = await new ScriptRunner(config).Deliver(message);

            Assert.IsNull(answer, "the hook refused the event: " + answer);

            Assert.AreEqual(ScriptPayload.ToJson(message), File.ReadAllText(received));
        }
    }
}
