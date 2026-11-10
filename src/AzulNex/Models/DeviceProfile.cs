namespace AzulNex.Models;
public sealed record DeviceProfile(string Name, string Manufacturer, int Width, int Height, int Density, string Category);
public static class DeviceCatalog
{
    // Display presets only; these do not install OEM firmware or alter hardware identity.
    public static IReadOnlyList<DeviceProfile> Profiles { get; } = new List<DeviceProfile>
    {
        new("Pixel 8", "Google (display preset)", 1080, 2400, 420, "Phone"),
        new("Pixel 8 Pro", "Google (display preset)", 1344, 2992, 480, "Phone"),
        new("Galaxy S24", "Samsung (display preset)", 1080, 2340, 420, "Phone"),
        new("Galaxy S24 Ultra", "Samsung (display preset)", 1440, 3120, 560, "Phone"),
        new("Xiaomi 14", "Xiaomi (display preset)", 1200, 2670, 460, "Phone"),
        new("OnePlus 12", "OnePlus (display preset)", 1440, 3168, 510, "Phone"),
        new("Pixel Tablet", "Google (display preset)", 2560, 1600, 276, "Tablet"),
        new("Galaxy Tab", "Samsung (display preset)", 2560, 1600, 280, "Tablet")
    };
}