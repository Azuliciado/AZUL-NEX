using System.Diagnostics;
using System.IO;
namespace AzulNex.Core;
public sealed class AndroidSdkService
{
    public string? SdkPath { get; }
    public string? EmulatorPath => FindTool("emulator.exe", "emulator");
    public string? AdbPath => FindTool("adb.exe", "platform-tools");
    public AndroidSdkService()
    {
        var candidates = new[] { Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT"), Environment.GetEnvironmentVariable("ANDROID_HOME"), Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Android", "Sdk") };
        SdkPath = candidates.FirstOrDefault(p => !string.IsNullOrWhiteSpace(p) && Directory.Exists(p));
    }
    private string? FindTool(string file, string subfolder) { if (SdkPath is null) return null; var p = Path.Combine(SdkPath, subfolder, file); return File.Exists(p) ? p : null; }
    public async Task<IReadOnlyList<string>> ListAvdsAsync()
    {
        if (EmulatorPath is null) return Array.Empty<string>();
        var result = await RunAsync(EmulatorPath, "-list-avds", 10000);
        return result.ExitCode == 0 ? result.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) : Array.Empty<string>();
    }
    public async Task<string> StartAvdAsync(string name)
    {
        if (EmulatorPath is null) throw new InvalidOperationException("Android Emulator was not found. Install it from Android SDK Manager.");
        _ = Process.Start(new ProcessStartInfo(EmulatorPath, $"-avd \"{name.Replace("\"", "") }\"") { UseShellExecute = false, CreateNoWindow = true });
        await Task.Delay(800);
        return $"Launch requested for {name}. Boot may take several minutes on older hardware.";
    }
    public async Task<IReadOnlyList<string>> RunningAsync()
    {
        if (AdbPath is null) return Array.Empty<string>();
        var r = await RunAsync(AdbPath, "devices", 10000);
        return r.Output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Skip(1).Where(x => x.EndsWith("\tdevice", StringComparison.Ordinal)).Select(x => x.Split('\t')[0]).ToArray();
    }
    public async Task<string> StopAsync(string serial) { var r = await AdbAsync(serial, "emu kill"); return r.ExitCode == 0 ? $"Stop requested for {serial}." : r.Output; }
    public async Task<string> KeyAsync(string serial, string key)
    {
        var code = key switch { "Back" => "4", "Home" => "3", "Recent" => "187", "Volume Up" => "24", "Volume Down" => "25", _ => throw new ArgumentException("Unsupported key") };
        var r = await AdbAsync(serial, $"shell input keyevent {code}"); return r.ExitCode == 0 ? $"Sent {key} to {serial}." : r.Output;
    }
    public async Task<string> InstallApkAsync(string serial, string path)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("APK file not found", path);
        var r = await AdbAsync(serial, $"install -r \"{path.Replace("\"", "") }\"");
        if (r.ExitCode != 0) throw new InvalidOperationException(r.Output); return r.Output.Trim();
    }
    private Task<(int ExitCode, string Output)> AdbAsync(string serial, string args) => AdbPath is null ? throw new InvalidOperationException("ADB was not found. Install Android SDK Platform-Tools.") : RunAsync(AdbPath, $"-s {serial} {args}", 60000);
    private static async Task<(int ExitCode, string Output)> RunAsync(string exe, string args, int timeout)
    {
        var psi = new ProcessStartInfo(exe, args) { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
        using var p = Process.Start(psi) ?? throw new InvalidOperationException("Could not start Android SDK tool.");
        var stdout = p.StandardOutput.ReadToEndAsync(); var stderr = p.StandardError.ReadToEndAsync(); using var cts = new CancellationTokenSource(timeout);
        try { await p.WaitForExitAsync(cts.Token); } catch (OperationCanceledException) { p.Kill(true); throw new TimeoutException("Android SDK command timed out."); }
        return (p.ExitCode, (await stdout) + Environment.NewLine + (await stderr));
    }
}