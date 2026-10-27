using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>Result of a single <c>adb</c> invocation.</summary>
    internal struct AdbResult
    {
        internal bool LaunchFailed;
        internal int ExitCode;
        internal string Output;
        internal string Error;

        internal bool Succeeded => !LaunchFailed && ExitCode == 0;

        internal string Combined
        {
            get
            {
                string output = Output ?? string.Empty;
                string error = Error ?? string.Empty;

                if (string.IsNullOrEmpty(error))
                {
                    return output;
                }

                if (string.IsNullOrEmpty(output))
                {
                    return error;
                }

                return output + Environment.NewLine + error;
            }
        }
    }

    /// <summary>Ports advertised by a device over mDNS while wireless debugging is on.</summary>
    internal sealed class AdbMdnsServices
    {
        /// <summary>Device IP, or an empty string when nothing was discovered.</summary>
        internal string IpAddress = string.Empty;

        /// <summary>Port to use with <c>adb pair</c>. Only valid while the pairing dialog is open.</summary>
        internal int PairingPort;

        /// <summary>Port to use with <c>adb connect</c>.</summary>
        internal int ConnectPort;

        internal bool HasPairing => PairingPort > 0;

        internal bool HasConnect => ConnectPort > 0;

        internal AdbMdnsServices Clone()
        {
            return new AdbMdnsServices
            {
                IpAddress = IpAddress,
                PairingPort = PairingPort,
                ConnectPort = ConnectPort
            };
        }

        public override string ToString()
        {
            return $"ip={IpAddress} pairing={PairingPort} connect={ConnectPort}";
        }
    }

    /// <summary>
    /// Locates the Android platform-tools <c>adb</c> executable, runs it, and parses
    /// the mDNS service list that exposes wireless-debugging pairing and connect ports.
    /// </summary>
    internal static class AdbCommandRunner
    {
        private const int DefaultTimeoutMs = 20000;

        private static readonly Regex AddressPattern =
            new Regex(@"(\d{1,3}(?:\.\d{1,3}){3}):(\d{1,5})", RegexOptions.Compiled);

        /// <summary>
        /// Explains the most recent failure in a way that points at the fix, or
        /// returns an empty string when the last call succeeded.
        /// </summary>
        internal static string LastHint { get; private set; } = string.Empty;

        private static string _cachedPath = string.Empty;

        /// <summary>Configured or auto-detected path to the adb executable.</summary>
        internal static string AdbPath
        {
            get
            {
                string configured = EditorPrefs.GetString(AdbWirelessDebuggerWindow.Prefs.AdbPath, string.Empty);

                if (!string.IsNullOrEmpty(configured) && File.Exists(configured))
                {
                    return configured;
                }

                if (!string.IsNullOrEmpty(_cachedPath) && File.Exists(_cachedPath))
                {
                    return _cachedPath;
                }

                _cachedPath = DetectAdbPath();
                return _cachedPath;
            }
            set => EditorPrefs.SetString(AdbWirelessDebuggerWindow.Prefs.AdbPath, value ?? string.Empty);
        }

        /// <summary>Forces the auto-detection to run again, for example after installing platform-tools.</summary>
        internal static void ClearCache()
        {
            _cachedPath = string.Empty;
        }

        /// <summary>Runs adb with the given arguments and captures stdout/stderr.</summary>
        internal static AdbResult Run(string arguments, int timeoutMs = DefaultTimeoutMs)
        {
            LastHint = string.Empty;

            var result = new AdbResult { ExitCode = -1 };
            string adbPath = AdbPath;

            if (string.IsNullOrEmpty(adbPath) || !File.Exists(adbPath))
            {
                result.LaunchFailed = true;
                result.Error = "adb was not found. Set the path at the bottom of the Wireless ADB window.";
                LastHint = "Install Android SDK platform-tools, or point the window at your adb executable.";
                return result;
            }

            try
            {
                var startInfo = new ProcessStartInfo
                {
                    FileName = adbPath,
                    Arguments = arguments,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

                var output = new StringBuilder();
                var error = new StringBuilder();

                using (Process process = new Process { StartInfo = startInfo })
                {
                    process.OutputDataReceived += (_, eventArgs) =>
                    {
                        if (eventArgs.Data != null)
                        {
                            output.AppendLine(eventArgs.Data);
                        }
                    };

                    process.ErrorDataReceived += (_, eventArgs) =>
                    {
                        if (eventArgs.Data != null)
                        {
                            error.AppendLine(eventArgs.Data);
                        }
                    };

                    if (!process.Start())
                    {
                        result.LaunchFailed = true;
                        result.Error = "Could not start adb.";
                        return result;
                    }

                    // Read asynchronously so a hung adb cannot block the editor forever.
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    if (!process.WaitForExit(timeoutMs))
                    {
                        try
                        {
                            process.Kill();
                        }
                        catch
                        {
                            // The process may have exited between the check and the kill.
                        }

                        result.LaunchFailed = true;
                        result.Error = $"adb {arguments} did not finish within {timeoutMs / 1000} seconds.";
                        return result;
                    }

                    // Let the async readers drain.
                    process.WaitForExit();

                    result.Output = output.ToString().Trim();
                    result.Error = error.ToString().Trim();
                    result.ExitCode = process.ExitCode;
                }
            }
            catch (Exception exception)
            {
                result.LaunchFailed = true;
                result.Error = exception.Message;
            }

            return result;
        }

        /// <summary>
        /// Asks adb for the mDNS services it can see. While the phone's "Pair device
        /// with pairing code" dialog is open this reports both the pairing port and
        /// the connect port, so the user only has to supply the six-digit code.
        /// </summary>
        internal static AdbMdnsServices DiscoverServices()
        {
            var services = new AdbMdnsServices();
            AdbResult result = Run("mdns services", 10000);

            if (result.LaunchFailed)
            {
                LastHint = result.Error;
                return services;
            }

            string combined = result.Combined;

            if (string.IsNullOrEmpty(combined))
            {
                return services;
            }

            foreach (string rawLine in combined.Split('\n'))
            {
                string line = rawLine.Trim();

                if (line.Length == 0)
                {
                    continue;
                }

                bool isPairing = line.IndexOf("_adb-tls-pairing", StringComparison.OrdinalIgnoreCase) >= 0;
                bool isConnect = line.IndexOf("_adb-tls-connect", StringComparison.OrdinalIgnoreCase) >= 0;

                if (!isPairing && !isConnect)
                {
                    continue;
                }

                Match match = AddressPattern.Match(line);

                if (!match.Success || !int.TryParse(match.Groups[2].Value, out int port) || port <= 0)
                {
                    continue;
                }

                if (string.IsNullOrEmpty(services.IpAddress))
                {
                    services.IpAddress = match.Groups[1].Value;
                }

                if (isPairing)
                {
                    services.PairingPort = port;
                }
                else
                {
                    services.ConnectPort = port;
                }
            }

            return services;
        }

        /// <summary>True when the output reports a successful pair or connect.</summary>
        internal static bool OutputIndicatesSuccess(AdbResult result, string verb)
        {
            if (result.LaunchFailed)
            {
                return false;
            }

            string text = result.Combined.ToLowerInvariant();

            if (text.Contains("failed") || text.Contains("error") || text.Contains("cannot"))
            {
                return false;
            }

            if (verb == "pair")
            {
                return text.Contains("successfully paired") || text.Contains("already paired");
            }

            return text.Contains("connected to") || text.Contains("already connected");
        }

        /// <summary>Splits an <c>ip:port</c> pair, returning false when the format is wrong.</summary>
        internal static bool TryParseAddress(string value, out string ip, out int port)
        {
            ip = string.Empty;
            port = 0;

            Match match = AddressPattern.Match(value ?? string.Empty);

            if (!match.Success)
            {
                return false;
            }

            ip = match.Groups[1].Value;
            return int.TryParse(match.Groups[2].Value, out port) && port > 0;
        }

        /// <summary>
        /// Finds adb without user input. Checks the environment SDK locations first,
        /// then the Android module bundled with the running Unity editor, then PATH.
        /// </summary>
        internal static string DetectAdbPath()
        {
            string executable = Application.platform == RuntimePlatform.WindowsEditor ? "adb.exe" : "adb";

            var roots = new List<string>();

            AddCandidate(roots, Environment.GetEnvironmentVariable("ANDROID_HOME"));
            AddCandidate(roots, Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"));
            AddCandidate(roots, Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Android", "Sdk"));
            AddCandidate(roots, Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "Library", "Android", "sdk"));

            // android-sdk / sdk folders commonly living beside the home directory.
            AddCandidate(roots, Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                "AppData", "Local", "Android", "Sdk"));

            foreach (string root in roots)
            {
                string candidate = Path.Combine(root, "platform-tools", executable);

                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            // The Android module installed through Unity Hub ships its own SDK.
            string unityBundled = UnityBundledAdb(executable);

            if (!string.IsNullOrEmpty(unityBundled))
            {
                return unityBundled;
            }

            return FromPathVariable(executable);
        }

        private static string UnityBundledAdb(string executable)
        {
            try
            {
                string dataPath = EditorApplication.applicationContentsPath;

                if (string.IsNullOrEmpty(dataPath))
                {
                    return string.Empty;
                }

                string candidate = Path.Combine(
                    dataPath, "PlaybackEngines", "AndroidPlayer", "SDK", "platform-tools", executable);

                return File.Exists(candidate) ? candidate : string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static string FromPathVariable(string executable)
        {
            string pathVariable = Environment.GetEnvironmentVariable("PATH");

            if (string.IsNullOrEmpty(pathVariable))
            {
                return string.Empty;
            }

            foreach (string entry in pathVariable.Split(Path.PathSeparator))
            {
                if (string.IsNullOrWhiteSpace(entry))
                {
                    continue;
                }

                string direct = Path.Combine(entry.Trim(), executable);

                if (File.Exists(direct))
                {
                    return direct;
                }

                string inPlatformTools = Path.Combine(entry.Trim(), "platform-tools", executable);

                if (File.Exists(inPlatformTools))
                {
                    return inPlatformTools;
                }
            }

            return string.Empty;
        }

        private static void AddCandidate(List<string> roots, string root)
        {
            if (string.IsNullOrWhiteSpace(root))
            {
                return;
            }

            if (!roots.Contains(root))
            {
                roots.Add(root);
            }
        }

        /// <summary>Writes an adb invocation and its output into the supplied log.</summary>
        internal static string Describe(string arguments, AdbResult result)
        {
            string combined = result.Combined;

            if (string.IsNullOrEmpty(combined))
            {
                combined = result.LaunchFailed ? result.Error : "(no output)";
            }

            return $"> adb {arguments}{Environment.NewLine}{combined}";
        }

        /// <summary>Logs a driver-level failure once, with the hint attached.</summary>
        internal static void LogLaunchFailure(AdbResult result)
        {
            if (result.LaunchFailed)
            {
                Debug.LogError($"Wireless ADB: {result.Error}");
            }
        }
    }
}
