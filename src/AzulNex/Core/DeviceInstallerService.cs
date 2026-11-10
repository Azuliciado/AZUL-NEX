using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace AzulNex.Core;

public sealed record EmulatorDeviceProfile(string Name, string Id);

public sealed class DeviceInstallerService
{
    public string? SdkPath { get; }

    private string? SdkManagerPath => FindCommandLineTool("sdkmanager.bat");
    private string? AvdManagerPath => FindCommandLineTool("avdmanager.bat");

    public DeviceInstallerService()
    {
        var candidates = new[]
        {
            Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"),
            Environment.GetEnvironmentVariable("ANDROID_HOME"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk")
        };

        SdkPath = candidates.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p) && Directory.Exists(p));
    }

    private string? FindCommandLineTool(string fileName)
    {
        if (SdkPath is null) return null;

        var root = Path.Combine(SdkPath, "cmdline-tools");
        if (!Directory.Exists(root)) return null;

        var latest = Path.Combine(root, "latest", "bin", fileName);
        if (File.Exists(latest)) return latest;

        return Directory.GetDirectories(root)
            .OrderByDescending(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .Select(directory => Path.Combine(directory, "bin", fileName))
            .FirstOrDefault(File.Exists);
    }

    public async Task<IReadOnlyList<EmulatorDeviceProfile>> ListDeviceProfilesAsync()
    {
        if (SdkPath is null)
            throw new InvalidOperationException("Android SDK was not found. Install Android Studio or set ANDROID_SDK_ROOT.");

        var avdManager = AvdManagerPath;
        if (avdManager is null)
            throw new InvalidOperationException("Android SDK Command-line Tools are missing. Open Android Studio > SDK Manager > SDK Tools and install Android SDK Command-line Tools (latest).");

        var result = await RunBatchAsync(avdManager, $"--sdk_root=\"{SdkPath}\" list device", null, 60000, null);
        if (result.ExitCode != 0)
            throw new InvalidOperationException("Could not read Android hardware profiles. " + TrimOutput(result.Output));

        var devices = new List<EmulatorDeviceProfile>();
        string? currentId = null;
        string? currentName = null;

        foreach (var line in result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var idMatch = Regex.Match(line, @"id:\s*(?:\d+\s+or\s+)?\"(?<id>[^\"]+)\"", RegexOptions.IgnoreCase);
            if (idMatch.Success)
            {
                if (currentId is not null && currentName is not null)
                    devices.Add(new EmulatorDeviceProfile(currentName.Trim(), currentId.Trim()));

                currentId = idMatch.Groups["id"].Value;
                currentName = null;
                continue;
            }

            if (currentId is not null && line.TrimStart().StartsWith("Name:", StringComparison.OrdinalIgnoreCase))
                currentName = line[(line.IndexOf(':') + 1)..].Trim();
        }

        if (currentId is not null && currentName is not null)
            devices.Add(new EmulatorDeviceProfile(currentName.Trim(), currentId.Trim()));

        return devices
            .Where(d => !string.IsNullOrWhiteSpace(d.Id) && !string.IsNullOrWhiteSpace(d.Name))
            .GroupBy(d => d.Id, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<string> InstallAsync(
        EmulatorDeviceProfile device,
        int api,
        string avdName,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (SdkPath is null)
            throw new InvalidOperationException("Android SDK was not found. Install Android Studio or set ANDROID_SDK_ROOT.");

        var sdkManager = SdkManagerPath;
        var avdManager = AvdManagerPath;
        if (sdkManager is null || avdManager is null)
            throw new InvalidOperationException("Android SDK Command-line Tools are required. Install Android SDK Command-line Tools (latest) from Android Studio > SDK Manager > SDK Tools.");

        if (api is < 33 or > 36)
            throw new ArgumentOutOfRangeException(nameof(api), "Choose a supported Android API version from the installer.");

        if (!Regex.IsMatch(avdName, "^[A-Za-z0-9_.-]{1,40}$"))
            throw new ArgumentException("The virtual device name contains unsupported characters.", nameof(avdName));

        var imagePackage = $"system-images;android-{api};google_apis;x86_64";

        progress?.Report("Step 1 of 3 — Reviewing Android SDK component licenses...");
        var licenses = await RunBatchAsync(
            sdkManager,
            $"--sdk_root=\"{SdkPath}\" --licenses",
            string.Concat(Enumerable.Repeat("y\n", 40)),
            180000,
            progress,
            cancellationToken);

        if (licenses.ExitCode != 0)
            throw new InvalidOperationException("Android SDK license review did not complete. " + TrimOutput(licenses.Output));

        progress?.Report($"Step 2 of 3 — Downloading Android {api} system image and SDK platform. This may take a while...");
        var packages = await RunBatchAsync(
            sdkManager,
            $"--sdk_root=\"{SdkPath}\" \"platform-tools\" \"platforms;android-{api}\" \"{imagePackage}\"",
            null,
            1200000,
            progress,
            cancellationToken);

        if (packages.ExitCode != 0)
            throw new InvalidOperationException("Android SDK package installation failed. " + TrimOutput(packages.Output));

        progress?.Report($"Step 3 of 3 — Creating {device.Name} with Android API {api}...");
        var avd = await RunBatchAsync(
            avdManager,
            $"--sdk_root=\"{SdkPath}\" create avd -n \"{avdName}\" -k \"{imagePackage}\" -d \"{device.Id}\"",
            "no\n",
            120000,
            progress,
            cancellationToken);

        if (avd.ExitCode != 0)
            throw new InvalidOperationException("The Android image was installed, but AVD creation failed. " + TrimOutput(avd.Output));

        return $"Installed successfully: {avdName}. Android {api} (Google APIs, x86_64) is ready. Open My Devices and launch it when you are ready.";
    }

    private static async Task<(int ExitCode, string Output)> RunBatchAsync(
        string batchFile,
        string arguments,
        string? standardInput,
        int timeoutMilliseconds,
        IProgress<string>? progress,
        CancellationToken cancellationToken = default)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/d /s /c \"\"{batchFile}\" {arguments}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = standardInput is not null,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = new Process { StartInfo = startInfo };
        if (!process.Start())
            throw new InvalidOperationException("Could not start an Android SDK command-line tool.");

        var output = new StringBuilder();
        var outputTask = CaptureAsync(process.StandardOutput, output, progress);
        var errorTask = CaptureAsync(process.StandardError, output, progress);

        if (standardInput is not null)
        {
            await process.StandardInput.WriteAsync(standardInput);
            process.StandardInput.Close();
        }

        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(timeoutMilliseconds);

        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            try { process.Kill(true); } catch { }
            cancellationToken.ThrowIfCancellationRequested();
            throw new TimeoutException("The Android SDK command timed out. Check your internet connection and SDK installation, then try again.");
        }

        await Task.WhenAll(outputTask, errorTask);
        return (process.ExitCode, output.ToString());
    }

    private static async Task CaptureAsync(StreamReader reader, StringBuilder output, IProgress<string>? progress)
    {
        while (await reader.ReadLineAsync() is { } line)
        {
            lock (output) output.AppendLine(line);
            if (!string.IsNullOrWhiteSpace(line))
                progress?.Report(line.Length > 180 ? line[..180] : line);
        }
    }

    private static string TrimOutput(string output)
    {
        var trimmed = output.Trim();
        return trimmed.Length <= 1800 ? trimmed : trimmed[^1800..];
    }
}