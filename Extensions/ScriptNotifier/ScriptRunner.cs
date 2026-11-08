using ServiceManager.Extensions.Interfaces;
using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace ServiceManager.Extensions.ScriptNotifier
{
    /// <summary>
    /// Starts the program and waits for it. A separate class because the extension itself reads its
    /// settings from the folder it was loaded from, which a test cannot change.
    /// </summary>
    public class ScriptRunner
    {
        private readonly Config.Config __config;

        private bool __missingPathReported;


        public ScriptRunner(Config.Config config)
        {
            __config = config ?? new Config.Config();
        }


        /// <summary>
        /// Runs the program once and answers null when it accepted the event, otherwise the reason. A
        /// program that is not there or that refuses the event must not throw over the service.
        /// </summary>
        public async Task<String> Deliver(NotificationMessage message)
        {
            if (message == null)
                return null;

            var path = (__config.ScriptPath ?? "").Trim();

            if (path == "")
                return ReportMissing(ref __missingPathReported, "no program is configured, open the settings of the Script provider");

            __missingPathReported = false;

            var seconds = __config.TimeoutSeconds;

            if (seconds <= 0)
                seconds = 30;

            var json = ScriptPayload.ToJson(message);

            var arguments = (__config.Arguments ?? "").Trim();

            if (__config.PassJsonAsArgument)
                arguments = (arguments + " " + Quote(json)).Trim();

            try
            {
                return await Run(path, arguments, json, seconds).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // The usual case is a path that does not exist any more, which is worth saying plainly.
                return ex.Message + " (" + path + ")";
            }
        }


        private async Task<String> Run(String path, String arguments, String json, int seconds)
        {
            // No shell in between: the program is the file that is started and the arguments are the
            // arguments. A path with a space or an ampersand in it stays one path.
            var info = new ProcessStartInfo
            {
                FileName = path,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardInput = __config.WriteJsonToStdin,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            var workdir = (__config.WorkingDirectory ?? "").Trim();

            if (workdir != "")
                info.WorkingDirectory = workdir;

            using (var process = new Process())
            {
                process.StartInfo = info;

                if (!process.Start())
                    return "the program could not be started (" + path + ")";

                var exited = new TaskCompletionSource<bool>();

                process.EnableRaisingEvents = true;
                process.Exited += (s, e) => exited.TrySetResult(true);

                if (process.HasExited)
                    exited.TrySetResult(true);

                var output = process.StandardOutput.ReadToEndAsync();
                var error = process.StandardError.ReadToEndAsync();

                Observe(output);
                Observe(error);

                var stdinError = await WriteStdin(process, json, path).ConfigureAwait(false);

                if (await Task.WhenAny(exited.Task, Task.Delay(seconds * 1000)).ConfigureAwait(false) != exited.Task)
                {
                    // A program that never returns would otherwise stand between every event that
                    // follows. Only the process itself is killed, what it started has to finish itself.
                    try { process.Kill(); } catch { }

                    return "the program did not finish within " + seconds + " seconds and was killed (" + path + ")";
                }

                await exited.Task.ConfigureAwait(false);

                var code = process.ExitCode;

                var drained = Task.WhenAll(output, error);

                if (await Task.WhenAny(drained, Task.Delay(2000)).ConfigureAwait(false) != drained)
                {
                    // The pipes are still open behind the program, so what it printed is lost. Worth a
                    // line, but the exit code is the statement.
                    return "the program exited with code " + code + " and did not close its output (" + path + ")";
                }

                if (code == 0)
                    return stdinError;

                var answer = OneLine(error.Result);

                if (answer == "")
                    answer = OneLine(output.Result);

                var reason = answer == "" ? "" : ": " + Cut(answer, 300);

                return "the program exited with code " + code + reason + " (" + path + ")";
            }
        }


        private static async Task<String> WriteStdin(Process process, String json, String path)
        {
            if (!process.StartInfo.RedirectStandardInput)
                return null;

            try
            {
                await process.StandardInput.WriteAsync(json).ConfigureAwait(false);
                await process.StandardInput.FlushAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // A program that closes its input before reading it still may have done its work, so
                // this is only worth a line when the exit code says nothing else.
                return "the program did not read its input: " + ex.Message + " (" + path + ")";
            }
            finally
            {
                // Without the end of the input most programs keep waiting for the next line forever.
                try { process.StandardInput.Close(); } catch { }
            }

            return null;
        }


        /// <summary>
        /// Quotes a value the way the operating system reads a command line. Backslashes only matter in
        /// front of a quote, so a JSON with a quote inside it needs the backslashes of that quote doubled
        /// as well. Writing the value between two quotes alone would end the argument one character too
        /// early, and a value that ends with a backslash would quote the closing quote. A batch file reads
        /// the same text differently, which is why the input stays the default.
        /// </summary>
        private static String Quote(String value)
        {
            var builder = new StringBuilder();

            builder.Append('"');

            var backslashes = 0;

            foreach (var character in value)
            {
                if (character == '\\')
                {
                    backslashes++;
                    continue;
                }

                if (character == '"')
                {
                    AddBackslashes(builder, backslashes * 2 + 1);
                    builder.Append('"');
                    backslashes = 0;
                    continue;
                }

                AddBackslashes(builder, backslashes);
                builder.Append(character);
                backslashes = 0;
            }

            AddBackslashes(builder, backslashes * 2);
            builder.Append('"');

            return builder.ToString();
        }


        private static void AddBackslashes(StringBuilder builder, int count)
        {
            for (var index = 0; index < count; index++)
                builder.Append('\\');
        }


        private static void Observe(Task task)
        {
            task.ContinueWith(t => { var ignored = t.Exception; }, TaskContinuationOptions.OnlyOnFaulted);
        }


        /// <summary>
        /// The answer of a program is only interesting in one line, a stack trace of a script would
        /// otherwise fill the console of the service.
        /// </summary>
        private static String OneLine(String value)
        {
            if (String.IsNullOrEmpty(value))
                return "";

            return value.Replace("\r\n", " ").Replace('\r', ' ').Replace('\n', ' ').Trim();
        }


        private static String Cut(String value, int max)
        {
            if (value == null || value.Length <= max)
                return value ?? "";

            return value.Substring(0, max);
        }


        /// <summary>
        /// A provider without a program is a configuration mistake, and one line per event would bury
        /// the console in it. The message appears again as soon as something ran successfully.
        /// </summary>
        private static String ReportMissing(ref bool reported, String message)
        {
            if (reported)
                return null;

            reported = true;

            return message;
        }
    }
}
