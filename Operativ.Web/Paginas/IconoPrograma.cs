using System;
using System.Collections.Generic;

namespace Operativ.Web.Paginas;
public static class IconoPrograma
{
    private const string CarpetaIconos = "~/Imagenes/Programas/";
    private const string IconoGenerico = "generico.svg";

    private static readonly Dictionary<string, string> iconosPorNombre = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "Windows 11 Pro", "windows-11.svg" },
        { "Office 365", "office.svg" },
        { "Google Chrome", "google-chrome.svg" },
        { "Git", "git.svg" },
        { "Visual Studio 2022", "visual-studio.svg" },
        { "Visual Studio Code", "vscode.svg" },
        { ".NET Framework 4.8", "dotnet.svg" },
        { ".NET 10 SDK", "dotnet.svg" },
        { "SQL Server Developer", "sql-server.svg" },
        { "Azure Data Studio", "azure-data-studio.svg" },
        { "Node.js LTS", "nodejs.svg" },
        { "Docker Desktop", "docker.svg" },
        { "Postman", "postman.svg" },
        { "GitHub Copilot", "github.svg" },
        { "JetBrains Rider", "rider.svg" }
    };

    public static string ObtenerRuta(string nombrePrograma)
    {
        string archivo;

        if (!iconosPorNombre.TryGetValue(nombrePrograma, out archivo))
        {
            archivo = IconoGenerico;
        }

        return CarpetaIconos + archivo;
    }
}