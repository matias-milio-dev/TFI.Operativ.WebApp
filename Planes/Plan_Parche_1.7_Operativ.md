|  | **UNIVERSIDAD ABIERTA INTERAMERICANA** Facultad de Tecnología Informática |
| --- | --- |
|  | **Materia:** Seminario de Trabajo Final | **Docentes:** Dr. Scali - Ing. Sabato – Dr. Ghigliani |
|  | **Alumno:** Matias Milio |
|  | **Año:** 2026 | **Comisión:** 5A | **Sede:** Lomas |

# Plan de Parche 1.7 — Chequeo de Integridad en Login + Acceso de Emergencia (Web Master) — Plataforma Operativ

Segunda etapa del requerimiento 10.8.3 / CU-001-001 / CU-001-002 (Carpeta de Tecnología). El **Parche 1.6** dejó calculados y mantenidos al día los DVH/DVV de las 8 tablas críticas, pero dejó explícitamente afuera todo el chequeo. Este parche cierra esa deuda: agrega la **verificación** en el `Page_Load` de `Login.aspx` (CU-001-001, paso 2 y alternativa 2.1 / ERR04), el **mensaje de error con el detalle de tablas/registros afectados**, y el **acceso de emergencia del Web Master contra un XML oculto** cuando la corrupción impide el login normal, seguido del recálculo de dígitos (CU-001-002, curso alternativo).

Escrito contra el estado real de `main` en `TFI.Operativ.WebApp` (revisado el 18/08/2026, commit `197aebb`).

**Decisiones ya tomadas (a confirmar en la implementación, no hay nada abierto salvo lo marcado):**
- El chequeo corre en **todo** `Page_Load` de `Login.aspx` (GET y POST), no solo en la primera carga, porque las credenciales se validan en el postback y no tiene sentido dejarlas pasar si la integridad ya se sabe rota.
- Diseño en dos fases por tabla: primero comparar el **DVV** (barato, una sola lectura de la columna `DVH`); solo si no coincide, releer la tabla completa y comparar el **DVH fila por fila** para poder señalar el registro exacto. Si el conteo de filas cambió pero ningún DVH individual difiere (alta o baja externa), se informa como tal en vez de inventar una clave.
- El acceso de emergencia **no** valida contra la base: lee un XML fuera del árbol web (`App_Data/`, no servido por IIS) con usuario, salt y hash (mismo algoritmo que `HashHelper`). Es de único uso operativo — nunca se commitea el archivo real, solo un `.example`.
- Tras autenticar por XML, se ejecuta el mismo `RecalcularTodo()` del Parche 1.6 (ya recorre las 8 tablas) — no hace falta un algoritmo de reparación nuevo, solo exponerlo como acción explícita (`RepararBaseDatos()`).
- `Bitacora.IdUsuario` pasa a ser **nullable**: es el único cambio de esquema. Sin esto no se puede dejar constancia en bitácora de una reparación de emergencia cuando `Usuario` es justamente la tabla sospechada.
- **No** se registra en bitácora cada vez que el chequeo falla en un `Page_Load` (correría en cada request a una página anónima y ensuciaría la tabla). Se registra una única vez, al terminar la reparación de emergencia con éxito.
- Este parche **no** incluye la pantalla "Reparar Base de Datos" completa de CU-001-002 (menú, confirmación, listado de tablas reparadas para un Web Master ya logueado). Eso son ítems 21/22 del `Plan_Entregable_1` y quedan para un parche posterior que reutiliza `IIntegridadService.RepararBaseDatos()`.

---

## 1. BE — nuevos tipos compartidos (DAL ⇄ SEC)

Van en `Operativ.BE` porque tanto el DAL (que los produce) como el SEC (que los consume) pueden referenciar hacia abajo en la cascada UI → BLL/SEC → DAL → BE.

**`Operativ.BE/Entidades/ResultadoVerificacionTabla.cs`** (nueva):

```csharp
using System.Collections.Generic;

namespace Operativ.BE.Entidades
{
    public class ResultadoVerificacionTabla
    {
        public string NombreTabla { get; set; }

        public bool Integra { get; set; }

        public long ValorDvvAlmacenado { get; set; }

        public long ValorDvvCalculado { get; set; }

        public List<string> ClavesFilasInvalidas { get; set; }

        public ResultadoVerificacionTabla()
        {
            ClavesFilasInvalidas = new List<string>();
        }
    }
}
```

**`Operativ.BE/Enums/TipoError.cs`** — agregar tres valores (no reordenar los existentes: los códigos ERR se asignan por `switch`, no por posición del enum):

```csharp
ErrorIntegridadCorrupta,
ErrorCredencialesEmergenciaInvalidas,
ErrorArchivoEmergenciaNoDisponible
```

**`Operativ.BE/Errores/ErroresHandler.cs`** — agregar a `GetCodigo` y `GetClaveRecurso`. Códigos libres confirmados contra el `switch` actual (usados: 00,01,02,03,05,06,11,12,13,14,15):

| TipoError | Código | Clave de recurso |
| --- | --- | --- |
| `ErrorIntegridadCorrupta` | `ERR04` | `MensajeErrorIntegridadCorrupta` |
| `ErrorCredencialesEmergenciaInvalidas` | `ERR07` | `MensajeErrorCredencialesEmergenciaInvalidas` |
| `ErrorArchivoEmergenciaNoDisponible` | `ERR08` | `MensajeErrorArchivoEmergenciaNoDisponible` |

`ERR04` no es arbitrario: es el código que la carpeta STF usa explícitamente en la alternativa 2.1 de CU-001-001 para este caso.

**`Operativ.BE/Enums/TipoAccionBitacora.cs`** — agregar un valor:

```csharp
ReparacionEmergenciaBaseDatos
```

## 2. Base de datos — `Bitacora.IdUsuario` nullable

En `Scripts/CrearBaseDatos.sql`, cambiar la definición de la tabla:

```sql
CREATE TABLE Bitacora
(
    IdBitacora INT IDENTITY(1,1) NOT NULL,
    IdUsuario INT NULL,
    FechaHora DATETIME NOT NULL CONSTRAINT DF_Bitacora_FechaHora DEFAULT (GETDATE()),
    Accion VARCHAR(50) NOT NULL,
    Criticidad VARCHAR(20) NOT NULL,
    Descripcion VARCHAR(300) NULL,
    DVH BIGINT NULL,
    CONSTRAINT PK_Bitacora PRIMARY KEY (IdBitacora),
    CONSTRAINT FK_Bitacora_Usuario FOREIGN KEY (IdUsuario) REFERENCES Usuario (IdUsuario)
);
```

Una FK admite `NULL` en la columna hija sin cambios adicionales (un `NULL` no exige coincidencia con el padre). Para una base ya creada con el script viejo, el cambio equivalente es `ALTER TABLE Bitacora ALTER COLUMN IdUsuario INT NULL;` — agregarlo como comentario en el script para quien migre una instancia existente en vez de recrearla.

**`Operativ.BE/Entidades/Bitacora.cs`**: cambiar `public int IdUsuario { get; set; }` a `public int? IdUsuario { get; set; }`.

**`Operativ.DAL/Implementaciones/BitacoraRepositorio.cs`**: en `Registrar`, tratar `IdUsuario` igual que ya se trata `Descripcion`:

```csharp
object idUsuario = entrada.IdUsuario.HasValue ? (object)entrada.IdUsuario.Value : DBNull.Value;
```

y usar `idUsuario` en el `SqlParameter` en vez de `entrada.IdUsuario` directo.

**`Operativ.SEC.Contratos.IBitacoraService` / `BitacoraService`**: cambiar la firma de `Registrar` de `int idUsuario` a `int? idUsuario`. Es compatible hacia atrás — todo call-site actual pasa un `int`, que convierte implícito a `int?` — así que no hay que tocar ningún llamador existente.

## 3. DAL — verificación (`IntegridadRepositorio`)

**`Operativ.DAL/Contratos/IIntegridadRepositorio.cs`** — agregar un método (mantener los dos del Parche 1.6):

```csharp
namespace Operativ.DAL.Contratos
{
    public interface IIntegridadRepositorio
    {
        bool ExisteLineaBase();

        void RecalcularTodo();

        List<ResultadoVerificacionTabla> VerificarTodo();
    }
}
```

**`Operativ.DAL/Implementaciones/IntegridadRepositorio.cs`** — agregar la implementación reutilizando la misma lista privada `TablasVerificables` que ya usa `RecalcularTodo()` (no duplicarla):

```csharp
public List<ResultadoVerificacionTabla> VerificarTodo()
{
    List<ResultadoVerificacionTabla> resultadosInvalidos = new List<ResultadoVerificacionTabla>();

    using (SqlConnection conexion = new SqlConnection(ConexionDB.Instancia.GetCadenaConexion()))
    {
        conexion.Open();

        foreach (TablaVerificable tabla in TablasVerificables)
        {
            ResultadoVerificacionTabla resultado = VerificarTabla(conexion, tabla);

            if (!resultado.Integra)
            {
                resultadosInvalidos.Add(resultado);
            }
        }
    }

    return resultadosInvalidos;
}

private ResultadoVerificacionTabla VerificarTabla(SqlConnection conexion, TablaVerificable tabla)
{
    long dvvAlmacenado = ObtenerDvvAlmacenado(conexion, tabla.Nombre);
    List<long> valoresDvh = new List<long>();

    using (SqlCommand comando = new SqlCommand(string.Format("SELECT DVH FROM {0}", tabla.Nombre), conexion))
    {
        using (SqlDataReader lector = comando.ExecuteReader())
        {
            while (lector.Read())
            {
                if (!lector.IsDBNull(0))
                {
                    valoresDvh.Add(lector.GetInt64(0));
                }
            }
        }
    }

    long dvvCalculado = IntegridadHelper.CalcularDVV(valoresDvh);

    ResultadoVerificacionTabla resultado = new ResultadoVerificacionTabla
    {
        NombreTabla = tabla.Nombre,
        ValorDvvAlmacenado = dvvAlmacenado,
        ValorDvvCalculado = dvvCalculado,
        Integra = dvvAlmacenado == dvvCalculado
    };

    if (!resultado.Integra)
    {
        VerificarFilas(conexion, tabla, resultado);
    }

    return resultado;
}

private void VerificarFilas(SqlConnection conexion, TablaVerificable tabla, ResultadoVerificacionTabla resultado)
{
    DataTable filas = new DataTable();

    using (SqlCommand comando = new SqlCommand(string.Format("SELECT * FROM {0}", tabla.Nombre), conexion))
    {
        using (SqlDataReader lector = comando.ExecuteReader())
        {
            filas.Load(lector);
        }
    }

    foreach (DataRow fila in filas.Rows)
    {
        string cadenaBase = IntegridadHelper.ConstruirCadenaBase(fila);
        long dvhCalculado = IntegridadHelper.CalcularDVH(cadenaBase);
        object valorAlmacenado = fila["DVH"];

        bool filaValida = valorAlmacenado != DBNull.Value
            && Convert.ToInt64(valorAlmacenado) == dvhCalculado;

        if (!filaValida)
        {
            resultado.ClavesFilasInvalidas.Add(FormatearClave(tabla, fila));
        }
    }
}

private string FormatearClave(TablaVerificable tabla, DataRow fila)
{
    List<string> partes = new List<string>();

    foreach (string columna in tabla.ColumnasClave)
    {
        partes.Add(string.Format("{0}={1}", columna, fila[columna]));
    }

    return string.Join(", ", partes);
}

private long ObtenerDvvAlmacenado(SqlConnection conexion, string nombreTabla)
{
    using (SqlCommand comando = new SqlCommand(
        "SELECT ValorDVV FROM DigitosVerticales WHERE NombreTabla = @NombreTabla", conexion))
    {
        comando.Parameters.Add(new SqlParameter("@NombreTabla", nombreTabla));
        object resultado = comando.ExecuteScalar();

        if (resultado == null)
        {
            return long.MinValue;
        }

        return Convert.ToInt64(resultado);
    }
}
```

`ObtenerDvvAlmacenado` devuelve `long.MinValue` cuando no existe línea base para esa tabla (no debería pasar si `InicializarDigitos()` corrió en `Application_Start`, pero cubre el caso borde sin lanzar excepción) — nunca va a coincidir con un DVV calculado real, así que la tabla queda correctamente marcada como no íntegra.

Si el conteo de filas cambió pero **ningún** DVH individual difiere del recalculado (alta o baja de fila fuera de la aplicación), `ClavesFilasInvalidas` queda vacía — eso es la señal para el mensaje "no coincide la cantidad de registros" en el paso 5, en vez de listar una clave inexistente.

## 4. SEC — servicio de integridad

**`Operativ.SEC/Contratos/IIntegridadService.cs`**:

```csharp
namespace Operativ.SEC.Contratos
{
    public interface IIntegridadService
    {
        void InicializarDigitos();

        List<ResultadoVerificacionTabla> VerificarIntegridad();

        string FormatearResumenFallas(List<ResultadoVerificacionTabla> resultados);

        void RepararBaseDatos();
    }
}
```

**`Operativ.SEC/Implementaciones/IntegridadService.cs`** — agregar:

```csharp
public List<ResultadoVerificacionTabla> VerificarIntegridad()
{
    return integridadRepositorio.VerificarTodo();
}

public void RepararBaseDatos()
{
    integridadRepositorio.RecalcularTodo();
}

public string FormatearResumenFallas(List<ResultadoVerificacionTabla> resultados)
{
    StringBuilder resumen = new StringBuilder();

    foreach (ResultadoVerificacionTabla resultado in resultados)
    {
        if (resumen.Length > 0)
        {
            resumen.Append("; ");
        }

        resumen.Append(resultado.NombreTabla);

        if (resultado.ClavesFilasInvalidas.Count > 0)
        {
            resumen.Append(" (registros alterados: ");
            resumen.Append(string.Join(", ", resultado.ClavesFilasInvalidas));
            resumen.Append(")");
        }
        else
        {
            resumen.Append(" (no coincide la cantidad de registros; posible alta o baja fuera del sistema)");
        }
    }

    return resumen.ToString();
}
```

`RepararBaseDatos()` es una delegación de una línea a `RecalcularTodo()` — mismo algoritmo del Parche 1.6, solo que ahora se puede invocar bajo demanda (no solo cuando no existe línea base) y con un nombre que dice lo que hace en este contexto.

`FormatearResumenFallas` vive acá (no en `ErroresHandler` ni en el code-behind) porque la va a necesitar también la futura pantalla "Reparar Base de Datos" (ítem 22 del `Plan_Entregable_1`) — un solo lugar, reutilizable desde cualquier UI.

## 5. SEC — acceso de emergencia contra XML oculto

**`Operativ.SEC/Helpers/EmergenciaHelper.cs`** (nueva, estática — cumple además el ítem 13 pendiente del `Plan_Entregable_1`: "Manejo de XML con clases clásicas", primera vez que se necesita en el proyecto):

```csharp
using System;
using System.Web;
using System.Xml;
using Operativ.BE.Enums;
using Operativ.BE.Errores;
using Operativ.SEC.Configuracion;

namespace Operativ.SEC.Helpers
{
    public static class EmergenciaHelper
    {
        public static bool ValidarCredenciales(string nombreUsuario, string contrasena)
        {
            XmlDocument documento = CargarDocumento();

            string nombreUsuarioEsperado = LeerNodo(documento, "NombreUsuario");
            string salt = LeerNodo(documento, "Salt");
            string hashAlmacenado = LeerNodo(documento, "HashContrasena");

            if (!string.Equals(nombreUsuario, nombreUsuarioEsperado, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            return HashHelper.ValidarContrasena(contrasena, salt, hashAlmacenado);
        }

        private static XmlDocument CargarDocumento()
        {
            try
            {
                string rutaVirtual = ConfiguracionAplicacion.RutaXmlEmergencia;
                string rutaFisica = HttpContext.Current.Server.MapPath(rutaVirtual);

                XmlDocument documento = new XmlDocument();
                documento.Load(rutaFisica);
                return documento;
            }
            catch (Exception)
            {
                throw new OperativException(TipoError.ErrorArchivoEmergenciaNoDisponible);
            }
        }

        private static string LeerNodo(XmlDocument documento, string nombreNodo)
        {
            XmlNode nodo = documento.SelectSingleNode("//" + nombreNodo);

            if (nodo == null)
            {
                throw new OperativException(TipoError.ErrorArchivoEmergenciaNoDisponible);
            }

            return nodo.InnerText.Trim();
        }
    }
}
```

Se usa `XmlDocument` (API clásica), no LINQ to XML, en línea con el punto 13 pendiente del plan de entrega. Cualquier problema leyendo o parseando el archivo se traduce a `ErrorArchivoEmergenciaNoDisponible` (`ERR08`) en el momento — no deja que una `XmlException`/`IOException` cruda llegue al code-behind, siguiendo el mismo criterio que ya usa `UsuarioService` (lanzar `OperativException` directamente para casos de negocio conocidos, reservando `ErroresHandler.TraducirExcepcion` para lo verdaderamente inesperado).

No se diferencia "usuario no coincide" de "contraseña incorrecta": ambos casos devuelven `false` y el llamador lanza el mismo `ErrorCredencialesEmergenciaInvalidas` (`ERR07`) sin distinguir — es la cuenta de última instancia, no conviene dar ninguna pista adicional.

**`Operativ.SEC/Configuracion/ConfiguracionAplicacion.cs`** — agregar, mismo patrón que las propiedades SMTP existentes:

```csharp
public static string RutaXmlEmergencia
{
    get { return GetConfiguracion("Operativ.Emergencia.RutaXml", "~/App_Data/AccesoEmergencia.xml"); }
}
```

**`Operativ.Web/Web.config`** — agregar a `appSettings` (opcional, ya tiene default arriba):

```xml
<add key="Operativ.Emergencia.RutaXml" value="~/App_Data/AccesoEmergencia.xml" />
```

**`Operativ.Web/App_Data/AccesoEmergencia.xml.example`** (nuevo, este sí se commitea):

```xml
<?xml version="1.0" encoding="utf-8"?>
<AccesoEmergencia>
  <NombreUsuario>webmaster.emergencia</NombreUsuario>
  <Salt>REEMPLAZAR_POR_SALT_BASE64</Salt>
  <HashContrasena>REEMPLAZAR_POR_HASH_BASE64</HashContrasena>
</AccesoEmergencia>
```

`Salt`/`HashContrasena` se generan con el mismo algoritmo que ya usa `HashHelper` (`GenerarSalt()` + `GenerarHash(contrasena, salt)`) — igual que se pre-calcularon los hashes de los usuarios semilla en `CrearBaseDatos.sql`. Documentar en el README el paso manual (un pequeño snippet de consola que llame a esos dos métodos alcanza) para generar el par real.

**`App_Data/AccesoEmergencia.xml`** (el archivo real, con los datos que se le entregan al cliente): **nunca se commitea**. Agregar al `.gitignore`:

```
Operativ.Web/App_Data/AccesoEmergencia.xml
```

`App_Data` no se sirve por HTTP (restricción a nivel de `system.web` que aplica independientemente del "flavor" de proyecto documentado en `CLAUDE.md`), así que cumple el "inaccesible desde la aplicación web" de la carpeta de Garza.

> ⚠️ **Nota aparte, no bloqueante para este parche:** `Web.config` ya tiene commiteada una contraseña real de aplicación de Gmail (`Operativ.Smtp.Contrasena`) en un repo público. No repetir el patrón acá es la razón concreta de este `.gitignore` — aprovechen para rotar esa contraseña de SMTP y sacarla del historial de git en algún momento, aunque sea en un parche aparte.

## 6. Web — `Login.aspx` y `Login.aspx.cs`

**Markup (`Login.aspx`)**: agregar un segundo `Panel`, oculto por defecto, debajo del formulario normal:

```aspx
<asp:Panel ID="pnlAccesoEmergencia" runat="server" Visible="false" CssClass="panel-emergencia">
    <p><asp:Literal ID="litAvisoEmergencia" runat="server" Text="Acceso de emergencia (Web Master)" /></p>
    <asp:TextBox ID="txtUsuarioEmergencia" runat="server" />
    <asp:RequiredFieldValidator ID="rfvUsuarioEmergencia" runat="server"
        ControlToValidate="txtUsuarioEmergencia" ValidationGroup="Emergencia" ErrorMessage="*" />
    <asp:TextBox ID="txtContrasenaEmergencia" runat="server" TextMode="Password" />
    <asp:RequiredFieldValidator ID="rfvContrasenaEmergencia" runat="server"
        ControlToValidate="txtContrasenaEmergencia" ValidationGroup="Emergencia" ErrorMessage="*" />
    <asp:Button ID="btnIngresoEmergencia" runat="server" Text="Ingresar y reparar"
        ValidationGroup="Emergencia" OnClick="btnIngresoEmergencia_Click" />
</asp:Panel>
```

Usar un `ValidationGroup` distinto (`"Emergencia"`) es necesario para que los validadores del formulario normal no bloqueen el envío de este panel y viceversa — revisar si el formulario normal ya tiene uno asignado y, si no, asignarle `"Login"` explícitamente en el mismo cambio.

El panel normal de login puede quedar visible o no junto con el de emergencia; alcanza con que ambos funcionen — es una decisión de diseño visual, no funcional, así que se resuelve al implementar mirando el resto de la paleta en `Estilos/`.

**`Login.aspx.cs`** — reescribir `Page_Load` y agregar el nuevo handler:

```csharp
public partial class Login : PaginaBase
{
    private SesionHandler sesionHandler;
    private ErroresHandler erroresHandler;
    private bool modoEmergencia;

    protected void Page_Load(object sender, EventArgs e)
    {
        sesionHandler = new SesionHandler();
        erroresHandler = new ErroresHandler();

        if (!IsPostBack && sesionHandler.HaySesionActiva())
        {
            Familia perfilActivo = sesionHandler.GetPerfil();
            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilActivo.Nombre));
        }

        VerificarIntegridadSistema();

        if (!IsPostBack && Request.QueryString["err"] == "sesion")
        {
            ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(TipoError.ErrorSesionExpirada));
        }
    }

    private void VerificarIntegridadSistema()
    {
        try
        {
            FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
            IIntegridadService integridadService = fabricaSeguridad.CrearIntegridadService();
            List<ResultadoVerificacionTabla> resultadosInvalidos = integridadService.VerificarIntegridad();

            modoEmergencia = resultadosInvalidos.Count > 0;

            if (modoEmergencia)
            {
                string detalle = integridadService.FormatearResumenFallas(resultadosInvalidos);
                ucNotificaciones.MostrarMensaje(
                    erroresHandler.GetMensaje(TipoError.ErrorIntegridadCorrupta, new string[] { detalle }));
                pnlAccesoEmergencia.Visible = true;
                btnIngresar.Enabled = false;
            }
        }
        catch (Exception excepcion)
        {
            modoEmergencia = true;
            OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
            ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
            pnlAccesoEmergencia.Visible = true;
            btnIngresar.Enabled = false;
        }
    }

    protected void btnIngresar_Click(object sender, EventArgs e)
    {
        if (modoEmergencia || !Page.IsValid)
        {
            return;
        }

        // ... sin cambios respecto del código actual ...
    }

    protected void btnIngresoEmergencia_Click(object sender, EventArgs e)
    {
        if (!Page.IsValid)
        {
            return;
        }

        try
        {
            bool credencialesValidas = EmergenciaHelper.ValidarCredenciales(
                txtUsuarioEmergencia.Text.Trim(), txtContrasenaEmergencia.Text);

            if (!credencialesValidas)
            {
                throw new OperativException(TipoError.ErrorCredencialesEmergenciaInvalidas);
            }

            FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
            IIntegridadService integridadService = fabricaSeguridad.CrearIntegridadService();
            integridadService.RepararBaseDatos();

            RegistrarReparacionEmergencia(fabricaSeguridad);

            Usuario usuarioEmergencia = new Usuario
            {
                IdUsuario = 0,
                NombreUsuario = txtUsuarioEmergencia.Text.Trim(),
                NombreCompleto = "Web Master (acceso de emergencia)",
                Activo = true,
                Bloqueado = false
            };

            Familia perfilEmergencia = new Familia
            {
                IdFamilia = 0,
                Nombre = "WebMaster",
                Descripcion = "Acceso de emergencia"
            };

            sesionHandler.IniciarSesion(usuarioEmergencia, perfilEmergencia, new FamiliaCompuesto());

            Response.Redirect(NavegacionHelper.ObtenerUrlHome(perfilEmergencia.Nombre) + "?reparado=1", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        catch (Exception excepcion)
        {
            OperativException excepcionOperativ = erroresHandler.TraducirExcepcion(excepcion);
            ucNotificaciones.MostrarMensaje(erroresHandler.GetMensaje(excepcionOperativ));
        }
    }

    private void RegistrarReparacionEmergencia(FabricaSeguridad fabricaSeguridad)
    {
        IBitacoraService bitacoraService = fabricaSeguridad.CrearBitacoraService();
        bitacoraService.Registrar(null, TipoAccionBitacora.ReparacionEmergenciaBaseDatos);
    }
}
```

Puntos a tener en cuenta al implementar:

- `modoEmergencia` es un campo de instancia calculado en cada `Page_Load` (GET y POST): en WebForms, `Page_Load` siempre corre antes que el handler del botón en el mismo request, así que `btnIngresar_Click` ve el valor recién calculado de ese mismo postback — no hace falta persistirlo en `ViewState` ni `Session`.
- Si `VerificarIntegridadSistema()` explota por una `SqlException` (no ya un DVV que no coincide, sino que ni siquiera se puede conectar), se trata igual que una corrupción: se activa `modoEmergencia` y se ofrece la vía de emergencia — es exactamente el escenario para el que existe. El mensaje que ve el usuario en ese caso es el de `ERR05` (`ErrorConexionBaseDatos`, ya existente), no el de `ERR04`.
- `BitacoraService.Registrar` recibe `TipoAccionBitacora.ReparacionEmergenciaBaseDatos` sin descripción explícita en este snippet — como ya existe un `switch` de descripciones en `BitacoraService.GetDescripcion`, agregar ahí el caso en vez de pasarlo desde afuera (mantiene el patrón actual, ver siguiente sección).
- `Descripcion` en `Bitacora` es `VARCHAR(300)`: si se decide incluir el detalle de fallas en la descripción de este evento (recomendado, para no perder qué se reparó), truncarlo defensivamente a 300 caracteres antes de pasarlo — el patrón actual de `BitacoraService.GetDescripcion` no recibe parámetros externos hoy, así que esto es una extensión menor a ese método o a `Registrar`.

**`Operativ.SEC/Implementaciones/BitacoraService.cs`** — agregar el caso nuevo en ambos `switch`:

```csharp
case TipoAccionBitacora.ReparacionEmergenciaBaseDatos:
    return CriticidadBitacora.Critico;
```
```csharp
case TipoAccionBitacora.ReparacionEmergenciaBaseDatos:
    return "Base de datos reparada mediante acceso de emergencia del Web Master";
```

## 7. Recursos de texto

`Operativ.Web/App_GlobalResources/Textos.resx` (y su par `Textos.en.resx`) — agregar tres claves. El mensaje de integridad lleva `{0}` para el detalle que arma `FormatearResumenFallas`:

| Clave | Texto (ES) |
| --- | --- |
| `MensajeErrorIntegridadCorrupta` | Se detectó una alteración en la integridad de los datos del sistema. Detalle: {0}. Si sos Web Master, podés ingresar por la vía de emergencia para reparar la base. |
| `MensajeErrorCredencialesEmergenciaInvalidas` | Las credenciales de emergencia ingresadas no son válidas. |
| `MensajeErrorArchivoEmergenciaNoDisponible` | No se pudo acceder a la configuración de acceso de emergencia. Contactá a soporte técnico. |

Texto equivalente en inglés para `Textos.en.resx`, mismo `{0}`.

## 8. `.csproj` — archivos nuevos

Al ser proyectos de .NET Framework clásico (sin globbing automático), agregar manualmente las entradas `<Compile Include="...">` en:

- `Operativ.BE.csproj`: `Entidades\ResultadoVerificacionTabla.cs`
- `Operativ.SEC.csproj`: `Helpers\EmergenciaHelper.cs`
- (los cambios en archivos existentes no requieren entradas nuevas)

Y `<Content Include="App_Data\AccesoEmergencia.xml.example">` en `Operativ.Web.csproj` (el `.xml` real, al no commitearse, tampoco entra en el `.csproj`).

## 9. Pasos de implementación (orden sugerido)

1. `TipoError`, `TipoAccionBitacora`, `ErroresHandler` (códigos + claves de recurso) — BE.
2. `Bitacora.IdUsuario` a `int?`; `Scripts/CrearBaseDatos.sql`; `BitacoraRepositorio.Registrar` con `DBNull.Value`; `IBitacoraService`/`BitacoraService.Registrar(int?, ...)` + caso nuevo en los dos `switch` de `BitacoraService`.
3. `ResultadoVerificacionTabla` — BE.
4. `IIntegridadRepositorio.VerificarTodo()` + implementación (`VerificarTabla`, `VerificarFilas`, `FormatearClave`, `ObtenerDvvAlmacenado`) — DAL.
5. `IIntegridadService` (`VerificarIntegridad`, `FormatearResumenFallas`, `RepararBaseDatos`) — SEC.
6. `ConfiguracionAplicacion.RutaXmlEmergencia`; `EmergenciaHelper`; `AccesoEmergenciaXML.example`; `.gitignore`; `Web.config` (appSetting opcional) — SEC/Web.
7. Recursos de texto (`Textos.resx`, `Textos.en.resx`).
8. `Login.aspx` (panel de emergencia) + `Login.aspx.cs` (`Page_Load`, `btnIngresar_Click` guard, `btnIngresoEmergencia_Click`).
9. Entradas nuevas en los `.csproj`.
10. Generar un XML de emergencia real de prueba (salt + hash calculados a mano con `HashHelper`) para las pruebas manuales del punto 10 — nunca commitearlo.
11. Probar (punto 10).

## 10. Pruebas manuales

1. **Camino feliz (sin corrupción)**: con la base intacta tras el Parche 1.6, entrar a `Login.aspx` → no aparece ningún aviso de integridad, el panel de emergencia permanece oculto, el login normal funciona igual que antes.
2. **Corrupción de una fila (DVH)**: por SSMS, `UPDATE Usuario SET NombreCompleto = 'Test' WHERE IdUsuario = 1` sin recalcular el DVH → recargar `Login.aspx` → debe aparecer `ERR04` con el detalle `Usuario (registros alterados: IdUsuario=1)`, el botón de login normal deshabilitado, panel de emergencia visible.
3. **Alta/baja externa (DVV sin DVH individual roto)**: insertar una fila en `Patente` directamente por SQL (sin pasar por la app, por ende sin DVH calculado o con uno que da igual el chequeo por fila) → el mensaje debe decir "no coincide la cantidad de registros" para `Patente`, no una clave inventada. (Ajustar el escenario según cómo termine quedando el DVH de la fila insertada a mano — el punto es validar la rama del código, no un escenario SQL específico.)
4. **Múltiples tablas corruptas**: repetir 2 sobre `Usuario` y `Bitacora` a la vez → el detalle en el mensaje debe listar ambas, separadas por `; `.
5. **Acceso de emergencia con credenciales incorrectas**: con el sistema en modo emergencia, ingresar usuario/contraseña que no coincidan con el XML → `ERR07`, no se ejecuta ninguna reparación (verificar por SSMS que `DigitosVerticales.FechaCalculo` no cambió).
6. **Acceso de emergencia exitoso**: credenciales correctas contra el XML de prueba → redirige a `HomeWebMaster.aspx?reparado=1`, y por SSMS: todas las tablas con `DVH` recalculado y coincidente, `DigitosVerticales` con `FechaCalculo` actualizada en las 8 filas, y una fila nueva en `Bitacora` con `IdUsuario NULL`, `Accion = 'ReparacionEmergenciaBaseDatos'`.
7. **Archivo de emergencia ausente o corrupto**: renombrar/borrar `AccesoEmergencia.xml` → intentar acceso de emergencia → `ERR08`, sin excepción no controlada (no debe caer en `Error.aspx`).
8. **Login con `SqlException` real** (parar el servicio de SQL Server o apuntar a una instancia inexistente): recargar `Login.aspx` → debe mostrar `ERR05` y ofrecer igual el panel de emergencia (la autenticación contra XML no depende de la base).
9. **Sesión post-emergencia respeta el perfil**: logueado por la vía de emergencia, verificar que `GestionUsuarios.aspx` (protegida por `PaginaSeguraBase` con `PerfilRequerido = "WebMaster"`) es accesible — confirma que el `Familia.Nombre = "WebMaster"` sintético alcanza sin tocar `AutorizacionHandler`.

## 11. Definición de "terminado"

- `Login.aspx` verifica integridad en todo `Page_Load` (GET y POST), antes de permitir el submit del formulario normal.
- El mensaje `ERR04` incluye, cuando es posible, la tabla y el/los registro(s) puntuales alterados; cuando no es posible (alta/baja externa), lo dice explícitamente en vez de omitir el detalle o inventarlo.
- El Web Master puede autenticarse contra el XML oculto sin que la base de datos participe de esa autenticación, y esa autenticación dispara el recálculo de DVH/DVV de las 8 tablas.
- La reparación de emergencia queda registrada en bitácora con `IdUsuario NULL`, sin romper ninguna escritura de bitácora existente (todos los call-sites actuales siguen pasando un `int` no nulo).
- El archivo XML real de emergencia no está en el repositorio; sí lo está su plantilla `.example` y la entrada correspondiente en `.gitignore`.
- Ningún mensaje de error nuevo se construye fuera de `ErroresHandler`/recursos `.resx` (se mantiene la regla de "Prohibidos los strings de error sueltos").
- Cero SQL dinámico nuevo con valores concatenados (los `string.Format` de nombres de tabla/columna siguen siendo internos a la lista fija `TablasVerificables`, igual que en el Parche 1.6; los valores siempre van como `SqlParameter`).

## 12. Fuera de alcance / diferido (explícito)

- **Pantalla "Reparar Base de Datos" completa** (CU-001-002 curso normal): menú accesible para un Web Master ya logueado, confirmación antes de recalcular, listado de tablas reparadas en pantalla — ítems 21/22 del `Plan_Entregable_1`. Este parche deja `IIntegridadService.RepararBaseDatos()` listo para que esa pantalla lo llame directamente.
- **Generación asistida del XML de emergencia** (herramienta o pantalla para crear/rotar el usuario y password de emergencia): por ahora es un paso manual documentado, igual que los hashes semilla de `CrearBaseDatos.sql`.
- **Registro en bitácora de la mera detección de falla** (sin reparación): decisión explícita de no hacerlo para no ensuciar la bitácora en cada `Page_Load` sobre una base corrupta.
- **Confirmación con doble factor o timeout para el acceso de emergencia**: la carpeta no lo pide; se deja como lectura de usuario/hash únicamente.
- **Atomicidad escritura+dígitos en una única transacción**: limitación ya aceptada desde el Parche 1.6, no cambia acá.
- **Rotación de la credencial SMTP expuesta en `Web.config`**: hallazgo aparte, no bloqueante para este parche, mencionado en la sección 5.

---

## Historial de cambios

| Versión | Cambio |
| --- | --- |
| 1.7 | Chequeo de integridad en `Login.aspx` (verificación DVV por tabla con detalle DVH por fila cuando corresponde), mensaje `ERR04` con tabla/registro afectado, acceso de emergencia del Web Master contra XML oculto (`EmergenciaHelper`, clases XML clásicas) con recálculo posterior de dígitos vía `IIntegridadService.RepararBaseDatos()`, y `Bitacora.IdUsuario` nullable para poder auditar la reparación de emergencia. |
