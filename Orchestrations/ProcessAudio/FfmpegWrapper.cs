using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace vp.orchestrations.processaudio
{
    static class FfmpegWrapper
    {
        public static async Task Transcode(string inputPath, string ffmpegParams, string outputFile, ILogger log)
        {
            //TODO: Move the duration to the configuration (or make it a parameter)
            var prefix = $"-ss 00:00:00 -to 00:00:30 -i \"{inputPath}\" ";
            var arguments = $"{prefix}{ffmpegParams} \"{outputFile}\"";
            await RunFfmpeg(arguments, log);
        }

        private static string GetFfmpegPath()
        {
            return Path.Combine(GetAssemblyDirectory(), Config.Home);
        }

        public static string GetAssemblyDirectory()
        {
            var codeBase = typeof(FfmpegWrapper).Assembly.Location;
            var uri = new UriBuilder(codeBase);
            var path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }

        private static async Task RunFfmpeg(string arguments, ILogger log)
        {
            var ffmpegPath = GetFfmpegPath();
            var processStartInfo = new ProcessStartInfo(ffmpegPath, arguments);
            processStartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            processStartInfo.CreateNoWindow = true;
            processStartInfo.UseShellExecute = false;
            processStartInfo.RedirectStandardError = true;
            var sb = new StringBuilder();
            var p = new Process();
            p.StartInfo = processStartInfo;
            p.ErrorDataReceived += (s, a) => sb.AppendLine(a.Data);
            p.EnableRaisingEvents = true;

            p.Start();
            p.BeginErrorReadLine();

            await p.WaitForExitAsync();
            if (p.ExitCode != 0)
            {
                log.LogError(sb.ToString());
                throw new InvalidOperationException($"Ffmpeg failed with exit code {p.ExitCode}");
            }
        }

        public static Task WaitForExitAsync(this Process process,
            CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if (cancellationToken != default)
                cancellationToken.Register(tcs.SetCanceled);

            return tcs.Task;
        }
    }
}

