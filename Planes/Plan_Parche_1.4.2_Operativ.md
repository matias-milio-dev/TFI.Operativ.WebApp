|  | **UNIVERSIDAD ABIERTA INTERAMERICANA** Facultad de Tecnología Informática |
| --- | --- |
|  | **Materia:** Seminario de Trabajo Final | **Docentes:** Dr. Scali - Ing. Sabato – Dr. Ghigliani |
|  | **Alumno:** Matias Milio |
|  | **Año:** 2026 | **Comisión:** 5A | **Sede:** Lomas |

# Plan de Parche 1.4.2 — Correcciones sobre ABM de Usuarios — Plataforma Operativ

Parche correctivo sobre el 1.4.1. Los tres puntos ajustan cosas que ya quedaron implementadas para que respeten un approach ya establecido en el proyecto (bitácora) o para sacar código de mala calidad (duplicación, HTML armado a mano en el code-behind).

---

## 0. Corrección de base: cómo se registra en bitácora (punto 1)

En el 1.4.1 diseñé `BitacoraService.Registrar` con una firma más rica (`entidadAfectada`, `idEntidadAfectada`, `detalle`) porque no conocía el approach real. **Era incorrecto** — el proyecto ya tiene esto resuelto con una firma bastante más simple:

```csharp
public void Registrar(int idUsuario, TipoAccionBitacora accion)
{
    Bitacora entrada = new Bitacora
    {
        IdUsuario = idUsuario,
        Accion = accion,
        Criticidad = GetCriticidad(accion),
        Descripcion = GetDescripcion(accion)
    };

    bitacoraRepositorio.Registrar(entrada);
}
```

`Criticidad` y `Descripcion` **no se pasan por parámetro**: se derivan del `TipoAccionBitacora` adentro de `GetCriticidad`/`GetDescripcion`. Esto implica dos cosas para el ABM de Usuarios:

- **No hace falta el `idUsuarioEjecutor`** que agregué en el 1.4.1 a `AltaUsuario`/`ModificarUsuario`/`BajaUsuario`. Con esta firma, lo único que necesita `Registrar` es el `IdUsuario` del registro — y cada uno de esos tres métodos ya tiene ese dato sin que nadie se lo pase desde afuera: en `AltaUsuario` es el `idUsuarioNuevo` que devuelve el `Insertar`; en `ModificarUsuario` es `usuario.IdUsuario`; en `BajaUsuario` es el `idUsuario` que ya recibe. Se revierten las firmas a como estaban en el plan 1.4:

  ```csharp
  int AltaUsuario(string nombreUsuario, string nombreCompleto, string correoElectronico, int idFamilia);
  void ModificarUsuario(Usuario usuario);
  void BajaUsuario(int idUsuario);
  ```

- Dentro de `UsuarioService.Abm.cs`, cada método llama a `bitacoraService.Registrar(...)` con **solo** el `IdUsuario` y el `TipoAccionBitacora` que corresponda, sin agregar parámetros nuevos a `Registrar` ni crear una sobrecarga:

  ```csharp
  bitacoraService.Registrar(idUsuarioNuevo, TipoAccionBitacora.AltaUsuario);   // en AltaUsuario
  bitacoraService.Registrar(usuario.IdUsuario, TipoAccionBitacora.ModificacionUsuario); // en ModificarUsuario
  bitacoraService.Registrar(idUsuario, TipoAccionBitacora.BajaUsuario);        // en BajaUsuario
  ```

- Falta confirmar/agregar en `TipoAccionBitacora` (BE/Enums) los tres valores `AltaUsuario`, `BajaUsuario`, `ModificacionUsuario` (si no están ya, por ejemplo porque hasta ahora `Registrar` solo se usaba para Login/Logout/Recuperación de clave), y sumar el `case` correspondiente en `GetCriticidad` y en `GetDescripcion` para cada uno. No propongo textos ni niveles de criticidad concretos acá porque no conozco los que ya usás para las otras acciones — se agregan siguiendo el mismo criterio que el resto de los `case` existentes.

- **Nota, no un cambio**: con esta firma, la bitácora registra sobre qué usuario ocurrió el evento (`IdUsuario`), no necesariamente quién lo ejecutó cuando es un admin actuando sobre otro usuario — para Login/Logout eso es lo mismo (el usuario actúa sobre sí mismo), pero para Alta/Baja/Modificación hechas por un Administrador, esta bitácora no deja registro de qué admin la hizo, solo de qué usuario fue afectado. Es el approach que ya está establecido, así que lo sigo tal cual — lo marco solo por si en algún momento te interesa poder auditar "qué admin dio de baja a quién" (ahí sí haría falta un campo más, y ese sería el único caso legítimo para tocar la firma de `Registrar`).

## 1. Reutilizar la carga de los `DropDownList` de Familias (punto 2)

`CargarFamilias` y `CargarFiltroFamilias` hacen lo mismo sobre dos controles distintos, con distinto texto de placeholder. Se unifican en un solo método parametrizado por el control y por la clave de recurso del placeholder, y de paso se pide `ListarFamilias()` una sola vez en vez de dos:

```csharp
private void CargarFamilias(DropDownList ddl, List<Familia> familias, string claveTextoPlaceholder)
{
    ddl.DataSource = familias;
    ddl.DataTextField = "Nombre";
    ddl.DataValueField = "IdFamilia";
    ddl.DataBind();

    string textoPlaceholder = (string)GetGlobalResourceObject("Textos", claveTextoPlaceholder);
    ddl.Items.Insert(0, new ListItem(textoPlaceholder, string.Empty));
}
```

Uso:

```csharp
List<Familia> familias = familiaService.ListarFamilias();
CargarFamilias(ddlFamilia, familias, "EtiquetaFamiliaPlaceholder");
CargarFamilias(ddlFiltroFamilia, familias, "EtiquetaTodasLasFamilias");
```

Se borran `CargarFamilias()` (la vieja, sin parámetros) y `CargarFiltroFamilias()`, y se reemplazan los dos llamados que hubiera en `Page_Load`/donde corresponda por las dos líneas de arriba.

**Ya lo agregué a `Estandares_Codigo_y_Estilo_Operativ.md`** (punto 9, Capa de presentación) como regla general: no duplicar métodos de code-behind que solo difieren en el control de destino, parametrizar en cambio. Está en el historial de cambios del documento como versión 1.4.2.

## 2. Sacar la generación de badges del code-behind (punto 3)

Se elimina `ObtenerBadgeHtml` (o como se llame el método que arma el `<span>` del badge como string en C#). La familia/perfil pasa a ser un campo más del `DataBind`, igual que `NombreCompleto` o `CorreoElectronico` — sin lógica de presentación calculada en el code-behind.

En el markup de la grilla (`GestionUsuarios.aspx`), la columna de Familia/Perfil queda como un `TemplateField` con data-binding directo, sin pasar por ningún método del `.aspx.cs`:

```html
<asp:TemplateField HeaderText="Familia / Perfil">
    <ItemTemplate>
        <span class='badge badge-<%# ((string)Eval("NombreFamilia")).ToLower() %>'>
            <%# Eval("NombreFamilia") %>
        </span>
    </ItemTemplate>
</asp:TemplateField>
```

Esto mantiene el look de badge de color por perfil (las clases `.badge-administrador/.badge-cliente/.badge-comercial/.badge-webmaster` ya están en `Estilos/` desde el 1.4.1) pero sin una sola línea de C# generando HTML. Si en realidad querés que sea texto plano sin badge — "un campo más" en el sentido literal, sin ningún tratamiento visual — sacá el `<span class='...'>` y dejá solo `<%# Eval("NombreFamilia") %>`; es un cambio de una línea en el markup, avisame cuál de las dos preferís y ajusto si hace falta.

## 3. Pasos de implementación

1. `TipoAccionBitacora`: confirmar/agregar `AltaUsuario`, `BajaUsuario`, `ModificacionUsuario`; sumar sus `case` en `GetCriticidad` y `GetDescripcion` de `BitacoraService`.
2. `IUsuarioService`/`UsuarioService.Abm.cs`: revertir las firmas de `AltaUsuario`/`ModificarUsuario`/`BajaUsuario` a sin `idUsuarioEjecutor`; reemplazar los llamados a `bitacoraService.Registrar(...)` por la forma de dos parámetros.
3. `GestionUsuarios.aspx.cs`: sacar el `idUsuarioEjecutor` de los tres llamados a `usuarioService.AltaUsuario/ModificarUsuario/BajaUsuario`.
4. `GestionUsuarios.aspx.cs`: unificar `CargarFamilias`/`CargarFiltroFamilias` en el método parametrizado; actualizar los dos llamados.
5. `GestionUsuarios.aspx.cs`: borrar `ObtenerBadgeHtml` (y cualquier propiedad/campo auxiliar que solo existiera para armar el badge).
6. `GestionUsuarios.aspx`: cambiar la columna de Familia/Perfil de la grilla a `TemplateField` con data-binding directo.
7. Compilar y probar: Alta, Baja y Modificación de un usuario dejan su fila en `Bitacora` con el `IdUsuario` y `Accion` correctos, sin que `Registrar` haya cambiado de firma; los dos dropdowns de Familia siguen cargando igual que antes; la grilla sigue mostrando el badge de color por perfil sin ningún método `ObtenerBadgeHtml`.

## 4. Definición de "terminado"

- `BitacoraService.Registrar` sigue siendo `Registrar(int idUsuario, TipoAccionBitacora accion)` — sin sobrecargas ni parámetros nuevos.
- `AltaUsuario`, `ModificarUsuario` y `BajaUsuario` no reciben `idUsuarioEjecutor`.
- Existe un solo método de carga de `DropDownList` de Familias, parametrizado, usado por ambos controles.
- No queda ningún método en `GestionUsuarios.aspx.cs` que arme HTML de badge; el color por perfil (si se mantiene) vive en el markup + CSS.

---

## Historial de cambios

| Versión | Cambio |
| --- | --- |
| 1.4.2 | Corrige el 1.4.1: `BitacoraService.Registrar` vuelve a su firma real de dos parámetros (`idUsuario`, `accion`), con `Criticidad`/`Descripcion` derivados vía `GetCriticidad`/`GetDescripcion` — se revierte el `idUsuarioEjecutor` agregado antes. Se unifica la carga de los `DropDownList` de Familia (agregado también a `Estandares_Codigo_y_Estilo_Operativ.md`). Se elimina `ObtenerBadgeHtml`; el badge de perfil pasa a resolverse por data-binding + CSS en el markup, no en el code-behind. |
