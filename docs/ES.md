# AZUL NEX — Documentación en español

AZUL NEX es una aplicación de Windows para administrar dispositivos virtuales Android mediante Android Emulator y ADB oficiales.

## Estado actual
- Detecta Android SDK en ubicaciones habituales o mediante `ANDROID_SDK_ROOT` / `ANDROID_HOME`.
- Enumera AVD configurados.
- Solicita iniciar un AVD, consulta emuladores activos y permite detenerlos.
- Envía acciones Atrás, Inicio y Recientes mediante ADB.
- Instala archivos APK en un emulador seleccionado.
- Incluye perfiles de resolución; son ajustes visuales, no firmware auténtico de Samsung, Google u otros fabricantes.

## Requisitos
Windows 10/11 de 64 bits, .NET 8 Desktop Runtime, Android SDK Platform-Tools, Android Emulator y al menos un AVD. La virtualización de hardware debe estar habilitada según los requisitos del emulador.

## Ejecutar
Instala .NET 8 SDK y Android SDK, crea un AVD desde Android Studio Device Manager y ejecuta `dotnet run --project src/AzulNex/AzulNex.csproj`.

## Limitaciones
Esta versión es una base alpha. La creación avanzada de dispositivos, perfiles completos de teclado/ratón, gestos multitáctiles, clonación y telemetría detallada aún no están implementados. El rendimiento depende del equipo y no se garantiza en hardware antiguo.