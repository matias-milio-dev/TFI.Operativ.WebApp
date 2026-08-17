|  | **UNIVERSIDAD ABIERTA INTERAMERICANA** Facultad de Tecnología Informática |
| --- | --- |
|  | **Materia:** Seminario de Trabajo Final | **Docentes:** Dr. Scali - Ing. Sabato – Dr. Ghigliani |
|  | **Alumno:** Matias Milio |
|  | **Año:** 2026 | **Comisión:** 5A | **Sede:** Lomas |

# Plan de Parche 1.4.3 — Columna Estado y desbloqueo desde Editar — Plataforma Operativ

Mini parche sobre el 1.4.2. Sigue la numeración de la serie de parches de Usuarios (1.4 → 1.4.1 → 1.4.2 → **1.4.3**).

---

## 0. Alcance

1. Columna nueva **Estado** en la grilla de `GestionUsuarios.aspx`, después de Familia/Perfil: `Activo` o `Bloqueado`. (Los dados de baja no aparecen en la grilla — ya los filtra `Listar` desde el 1.4.1 — así que no hace falta un tercer valor para eso, tal cual lo aclaraste.)
2. Si el usuario está **bloqueado**, tocar "Editar" no muestra el formulario de edición de la imagen que mandaste: muestra un botón "Desbloquear usuario" (mismo estilo que "Guardar"). Al tocarlo, desbloquea en base y ahí sí aparece el formulario de edición normal, por si querés seguir editando algo más.

## 1. Columna Estado

Mismo criterio que la columna Familia/Perfil desde el 1.4.2: **sin lógica en el code-behind**, se resuelve en el markup con data-binding directo sobre lo que ya trae la entidad `Usuario` (`Bloqueado`):

```html
<asp:TemplateField HeaderText="Estado">
    <ItemTemplate>
        <span class='badge <%# (bool)Eval("Bloqueado") ? "badge-bloqueado" : "badge-activo" %>'>
            <%# (bool)Eval("Bloqueado")
                ? (string)GetGlobalResourceObject("Textos", "EstadoBloqueado")
                : (string)GetGlobalResourceObject("Textos", "EstadoActivo") %>
        </span>
    </ItemTemplate>
</asp:TemplateField>
```

Dos clases CSS nuevas en `Estilos/` (mismo patrón de badge-píldora que ya existe para Familia/Perfil): `.badge-activo` (verde) y `.badge-bloqueado` (rojo, el mismo tono que ya usás para el borde/texto de "Dar de baja").

## 2. Editar sobre un usuario bloqueado → botón Desbloquear

**Servicio**: `DesbloquearUsuario(int idUsuario)` nuevo en `IUsuarioService`. Va en `UsuarioService.cs` (la partial de seguridad del 1.3.2, no en `UsuarioService.Abm.cs`) porque `Bloqueado`/`IntentosFallidos` son el mismo estado que ya maneja `ValidarCredenciales` ahí — desbloquear es la operación inversa de esa misma lógica, no un CRUD de negocio.

```csharp
void DesbloquearUsuario(int idUsuario);
```

Internamente: `usuarioRepositorio.Desbloquear(idUsuario)` → `UPDATE Usuario SET Bloqueado = 0, IntentosFallidos = 0 WHERE IdUsuario = @IdUsuario` (método nuevo en `IUsuarioRepositorio`/`UsuarioRepositorio`).

> No lo pediste explícitamente, pero dado que ya quedó establecido que toda acción administrativa sobre un usuario se registra en bitácora (1.4.1/1.4.2), agrego acá también `bitacoraService.Registrar(idUsuario, TipoAccionBitacora.DesbloqueoUsuario)` al final de `DesbloquearUsuario`, siguiendo exactamente el mismo patrón de dos parámetros del 1.4.2 (hace falta sumar `DesbloqueoUsuario` a `TipoAccionBitacora` y su `case` en `GetCriticidad`/`GetDescripcion`). Es un paso aislado en la sección 4 — si no lo querés, se salta sin afectar el resto del parche.

**UI — dos sub-paneles dentro del panel de edición**, en vez de uno solo:

```html
<asp:Panel ID="pnlFormularioUsuario" runat="server" Visible="false" CssClass="card">

    <asp:Panel ID="pnlDesbloqueo" runat="server" Visible="false">
        <asp:Literal ID="litMensajeBloqueado" runat="server" />
        <asp:Button ID="btnDesbloquear" runat="server" CssClass="btn-primario"
            OnClick="btnDesbloquear_Click" />
    </asp:Panel>

    <asp:Panel ID="pnlCamposEdicion" runat="server" Visible="true">
        <!-- Usuario, Nombre completo, Correo electrónico, Familia/Perfil, tal como ya está -->
        <asp:Button ID="btnGuardar" runat="server" CssClass="btn-primario" OnClick="btnGuardar_Click" />
    </asp:Panel>

    <asp:Button ID="btnCancelar" runat="server" CssClass="btn-outline" OnClick="btnCancelar_Click" />
</asp:Panel>
```

El header (ícono + título "Editar usuario") y el botón "Cancelar" quedan fijos, comunes a los dos sub-paneles; lo que cambia es el contenido del medio.

**Code-behind** (`GestionUsuarios.aspx.cs`):

- `btnEditar_Click` (el de cada fila de la grilla): además de lo que ya hace hoy, chequea `usuario.Bloqueado`:
  - Si `true`: `pnlDesbloqueo.Visible = true`, `pnlCamposEdicion.Visible = false`, arma `litMensajeBloqueado.Text` con el nombre de usuario (vía recurso `MensajeUsuarioBloqueado`), y guarda igual el `IdUsuario` en el hidden field que ya se usa para saber qué usuario se está editando.
  - Si `false`: como está hoy — `pnlCamposEdicion.Visible = true`, `pnlDesbloqueo.Visible = false`, carga los campos.
  - En los dos casos se abre `pnlFormularioUsuario` y se aplica el scroll/foco del 1.4.1.
- `btnDesbloquear_Click` (nuevo): toma el `IdUsuario` del hidden field, llama a `usuarioService.DesbloquearUsuario(idUsuario)`, vuelve a traer el usuario (ya con `Bloqueado = false`), y pasa el panel a modo edición normal: `pnlDesbloqueo.Visible = false`, `pnlCamposEdicion.Visible = true`, carga los campos con los datos ya frescos. No hace falta otro click en "Editar" — la misma pantalla pasa de "desbloquear" a "editar" en el mismo lugar.
- `btnCancelar_Click`: sin cambios — cierra `pnlFormularioUsuario` completo, sea cual sea el sub-panel que estuviera visible.

Textos nuevos vía `GetGlobalResourceObject("Textos", ...)`, siguiendo el mismo patrón que ya usás en todo el resto (`EtiquetaFamiliaPlaceholder`, etc.), no hardcodeados: `EtiquetaEstado`, `EstadoActivo`, `EstadoBloqueado`, `BotonDesbloquearUsuario` (texto del botón, "Desbloquear usuario"), `MensajeUsuarioBloqueado` (ej.: algo como "El usuario {0} está bloqueado por superar el máximo de intentos fallidos. Desbloquealo para poder editar sus datos.", con `string.Format` para el nombre de usuario).

## 3. Pasos de implementación

1. `IUsuarioRepositorio`/`UsuarioRepositorio`: agregar `Desbloquear(int idUsuario)`.
2. `IUsuarioService`/`UsuarioService.cs`: agregar `DesbloquearUsuario(int idUsuario)`, con la llamada al repositorio.
3. *(Opcional, ver nota de la sección 2)* `TipoAccionBitacora`: sumar `DesbloqueoUsuario` + sus `case` en `GetCriticidad`/`GetDescripcion`; llamar a `bitacoraService.Registrar(...)` desde `DesbloquearUsuario`.
4. Recursos nuevos en el `.resx` de Textos: `EtiquetaEstado`, `EstadoActivo`, `EstadoBloqueado`, `BotonDesbloquearUsuario`, `MensajeUsuarioBloqueado`.
5. CSS: `.badge-activo`, `.badge-bloqueado` en `Estilos/`.
6. `GestionUsuarios.aspx`: agregar la columna Estado a la grilla; partir el panel de edición en `pnlDesbloqueo`/`pnlCamposEdicion` dentro de `pnlFormularioUsuario`.
7. `GestionUsuarios.aspx.cs`: ajustar `btnEditar_Click` para ramificar según `Bloqueado`; agregar `btnDesbloquear_Click`.
8. Probar: grilla muestra Estado correcto por usuario; Editar sobre un usuario bloqueado muestra el botón de desbloqueo (no el formulario); tocarlo desbloquea en base (`Bloqueado = 0`, `IntentosFallidos = 0`), y ahí mismo aparece el formulario normal ya cargado; Editar sobre un usuario activo sigue funcionando exactamente igual que antes; Cancelar cierra el panel entero en cualquiera de los dos modos.

## 4. Definición de "terminado"

- La grilla tiene columna Estado (Activo/Bloqueado), sin tercer valor para dados de baja (ya filtrados).
- Editar sobre un usuario bloqueado nunca muestra los campos editables directamente; primero exige desbloquear.
- Desbloquear cambia `Bloqueado` e `IntentosFallidos` en base, y deja el mismo panel listo para editar sin recargar la página ni pedir un segundo click en "Editar".
- Ningún texto nuevo quedó hardcodeado fuera de los recursos de `Textos`.

---

## Historial de cambios

| Versión | Cambio |
| --- | --- |
| 1.4.3 | Columna Estado (Activo/Bloqueado) en la grilla de Usuarios. Editar sobre un usuario bloqueado muestra un botón "Desbloquear usuario" en vez del formulario; al desbloquear, el mismo panel pasa a edición normal. Nuevo `UsuarioService.DesbloquearUsuario`, y opcionalmente su registro en bitácora (`TipoAccionBitacora.DesbloqueoUsuario`). |
