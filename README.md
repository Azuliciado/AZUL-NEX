# AZUL NEX

**One PC. Infinite Devices.**  
A Windows desktop manager for Android virtual devices, developed by **Azuliciado**.

> Early development / alpha. AZUL NEX manages the official Android SDK Emulator and ADB. It does not contain Android itself, provide genuine OEM firmware, or spoof device identity.

## Current scope
- Detect Android SDK from `ANDROID_SDK_ROOT`, `ANDROID_HOME`, or the default Windows SDK folder.
- Discover configured Android Virtual Devices (AVDs).
- Start an AVD, refresh running emulator state, stop an emulator, and send navigation key events.
- Install an APK to a selected running emulator with ADB.
- Display hardware/virtualization guidance and errors.
- English and Spanish documentation.

## Requirements
Windows 10/11 x64, .NET 8 Desktop Runtime, Android SDK Platform-Tools (ADB), Android Emulator, and at least one AVD. Hardware virtualization must be enabled and configured as required by the emulator.

The Intel Core i5-6500 / Intel HD Graphics 530 class of hardware may require low-resolution settings and may not support multiple smooth instances. No FPS or boot-time guarantee is made.

## Run from source
1. Install the .NET 8 SDK.
2. Install Android Studio or Android SDK command-line tools, Platform-Tools, and Android Emulator.
3. Create an AVD using Android Studio Device Manager.
4. Run `dotnet run --project src/AzulNex/AzulNex.csproj`.
5. If the SDK is not detected, set `ANDROID_SDK_ROOT` to its folder.

## Build
`dotnet publish src/AzulNex/AzulNex.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true`

GitHub Actions builds the Windows app and installer. No download is advertised until a successful build exists.

## Status
Early functional foundation, not the complete requested feature set. Device cloning/import/export, custom AVD creation, advanced multi-touch mapping, OEM firmware, and comprehensive performance telemetry are future work. Only features actually implemented and verified should be considered supported.

## License
MIT for this repository's original code. Android SDK components and other dependencies retain their own licenses.