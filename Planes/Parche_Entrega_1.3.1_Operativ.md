# Parche Entrega 1.3.1 — Corrección de diseño de Bitácora — Plataforma Operativ

Este parche **reemplaza la sección 5** de `Parche_Entrega_1.3_Operativ.md` (todavía no implementada). Las secciones 1, 2, 3, 4, 6, 7 y 8 de 1.3 quedan exactamente igual — la tabla `Bitacora`, la entidad `Bitacora`, los enums `TipoAccionBitacora`/`CriticidadBitacora`, `IBitacoraRepositorio`/`BitacoraRepositorio` y su alta en `FabricaRepositorio` no cambian en nada. Lo único que cambia es **quién decide** la criticidad y la descripción de cada entrada, y cómo se llama a ese registro desde `UsuarioNegocio` y desde el Logout.

## Motivo del cambio

En 1.3, `UsuarioNegocio` armaba la entidad `Bitacora` a mano en cada punto de llamada (`RegistrarBitacora` privado) y `RegistrarIntentoFallido` terminaba mezclando la lógica de bloqueo (incrementar intentos, decidir si bloquea) con la lógica de auditoría (con qué criticidad y qué texto va a bitácora). Eso reparte en varios lugares una decisión que debería vivir en un solo sitio, y hace que los métodos de negocio de usuario hagan más de una cosa a la vez.

## Diseño nuevo

Se agrega `BitacoraNegocio` (BLL), responsable exclusivo de decidir, para cada `TipoAccionBitacora`, la `Criticidad` y la `Descripcion`, armar la entidad `Bitacora` y llamar a `IBitacoraRepositorio.Registrar(...)`. Desde cualquier otro lado del código, registrar en bitácora pasa a ser una sola línea: `bitacoraNegocio.Registrar(idUsuario, accion)`.

---

## 1. `Operativ.BLL/Contratos/IBitacoraNegocio.cs` (nuevo)

```csharp
using Operativ.BE.Enums;

namespace Operativ.BLL.Contratos
{
    public interface IBitacoraNegocio
    {
        void Registrar(int idUsuario, TipoAccionBitacora accion);
    }
}
```

## 2. `Operativ.BLL/Implementaciones/BitacoraNegocio.cs` (nuevo)

Mismo estilo que `ErroresHandler` (switch sobre el enum para mapear a valores fijos):

```csharp
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BLL.Configuracion;
using Operativ.BLL.Contratos;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;

namespace Operativ.BLL.Implementaciones
{
    public class BitacoraNegocio : IBitacoraNegocio
    {
        private readonly IBitacoraRepositorio bitacoraRepositorio;

        public BitacoraNegocio()
        {
            FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
            bitacoraRepositorio = fabricaRepositorio.CrearBitacoraRepositorio();
        }

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

        private CriticidadBitacora GetCriticidad(TipoAccionBitacora accion)
        {
            switch (accion)
            {
                case TipoAccionBitacora.LoginExitoso:
                    return CriticidadBitacora.Informativo;
                case TipoAccionBitacora.LoginBloqueado:
                    return CriticidadBitacora.Critico;
                case TipoAccionBitacora.RecuperacionContrasena:
                    return CriticidadBitacora.Advertencia;
                case TipoAccionBitacora.CierreSesion:
                    return CriticidadBitacora.Informativo;
                default:
                    return CriticidadBitacora.Informativo;
            }
        }

        private string GetDescripcion(TipoAccionBitacora accion)
        {
            switch (accion)
            {
                case TipoAccionBitacora.LoginExitoso:
                    return "Inicio de sesión exitoso";
                case TipoAccionBitacora.LoginBloqueado:
                    return string.Format("Usuario bloqueado tras {0} intentos fallidos", ConfiguracionAplicacion.IntentosMaximosLogin);
                case TipoAccionBitacora.RecuperacionContrasena:
                    return "Contraseña restablecida por recuperación";
                case TipoAccionBitacora.CierreSesion:
                    return "Cierre de sesión";
                default:
                    return string.Empty;
            }
        }
    }
}
```

`LoginBloqueado` no necesita que nadie le pase el número de intentos máximos: lo lee de `ConfiguracionAplicacion.IntentosMaximosLogin` directamente, porque ya es una constante de configuración accesible desde el BLL. Ningún caso actual necesita datos que no tenga ya disponibles el propio `BitacoraNegocio`; por eso `Registrar` solo pide `idUsuario` y `accion`, nada más.

## 3. `Operativ.BLL/Fabricas/FabricaNegocio.cs` (modificado)

Agregar, sin tocar los métodos existentes:

```csharp
public IBitacoraNegocio CrearBitacoraNegocio()
{
    return new BitacoraNegocio();
}
```

## 4. `Operativ.BLL/Implementaciones/UsuarioNegocio.cs` (reemplaza el punto 5.3 de 1.3)

- Sacar el campo `bitacoraRepositorio` y el método privado `RegistrarBitacora` que proponía 1.3.
- Agregar `private readonly IBitacoraNegocio bitacoraNegocio;` y `using Operativ.BLL.Fabricas;` (para `FabricaNegocio`) y `using Operativ.BE.Enums;` (para `TipoAccionBitacora`, si no está ya).
- Constructor:

```csharp
public UsuarioNegocio()
{
    FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
    usuarioRepositorio = fabricaRepositorio.CrearUsuarioRepositorio();

    FabricaNegocio fabricaNegocio = new FabricaNegocio();
    bitacoraNegocio = fabricaNegocio.CrearBitacoraNegocio();
}
```

- `ValidarCredenciales`: agregar la línea de registro **después** de `usuario.IntentosFallidos = 0;` y antes del `return usuario;`. El resto del método no cambia:

```csharp
usuarioRepositorio.ResetearIntentosFallidos(usuario.IdUsuario);
usuario.IntentosFallidos = 0;

bitacoraNegocio.Registrar(usuario.IdUsuario, TipoAccionBitacora.LoginExitoso);

return usuario;
```

- `RegistrarIntentoFallido`: la lógica de bloqueo queda intacta, solo se agrega una línea dentro del `if`. Ya no arma ninguna entidad ni decide criticidad — eso es responsabilidad de `BitacoraNegocio`:

```csharp
private void RegistrarIntentoFallido(Usuario usuario)
{
    int intentosFallidos = usuario.IntentosFallidos + 1;
    bool bloqueado = intentosFallidos >= ConfiguracionAplicacion.IntentosMaximosLogin;

    usuarioRepositorio.ActualizarIntentosFallidos(usuario.IdUsuario, intentosFallidos, bloqueado);

    if (bloqueado)
    {
        bitacoraNegocio.Registrar(usuario.IdUsuario, TipoAccionBitacora.LoginBloqueado);
        throw new OperativException(TipoError.ErrorUsuarioBloqueado, new string[] { usuario.NombreUsuario });
    }

    int intentosRestantes = ConfiguracionAplicacion.IntentosMaximosLogin - intentosFallidos;
    throw new OperativException(TipoError.ErrorContrasenaIncorrecta, new string[] { intentosRestantes.ToString() });
}
```

- `RecuperarContrasena`: una línea al final, antes de que el método termine:

```csharp
usuarioRepositorio.ActualizarContrasena(usuario.IdUsuario, nuevoHash, nuevoSalt);

bitacoraNegocio.Registrar(usuario.IdUsuario, TipoAccionBitacora.RecuperacionContrasena);
```

- **Eliminar** el método `RegistrarCierreSesion(int idUsuario)` y su firma en `IUsuarioNegocio` que proponía 1.3: ya no hace falta, ver punto 5.

## 5. `Operativ.Web/Controles/ResumenUsuario.ascx.cs` (reemplaza el punto 5.4 de 1.3)

Con `BitacoraNegocio` disponible directamente vía `FabricaNegocio`, el Logout no necesita pasar por `UsuarioNegocio` para auditar: llama a `IBitacoraNegocio` directo. Se mantiene la decisión de que una falla al auditar no debe impedir el logout:

```csharp
protected void lnkCerrarSesion_Click(object sender, EventArgs e)
{
    Usuario usuario = sesionHandler.GetUsuario();

    if (usuario != null)
    {
        try
        {
            FabricaNegocio fabricaNegocio = new FabricaNegocio();
            IBitacoraNegocio bitacoraNegocio = fabricaNegocio.CrearBitacoraNegocio();
            bitacoraNegocio.Registrar(usuario.IdUsuario, TipoAccionBitacora.CierreSesion);
        }
        catch (Exception)
        {
        }
    }

    sesionHandler.CerrarSesion();
    Response.Redirect("~/Login.aspx");
}
```

Agregar `using Operativ.BE.Enums;`, `using Operativ.BLL.Contratos;` y `using Operativ.BLL.Fabricas;` al archivo.

---

## 6. Qué no cambia

- Sección 2 de 1.3 (tabla `Bitacora` en `Scripts/CrearBaseDatos.sql`): igual.
- Sección 3 de 1.3 (`TipoAccionBitacora`, `CriticidadBitacora`, entidad `Bitacora`): igual.
- Sección 4 de 1.3 (`IBitacoraRepositorio`, `BitacoraRepositorio`, alta en `FabricaRepositorio`): igual.
- Sección 6 de 1.3 (fuera de alcance: Consultar Bitácora, DVH/DVV): igual.
- Sección 8 de 1.3 (criterio de "terminado"): el comportamiento observable es idéntico — mismas 4 filas esperadas en `SELECT * FROM Bitacora`, con los mismos valores de `Accion`/`Criticidad`. Lo único que cambió es el diseño interno de quién arma cada entrada.

---

*Este documento se pasa junto con `Parche_Entrega_1.3_Operativ.md` (secciones 1 a 4, 6 a 8) a Claude Code; la sección 5 de 1.3 queda reemplazada por este archivo.*
