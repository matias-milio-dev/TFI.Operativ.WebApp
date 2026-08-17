|  | **UNIVERSIDAD ABIERTA INTERAMERICANA** Facultad de Tecnología Informática |
| --- | --- |
|  | **Materia:** Seminario de Trabajo Final | **Docentes:** Dr. Scali - Ing. Sabato – Dr. Ghigliani |
|  | **Alumno:** Matias Milio |
|  | **Año:** 2026 | **Comisión:** 5A | **Sede:** Lomas |

# Plan de Parche 1.5 — Complejidad de contraseña y Cambiar Clave — Plataforma Operativ

Nuevo número de parche (no `1.4.x`) porque es una funcionalidad distinta del ABM de Usuarios — cae en el área de seguridad de sesión/credenciales, no en la administración de cuentas.

---

## 0. Alcance

1. Regla de complejidad de contraseña (mínimo 8 caracteres, una mayúscula, una minúscula, un número), como validación reutilizable en `Operativ.SEC`.
2. Overhaul del menú de usuario en el header (`ResumenUsuario.ascx`): botón de avatar con menú desplegable (Idioma / Cambiar contraseña / Cerrar sesión), según la referencia visual.
3. Modal **Cambiar contraseña**, disparado desde ese menú: cualquier usuario logueado cambia su propia contraseña (actual + nueva + confirmar), con la regla de complejidad aplicada a la nueva.
4. La clave temporal que genera `RecuperarClave` (parche 1.3.2) pasa a cumplir la misma regla.

Las dos referencias visuales (header con menú desplegable abierto, y modal de Cambiar contraseña) ya están disponibles — se siguen sus estilos, textos y layout tal cual, según lo acordado con el alumno.

## 1. Regla de complejidad — `ClaveHelper`

Nuevo Helper estático (sufijo `Helper`, permitido ser estática por el estándar) en `SEC/Helpers/`, junto a `HashHelper`/`AesHelper`/`EmailHelper`:

```csharp
public static class ClaveHelper
{
    public static bool EsCompleja(string clave)
    {
        string patron = @"^(?=.*[A-Z])(?=.*[a-z])(?=.*[0-9]).{8,}$";
        return Regex.IsMatch(clave, patron);
    }
}
```

Un solo regex con lookaheads: exige al menos una mayúscula, al menos una minúscula, al menos un dígito, y longitud mínima de 8 — sin condiciones `if` separadas para cada regla. **Cambio respecto al borrador anterior de este plan:** la versión previa exigía un carácter especial (`[^a-zA-Z0-9]`) y no exigía minúscula; se invierte según la referencia visual del modal, que lista explícitamente "mayúscula / minúscula / número / 8 caracteres" y no menciona carácter especial. El texto del modal (sección 3) y este regex tienen que quedar siempre alineados — si se edita uno se edita el otro.

**Corrección de códigos de error respecto al borrador anterior:** el plan original asumía que el próximo código disponible era `ERR28` (después de un supuesto `ERR27` del parche 1.4). Revisando `ErroresHandler.GetCodigo`/`TipoError` en el código actual, los códigos realmente materializados llegan hasta `ERR13` (`ErrorEmailYaRegistrado`) — el `ERR27` nunca se implementó así, quedó como `ERR13`. El próximo código libre real es **`ERR14`**, no `ERR28`.

También se corrige un bug del borrador anterior: proponía reutilizar `TipoError.ErrorContrasenaIncorrecta` (ERR02) para "contraseña actual incorrecta" dentro de `CambiarClave`. Pero el recurso de ERR02 es `"La contraseña ingresada es incorrecta (Quedan {0} intentos)"` — un mensaje pensado para el conteo de intentos fallidos de **login**, que no aplica al cambio de contraseña autogestionado (no hay bloqueo por intentos acá). Reusarlo sin pasar el parámetro `{0}` rompe el `string.Format`. Se agrega un error nuevo en vez de reutilizar ERR02:

| Constante | Código | Mensaje |
| --- | --- | --- |
| `ErrorContrasenaActualIncorrecta` | **ERR14** (nuevo) | La contraseña actual ingresada es incorrecta |
| `ErrorClaveNoCumpleComplejidad` | **ERR15** (nuevo) | La contraseña no cumple los requisitos de complejidad |

`if (!ClaveHelper.EsCompleja(claveNueva)) { throw new OperativException(TipoError.ErrorClaveNoCumpleComplejidad); }` — con llaves, como marca el estándar; no aplica el patrón `?? throw` porque no es un null-check.

## 2. Header — menú de usuario (`ResumenUsuario.ascx`)

Reemplaza el texto de bienvenida + botón de logout plano actual por el patrón de la primera referencia visual:

- El texto `"Bienvenido {Nombre}, está logueado como {Perfil}"` (`lblBienvenida`) se mantiene tal cual, ya generado en `Page_Load` — solo cambia lo que hay a su derecha.
- A la derecha, un botón de avatar circular (ícono de persona + chevron hacia abajo) que abre/cierra un panel desplegable anclado a la esquina superior derecha, con tres secciones separadas por una línea fina:
  1. **Idioma** — ícono de globo + etiqueta "Idioma" + selector ES/EN como toggle tipo píldora (el idioma activo con fondo/color de acento, el inactivo en gris). Reemplaza el `SelectorIdioma.ascx` actual (hoy es `ES | EN` en texto plano con separador) — mismo control, mismo code-behind (`lnkEspanol_Click`/`lnkIngles_Click`), pero el markup pasa a un contenedor tipo píldora con dos `LinkButton` dentro y una clase `.activo` condicional en vez del separador `|`.
  2. **Cambiar contraseña** — ícono de candado + etiqueta. No es un postback ni una navegación: un botón/`<a>` puramente cliente que dispara el JS que abre el modal de la sección 3 (`abrirModalCambiarClave()`) y cierra el desplegable.
  3. **Cerrar sesión** — ícono de logout + etiqueta. Sigue siendo el `lnkCerrarSesion` existente (postback a `lnkCerrarSesion_Click`, mismo código C# sin tocar), solo cambia de lugar y estilo: pasa de botón naranja suelto a ítem de fila dentro del desplegable.

**CSS nuevo** en `Estilos/operativ.css` (reemplaza/extiende `.resumen-usuario`, `.selector-idioma*`, `.boton-logout` existentes):
- `.menu-usuario` (wrapper relativo), `.menu-usuario-boton` (círculo avatar + chevron, reutiliza la paleta de `.icono-circulo`), `.menu-usuario-dropdown` (panel blanco, `position: absolute`, `border-radius`, `box-shadow`, oculto por defecto vía `display:none` / clase `.activo`), `.menu-usuario-item` (fila ícono + texto, hover), `.menu-usuario-separador` (borde inferior fino entre filas).
- `.selector-idioma-pill` / `.selector-idioma-pill-opcion` / `.selector-idioma-pill-opcion.activo` para el toggle ES/EN dentro del ítem de Idioma.

**JS nuevo** (no existe ningún `.js` en el proyecto hoy — se crea `Operativ.Web/Scripts/operativ-ui.js`, referenciado desde `Principal.Master`):
- Abrir/cerrar el desplegable al click en `.menu-usuario-boton`; cerrar al click afuera o `Escape`.
- Abrir/cerrar el modal de Cambiar contraseña (clase `.activo` sobre `.modal-overlay`).
- Mostrar/ocultar contraseña en cada campo del modal (toggle `type="password"`/`type="text"` del input asociado al botón de ojo).

Íconos: SVG inline (persona, chevron, globo, candado, logout, ojo/ojo-tachado, cerrar-×), mismo criterio que ya usan los `.icono-circulo svg` de los dashboards (24px), sin depender de una librería de íconos externa.

## 3. Modal "Cambiar contraseña"

**Cambio respecto al borrador anterior:** la referencia visual muestra un modal superpuesto sobre la pantalla actual (fondo atenuado), no una página nueva. Se abandona `CambiarClave.aspx` como página independiente; en su lugar:

- Nuevo `UserControl` `Controles/ModalCambiarClave.ascx` (+ code-behind), registrado una sola vez en `Principal.Master` junto a `Notificaciones.ascx`, para que esté disponible en cualquier página autenticada sin duplicar markup.
- Renderizado siempre en el DOM pero oculto (`class="modal-overlay"`, `display:none` hasta que JS agregue `.activo`) — el trigger es el ítem "Cambiar contraseña" del menú de usuario (sección 2).

**Estructura** (según la segunda referencia visual), con las clases ya existentes de tarjetas/botones del 1.4.1 (`.tarjeta`-like card, `.btn-primario`, `.btn-outline`) más las nuevas de modal:
- `.modal-caja`: tarjeta blanca centrada, `border-radius`, sombra.
- Encabezado: `.icono-circulo` con ícono de candado, título "Cambiar contraseña", subtítulo "Por seguridad, ingresa tu nueva contraseña.", botón `×` de cierre arriba a la derecha (cliente, cierra el modal sin postback).
- Tres campos, cada uno `TextBox TextMode="Password"` + botón de ojo (mostrar/ocultar, cliente):
  - "Contraseña actual" — `RequiredFieldValidator`.
  - "Nueva contraseña" — `RequiredFieldValidator` + `RegularExpressionValidator` con el mismo patrón de `ClaveHelper.EsCompleja` (feedback inmediato en cliente; reforzado del lado servidor antes de llamar al service).
  - "Confirmar nueva contraseña" — `RequiredFieldValidator` + `CompareValidator` contra "Nueva contraseña".
  - Todos los validadores en el mismo `ValidationGroup="CambiarClave"`, para no interferir con validadores de la página que está detrás del modal.
- Caja de info "Requisitos de la contraseña" (fondo gris claro, ícono ⓘ), con exactamente estos 4 ítems, en este orden, tal como en la referencia:
  - Mínimo 8 caracteres
  - Debe incluir una letra mayúscula
  - Debe incluir una letra minúscula
  - Debe incluir un número
- Footer: botón "Cancelar" (`.btn-outline`, `CausesValidation="false"`, cierra el modal por JS sin tocar el servidor) y botón "Guardar" (`.btn-primario`, `ValidationGroup="CambiarClave"`, dispara el postback).

**Code-behind `ModalCambiarClave.ascx.cs`** — `btnGuardar_Click`:
1. `sesionHandler.GetUsuario()` para el `idUsuario` (si no hay sesión, no debería llegar acá — el control solo se pinta en páginas ya protegidas por el Master).
2. `FabricaSeguridad.Instancia.CrearUsuarioService().CambiarClave(idUsuario, txtActual.Text, txtNueva.Text)`.
3. `catch (OperativException ex)`: delega el mensaje a `Notificaciones.ascx` igual que el resto de la app (`ERRXX - {mensaje}`).
4. Éxito: notificación de éxito vía `Notificaciones.ascx`.

**Nota de comportamiento, explícita porque no es obvia mirando solo el mock:** en el proyecto no hay `UpdatePanel`/AJAX en ningún lado (se confirmó buscando en todo `Operativ.Web`) — todo es postback clásico de Web Forms. Eso significa que si el servidor rechaza el cambio (ERR14 contraseña actual incorrecta, ERR15 complejidad, o un error de conexión), la página hace postback completo, el modal se cierra, y el error se ve en el banner superior de `Notificaciones.ascx` como en cualquier otro formulario de la app — el usuario reabre el modal para reintentar. No se agrega ningún mecanismo nuevo (UpdatePanel, fetch, etc.) solo para mantener el modal abierto tras un error, para no romper la consistencia con el resto del proyecto. En la práctica el roundtrip al servidor solo ocurre para ERR14 o errores de infraestructura, porque `RequiredFieldValidator`/`RegularExpressionValidator`/`CompareValidator` ya frenan en el cliente los casos de complejidad y confirmación antes del postback.

**DAL** (`IUsuarioRepositorio`/`UsuarioRepositorio`): agregar `ActualizarClave(int idUsuario, byte[] claveHash, byte[] salt)` (o los tipos que ya uses para hash/salt en el resto del repositorio) — separado de `Modificar` (que es del ABM y toca nombre/email/familia, no la contraseña).

**Servicio** (`UsuarioService.cs`, la partial de seguridad del 1.3.2 — junto a `ValidarCredenciales`/`RecuperarClave`, no en `UsuarioService.Abm.cs`):

```csharp
void CambiarClave(int idUsuario, string claveActual, string claveNueva);
```

Pasos adentro:
1. Traer el usuario por `idUsuario`.
2. Verificar `claveActual` contra el hash guardado (mismo mecanismo de `HashHelper` que ya usa `ValidarCredenciales` — sin pasar por el chequeo de intentos fallidos/bloqueo, que es un tema aparte de login). Si no coincide: `throw new OperativException(TipoError.ErrorContrasenaActualIncorrecta)` (ERR14).
3. Validar `claveNueva` con `ClaveHelper.EsCompleja(...)`. Si no cumple: `ErrorClaveNoCumpleComplejidad` (ERR15).
4. Generar salt + hash nuevos con `HashHelper` (igual que en Alta/Recuperar Clave) y persistir con `usuarioRepositorio.ActualizarClave(idUsuario, nuevoHash, nuevoSalt)`.
5. `bitacoraService.Registrar(idUsuario, TipoAccionBitacora.CambioClave)`. Como en los parches anteriores, sigue el patrón ya establecido para acciones sobre la cuenta — si no se quiere, se saca sin afectar el resto.

## 4. Clave temporal de `RecuperarClave` cumple la misma regla

*(Recomendado, no estrictamente lo que pediste — lo marco aparte por si preferís no tocarlo.)*

El generador de clave temporal (`HashHelper.GenerarClaveTemporal()` o donde esté, desde el 1.3.2) pasa a construirse garantizando al menos una mayúscula, una minúscula y un número, para que la clave que llega por mail ya cumpla `ClaveHelper.EsCompleja(...)` sin que haga falta validarla aparte. No hace falta tocar `RecuperarClave` en sí, solo el generador.

## 5. Pasos de implementación

1. `SEC/Helpers/ClaveHelper.cs` con `EsCompleja(string clave)` (regex mayúscula+minúscula+número+8).
2. `BE/Enums/TipoError.cs` + `BE/Errores/ErroresHandler.cs`: agregar `ErrorContrasenaActualIncorrecta` (ERR14) y `ErrorClaveNoCumpleComplejidad` (ERR15), con sus claves de recurso.
3. `Textos.resx`/`Textos.en.resx`: mensajes de ERR14/ERR15 + todas las etiquetas nuevas de UI (menú de usuario, modal, requisitos de contraseña).
4. `TipoAccionBitacora`: sumar `CambioClave` + su `case` en `GetCriticidad`/`GetDescripcion`.
5. `IUsuarioRepositorio`/`UsuarioRepositorio`: agregar `ActualizarClave(int idUsuario, ...)`.
6. `IUsuarioService`/`UsuarioService.cs`: agregar `CambiarClave(int idUsuario, string claveActual, string claveNueva)`.
7. *(Opcional, sección 4)* Ajustar el generador de clave temporal para que cumpla la regla de complejidad.
8. `Estilos/operativ.css`: clases nuevas de menú de usuario (`.menu-usuario*`), píldora de idioma (`.selector-idioma-pill*`) y modal (`.modal-overlay`, `.modal-caja`, etc.).
9. `Scripts/operativ-ui.js` (nuevo archivo): toggle del desplegable, toggle del modal, toggle de mostrar/ocultar contraseña, cierre por click-afuera/Escape. Referenciarlo desde `Principal.Master`.
10. `Controles/ResumenUsuario.ascx` (+ code-behind): reemplazar el markup plano por el botón de avatar + desplegable; `Controles/SelectorIdioma.ascx`: restyle a toggle tipo píldora (mismo code-behind).
11. `Controles/ModalCambiarClave.ascx` + code-behind: formulario, validadores, wiring a `FabricaSeguridad.Instancia.CrearUsuarioService()`; registrar el control en `Principal.Master`.
12. Compilar y probar: contraseña actual incorrecta da ERR14; contraseña nueva sin complejidad da ERR15; confirmación que no matchea la frena el `CompareValidator` antes de llegar al servidor; cambio exitoso actualiza hash/salt; el desplegable de usuario abre/cierra correctamente y el toggle de idioma sigue funcionando igual que antes; si se sumó el paso 7, la clave temporal de "Olvidé mi contraseña" también pasa `ClaveHelper.EsCompleja`.

## 6. Definición de "terminado"

- `ClaveHelper.EsCompleja` es la única fuente de verdad de la regla (un solo regex, sin duplicarlo en varios `if`), y coincide exactamente con los 4 requisitos listados en el modal.
- El menú de usuario del header (avatar + desplegable con Idioma/Cambiar contraseña/Cerrar sesión) sigue el estilo de la referencia visual: colores, tipografía, espaciado e íconos coherentes con el resto de `operativ.css`, sin CSS ad-hoc fuera de esa hoja.
- El modal de Cambiar contraseña no es accesible sin sesión activa (vive dentro de páginas ya protegidas por el Master), sin restricción de perfil — cualquier usuario logueado cambia su propia clave.
- Contraseña actual incorrecta y contraseña nueva sin complejidad dan errores distintos y específicos (ERR14 / ERR15) vía `Notificaciones.ascx`.
- Ningún texto de la UI nueva (menú, modal, requisitos) quedó hardcodeado fuera de los recursos de `Textos`.

---

## Historial de cambios

| Versión | Cambio |
| --- | --- |
| 1.5 | `ClaveHelper.EsCompleja` (SEC/Helpers) como regla única de complejidad de contraseña (mayúscula+minúscula+número+8, ajustada a la referencia visual). Menú de usuario nuevo en el header (`ResumenUsuario.ascx`: avatar + desplegable con Idioma/Cambiar contraseña/Cerrar sesión). Modal `ModalCambiarClave.ascx` (reemplaza la idea original de página `CambiarClave.aspx`) para autogestión de contraseña vía `UsuarioService.CambiarClave`. Errores corregidos a ERR14 (`ErrorContrasenaActualIncorrecta`, nuevo — ya no se reutiliza ERR02, que traía un placeholder de intentos que no aplica acá) y ERR15 (`ErrorClaveNoCumpleComplejidad`, nuevo — el borrador previo decía ERR28, pero el próximo código real disponible en `ErroresHandler` es ERR14). Clave temporal de `RecuperarClave` alineada a la misma regla (opcional). |
| 1.5 (borrador previo) | Versión inicial con regla mayúscula+número+carácter especial, reutilizando ERR02, y `CambiarClave.aspx` como página independiente — pendiente de la imagen de referencia para el header. Superada por la fila de arriba. |
