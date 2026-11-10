# AZUL NEX — Documentación en español

**Un PC. Dispositivos infinitos.**

AZUL NEX es una aplicación de escritorio para Windows que reúne tareas básicas de administración de dispositivos virtuales Android mediante Android Emulator y Android Debug Bridge (ADB).

> **Estado: alpha / desarrollo temprano.** Esta aplicación no incluye Android, firmware original de fabricantes ni funciones para falsificar la identidad de un dispositivo físico. Las funciones marcadas como planificadas todavía no están disponibles.

## Funciones actuales

- Detecta Android SDK mediante `ANDROID_SDK_ROOT`, `ANDROID_HOME` o la ubicación habitual.
- Muestra los AVD configurados.
- Solicita iniciar un emulador seleccionado.
- Consulta los emuladores conectados mediante ADB.
- Envía las acciones Atrás, Inicio y Recientes.
- Instala archivos APK en un emulador activo.
- Muestra algunos perfiles de referencia de resolución de pantalla.

Los perfiles de pantalla son valores de referencia. No instalan firmware auténtico de Google, Samsung, Xiaomi u otros fabricantes.

## Requisitos

- Windows 10 u 11 de 64 bits.
- .NET 8 SDK para compilar desde el código fuente.
- Android SDK Platform-Tools (ADB).
- Android Emulator.
- Al menos un dispositivo virtual (AVD) creado desde Android Studio Device Manager.
- Virtualización de hardware configurada según los requisitos del emulador.

## Instalación y ejecución

1. Instala Android Studio o las herramientas de línea de comandos del SDK.
2. Instala Android Emulator y Android SDK Platform-Tools desde SDK Manager.
3. Crea un AVD desde Device Manager.
4. Instala .NET 8 SDK.
5. Clona el repositorio y ejecuta:

```bash
git clone https://github.com/Azuliciado/AZUL-NEX.git
cd AZUL-NEX
dotnet run --project src/AzulNex/AzulNex.csproj
```

Si no detecta el SDK, configura la variable de entorno `ANDROID_SDK_ROOT` con la ruta de tu instalación y vuelve a abrir la aplicación.

## Rendimiento

El rendimiento depende del procesador, la virtualización, la memoria RAM, los controladores gráficos, la imagen de Android y la resolución. Los equipos antiguos o con gráficos integrados pueden necesitar resoluciones bajas y una sola instancia activa.

No se garantiza una cantidad concreta de FPS, tiempo de inicio ni número de emuladores simultáneos.

## Próximos pasos

Consulta la [hoja de ruta](../ROADMAP.md). Entre las mejoras previstas están la creación y configuración de AVD, los controles configurables de teclado y ratón, la visualización de pantalla, la administración de varias instancias y los diagnósticos de rendimiento.

## Contribuir

Lee [CONTRIBUTING.md](../CONTRIBUTING.md) y utiliza las plantillas de incidencias de GitHub para informar de errores o proponer funciones.

## Licencia

El código original de AZUL NEX se distribuye bajo la licencia [MIT](../LICENSE). Los componentes del SDK de Android y otros programas externos conservan sus propias licencias.
