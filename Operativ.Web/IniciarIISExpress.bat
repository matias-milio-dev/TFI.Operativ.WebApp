@echo off
set PUERTO=8901
set RUTA=%~dp0
if "%RUTA:~-1%"=="\" set RUTA=%RUTA:~0,-1%

echo Iniciando IIS Express en http://localhost:%PUERTO%/
echo Ruta del sitio: %RUTA%
echo.

"C:\Program Files\IIS Express\iisexpress.exe" /path:"%RUTA%" /port:%PUERTO%
