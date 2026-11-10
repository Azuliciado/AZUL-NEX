using AzulNex.Core;
using System.Windows;
using Microsoft.Win32;

namespace AzulNex;

public partial class MainWindow : Window
{
    private readonly AndroidSdkService _sdk = new();

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await RefreshAsync();
    }

    private async Task RefreshAsync()
    {
        SdkStatus.Text = _sdk.SdkPath is null ? "Not detected" : "Detected";
        var avds = await _sdk.ListAvdsAsync();
        AvdList.ItemsSource = avds;
        AvdCount.Text = avds.Count.ToString();
        var running = await _sdk.RunningAsync();
        RunningList.ItemsSource = running;
        RunningCount.Text = running.Count.ToString();
        StatusText.Text = _sdk.SdkPath is null
            ? "Install Android SDK and set ANDROID_SDK_ROOT if needed."
            : $"SDK: {_sdk.SdkPath}";
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        try { await RefreshAsync(); } catch (Exception ex) { ShowError(ex); }
    }

    private async void Launch_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (AvdList.SelectedItem is not string name)
                throw new InvalidOperationException("Select an AVD first.");
            StatusText.Text = await _sdk.StartAvdAsync(name);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void Stop_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (RunningList.SelectedItem is not string serial)
                throw new InvalidOperationException("Select a running emulator first.");
            StatusText.Text = await _sdk.StopAsync(serial);
            await RefreshAsync();
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async Task SendKey(string key)
    {
        try
        {
            if (RunningList.SelectedItem is not string serial)
                throw new InvalidOperationException("Select a running emulator first.");
            StatusText.Text = await _sdk.KeyAsync(serial, key);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async void Back_Click(object sender, RoutedEventArgs e) => await SendKey("Back");
    private async void Home_Click(object sender, RoutedEventArgs e) => await SendKey("Home");
    private async void Recent_Click(object sender, RoutedEventArgs e) => await SendKey("Recent");

    private void Install_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (RunningList.SelectedItem is not string serial)
                throw new InvalidOperationException("Select a running emulator first.");
            var dlg = new OpenFileDialog { Filter = "Android package (*.apk)|*.apk" };
            if (dlg.ShowDialog() == true) _ = InstallAsync(serial, dlg.FileName);
        }
        catch (Exception ex) { ShowError(ex); }
    }

    private async Task InstallAsync(string serial, string path)
    {
        try { StatusText.Text = await _sdk.InstallApkAsync(serial, path); }
        catch (Exception ex) { ShowError(ex); }
    }

    private void DeviceInstaller_Click(object sender, RoutedEventArgs e)
    {
        var window = new DeviceInstallerWindow { Owner = this };
        window.ShowDialog();
    }

    private void Library_Click(object sender, RoutedEventArgs e)
    {
        var text = string.Join(Environment.NewLine, AzulNex.Models.DeviceCatalog.Profiles.Select(p => $"{p.Name} — {p.Width}×{p.Height} — {p.Category}"));
        MessageBox.Show(text + Environment.NewLine + Environment.NewLine + "Display presets only; not genuine OEM firmware.", "AZUL NEX Device Library");
    }

    private void Controls_Click(object sender, RoutedEventArgs e) =>
        MessageBox.Show("Select a running emulator, then use Back, Home, or Recent. Full configurable keyboard/mouse mapping is planned.", "Input Controls");

    private void ShowError(Exception ex)
    {
        StatusText.Text = ex.Message;
        MessageBox.Show(ex.Message, "AZUL NEX", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}