# Parche Entrega 1.1 — Multi-idioma y Revamp de Estilo — Plataforma Operativ

Este parche se aplica sobre el código ya generado de la Entrega Oficial 1. Agrega dos features nuevos: internacionalización real (español/inglés) y un rediseño visual con identidad propia. No se toca el alcance funcional ya implementado (login, homes por perfil, recuperar contraseña, logout, protección de URLs): el flujo sigue siendo el mismo, solo cambia cómo se presentan los textos y el estilo visual.

Se apoya en `Estandares_Codigo_y_Estilo_Operativ.md` (sigue vigente todo lo ahí definido) y actualiza el checklist de `Plan_Entregable_1_Operativ.md`: el punto **2. Internacionalización** pasa a **incluido**.

---

## Parte 1 — Multi-idioma (español / inglés)

### 1.1 Objetivo

Ningún texto visible al usuario puede estar hardcodeado en el código ni en el markup `.aspx`. Todo texto (labels, botones, títulos, mensajes de error, mensajes de éxito) sale de archivos de recursos y se resuelve según el idioma activo.

### 1.2 Estructura de recursos

- Carpeta `App_GlobalResources/` en `Operativ.Web` con un recurso por idioma:
  - `Textos.resx` (idioma por defecto, español)
  - `Textos.en.resx` (inglés)
- Usar `App_LocalResources/` por página solo si un texto es exclusivo de esa pantalla (por ejemplo, títulos específicos de `HomeCliente.aspx`); todo lo compartido (menú, botones genéricos, mensajes de error) va en el recurso global.
- Cada clave de recurso se nombra en español y de forma descriptiva, igual que el resto del código: `EtiquetaNombreUsuario`, `BotonIniciarSesion`, `TituloRecuperarContrasena`.

### 1.3 Refactor de `ErroresHandler`

- Se elimina cualquier mensaje de error hardcodeado dentro de `ErroresHandler.cs` (en `BLL/Errores/`).
- Cada constante de error (`ErrorUsuarioNoExiste`, `ErrorContrasenaIncorrecta`, etc.) pasa a mapear a una **clave de recurso**, no a un string literal:

```csharp
public class OperativException : Exception
{
    public TipoError TipoError { get; }
}
```

- `ErroresHandler` expone un método que, dado un `TipoError`, devuelve el código (`ERR01`) y resuelve el mensaje desde el recurso (`GetGlobalResourceObject("Textos", "MensajeErrorUsuarioNoExiste")`) según el idioma activo, y arma el string final `ERRXX - {mensaje}`.
- Los mensajes con placeholders (ej. `ERR02` con `{IntentosRestantes}`, `ERR03` con `{NombreUsuario}`) se resuelven con `string.Format` sobre el texto obtenido del recurso, nunca concatenando literales sueltos en el código.

### 1.4 Controles de servidor

- Los controles `.aspx` usan `meta:resourcekey` para resolver sus textos declarativamente (labels, botones, validadores) contra el recurso local/global según corresponda.
- Donde se necesite resolver texto desde code-behind, usar `GetGlobalResourceObject`/`GetLocalResourceObject`, nunca strings fijos.

### 1.5 Selector de idioma

- Agregar el selector de idioma (ES / EN) al control `ResumenUsuario.ascx`, junto a la barra "Bienvenido {nombreUsuario}...".
- También debe estar disponible en `Login.aspx` (antes de loguearse, ya que ese control no se muestra sin sesión).
- El selector persiste la preferencia en una cookie (para que sobreviva entre sesiones) y en `Session` mientras dura la sesión activa.
- El cambio de idioma ajusta `Thread.CurrentThread.CurrentUICulture` y `CurrentCulture` en `Page_PreInit` de cada página (puede centralizarse en una clase base `PaginaBase` de la que hereden todas las páginas, o en `Global.asax`).

### 1.6 Configuración

- `web.config`: `<globalization culture="auto" uiCulture="auto" />` como valor por defecto, con el override explícito de la preferencia guardada del usuario aplicándose después.
- Idioma por defecto si no hay preferencia guardada: **español**.

### 1.7 Criterio de "terminado" — Multi-idioma

- Cambiar el idioma desde el selector traduce inmediatamente todos los textos de la página actual (labels, botones, placeholders) y persiste al navegar a otras páginas.
- Provocar cada uno de los errores del login (usuario inexistente, contraseña incorrecta, bloqueo) y verificar que el mensaje aparece completo y correcto en ambos idiomas, con el formato `ERRXX - {mensaje}`.
- No queda ningún string de texto de usuario hardcodeado en `.aspx` ni en `.cs` (búsqueda manual de literales en español sueltos en el código como criterio de revisión).

---

## Parte 2 — Revamp de estilo visual

### 2.1 Objetivo

El estilo generado hasta ahora es demasiado genérico (parecido a una plantilla Bootstrap estándar, similar a otras aplicaciones vistas). Se pide una identidad visual propia y reconocible para Operativ, manteniendo la premisa de "diseño funcional de nivel trabajo de facultad" (sin frameworks pesados, sin animaciones ni responsive avanzado), pero con una paleta y tipografía distintivas.

### 2.2 Identidad de marca

Operativ es una plataforma de orquestación y monitoreo de estaciones de trabajo (dispositivo como servicio). La estética debe transmitir **control técnico y confiabilidad**, no un genérico dashboard corporativo azul-y-blanco.

- **Paleta de colores** (definir como variables CSS al inicio de la hoja de estilos):
  - Color base oscuro para header/navbar: `#14213D` (azul noche, distinto del celeste corporativo típico).
  - Color de acento: `#FCA311` (ámbar/naranja), usado en botones primarios, links activos y elementos de énfasis — es lo que le da carácter propio y lo aleja de la paleta azul genérica.
  - Fondo general: `#F5F6F8` (gris muy claro, no blanco puro).
  - Texto principal: `#14213D`; texto secundario: `#6C757D`.
  - Estados: éxito `#2E7D32`, error `#C62828`, advertencia `#F9A825` (diferenciado del ámbar de marca para no confundir acento con alerta).
- **Tipografía**: una sola familia sans-serif con carácter (por ejemplo `"Inter"` o `"Poppins"` vía Google Fonts, con `Segoe UI`/`Arial` como fallback), en vez de la fuente por defecto de Bootstrap. Títulos en peso semibold, cuerpo en regular.
- **Logo/wordmark**: mientras no haya un logo diseñado, usar el nombre "Operativ" en el navbar con tipografía propia (peso bold, el punto o una letra en el color de acento) en vez de un ícono genérico.

### 2.3 Elementos de UI distintivos

- Navbar oscura (color base) con el acento de color solo en el link/página activa y en el botón de logout.
- Botones primarios con el color de acento y esquinas levemente redondeadas (4-6px), no el celeste/gris por defecto de Bootstrap.
- Cards/paneles (para `DashboardResumen.ascx` y los formularios) con borde superior de 3-4px en el color de acento, para dar una firma visual reconocible en todos los módulos futuros.
- Grillas (`GridView`) con encabezado en el color base oscuro y texto claro, filas alternadas en gris muy sutil (no blanco/gris estándar de Bootstrap).
- Evitar iconografía genérica de Bootstrap Icons/Font Awesome sin criterio: si se usan íconos, que sean consistentes en un único set y de trazo simple (line icons), no una mezcla.

### 2.4 Alcance técnico

- Una única hoja de estilos propia (`Estilos/operativ.css`), sin depender de un framework CSS completo (nada de Bootstrap/Bulma/Tailwind como dependencia). Puede tomarse Bootstrap solo como referencia de grilla/reset si hace falta, pero el look final no debe ser reconocible como "un Bootstrap default".
- Variables CSS (`:root { --color-base: ...; --color-acento: ...; }`) para que la paleta se pueda ajustar centralizadamente.
- Se mantiene todo lo demás de la sección de Presentación de `Plan_Entregable_1_Operativ.md` y de `Estandares_Codigo_y_Estilo_Operativ.md` (sin animaciones, sin responsive avanzado, controles de validación estándar de ASP.NET).

### 2.5 Criterio de "terminado" — Revamp de estilo

- Ningún color celeste/azul Bootstrap por defecto ni tipografía por defecto del framework visible en la aplicación.
- Las 4 homes, el login, y las páginas de error/no autorizado usan consistentemente la nueva paleta y tipografía.
- La hoja de estilos usa variables CSS centralizadas para los colores de marca.

---

*Este parche complementa, no reemplaza, a `Plan_Entregable_1_Operativ.md`, `Estandares_Codigo_y_Estilo_Operativ.md` y `Parche_Codigo_Entrega_1_Operativ.md`. Pasar los cuatro documentos juntos a Claude Code para aplicar esta iteración (Entrega 1.1).*
