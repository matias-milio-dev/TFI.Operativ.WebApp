|  | **UNIVERSIDAD ABIERTA INTERAMERICANA** Facultad de Tecnología Informática |
| --- | --- |
|  | **Materia:** Seminario de Trabajo Final | **Docentes:** Dr. Scali - Ing. Sabato – Dr. Ghigliani |
|  | **Alumno:** Matias Milio |
|  | **Año:** 2026 | **Comisión:** 5A | **Sede:** Lomas |

# Plan de Parche 1.4.1 — Mejoras sobre ABM de Usuarios — Plataforma Operativ

Parche sobre el 1.4 (que ya está andando de punta a punta). No reconstruye el ABM: le suma estilo, comportamiento de UI, bitácora real, baja lógica reforzada y un flag de configuración. Convención de nombres: `Operativ.SEC` (`UsuarioService`, `FabricaSeguridad`), confirmada como la real por vos.

---

## 0. Alcance

1. Overhaul visual de `GestionUsuarios.aspx` y de los componentes compartidos que aparecen en el mock (Navbar, barra de usuario, botones, tabla, badges).
2. El panel de Alta/Edición arranca oculto; se muestra y se lleva el foco/scroll ahí solo al tocar "Nuevo usuario" o "Editar".
3. `BitacoraService` deja de ser un placeholder (como quedó preparado en el 1.3.2): se implementa y se engancha a Alta, Baja y Modificación de usuario.
4. Baja lógica reforzada: campo en base, filtro en todas las búsquedas.
5. Envío de email controlado por `Web.config`, en `false` por ahora.

## 1. Overhaul de estilos

Tomé del mock estos tokens de diseño. Son una lectura visual, no medí píxeles exactos — ajustá el hex si al ojo no coincide:

| Token | Valor aproximado | Uso |
| --- | --- | --- |
| `--color-navy` | `#111c44` (azul marino muy oscuro) | Barra superior, header de la tabla |
| `--color-accent` | `#f5820c` (naranja) | Botones primarios, "Inicio" en el navbar, "Cerrar sesión" |
| `--color-bg` | `#eef1f6` (gris azulado claro) | Fondo de página |
| `--color-card` | `#ffffff` | Tarjetas |
| Radio de borde | `8px` botones / `12px` tarjetas | |
| Sombra de tarjeta | sombra suave, `0 1px 3px rgba(0,0,0,.08)` | |
| Badges por perfil | Administrador lila, Cliente verde, Comercial azul, Webmaster ámbar — fondo pastel + texto del mismo tono, forma de píldora (`border-radius: 999px`) | Columna "Familia / Perfil" de la grilla |

Sigue sin frameworks CSS pesados (Bootstrap/Tailwind no entran, tal como dice el estándar): todo esto se logra con CSS propio — clases reutilizables, no un rediseño ad-hoc por página. Íconos como SVG inline chicos (lupa, lápiz para Editar, tacho para Dar de baja, +usuario, disquete para Guardar, X para Cancelar) — sin librería de íconos, sin JS extra.

**Archivos que toca:**
- `Estilos/` (la hoja de estilos del sitio): agrega las clases `.card`, `.btn-primario`, `.btn-outline`, `.btn-outline-peligro`, `.badge` + `.badge-administrador/.badge-cliente/.badge-comercial/.badge-webmaster`, `.tabla-operativ`, `.icono-circulo` (el círculo naranja pastel con el ícono, como el de "Gestión de usuarios" y "Nuevo usuario" del mock).
- `Controles/Navbar.ascx`: barra superior azul marino con el logo, links "Inicio"/"Usuarios" (el activo en blanco y negrita, el resto en naranja).
- `Controles/ResumenUsuario.ascx`: el bloque de la derecha ("Bienvenido {usuario}, está logueado como {perfil}" + botón "Cerrar sesión" naranja).
- `Paginas/GestionUsuarios.aspx`: reordena el markup para calzar con el mock — tarjeta de título con ícono circular, barra de búsqueda (texto + dropdown de Familia/Perfil + botón Buscar), tabla con header oscuro y badges, paginado ("Mostrando X a Y de Z usuarios" + Anterior/1/Siguiente), tarjeta de formulario abajo.

**Un agregado funcional chico que el mock trae** (no es solo CSS): el dropdown "Familia / Perfil" en la barra de búsqueda filtra la grilla por perfil, además del texto libre. Esto suma un parámetro a `Listar`:

```csharp
List<Usuario> ListarUsuarios(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina);
int ContarUsuarios(string filtro, int? idFamilia);
```

`ContarUsuarios` es nuevo — hace falta para el "Mostrando 1 a 4 de 4 usuarios" y para calcular la cantidad de páginas del paginado. Va en `IUsuarioRepositorio`/`UsuarioRepositorio`, con `EjecutarEscalar` (`COUNT(*)`).

> Nota aparte: el mock trae un selector **ES | EN** en el navbar. La internacionalización sigue fuera de alcance del proyecto (no está implementada). Lo dejo como un elemento visual **estático** (muestra "ES" resaltado, sin funcionalidad real de cambio de idioma) para no inflar este parche — avisame si en algún momento querés que eso pase a funcionar de verdad, es un parche aparte.

## 2. Panel oculto + foco/scroll (punto 2)

- El `Panel` del formulario (`pnlFormularioUsuario`) arranca con `Visible = false` en el `Page_Load` cuando `!IsPostBack`. Ni "Nuevo usuario" ni "Editar" están visibles al entrar a la página — la grilla es lo único que se ve.
- Al click en "Nuevo usuario" o en "Editar" de una fila: el code-behind pone `pnlFormularioUsuario.Visible = true`, limpia o carga los campos según corresponda, y:
  - `this.SetFocus(txtNombreUsuario)` (o el primer campo editable — en modo edición sería `txtNombreCompleto`, ya que el nombre de usuario no se edita) para el foco de teclado.
  - Un `ClientScript.RegisterStartupScript` con un scroll suave al panel:
    ```javascript
    document.getElementById('<%= pnlFormularioUsuario.ClientID %>')
      .scrollIntoView({ behavior: 'smooth', block: 'start' });
    ```
  - Este script solo se registra en el postback que abre el panel, no en cada carga de página.
- "Cancelar" vuelve a poner `Visible = false`, limpia los campos y no dispara el scroll (te quedás donde estabas, en la grilla).
- Sigue siendo postback clásico (sin `UpdatePanel`/AJAX) — con el `SetFocus` + `scrollIntoView` alcanza para el efecto pedido, sin sumar la complejidad de partial postbacks. Si en algún momento se vuelve molesto el parpadeo de la recarga completa, `UpdatePanel` sería el paso siguiente natural, pero no hace falta para esto.

## 3. Bitácora real (punto 3)

Hasta ahora `IBitacoraService`/`BitacoraService` estaban preparados pero vacíos (nota del plan 1.3.2: "se suma cuando Bitácora entre en alcance"). Entra en alcance ahora, acotado a Alta/Baja/Modificación de Usuario.

**Tabla nueva** (script de migración, sin tocar las tablas existentes):

```sql
CREATE TABLE dbo.Bitacora
(
    IdBitacora        INT IDENTITY(1,1) NOT NULL,
    FechaHora         DATETIME2         NOT NULL CONSTRAINT DF_Bitacora_FechaHora DEFAULT (SYSDATETIME()),
    IdUsuarioEjecutor INT               NOT NULL,
    Accion            VARCHAR(20)       NOT NULL,  -- Alta / Baja / Modificacion
    EntidadAfectada   VARCHAR(50)       NOT NULL,  -- 'Usuario'
    IdEntidadAfectada INT               NULL,
    Detalle           NVARCHAR(500)     NULL,
    CONSTRAINT PK_Bitacora PRIMARY KEY CLUSTERED (IdBitacora),
    CONSTRAINT FK_Bitacora_UsuarioEjecutor FOREIGN KEY (IdUsuarioEjecutor) REFERENCES dbo.Usuario (IdUsuario)
);
```

Simple a propósito — sin criticidad ni DVH todavía (eso es integridad de datos, sigue fuera de alcance del proyecto; se puede sumar cuando esa entrega llegue, sin romper esta tabla).

- `TipoAccionBitacora` (enum nuevo, en `BE/Enums/`): `Alta`, `Baja`, `Modificacion`.
- `IBitacoraRepositorio`/`BitacoraRepositorio` (DAL, `Fabricas/FabricaRepositorio` ya lo instancia): `Registrar(RegistroBitacora registro)` — solo `INSERT`, nunca `UPDATE`/`DELETE`. **Los registros de bitácora son inmutables.**
- `IBitacoraService`/`BitacoraService` (SEC): `Registrar(int idUsuarioEjecutor, TipoAccionBitacora accion, string entidadAfectada, int? idEntidadAfectada, string detalle)`.
- **Enganche en `UsuarioService.Abm.cs`**: `AltaUsuario`, `ModificarUsuario` y `BajaUsuario` necesitan saber quién ejecuta la acción. Ese dato es de sesión, y `UsuarioService` (SEC) no debería leer `Session`/`HttpContext` directamente — mejor que se lo pase quien sí lo sabe: el code-behind. Cambian las firmas:

```csharp
int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int idFamilia, int idUsuarioEjecutor);
void ModificarUsuario(Usuario usuario, int idUsuarioEjecutor);
void BajaUsuario(int idUsuario, int idUsuarioEjecutor);
```

  `GestionUsuarios.aspx.cs` obtiene `idUsuarioEjecutor` de `SesionHandler.Instancia.ObtenerUsuarioLogueado().IdUsuario` y lo pasa en cada llamada. Cada método, después de la operación en base exitosa, llama a `bitacoraService.Registrar(...)` con un detalle legible (ej.: `"Alta de usuario '{nombreUsuario}'"`, `"Modificación de usuario Id {idUsuario}"`, `"Baja lógica de usuario Id {idUsuario}"`).
- No se pide (ni se hace en este parche) una pantalla para ver la bitácora — solo que las acciones queden registradas. Si más adelante querés una `ConsultaBitacora.aspx`, es otro parche.

## 4. Baja lógica reforzada (punto 4)

- **Campo**: verificar si `Usuario.Activo BIT` ya existe. Si no está, agregarlo por migración:
  ```sql
  ALTER TABLE dbo.Usuario ADD Activo BIT NOT NULL CONSTRAINT DF_Usuario_Activo DEFAULT (1);
  ```
  (el `DEFAULT (1)` hace que todos los usuarios existentes queden activos al aplicar la migración, sin romper nada).
- `BajaUsuario` en el DAL pasa a ser estrictamente `UPDATE dbo.Usuario SET Activo = 0 WHERE IdUsuario = @IdUsuario` — confirmar que en ningún lado quedó un `DELETE FROM Usuario`.
- **Filtro en las búsquedas** (punto 4, "en cada búsqueda que traiga usuarios"):
  - `Listar`/`ContarUsuarios` (grilla del ABM): `WHERE Activo = 1` por defecto. No se agrega en este parche un toggle para "ver también los dados de baja" — si lo querés, es un agregado chico a futuro.
  - `GetPorNombreUsuario` (usado en el login): también filtra `Activo = 1`. Un usuario dado de baja no tiene que poder loguearse. Tratamiento recomendado: mismo error que "no existe" (`ERR01`), no un mensaje distinto — no conviene revelar que la cuenta existió.
  - `ExisteNombreUsuario`/`ExisteEmail` (validación de unicidad en Alta/Modificación): a propósito **no** filtran por `Activo` — cuentan también los usuarios dados de baja. Si se permitiera reusar el nombre de usuario o el email de alguien dado de baja, se pierde trazabilidad contra la bitácora (que referencia `IdUsuario`) y es más fácil de confundir. Si en algún momento preferís permitir la reutilización, se ajusta ese único punto.
  - `ObtenerUsuarioPorId` (usado al abrir "Editar"): sin filtro — se accede por Id explícito, no es una búsqueda; en la práctica no se llega ahí para un usuario de baja porque ya no aparece en la grilla.

## 5. Envío de email por configuración (punto 5)

- `Web.config`, en `<appSettings>`:
  ```xml
  <add key="HabilitarEnvioEmail" value="false" />
  ```
- `EmailHelper.Enviar(...)` (SEC/Helpers), al principio del método:
  ```csharp
  bool habilitado = ConfigurationManager.AppSettings["HabilitarEnvioEmail"] == "true";
  if (!habilitado)
  {
      return;
  }
  ```
  No-op si está en `false`: el resto del flujo (generar clave temporal, insertar el usuario, registrar en bitácora) sigue andando igual — solo se salta el `SmtpClient.Send(...)`. Así podés seguir probando Alta y Recuperar Contraseña sin SMTP real, y el día que tengas uno, cambiás el `value` a `true` sin tocar código.
- Nota al margen, no es parte de este parche: `ConfiguracionAplicacion.IntentosMaximosLogin` sigue siendo una constante hardcodeada en `BLL/Configuracion/` (por diseño, según el plan de Entrega 1) — este es el primer valor del proyecto que sale de verdad de `Web.config`. Si en algún momento querés que los límites de seguridad (intentos máximos, etc.) también sean configurables, o que vivan en `SEC/Configuracion/` en vez de `BLL/Configuracion/` (más prolijo desde el 1.3.2, que ya sacó todo lo de seguridad de BLL), decímelo y lo armo — no lo toco ahora para no salirme del pedido.

## 6. Pasos de implementación (orden sugerido)

1. Migraciones SQL: `Usuario.Activo` (si falta) + tabla `Bitacora`.
2. `TipoAccionBitacora` (BE/Enums), `IBitacoraRepositorio`/`BitacoraRepositorio` (DAL), completar `IBitacoraService`/`BitacoraService` (SEC) con `Registrar(...)`.
3. Sumar `idUsuarioEjecutor` a `AltaUsuario`/`ModificarUsuario`/`BajaUsuario` en `IUsuarioService` y `UsuarioService.Abm.cs`; enganchar la llamada a `bitacoraService.Registrar(...)` en cada uno.
4. Agregar `ContarUsuarios` y el parámetro `idFamilia` a `Listar` en `IUsuarioRepositorio`/`UsuarioRepositorio`; aplicar el filtro `Activo = 1` ahí y en `GetPorNombreUsuario`.
5. `Web.config`: `HabilitarEnvioEmail`; ajustar `EmailHelper.Enviar` para respetarlo.
6. CSS: clases nuevas en `Estilos/` (tarjetas, botones, badges, tabla).
7. `Navbar.ascx` y `ResumenUsuario.ascx`: aplicar la barra superior azul marino + botón naranja.
8. `GestionUsuarios.aspx`: reordenar markup (búsqueda + filtro por perfil, tabla con badges, paginado, panel de formulario), panel oculto por defecto.
9. `GestionUsuarios.aspx.cs`: mostrar/ocultar panel + `SetFocus` + script de scroll en Nuevo/Editar; pasar `idUsuarioEjecutor` a los tres métodos de ABM; usar `ListarUsuarios`/`ContarUsuarios` con el filtro de perfil para la grilla.
10. Probar: Alta/Baja/Modificación quedan en `Bitacora` con el usuario ejecutor correcto; un usuario dado de baja no aparece en la grilla, no puede loguearse, pero su nombre/email siguen "ocupados" para Alta; con `HabilitarEnvioEmail=false` el Alta y la recuperación de clave completan sin intentar mandar mail; panel oculto al entrar, visible y con scroll/foco al tocar Nuevo o Editar.

## 7. Definición de "terminado"

- `Usuario.Activo` existe (o ya existía y se confirmó) y `BajaUsuario` nunca hace `DELETE`.
- `Bitacora` tiene una fila por cada Alta, Baja y Modificación de usuario, con el `IdUsuarioEjecutor` correcto.
- La grilla y el login excluyen usuarios con `Activo = 0`; la validación de unicidad los sigue contando.
- `Web.config` controla el envío de mails; en `false` no se intenta ningún `SmtpClient.Send`.
- El panel de Alta/Edición está oculto al entrar a `GestionUsuarios.aspx` y aparece con scroll + foco solo al accionarlo.
- La pantalla sigue la paleta y componentes del mock (navy/naranja, tarjetas, badges por perfil), sin frameworks CSS nuevos.

## 8. Riesgos y notas

- Cambiar la firma de `AltaUsuario`/`ModificarUsuario`/`BajaUsuario` (agregar `idUsuarioEjecutor`) rompe cualquier otro código que ya las llame — según lo que confirmaste, el único llamador es `GestionUsuarios.aspx.cs`, así que el impacto debería ser acotado a ese archivo.
- Si `Usuario.Activo` en tu repo real ya existe pero con otro propósito (por ejemplo, "cuenta verificada" en vez de "no dado de baja"), avisame antes de correr la migración — el `ALTER TABLE` de arriba asume que no existe todavía.
- El selector ES|EN del mock queda decorativo (ver nota del punto 1); si en algún momento se activa de verdad, es un parche de internacionalización aparte, no algo que deba colarse acá.

---

## Historial de cambios

| Versión | Cambio |
| --- | --- |
| 1.4.1 | Overhaul de estilos (navy/naranja, tarjetas, badges por perfil) + filtro por Familia/Perfil en la búsqueda; panel de Alta/Edición oculto por defecto con scroll/foco al abrirse; `BitacoraService` implementado y enganchado a Alta/Baja/Modificación de usuario; `Usuario.Activo` (baja lógica) reforzado con filtro en listado y login, sin filtrar en las validaciones de unicidad; envío de email controlado por `Web.config` (`HabilitarEnvioEmail`, en `false`). |
