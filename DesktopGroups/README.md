# Desktop Groups

Organiza tu escritorio de Windows 11 con grupos tipo cajon que se abren hacia abajo.
Vive en la bandeja del sistema (~15-30 MB RAM).

## Requisitos
- Windows 10/11 (64-bit)
- .NET 8 Runtime: https://dotnet.microsoft.com/download/dotnet/8.0
- .NET 8 SDK (solo para compilar): misma URL

## Compilar
    dotnet restore
    dotnet build -c Release

El ejecutable queda en:
    bin\Release\net8.0-windows\win-x64\DesktopGroups.exe

## Uso
1. Ejecuta DesktopGroups.exe
2. Icono en bandeja del sistema (junto al reloj)
3. Clic derecho → Nuevo Grupo
4. Gestionar Grupos → renombrar, cambiar icono, eliminar
5. Arrastra .lnk/.exe al cajon para agregar accesos directos
6. Clic en un elemento del cajon para abrirlo
7. Clic derecho en elemento → Eliminar del grupo
8. El cajon se cierra solo al hacer clic fuera

## Iconos (.ico)
| Parametro   | Valor                                             |
|-------------|---------------------------------------------------|
| Tamano      | 256x256 px (principal)                            |
| Capas       | 256, 128, 64, 48, 32, 16 px                       |
| Formato     | ICO estandar, 32-bit RGBA, fondo transparente     |
| Herramienta | https://www.icoconverter.com o GIMP               |

## Datos guardados
    %AppData%\DesktopGroups\groups.json
