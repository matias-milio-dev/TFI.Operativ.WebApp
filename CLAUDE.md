# Operativ — notas para Claude Code

## Stack y estructura

Solución .NET Framework 4.7.2 clásica (sin SDK-style projects), ASP.NET Web Forms, ADO.NET sin ORM. 5 proyectos:

- `Operativ.BE` — entidades, enums, Composite
- `Operativ.DAL` — acceso a datos (ADO.NET simplificado)
- `Operativ.SEC` — seguridad transversal (hash, sesión, autorización)
- `Operativ.BLL` — lógica de negocio
- `Operativ.Web` — UI Web Forms (proyecto de inicio, `Login.aspx`)

Detalle completo del alcance funcional en `Planes/Plan_Entregable_1_Operativ.md`.

## ⚠️ Visual Studio: NO reintroducir el "flavor" de Web Application Project

`Operativ.Web.csproj` es intencionalmente un proyecto **C# de biblioteca de clases común**, sin `<ProjectTypeGuids>`, sin import de `Microsoft.WebApplication.targets` y sin bloque `<ProjectExtensions><VisualStudio><FlavorProperties>...`. Esto es a propósito, no un olvido.

**Por qué:** en esta máquina, el "flavor" de proyecto ASP.NET Web Application (el que integra el proyecto con IIS Express vía COM) está roto — pasa **tanto en Visual Studio 2022 como en VS 2026 Insiders**. Los síntomas si se reintroduce esa integración:

- El proyecto aparece "unloaded" al abrir la solución.
- Al intentar cargarlo manualmente: diálogo `Unexpected null value of type 'IVsHierarchy'`.
- En build/output: `error : The application for the project is not installed.` + `Error HRESULT E_FAIL has been returned from a call to a COM component.`

**Ya se descartó como causa (no perder tiempo re-investigando esto):**
- El código o los `.cs`/`.aspx` del proyecto — compila perfecto por línea de comandos con MSBuild de VS2022 y de VS2026 (`.../MSBuild/Current/Bin/amd64/MSBuild.exe`) en ambos casos.
- Espacios en el path de la carpeta (se probó copiando la solución a una ruta sin espacios, mismo error).
- Caché de VS (`.vs/`) corrupta — se limpió, mismo error.
- `applicationhost.config` de IIS Express (`%USERPROFILE%\Documents\IISExpress\config\`) corrupto por proyectos viejos — se reseteó, mismo error.
- GUID del proyecto "pegado" en algún caché por GUID — se probó con un GUID nuevo, mismo error.

Es un problema del componente WAP (`Microsoft.VisualStudio.Web.Application.WAPackage`) / integración COM con IIS Express de esta instalación de Visual Studio, no del proyecto.

**Fix aplicado (ya está así, dejarlo):**
1. `Operativ.Web.csproj`: sin `ProjectTypeGuids`, sin import de `Microsoft.WebApplication.targets`, sin `ProjectExtensions`. Solo `Import Project="$(MSBuildToolsPath)\Microsoft.CSharp.targets"` al final, como cualquier Class Library.
2. `Operativ.sln`: la línea `Project(...)` de `Operativ.Web` usa el GUID de proyecto C# normal `{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}`, no el GUID de Web Application (`{349c5851-...}`).
3. `Operativ.Web.csproj.user` con `StartAction=Program` apuntando a `iisexpress.exe` (`/path:"..." /port:8901`) → **esto es lo que hace andar F5**, confirmado funcionando. Si en algún momento F5 deja de andar de nuevo, revisar en este orden:
   - Que este archivo no tenga un segundo `<PropertyGroup Condition="...Debug|AnyCPU...">` con `StartAction=Project` pisando el de arriba — eso es lo que VS agrega solo si abrís la pestaña Debug de las propiedades del proyecto y tocás algo ahí. Sacar ese bloque conflictivo alcanza para arreglarlo.
   - Que el `StartArguments` (`/path:"..."`) siga apuntando a la carpeta real del proyecto. **Ya pasó una vez** (2026-08-15): la carpeta de la solución se había movido/renombrado de `TFI Operativ` a `TFI.Operativ.WebApp` y `Operativ.Web.csproj.user` había quedado con el `/path:` viejo — F5 lanzaba `iisexpress.exe`, pero apuntando a una carpeta inexistente, así que el proceso moría al toque y parecía "no arrancar" (sin ningún error de COM/WAP, `.vs/` limpio, todo compilando bien). Si el proyecto se movió de carpeta en algún momento, este es el primer sospechoso, no el problema de WAP de más arriba. `IniciarIISExpress.bat` no tiene este problema porque usa `%~dp0` (ruta relativa a sí mismo).

## Cómo correr/debuggear

**Método soportado (Visual Studio 2022 o 2026):**
1. Abrir `Operativ.sln`, click derecho en `Operativ.Web` → *Set as Startup Project* (solo la primera vez, VS después lo recuerda).
2. F5. Levanta `iisexpress.exe` como proceso debuggeable directo (breakpoints andan normal). No hay auto-launch de navegador (eso es exclusivo del flavor Web que sacamos), así que abrir manualmente `http://localhost:8901/` (redirige a `Paginas/Usuarios/Login.aspx` vía `Default.aspx`) o directamente `http://localhost:8901/Paginas/Usuarios/Login.aspx`.

**Alternativa manual** (si F5 alguna vez deja de andar por el motivo de arriba, o para levantar el sitio sin abrir VS): doble click en `Operativ.Web\IniciarIISExpress.bat`, y para breakpoints usar *Debug → Attach to Process → iisexpress.exe* (tipo de código *Managed (.NET Framework)*).

**Por línea de comandos:**
```
"D:\Program Files\Visual Studio\MSBuild\Current\Bin\amd64\MSBuild.exe" Operativ.sln /p:Configuration=Debug /p:Platform="Any CPU"
```
Luego levantar IIS Express manualmente:
```
"C:\Program Files\IIS Express\iisexpress.exe" /path:"C:\Users\Matias\Documents\TFI.Operativ.WebApp\Operativ.Web" /port:8901
```

## Base de datos

`Scripts/CrearBaseDatos.sql` — crea `OperativDb` en `.\SQLEXPRESS` con las 4 familias/perfiles y 4 usuarios semilla. Contraseña de todos: `Operativ123` (hasheada con salt individual, algoritmo en `Operativ.SEC.Helpers.HashHelper`).

| Usuario | Perfil |
| --- | --- |
| `webmaster` | WebMaster |
| `admin` | Administrador |
| `comercial` | Comercial |
| `cliente` | Cliente |

## Pendiente / limitación conocida

`RecuperarContrasena.aspx` requiere un servidor SMTP real configurado en `Web.config` (`Operativ.Smtp.*` en `appSettings`) para probar el envío de email de punta a punta. Sin eso, `EmailHelper` va a tirar excepción de conexión (capturada y mostrada como `ERR05` por el manejo centralizado de errores, no rompe la app).
