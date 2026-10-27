using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Adeel.UnityToolbox.EditorTools
{
    /// <summary>
    /// Builds an APK, installs it on the connected Android device and launches it —
    /// without going through Unity's "Run Device" dropdown.
    /// </summary>
    /// <remarks>
    /// The Build Settings "Run Device" list is populated and refreshed on Unity's own
    /// schedule, and because a wireless device's port changes every time debugging
    /// is re-enabled, a saved selection goes stale. Querying <c>adb devices</c>
    /// directly always reflects reality, so this path needs no selection at all.
    /// </remarks>
    internal static class AdbDeviceDeployer
    {
        private const int InstallTimeoutMs = 300000;

        /// <summary>Builds, installs and launches the current project on a device.</summary>
        internal static void BuildAndRun(Action<string> log)
        {
            if (log == null)
            {
                log = _ => { };
            }

            if (EditorApplication.isPlaying)
            {
                log("Exit Play mode before building.");
                return;
            }

            if (!EnsureAndroidTarget(log))
            {
                return;
            }

            string[] scenes = EnabledScenes();

            if (scenes.Length == 0)
            {
                log("No scenes are enabled in Build Settings — add at least one.");
                return;
            }

            if (!TryResolveDevice(out string serial, log))
            {
                return;
            }

            string apkPath = ApkPath();
            string directory = Path.GetDirectoryName(apkPath);

            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // adb install needs an APK, so an App Bundle build would produce nothing usable.
            bool previousBundleSetting = EditorUserBuildSettings.buildAppBundle;
            EditorUserBuildSettings.buildAppBundle = false;

            BuildReport report;

            try
            {
                log($"Building APK → {apkPath} (the editor is blocked until this finishes)");

                report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = apkPath,
                    target = BuildTarget.Android,
                    targetGroup = BuildTargetGroup.Android,
                    options = BuildOptions.None
                });
            }
            finally
            {
                EditorUserBuildSettings.buildAppBundle = previousBundleSetting;
            }

            if (report == null || report.summary.result != BuildResult.Succeeded)
            {
                log($"Build failed: {(report == null ? "no build report" : report.summary.result.ToString())}");
                return;
            }

            log($"Built in {report.summary.totalTime.TotalSeconds:F0}s, " +
                $"{report.summary.totalSize / (1024f * 1024f):F1} MB");

            string installArguments = $"-s {serial} install -r \"{apkPath}\"";
            AdbResult install = AdbCommandRunner.Run(installArguments, InstallTimeoutMs);
            log(AdbCommandRunner.Describe(installArguments, install));

            if (!install.Combined.ToLowerInvariant().Contains("success"))
            {
                log("Install failed. If the app is already installed with a different signature, uninstall it first.");
                return;
            }

            string identifier = PlayerSettings.applicationIdentifier;

            if (string.IsNullOrEmpty(identifier))
            {
                log("No Android package name configured (Player Settings > Other Settings > Package Name).");
                return;
            }

            string launchArguments =
                $"-s {serial} shell monkey -p {identifier} -c android.intent.category.LAUNCHER 1";
            AdbResult launch = AdbCommandRunner.Run(launchArguments, 30000);
            log(AdbCommandRunner.Describe(launchArguments, launch));
            log($"Launched {identifier} on {serial}.");
        }

        /// <summary>
        /// Picks the serial to target: the remembered wireless device when it is still
        /// attached, otherwise the first device adb reports as ready.
        /// </summary>
        internal static bool TryResolveDevice(out string serial, Action<string> log)
        {
            serial = string.Empty;

            AdbResult result = AdbCommandRunner.Run("devices");

            if (result.LaunchFailed)
            {
                log(result.Error);
                return false;
            }

            var ready = new List<string>();
            var unauthorized = new List<string>();

            foreach (string rawLine in result.Combined.Split('\n'))
            {
                string line = rawLine.Trim();

                if (line.Length == 0 || line.StartsWith("List of devices", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string[] parts = line.Split('\t');

                if (parts.Length < 2)
                {
                    parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                }

                if (parts.Length < 2)
                {
                    continue;
                }

                string deviceSerial = parts[0].Trim();
                string state = parts[1].Trim();

                if (state == "device")
                {
                    ready.Add(deviceSerial);
                }
                else if (state.StartsWith("unauthorized", StringComparison.OrdinalIgnoreCase))
                {
                    unauthorized.Add(deviceSerial);
                }
            }

            if (ready.Count == 0)
            {
                log(unauthorized.Count > 0
                    ? $"Device {unauthorized[0]} is unauthorized — accept the \"Allow USB debugging\" prompt on the phone."
                    : "No device ready. Pair and connect one from the Wireless ADB window first.");
                return false;
            }

            // Prefer the wireless device the user last connected.
            string rememberedIp = EditorPrefs.GetString(AdbWirelessDebuggerWindow.Prefs.Ip, string.Empty);
            string rememberedPort = EditorPrefs.GetString(AdbWirelessDebuggerWindow.Prefs.ConnectPort, string.Empty);

            if (!string.IsNullOrEmpty(rememberedIp) && !string.IsNullOrEmpty(rememberedPort))
            {
                string remembered = $"{rememberedIp}:{rememberedPort}";

                if (ready.Contains(remembered))
                {
                    serial = remembered;
                    return true;
                }
            }

            serial = ready[0];
            return true;
        }

        private static bool EnsureAndroidTarget(Action<string> log)
        {
            if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
            {
                return true;
            }

            log($"Switching build target from {EditorUserBuildSettings.activeBuildTarget} to Android " +
                "(assets will reimport)…");

            bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(
                BuildTargetGroup.Android, BuildTarget.Android);

            if (!switched)
            {
                log("Could not switch to Android. Is the Android Build Support module installed?");
                return false;
            }

            return true;
        }

        private static string[] EnabledScenes()
        {
            return EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();
        }

        private static string ApkPath()
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            string product = SanitizeFileName(PlayerSettings.productName);

            if (string.IsNullOrEmpty(product))
            {
                product = "app";
            }

            return Path.Combine(projectRoot, "Builds", "Android", product + ".apk");
        }

        private static string SanitizeFileName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            char[] invalid = Path.GetInvalidFileNameChars();
            return new string(value.Where(character => !invalid.Contains(character)).ToArray()).Trim();
        }
    }
}
