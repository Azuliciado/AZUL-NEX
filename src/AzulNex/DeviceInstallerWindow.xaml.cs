using AzulNex.Core;
using System.Windows;

namespace AzulNex;

public partial class DeviceInstallerWindow : Window
{
    private readonly DeviceInstallerService _installer = new();

    private readonly List<AndroidVersionChoice> _versions = new()
    {
        new("Android 16  •  API 36", 36),
        new("Android 15  •  API 35", 35),
        new("Android 14  •  API 34", 34),
        new("Android 13  •  API 33", 33)
    };

    public DeviceInstallerWindow()
    {
        InitializeComponent();
        AndroidVersionCombo.ItemsSource = _versions;
        AndroidVersionCombo.SelectedIndex = 0;
        Loaded += async (_, _) => await LoadDeviceProfilesAsync();
    }

    private async Task LoadDeviceProfilesAsync()
    {
        try
        {
            StatusText.Text = "Looking for Android SDK command-line tools and supported device profiles...";
            var devices = await _installer.ListDeviceProfilesAsync();
            DeviceCombo.ItemsSource = devices;
            if (devices.Count > 0)
            {
                DeviceCombo.SelectedIndex = 0;
                StatusText.Text = $"Found {devices.Count} hardware profiles. Choose a device and Android release.";
                UpdateSuggestedName();
            }
            else
            {
                StatusText.Text = "No hardware profiles were returned. Install Android SDK Command-line Tools from Android Studio > SDK Manager > SDK Tools.";
                InstallButton.IsEnabled = false;
            }
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
            InstallButton.IsEnabled = false;
        }
    }

    private void SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (IsLoaded) UpdateSuggestedName();
    }

    private void UpdateSuggestedName()
    {
        if (DeviceCombo.SelectedItem is not EmulatorDeviceProfile device ||
            AndroidVersionCombo.SelectedItem is not AndroidVersionChoice version)
            return;

        var safeDevice = System.Text.RegularExpressions.Regex.Replace(device.Id, "[^A-Za-z0-9_-]", "_");
        AvdNameBox.Text = $"AZUL_{safeDevice}_API_{version.Api}";
    }

    private async void Install_Click(object sender, RoutedEventArgs e)
    {
        if (DeviceCombo.SelectedItem is not EmulatorDeviceProfile device)
        {
            StatusText.Text = "Select a hardware profile first.";
            return;
        }

        if (AndroidVersionCombo.SelectedItem is not AndroidVersionChoice version)
        {
            StatusText.Text = "Select an Android version first.";
            return;
        }

        if (LicenseCheckBox.IsChecked != true)
        {
            MessageBox.Show("Please confirm that you are ready to review and accept the Android SDK component licenses. The SDK license text will be presented by the official sdkmanager process.", "Android SDK licenses", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var avdName = AvdNameBox.Text.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(avdName, "^[A-Za-z0-9_.-]{1,40}$"))
        {
            StatusText.Text = "Use 1–40 letters, numbers, underscores, dots, or hyphens for the virtual device name.";
            return;
        }

        InstallButton.IsEnabled = false;
        DeviceCombo.IsEnabled = false;
        AndroidVersionCombo.IsEnabled = false;
        AvdNameBox.IsEnabled = false;
        InstallProgress.Visibility = Visibility.Visible;

        try
        {
            var progress = new Progress<string>(message => StatusText.Text = message);
            var result = await _installer.InstallAsync(device, version.Api, avdName, progress);
            StatusText.Text = result;
            MessageBox.Show(result, "AZUL NEX — Installation complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusText.Text = ex.Message;
            MessageBox.Show(ex.Message, "AZUL NEX — Installation failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        finally
        {
            InstallProgress.Visibility = Visibility.Collapsed;
            InstallButton.IsEnabled = true;
            DeviceCombo.IsEnabled = true;
            AndroidVersionCombo.IsEnabled = true;
            AvdNameBox.IsEnabled = true;
        }
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private sealed record AndroidVersionChoice(string Label, int Api);
}