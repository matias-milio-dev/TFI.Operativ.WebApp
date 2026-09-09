# Plan: Consultar Bitácora — filtros, paginado y permisos — Operativ (2.2)

**Repo:** `matias-milio-dev/TFI.Operativ.WebApp`, base `main` (HEAD auditado: `2978dfeb6f0eb0838a1d4200425937028c757342`, mismo HEAD que el plan 2.1 — todavía no se aplicó ninguno de los dos).
**Rama a crear:** `feature/parche-2.2-consultar-bitacora` (puede partir de `main` directo, o de encima de la rama de 2.1 si ese se aplica primero — no hay dependencia real entre ambos, salvo que los dos tocan `CategoriaPatente.Sistema` y el seed de `Patente`/`FamiliaPatente`, ver nota al final de la sección 2).
**No hacer merge ni push a `main`.** Dejar los commits listos en la rama local para que el dueño del repo los revise y los suba él mismo.

Recordatorio de estándares: sin comentarios en C#, sin `var`, sin tuplas/record/LINQ/lambdas, `namespace X;` scoped, ifs con llaves, orden público→privado, repositorios/servicios solo vía fábricas, acceso a datos parametrizado vía `AccesoDatos` (nada de Stored Procedures acá — esa fue una excepción puntual del parche 2.1), errores vía `OperativException`/`TipoError`/`DefinicionError`.

---

## 0. Corrección a implementar

Página para consultar la bitácora del sistema (CU-003-011 de la carpeta de TFI), con los filtros que ya prevé esa misma especificación — fecha, usuario, acción, criticidad — y paginado. Mismos estilos que `GestionUsuarios` (que es la referencia de "grilla con barra de filtros" del proyecto). Otra vez, pensar permisos por familia y patente individual antes de tocar código.

---

## 1. Diseño y decisiones

### 1.1 Familia y patente: quién puede entrar

La carpeta de TFI es explícita en esto — **CU-003-011 Consultar Bitácora, Actores: Web Master** (único actor listado, a diferencia de otros CU que listan Administrador y Web Master juntos). Se sigue el mismo criterio de fidelidad a los actores documentados que ya se usó en parches anteriores (por ejemplo, `GestionUsuarios` nunca se abrió para WebMaster aunque la carpeta mencione a ese perfil en algunos CU de usuario). Se agrega una patente nueva, `ConsultarBitacora`, sembrada solo para la familia `WebMaster`, en la categoría `Sistema` que ya agrupa las otras capacidades técnicas de esa familia (`RealizarBackup`, `RestaurarBackup` del parche 2.1, `RepararBaseDatos`).

Como es una sola patente (no dos acciones con distinto riesgo, como pasó con backup/restore o asignar/remover), no hace falta ningún desdoblamiento — es un permiso de solo lectura.

### 1.2 Acceso a la página: un solo patrón, sin partial-access

`GestionUsuarios` gatea `PatentesPermitidas` con **toda una categoría** (`CategoriaPatente.Usuarios.NombresPatente`, 8 patentes), lo que genera escenarios de "acceso parcial" reales: alguien puede entrar a la página con `AltaUsuario` pero sin `ConsultarUsuario`, y por eso `AplicarVisibilidadPorPatentes` oculta `pnlFiltros`/`pnlListado` puntualmente y `CargarGrilla` tiene una guarda extra por las dudas.

Acá no aplica ese caso: `PatentesPermitidas` de esta página es **una sola patente** (`ConsultarBitacora`). Si `ValidarAcceso` (en `PaginaSeguraBase`, sin cambios) ya te dejó entrar, es porque tenés exactamente esa patente — no existe un escenario de "entré pero me falta ver la grilla". Por eso esta página no necesita `AplicarVisibilidadPorPatentes` ni una guarda repetida en `CargarGrilla`: sería una defensa contra un caso que no puede ocurrir. Si en algún parche futuro esta página gana una segunda patente (por ejemplo, para exportar a CSV), ahí sí correspondería agregar ese mismo patrón.

### 1.3 Filtros: mismo layout que GestionUsuarios, con una vuelta extra

Misma `barra-busqueda` / `barra-busqueda-filtros` / `campo-formulario` que ya usa `GestionUsuarios`, con 5 campos en vez de 2 (orden calcado del texto del propio CU: "fecha, usuario, acción, criticidad"):

- **Desde / Hasta**: dos `asp:TextBox TextMode="Date"` (renderiza `<input type="date">`). El CSS compartido de inputs (`.campo-formulario input[type=text], input[type=password], select`) no incluía `input[type=date]` — se agrega a esa misma regla para que el date picker salga con el mismo aspecto que el resto del formulario, en vez de quedar sin estilo.
- **Usuario**: `asp:TextBox` de texto libre, igual que el filtro de `GestionUsuarios` — busca por `LIKE` contra `NombreUsuario`, no un combo de selección.
- **Acción**: `asp:DropDownList` con las 17 acciones de `TipoAccionBitacora`, poblado desde `AccionBitacora.ObtenerTodas()` (reutiliza el smart enum que ya existe, no hay que duplicar la lista de acciones en la capa Web).
- **Criticidad**: `asp:DropDownList` con los 4 valores de `CriticidadBitacora`, con texto traducido por recurso (ver 1.5).

Todo dentro del mismo `pnlFiltros` (`CssClass="barra-busqueda-filtros"`) + botón "Buscar", igual estructura que `GestionUsuarios`. No hay un botón equivalente a "Nuevo usuario" (esta página es de solo consulta), así que la `barra-busqueda` de esta página solo tiene el panel de filtros, sin acción hermana al lado.

### 1.4 Por qué la grilla no tiene una columna "Acción" separada

`AccionBitacora.LoginBloqueado.Descripcion` es `"Usuario bloqueado tras {0} intentos fallidos"` — un texto con placeholder, que `BitacoraService.Registrar` ya rellena con el valor real **al momento de grabar** cada fila (`Bitacora.Descripcion` en la base ya viene armado y completo, sin placeholders sueltos). Si la grilla mostrara una columna "Acción" separada traducida en el momento de leer (llamando de nuevo a `AccionBitacora.ObtenerPorTipo(...).Descripcion`), esa fila en particular mostraría el `{0}` sin rellenar — un bug, no una decisión de diseño.

La solución más simple y más correcta es no derivar nada de nuevo al leer: la grilla muestra directamente `Bitacora.Descripcion`, que ya es el texto completo y humano tal como quedó grabado (incluye la acción y, si la hubo, la información adicional — por ejemplo, para una asignación de patentes el texto ya trae los nombres de las patentes asignadas). Columnas finales: **Fecha, Usuario, Descripción, Criticidad**. El filtro por "acción" sigue existiendo (filtra por `TipoAccionBitacora` en la base), solo que no hace falta una columna separada para eso porque la descripción ya lo cuenta.

Para las filas sin usuario asociado (`IdUsuario` nulo — reparación de emergencia, alteración de integridad, backup, restore), la columna "Usuario" muestra "Sistema" en vez de quedar vacía.

### 1.5 Traducción de `CriticidadBitacora`

A diferencia de `TipoAccionBitacora` (que ya tiene su smart enum `AccionBitacora` con una `Descripcion` en español para mostrar), `CriticidadBitacora` es un enum simple de 4 valores (`Informativo`, `Advertencia`, `Critico`, `Grave`) sin ningún wrapper. Sus nombres ya son palabras en español razonables, pero mostrarlos tal cual rompería el inglés cuando el sitio está en ese idioma (el enum no está pensado para mostrarse directo, es infraestructura interna). Se agregan 4 claves de recurso nuevas (`EtiquetaCriticidad{Valor}`) y un método `ObtenerTextoCriticidad` con un `switch` explícito — no se arma un smart enum nuevo para 4 valores fijos que no van a crecer, sería sobre-ingeniería para este caso.

Se muestra como badge en la grilla, reutilizando la clase base `.badge` ya existente con 4 modificadores de color nuevos (`.badge-informativo`, `.badge-advertencia`, `.badge-critico`, `.badge-grave`) que arman una escalada visual con los mismos tokens de color que ya define `:root` (`--color-advertencia`, `--color-error`) — nada de colores nuevos inventados.

### 1.6 Qué NO se toca (fuera de alcance)

- No hay exportación a CSV/Excel ni impresión — la carpeta de TFI no lo pide para este CU, solo consultar con filtros.
- No se le da acceso a `Administrador` aunque gestione usuarios y algo de esa actividad termine en la bitácora — el CU es explícito en que el actor es Web Master únicamente.
- No se agrega una forma de borrar o purgar registros viejos de la bitácora — es un log de auditoría, se asume de solo lectura y sin fecha de expiración en este alcance.

---

## 2. Base de datos

### 2.1 `Scripts/CrearBaseDatos.sql` — agregar la patente al seed

Agregar una fila al `INSERT INTO Patente` (después de `RepararBaseDatos`/`RealizarBackup`/`RestaurarBackup` si ya se aplicó el parche 2.1, o después de `RepararBaseDatos`/`RealizarBackup` si este parche se aplica antes que ese — cualquiera de los dos órdenes es válido, son cambios independientes sobre el mismo `INSERT`):

```sql
('ConsultarBitacora', 'Permite consultar los registros de actividad del sistema.'),
```

Y agregar `ConsultarBitacora` a la lista de `WebMaster` en el `INSERT INTO FamiliaPatente`:

```sql
WHERE (F.Nombre = 'WebMaster' AND P.Nombre IN ('RepararBaseDatos', 'RealizarBackup', 'RestaurarBackup', 'ConsultarBitacora'))
```

(Si el parche 2.1 todavía no se aplicó, la lista de WebMaster en ese `IN (...)` va a ser `('RepararBaseDatos', 'RealizarBackup', 'ConsultarBitacora')` — sin `RestaurarBackup` — hasta que se aplique.)

No hay cambios de esquema: la tabla `Bitacora` ya tiene todas las columnas que hacen falta para filtrar y listar.

---

## 3. Capa BE

### 3.1 `Operativ.BE/Entidades/Bitacora.cs` — contenido completo

Se agrega `NombreUsuario`, poblada solo cuando se lee con el JOIN contra `Usuario` (sección 4.2) — mismo criterio que ya usa `Usuario.NombreFamilia`: una propiedad de conveniencia para mostrar, que no todos los caminos de lectura llenan.

```csharp
using System;
using Operativ.BE.Enums;

namespace Operativ.BE.Entidades;
public class Bitacora
{
    public int IdBitacora { get; set; }

    public int? IdUsuario { get; set; }

    public DateTime FechaHora { get; set; }

    public TipoAccionBitacora Accion { get; set; }

    public CriticidadBitacora Criticidad { get; set; }

    public string Descripcion { get; set; }

    public string NombreUsuario { get; set; }
}
```

### 3.2 `Operativ.BE/Modelos/NombrePatente.cs` — contenido completo

```csharp
namespace Operativ.BE.Modelos;

public static class NombrePatente
{
    public const string ConsultarUsuario = "ConsultarUsuario";
    public const string AltaUsuario = "AltaUsuario";
    public const string BajaUsuario = "BajaUsuario";
    public const string ModificacionUsuario = "ModificacionUsuario";
    public const string DesbloqueoUsuario = "DesbloqueoUsuario";
    public const string BloqueoUsuario = "BloqueoUsuario";
    public const string AsignarPatente = "AsignarPatente";
    public const string RemoverPatente = "RemoverPatente";
    public const string GestionarFamilias = "GestionarFamilias";
    public const string GestionarClientes = "GestionarClientes";
    public const string GestionarCatalogo = "GestionarCatalogo";
    public const string GestionarSuscripciones = "GestionarSuscripciones";
    public const string ConsultarFacturas = "ConsultarFacturas";
    public const string ReportarIncidentes = "ReportarIncidentes";
    public const string RealizarBackup = "RealizarBackup";
    public const string RestaurarBackup = "RestaurarBackup";
    public const string RepararBaseDatos = "RepararBaseDatos";
    public const string ConsultarBitacora = "ConsultarBitacora";
}
```

(Si el parche 2.1 todavía no se aplicó, sacar la línea de `RestaurarBackup` de este archivo — no es parte de este parche.)

### 3.3 `Operativ.BE/Modelos/CategoriaPatente.cs` — contenido completo

Solo cambia `Sistema`, que suma la patente nueva.

```csharp
using System.Collections.Generic;
using Operativ.BE.Enums;

namespace Operativ.BE.Modelos;

public class CategoriaPatente
{
    public TipoCategoriaPatente Tipo { get; }

    public string ClaveRecurso { get; }

    public string[] NombresPatente { get; }

    public CategoriaPatente(TipoCategoriaPatente tipo, string claveRecurso, string[] nombresPatente)
    {
        Tipo = tipo;
        ClaveRecurso = claveRecurso;
        NombresPatente = nombresPatente;
    }

    public static readonly CategoriaPatente Usuarios =
        new(TipoCategoriaPatente.Usuarios, "CategoriaUsuarios", new[]
        {
            NombrePatente.ConsultarUsuario,
            NombrePatente.AltaUsuario,
            NombrePatente.BajaUsuario,
            NombrePatente.ModificacionUsuario,
            NombrePatente.DesbloqueoUsuario,
            NombrePatente.BloqueoUsuario,
            NombrePatente.AsignarPatente,
            NombrePatente.RemoverPatente
        });
    public static readonly CategoriaPatente Familias =
        new(TipoCategoriaPatente.Familias, "CategoriaFamilias", new[] { NombrePatente.GestionarFamilias });
    public static readonly CategoriaPatente Clientes =
        new(TipoCategoriaPatente.Clientes, "CategoriaClientes", new[] { NombrePatente.GestionarClientes });
    public static readonly CategoriaPatente Catalogo =
        new(TipoCategoriaPatente.Catalogo, "CategoriaCatalogo", new[] { NombrePatente.GestionarCatalogo });
    public static readonly CategoriaPatente Suscripciones =
        new(TipoCategoriaPatente.Suscripciones, "CategoriaSuscripciones", new[] { NombrePatente.GestionarSuscripciones });
    public static readonly CategoriaPatente Facturacion =
        new(TipoCategoriaPatente.Facturacion, "CategoriaFacturacion", new[] { NombrePatente.ConsultarFacturas });
    public static readonly CategoriaPatente Incidentes =
        new(TipoCategoriaPatente.Incidentes, "CategoriaIncidentes", new[] { NombrePatente.ReportarIncidentes });
    public static readonly CategoriaPatente Sistema =
        new(TipoCategoriaPatente.Sistema, "CategoriaSistema", new[] { NombrePatente.RealizarBackup, NombrePatente.RestaurarBackup, NombrePatente.RepararBaseDatos, NombrePatente.ConsultarBitacora });

    public static List<CategoriaPatente> ObtenerTodas()
    {
        return new List<CategoriaPatente>
        {
            Usuarios,
            Familias,
            Clientes,
            Catalogo,
            Suscripciones,
            Facturacion,
            Incidentes,
            Sistema
        };
    }

    public static CategoriaPatente ObtenerPorTipo(TipoCategoriaPatente tipo)
    {
        foreach (CategoriaPatente categoria in ObtenerTodas())
        {
            if (categoria.Tipo == tipo)
            {
                return categoria;
            }
        }

        return null;
    }

    public static CategoriaPatente ObtenerPorPatente(string nombrePatente)
    {
        foreach (CategoriaPatente categoria in ObtenerTodas())
        {
            foreach (string nombre in categoria.NombresPatente)
            {
                if (nombre == nombrePatente)
                {
                    return categoria;
                }
            }
        }

        return Sistema;
    }
}
```

(Si el parche 2.1 no se aplicó todavía, sacar `NombrePatente.RestaurarBackup` del arreglo de `Sistema`.)

---

## 4. Capa DAL

### 4.1 `Operativ.DAL/Convertidores/BitacoraConvertidor.cs` — contenido completo (archivo nuevo)

`Accion`/`Criticidad` se graban como `VARCHAR` con el nombre del enum (`BitacoraRepositorio.Registrar` ya hace `.ToString()`) — leerlos de vuelta es `Enum.Parse` en sentido inverso. `NombreUsuario` puede venir `DBNull` (filas sin usuario asociado, o cuando se lee sin el JOIN).

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.DAL.Convertidores;
public static class BitacoraConvertidor
{
    public static Bitacora ToBitacora(this DataRow fila)
    {
        Bitacora bitacora = new Bitacora
        {
            IdBitacora = (int)fila["IdBitacora"],
            IdUsuario = fila["IdUsuario"] == DBNull.Value ? (int?)null : (int)fila["IdUsuario"],
            FechaHora = (DateTime)fila["FechaHora"],
            Accion = (TipoAccionBitacora)Enum.Parse(typeof(TipoAccionBitacora), fila["Accion"].ToString()),
            Criticidad = (CriticidadBitacora)Enum.Parse(typeof(CriticidadBitacora), fila["Criticidad"].ToString()),
            Descripcion = fila["Descripcion"] == DBNull.Value ? null : fila["Descripcion"].ToString()
        };

        if (fila.Table.Columns.Contains("NombreUsuario") && fila["NombreUsuario"] != DBNull.Value)
        {
            bitacora.NombreUsuario = fila["NombreUsuario"].ToString();
        }

        return bitacora;
    }

    public static List<Bitacora> ToListaBitacoras(this DataTable tabla)
    {
        List<Bitacora> bitacoras = new List<Bitacora>();

        foreach (DataRow fila in tabla.Rows)
        {
            bitacoras.Add(fila.ToBitacora());
        }

        return bitacoras;
    }
}
```

### 4.2 `Operativ.DAL/Contratos/IBitacoraRepositorio.cs` — contenido completo

```csharp
using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.DAL.Contratos;
public interface IBitacoraRepositorio
{
    void Registrar(Bitacora entrada);

    List<Bitacora> Buscar(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta, int numeroPagina, int tamanioPagina);

    int ContarRegistros(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta);
}
```

### 4.3 `Operativ.DAL/Implementaciones/BitacoraRepositorio.cs` — contenido completo

`Buscar`/`ContarRegistros` arman el `WHERE` igual que `UsuarioRepositorio.Listar`: parámetros que solo se agregan a la lista si el filtro correspondiente está activo. `FechaHasta` se ajusta sumando un día y comparando con `<`, para que el filtro sea inclusivo del día completo elegido (si no, un usuario que filtra "hasta hoy" no vería nada de hoy, porque una fecha sin hora se interpreta como las 00:00).

```csharp
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;
using Operativ.DAL.Conexion;
using Operativ.DAL.Integridad;

namespace Operativ.DAL.Implementaciones;
public class BitacoraRepositorio : IBitacoraRepositorio, IVerificable
{
    private readonly AccesoDatos accesoDatos;

    public BitacoraRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    public void Registrar(Bitacora entrada)
    {
        string consulta = "INSERT INTO Bitacora (IdUsuario, Accion, Criticidad, Descripcion) "
            + "VALUES (@IdUsuario, @Accion, @Criticidad, @Descripcion); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        object descripcion = entrada.Descripcion ?? (object)DBNull.Value;
        object idUsuario = entrada.IdUsuario.HasValue ? (object)entrada.IdUsuario.Value : DBNull.Value;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@Accion", entrada.Accion.ToString()),
            new SqlParameter("@Criticidad", entrada.Criticidad.ToString()),
            new SqlParameter("@Descripcion", descripcion)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idBitacora = Convert.ToInt32(resultado);
        ActualizarDVH(idBitacora);
    }

    public List<Bitacora> Buscar(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT B.IdBitacora, B.IdUsuario, B.FechaHora, B.Accion, B.Criticidad, B.Descripcion, U.NombreUsuario "
            + "FROM Bitacora B "
            + "LEFT JOIN Usuario U ON U.IdUsuario = B.IdUsuario "
            + "WHERE (@FiltroUsuario = '' OR U.NombreUsuario LIKE '%' + @FiltroUsuario + '%') "
            + (accion.HasValue ? "AND B.Accion = @Accion " : string.Empty)
            + (criticidad.HasValue ? "AND B.Criticidad = @Criticidad " : string.Empty)
            + (fechaDesde.HasValue ? "AND B.FechaHora >= @FechaDesde " : string.Empty)
            + (fechaHasta.HasValue ? "AND B.FechaHora < @FechaHasta " : string.Empty)
            + "ORDER BY B.FechaHora DESC "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@FiltroUsuario", filtroUsuario ?? string.Empty),
            new SqlParameter("@Salteo", salteo),
            new SqlParameter("@TamanioPagina", tamanioPagina)
        };

        if (accion.HasValue)
        {
            parametros.Add(new SqlParameter("@Accion", accion.Value.ToString()));
        }

        if (criticidad.HasValue)
        {
            parametros.Add(new SqlParameter("@Criticidad", criticidad.Value.ToString()));
        }

        if (fechaDesde.HasValue)
        {
            parametros.Add(new SqlParameter("@FechaDesde", fechaDesde.Value.Date));
        }

        if (fechaHasta.HasValue)
        {
            parametros.Add(new SqlParameter("@FechaHasta", fechaHasta.Value.Date.AddDays(1)));
        }

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaBitacoras();
    }

    public int ContarRegistros(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        string consulta = "SELECT COUNT(*) FROM Bitacora B "
            + "LEFT JOIN Usuario U ON U.IdUsuario = B.IdUsuario "
            + "WHERE (@FiltroUsuario = '' OR U.NombreUsuario LIKE '%' + @FiltroUsuario + '%') "
            + (accion.HasValue ? "AND B.Accion = @Accion " : string.Empty)
            + (criticidad.HasValue ? "AND B.Criticidad = @Criticidad " : string.Empty)
            + (fechaDesde.HasValue ? "AND B.FechaHora >= @FechaDesde " : string.Empty)
            + (fechaHasta.HasValue ? "AND B.FechaHora < @FechaHasta " : string.Empty);

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@FiltroUsuario", filtroUsuario ?? string.Empty)
        };

        if (accion.HasValue)
        {
            parametros.Add(new SqlParameter("@Accion", accion.Value.ToString()));
        }

        if (criticidad.HasValue)
        {
            parametros.Add(new SqlParameter("@Criticidad", criticidad.Value.ToString()));
        }

        if (fechaDesde.HasValue)
        {
            parametros.Add(new SqlParameter("@FechaDesde", fechaDesde.Value.Date));
        }

        if (fechaHasta.HasValue)
        {
            parametros.Add(new SqlParameter("@FechaHasta", fechaHasta.Value.Date.AddDays(1)));
        }

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Bitacora", "IdBitacora", id);
    }
}
```

---

## 5. Capa SEC

### 5.1 `Operativ.SEC/Configuracion/ConfiguracionAplicacion.cs` — contenido completo

Se agrega `TamanoPredeterminadoGrillaBitacora`, mismo patrón que el de usuarios (una constante de paginación por pantalla, no una compartida entre las dos grillas — mismo criterio que ya separaba `TamanoPredeterminadoGrillaUsuarios` en vez de una genérica).

```csharp
using System.Configuration;

namespace Operativ.SEC.Configuracion;
public static class ConfiguracionAplicacion
{
    public static int IntentosMaximosLogin
    {
        get { return int.Parse(GetConfiguracion("Operativ.IntentosMaximosLogin", "3")); }
    }

    public static int LongitudContrasenaTemporal
    {
        get { return int.Parse(GetConfiguracion("Operativ.LongitudContrasenaTemporal", "10")); }
    }

    public static int TamanoPredeterminadoGrillaUsuarios
    {
        get { return int.Parse(GetConfiguracion("Operativ.TamanoPredeterminadoGrillaUsuarios", "10")); }
    }

    public static int TamanoPredeterminadoGrillaBitacora
    {
        get { return int.Parse(GetConfiguracion("Operativ.TamanoPredeterminadoGrillaBitacora", "10")); }
    }

    public static string ServidorSmtp
    {
        get { return GetConfiguracion("Operativ.Smtp.Servidor", "localhost"); }
    }

    public static int PuertoSmtp
    {
        get { return int.Parse(GetConfiguracion("Operativ.Smtp.Puerto", "25")); }
    }

    public static string UsuarioSmtp
    {
        get { return GetConfiguracion("Operativ.Smtp.Usuario", string.Empty); }
    }

    public static string ContrasenaSmtp
    {
        get { return GetConfiguracion("Operativ.Smtp.Contrasena", string.Empty); }
    }

    public static bool UsarSslSmtp
    {
        get { return bool.Parse(GetConfiguracion("Operativ.Smtp.UsarSsl", "false")); }
    }

    public static string EmailRemitente
    {
        get { return GetConfiguracion("Operativ.Smtp.EmailRemitente", "no-responder@operativ.com"); }
    }

    public static bool HabilitarEnvioEmail
    {
        get { return bool.Parse(GetConfiguracion("HabilitarEnvioEmail", "false")); }
    }

    public static string RutaXmlEmergencia
    {
        get { return GetConfiguracion("Operativ.Emergencia.RutaXml", "~/App_Data/AccesoEmergencia.xml"); }
    }

    private static string GetConfiguracion(string clave, string valorPorDefecto)
    {
        string valor = ConfigurationManager.AppSettings[clave];

        if (string.IsNullOrEmpty(valor))
        {
            valor = valorPorDefecto;
        }

        return valor;
    }
}
```

(Si el parche 2.1 ya se aplicó, esta clase también va a tener `CarpetaBackups` — agregar `TamanoPredeterminadoGrillaBitacora` junto al resto sin sacar esa propiedad.)

### 5.2 `Operativ.Web/Web.config` — agregar una clave a `<appSettings>`

```xml
<add key="Operativ.TamanoPredeterminadoGrillaBitacora" value="10" />
```

### 5.3 `Operativ.SEC/Contratos/IBitacoraService.cs` — contenido completo

```csharp
using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;

namespace Operativ.SEC.Contratos;
public interface IBitacoraService
{
    void Registrar(int? idUsuario, TipoAccionBitacora accion);

    void Registrar(int? idUsuario, TipoAccionBitacora accion, string detalleAdicional);

    List<Bitacora> Buscar(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta, int numeroPagina, int tamanioPagina);

    int ContarRegistros(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta);
}
```

### 5.4 `Operativ.SEC/Implementaciones/BitacoraService.cs` — contenido completo

`Buscar`/`ContarRegistros` son pasamanos directos al repositorio — no hay ninguna regla de negocio que aplicar a una consulta de solo lectura, mismo criterio que ya usan `ListarUsuarios`/`ContarUsuarios` en `UsuarioService`.

```csharp
using System;
using System.Collections.Generic;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.DAL.Contratos;
using Operativ.DAL.Fabricas;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;

namespace Operativ.SEC.Implementaciones;
public class BitacoraService : IBitacoraService
{
    private const int LongitudMaximaDescripcion = 300;

    private readonly IBitacoraRepositorio bitacoraRepositorio;

    public BitacoraService()
    {
        FabricaRepositorio fabricaRepositorio = new FabricaRepositorio();
        bitacoraRepositorio = fabricaRepositorio.CrearBitacoraRepositorio();
    }

    public void Registrar(int? idUsuario, TipoAccionBitacora accion)
    {
        Registrar(idUsuario, accion, null);
    }

    public void Registrar(int? idUsuario, TipoAccionBitacora accion, string detalleAdicional)
    {
        AccionBitacora definicion = AccionBitacora.ObtenerPorTipo(accion);
        string descripcion = string.Format(definicion.Descripcion, ConfiguracionAplicacion.IntentosMaximosLogin);

        if (!string.IsNullOrEmpty(detalleAdicional))
        {
            descripcion = descripcion + ": " + detalleAdicional;

            if (descripcion.Length > LongitudMaximaDescripcion)
            {
                descripcion = descripcion.Substring(0, LongitudMaximaDescripcion);
            }
        }

        Bitacora entrada = new Bitacora
        {
            IdUsuario = idUsuario,
            Accion = accion,
            Criticidad = definicion.Criticidad,
            Descripcion = descripcion
        };

        bitacoraRepositorio.Registrar(entrada);
    }

    public List<Bitacora> Buscar(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta, int numeroPagina, int tamanioPagina)
    {
        return bitacoraRepositorio.Buscar(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta, numeroPagina, tamanioPagina);
    }

    public int ContarRegistros(string filtroUsuario, TipoAccionBitacora? accion, CriticidadBitacora? criticidad, DateTime? fechaDesde, DateTime? fechaHasta)
    {
        return bitacoraRepositorio.ContarRegistros(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta);
    }
}
```

No hace falta tocar `FabricaSeguridad.cs` ni `FabricaRepositorio.cs`: `CrearBitacoraService`/`CrearBitacoraRepositorio` ya existen desde antes de este parche.

---

## 6. Capa Web

### 6.1 `Operativ.Web/Paginas/Sistema/ConsultarBitacora.aspx` — contenido completo (archivo nuevo)

```html
<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ConsultarBitacora.aspx.cs" Inherits="Operativ.Web.Paginas.ConsultarBitacora" MasterPageFile="~/Master/Principal.Master" %>
<asp:Content ID="ContentConsultarBitacora" ContentPlaceHolderID="ContenidoPrincipal" runat="server">
    <div class="tarjeta">
        <div class="tarjeta-encabezado">
            <div class="tarjeta-encabezado-titulo">
                <span class="icono-circulo">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M9 5H7a2 2 0 0 0-2 2v12a2 2 0 0 0 2 2h10a2 2 0 0 0 2-2V7a2 2 0 0 0-2-2h-2"></path><rect x="9" y="3" width="6" height="4" rx="2" ry="2"></rect><line x1="9" y1="12" x2="15" y2="12"></line><line x1="9" y1="16" x2="15" y2="16"></line></svg>
                </span>
                <div>
                    <h1 runat="server" meta:resourcekey="TituloConsultarBitacora">Consultar bitácora</h1>
                    <p runat="server" meta:resourcekey="DescripcionConsultarBitacora">Registro de actividad del sistema.</p>
                </div>
            </div>
        </div>

        <div class="barra-busqueda">
            <asp:Panel ID="pnlFiltros" runat="server" CssClass="barra-busqueda-filtros">
                <div class="campo-formulario">
                    <label for="<%= txtFechaDesde.ClientID %>"><asp:Literal ID="litEtiquetaFechaDesde" runat="server" Text="<%$ Resources:Textos, EtiquetaFechaDesde %>" /></label>
                    <asp:TextBox ID="txtFechaDesde" runat="server" TextMode="Date" />
                </div>
                <div class="campo-formulario">
                    <label for="<%= txtFechaHasta.ClientID %>"><asp:Literal ID="litEtiquetaFechaHasta" runat="server" Text="<%$ Resources:Textos, EtiquetaFechaHasta %>" /></label>
                    <asp:TextBox ID="txtFechaHasta" runat="server" TextMode="Date" />
                </div>
                <div class="campo-formulario">
                    <label for="<%= txtFiltroUsuario.ClientID %>"><asp:Literal ID="litEtiquetaUsuario" runat="server" Text="<%$ Resources:Textos, EtiquetaUsuarioBitacora %>" /></label>
                    <asp:TextBox ID="txtFiltroUsuario" runat="server" />
                </div>
                <div class="campo-formulario">
                    <label for="<%= ddlFiltroAccion.ClientID %>"><asp:Literal ID="litEtiquetaAccion" runat="server" Text="<%$ Resources:Textos, EtiquetaAccionBitacora %>" /></label>
                    <asp:DropDownList ID="ddlFiltroAccion" runat="server" />
                </div>
                <div class="campo-formulario">
                    <label for="<%= ddlFiltroCriticidad.ClientID %>"><asp:Literal ID="litEtiquetaCriticidad" runat="server" Text="<%$ Resources:Textos, EtiquetaCriticidadBitacora %>" /></label>
                    <asp:DropDownList ID="ddlFiltroCriticidad" runat="server" />
                </div>
                <asp:LinkButton ID="btnBuscar" runat="server" CssClass="btn-primario" CausesValidation="false" OnClick="btnBuscar_Click">
                    <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="11" cy="11" r="8"></circle><line x1="21" y1="21" x2="16.65" y2="16.65"></line></svg>
                    <asp:Literal runat="server" Text="<%$ Resources:Textos, BotonBuscar %>" />
                </asp:LinkButton>
            </asp:Panel>
        </div>

        <div class="tabla-contenedor">
        <asp:GridView ID="gvBitacora" runat="server" AutoGenerateColumns="false" CssClass="tabla-operativ"
            DataKeyNames="IdBitacora" GridLines="None">
            <Columns>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaFechaBitacora %>">
                    <ItemTemplate>
                        <%# ((DateTime)Eval("FechaHora")).ToString("dd/MM/yyyy HH:mm") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaUsuarioBitacora %>">
                    <ItemTemplate>
                        <%# string.IsNullOrEmpty((string)Eval("NombreUsuario"))
                            ? (string)GetGlobalResourceObject("Textos", "EtiquetaSistemaBitacora")
                            : Eval("NombreUsuario") %>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaDescripcionBitacora %>">
                    <ItemTemplate>
                        <span class="descripcion-bitacora" title='<%# Eval("Descripcion") %>'><%# Eval("Descripcion") %></span>
                    </ItemTemplate>
                </asp:TemplateField>
                <asp:TemplateField HeaderText="<%$ Resources:Textos, EtiquetaCriticidadBitacora %>">
                    <ItemTemplate>
                        <span class='badge <%# ObtenerClaseCriticidad((CriticidadBitacora)Eval("Criticidad")) %>'>
                            <%# ObtenerTextoCriticidad((CriticidadBitacora)Eval("Criticidad")) %>
                        </span>
                    </ItemTemplate>
                </asp:TemplateField>
            </Columns>
            <EmptyDataTemplate>
                <asp:Literal runat="server" Text="<%$ Resources:Textos, MensajeSinRegistrosBitacora %>" />
            </EmptyDataTemplate>
        </asp:GridView>
        </div>

        <div class="paginado">
            <asp:Literal ID="litResumenPaginado" runat="server" />
            <div class="paginado-controles">
                <asp:Button ID="btnPaginaAnterior" runat="server" Text="<%$ Resources:Textos, BotonPaginaAnterior %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnPaginaAnterior_Click" />
                <span class="paginado-numero"><asp:Literal ID="litNumeroPagina" runat="server" /></span>
                <asp:Button ID="btnPaginaSiguiente" runat="server" Text="<%$ Resources:Textos, BotonPaginaSiguiente %>" CssClass="btn-outline" CausesValidation="false" OnClick="btnPaginaSiguiente_Click" />
            </div>
        </div>
    </div>
</asp:Content>
```

### 6.2 `Operativ.Web/Paginas/Sistema/ConsultarBitacora.aspx.designer.cs` — contenido completo (archivo nuevo)

```csharp
namespace Operativ.Web.Paginas;
public partial class ConsultarBitacora
{
    protected global::System.Web.UI.WebControls.Panel pnlFiltros;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaFechaDesde;

    protected global::System.Web.UI.WebControls.TextBox txtFechaDesde;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaFechaHasta;

    protected global::System.Web.UI.WebControls.TextBox txtFechaHasta;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaUsuario;

    protected global::System.Web.UI.WebControls.TextBox txtFiltroUsuario;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaAccion;

    protected global::System.Web.UI.WebControls.DropDownList ddlFiltroAccion;

    protected global::System.Web.UI.WebControls.Literal litEtiquetaCriticidad;

    protected global::System.Web.UI.WebControls.DropDownList ddlFiltroCriticidad;

    protected global::System.Web.UI.WebControls.LinkButton btnBuscar;

    protected global::System.Web.UI.WebControls.GridView gvBitacora;

    protected global::System.Web.UI.WebControls.Literal litResumenPaginado;

    protected global::System.Web.UI.WebControls.Button btnPaginaAnterior;

    protected global::System.Web.UI.WebControls.Literal litNumeroPagina;

    protected global::System.Web.UI.WebControls.Button btnPaginaSiguiente;
}
```

### 6.3 `Operativ.Web/Paginas/Sistema/ConsultarBitacora.aspx.cs` — contenido completo (archivo nuevo)

```csharp
using System;
using System.Collections.Generic;
using System.Web.UI.WebControls;
using Operativ.BE.Entidades;
using Operativ.BE.Enums;
using Operativ.BE.Modelos;
using Operativ.SEC.Configuracion;
using Operativ.SEC.Contratos;
using Operativ.SEC.Fabricas;

namespace Operativ.Web.Paginas;
public partial class ConsultarBitacora : PaginaSeguraBase
{
    private readonly int tamanioPagina = ConfiguracionAplicacion.TamanoPredeterminadoGrillaBitacora;
    private readonly IBitacoraService bitacoraService;

    protected override string[] PatentesPermitidas
    {
        get { return new[] { NombrePatente.ConsultarBitacora }; }
    }

    private int NumeroPagina
    {
        get { return ViewState["NumeroPagina"] == null ? 1 : (int)ViewState["NumeroPagina"]; }
        set { ViewState["NumeroPagina"] = value; }
    }

    public ConsultarBitacora()
    {
        FabricaSeguridad fabricaSeguridad = new FabricaSeguridad();
        bitacoraService = fabricaSeguridad.CrearBitacoraService();
    }

    protected void Page_Load(object sender, EventArgs e)
    {
        if (!IsPostBack)
        {
            CargarFiltros();
        }

        CargarGrilla();
    }

    protected void btnBuscar_Click(object sender, EventArgs e)
    {
        NumeroPagina = 1;
        CargarGrilla();
    }

    protected void btnPaginaAnterior_Click(object sender, EventArgs e)
    {
        if (NumeroPagina > 1)
        {
            NumeroPagina--;
        }

        CargarGrilla();
    }

    protected void btnPaginaSiguiente_Click(object sender, EventArgs e)
    {
        NumeroPagina++;
        CargarGrilla();
    }

    protected string ObtenerClaseCriticidad(CriticidadBitacora criticidad)
    {
        switch (criticidad)
        {
            case CriticidadBitacora.Informativo:
                return "badge-informativo";
            case CriticidadBitacora.Advertencia:
                return "badge-advertencia";
            case CriticidadBitacora.Critico:
                return "badge-critico";
            case CriticidadBitacora.Grave:
                return "badge-grave";
            default:
                return "badge-informativo";
        }
    }

    protected string ObtenerTextoCriticidad(CriticidadBitacora criticidad)
    {
        switch (criticidad)
        {
            case CriticidadBitacora.Informativo:
                return (string)GetGlobalResourceObject("Textos", "EtiquetaCriticidadInformativo");
            case CriticidadBitacora.Advertencia:
                return (string)GetGlobalResourceObject("Textos", "EtiquetaCriticidadAdvertencia");
            case CriticidadBitacora.Critico:
                return (string)GetGlobalResourceObject("Textos", "EtiquetaCriticidadCritico");
            case CriticidadBitacora.Grave:
                return (string)GetGlobalResourceObject("Textos", "EtiquetaCriticidadGrave");
            default:
                return criticidad.ToString();
        }
    }

    private void CargarFiltros()
    {
        ddlFiltroAccion.Items.Clear();
        ddlFiltroAccion.Items.Add(new ListItem((string)GetGlobalResourceObject("Textos", "EtiquetaTodasLasAcciones"), string.Empty));

        foreach (AccionBitacora accion in AccionBitacora.ObtenerTodas())
        {
            string texto = accion.Descripcion.Replace("{0}", "N");
            ddlFiltroAccion.Items.Add(new ListItem(texto, accion.Tipo.ToString()));
        }

        ddlFiltroCriticidad.Items.Clear();
        ddlFiltroCriticidad.Items.Add(new ListItem((string)GetGlobalResourceObject("Textos", "EtiquetaTodasLasCriticidades"), string.Empty));
        ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(CriticidadBitacora.Informativo), CriticidadBitacora.Informativo.ToString()));
        ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(CriticidadBitacora.Advertencia), CriticidadBitacora.Advertencia.ToString()));
        ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(CriticidadBitacora.Critico), CriticidadBitacora.Critico.ToString()));
        ddlFiltroCriticidad.Items.Add(new ListItem(ObtenerTextoCriticidad(CriticidadBitacora.Grave), CriticidadBitacora.Grave.ToString()));
    }

    private void CargarGrilla()
    {
        string filtroUsuario = txtFiltroUsuario.Text.Trim();
        TipoAccionBitacora? accion = ObtenerAccionSeleccionada();
        CriticidadBitacora? criticidad = ObtenerCriticidadSeleccionada();
        DateTime? fechaDesde = ObtenerFecha(txtFechaDesde.Text);
        DateTime? fechaHasta = ObtenerFecha(txtFechaHasta.Text);

        List<Bitacora> registros = bitacoraService.Buscar(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta, NumeroPagina, tamanioPagina);
        int total = bitacoraService.ContarRegistros(filtroUsuario, accion, criticidad, fechaDesde, fechaHasta);

        gvBitacora.DataSource = registros;
        gvBitacora.DataBind();

        ActualizarResumenPaginado(total, registros.Count);
    }

    private TipoAccionBitacora? ObtenerAccionSeleccionada()
    {
        if (string.IsNullOrEmpty(ddlFiltroAccion.SelectedValue))
        {
            return null;
        }

        return (TipoAccionBitacora)Enum.Parse(typeof(TipoAccionBitacora), ddlFiltroAccion.SelectedValue);
    }

    private CriticidadBitacora? ObtenerCriticidadSeleccionada()
    {
        if (string.IsNullOrEmpty(ddlFiltroCriticidad.SelectedValue))
        {
            return null;
        }

        return (CriticidadBitacora)Enum.Parse(typeof(CriticidadBitacora), ddlFiltroCriticidad.SelectedValue);
    }

    private DateTime? ObtenerFecha(string texto)
    {
        DateTime fecha;

        if (DateTime.TryParse(texto, out fecha))
        {
            return fecha;
        }

        return null;
    }

    private void ActualizarResumenPaginado(int total, int cantidadEnPagina)
    {
        int desde = total == 0 ? 0 : ((NumeroPagina - 1) * tamanioPagina) + 1;
        int hasta = total == 0 ? 0 : desde + cantidadEnPagina - 1;

        string formato = (string)GetGlobalResourceObject("Textos", "MensajeResumenPaginadoBitacora");
        litResumenPaginado.Text = string.Format(formato, desde, hasta, total);
        litNumeroPagina.Text = NumeroPagina.ToString();

        btnPaginaAnterior.Enabled = NumeroPagina > 1;
        btnPaginaSiguiente.Enabled = (NumeroPagina * tamanioPagina) < total;
    }
}
```

### 6.4 `Operativ.Web/Paginas/Controles/Navbar.ascx` — contenido completo

```html
<%@ Control Language="C#" AutoEventWireup="true" CodeBehind="Navbar.ascx.cs" Inherits="Operativ.Web.Controles.Navbar" %>
<div class="navbar">
    <span class="navbar-marca">Operativ<span class="navbar-marca-acento">.</span></span>
    <asp:HyperLink ID="lnkHome" runat="server" CssClass="navbar-link navbar-link-activo" Text="<%$ Resources:Textos, EnlaceInicio %>" />
    <asp:HyperLink ID="lnkUsuarios" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Usuarios/GestionUsuarios.aspx" Text="<%$ Resources:Textos, EnlaceUsuarios %>" Visible="false" />
    <asp:HyperLink ID="lnkBackup" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Sistema/BackupRestore.aspx" Text="<%$ Resources:Textos, EnlaceBackup %>" Visible="false" />
    <asp:HyperLink ID="lnkBitacora" runat="server" CssClass="navbar-link" NavigateUrl="~/Paginas/Sistema/ConsultarBitacora.aspx" Text="<%$ Resources:Textos, EnlaceBitacora %>" Visible="false" />
</div>
```

(Si el parche 2.1 todavía no se aplicó, sacar la línea de `lnkBackup` — no es parte de este parche.)

### 6.5 `Operativ.Web/Paginas/Controles/Navbar.ascx.designer.cs` — contenido completo

```csharp
namespace Operativ.Web.Controles;
public partial class Navbar
{
    protected global::System.Web.UI.WebControls.HyperLink lnkHome;

    protected global::System.Web.UI.WebControls.HyperLink lnkUsuarios;

    protected global::System.Web.UI.WebControls.HyperLink lnkBackup;

    protected global::System.Web.UI.WebControls.HyperLink lnkBitacora;
}
```

### 6.6 `Operativ.Web/Paginas/Controles/Navbar.ascx.cs` — contenido completo

```csharp
using System;
using System.Web.UI;
using Operativ.BE.Modelos;
using Operativ.SEC.Handlers;
using Operativ.Web.Paginas;

namespace Operativ.Web.Controles;
public partial class Navbar : UserControl
{
    protected void Page_Load(object sender, EventArgs e)
    {
        AutorizacionHandler autorizacionHandler = new AutorizacionHandler();
        string nombrePerfil = autorizacionHandler.GetNombrePerfil();

        lnkHome.Visible = !string.IsNullOrEmpty(nombrePerfil);

        if (lnkHome.Visible)
        {
            lnkHome.NavigateUrl = ResolveUrl(NavegacionHelper.ObtenerUrlHome(nombrePerfil));
        }

        lnkUsuarios.Visible = autorizacionHandler.TieneAlgunaPatente(CategoriaPatente.Usuarios.NombresPatente);
        lnkBackup.Visible = autorizacionHandler.TieneAlgunaPatente(new[] { NombrePatente.RealizarBackup, NombrePatente.RestaurarBackup });
        lnkBitacora.Visible = autorizacionHandler.TienePatente(NombrePatente.ConsultarBitacora);
    }
}
```

(Si el parche 2.1 no se aplicó, sacar la línea de `lnkBackup.Visible` del código-behind también.)

---

## 7. Recursos (`Textos.resx` / `Textos.en.resx`)

Agregar en `Textos.resx`:

```xml
<data name="TituloConsultarBitacora" xml:space="preserve">
  <value>Consultar bitácora</value>
</data>
<data name="DescripcionConsultarBitacora" xml:space="preserve">
  <value>Registro de actividad del sistema.</value>
</data>
<data name="EtiquetaFechaDesde" xml:space="preserve">
  <value>Desde</value>
</data>
<data name="EtiquetaFechaHasta" xml:space="preserve">
  <value>Hasta</value>
</data>
<data name="EtiquetaUsuarioBitacora" xml:space="preserve">
  <value>Usuario</value>
</data>
<data name="EtiquetaAccionBitacora" xml:space="preserve">
  <value>Acción</value>
</data>
<data name="EtiquetaCriticidadBitacora" xml:space="preserve">
  <value>Criticidad</value>
</data>
<data name="EtiquetaFechaBitacora" xml:space="preserve">
  <value>Fecha</value>
</data>
<data name="EtiquetaDescripcionBitacora" xml:space="preserve">
  <value>Descripción</value>
</data>
<data name="EtiquetaSistemaBitacora" xml:space="preserve">
  <value>Sistema</value>
</data>
<data name="EtiquetaTodasLasAcciones" xml:space="preserve">
  <value>Todas las acciones</value>
</data>
<data name="EtiquetaTodasLasCriticidades" xml:space="preserve">
  <value>Todas las criticidades</value>
</data>
<data name="EtiquetaCriticidadInformativo" xml:space="preserve">
  <value>Informativo</value>
</data>
<data name="EtiquetaCriticidadAdvertencia" xml:space="preserve">
  <value>Advertencia</value>
</data>
<data name="EtiquetaCriticidadCritico" xml:space="preserve">
  <value>Crítico</value>
</data>
<data name="EtiquetaCriticidadGrave" xml:space="preserve">
  <value>Grave</value>
</data>
<data name="MensajeSinRegistrosBitacora" xml:space="preserve">
  <value>No se encontraron registros con los filtros seleccionados.</value>
</data>
<data name="MensajeResumenPaginadoBitacora" xml:space="preserve">
  <value>Mostrando {0} a {1} de {2} registros</value>
</data>
<data name="EnlaceBitacora" xml:space="preserve">
  <value>Bitácora</value>
</data>
```

Agregar en `Textos.en.resx`:

```xml
<data name="TituloConsultarBitacora" xml:space="preserve">
  <value>Audit log</value>
</data>
<data name="DescripcionConsultarBitacora" xml:space="preserve">
  <value>System activity log.</value>
</data>
<data name="EtiquetaFechaDesde" xml:space="preserve">
  <value>From</value>
</data>
<data name="EtiquetaFechaHasta" xml:space="preserve">
  <value>To</value>
</data>
<data name="EtiquetaUsuarioBitacora" xml:space="preserve">
  <value>User</value>
</data>
<data name="EtiquetaAccionBitacora" xml:space="preserve">
  <value>Action</value>
</data>
<data name="EtiquetaCriticidadBitacora" xml:space="preserve">
  <value>Severity</value>
</data>
<data name="EtiquetaFechaBitacora" xml:space="preserve">
  <value>Date</value>
</data>
<data name="EtiquetaDescripcionBitacora" xml:space="preserve">
  <value>Description</value>
</data>
<data name="EtiquetaSistemaBitacora" xml:space="preserve">
  <value>System</value>
</data>
<data name="EtiquetaTodasLasAcciones" xml:space="preserve">
  <value>All actions</value>
</data>
<data name="EtiquetaTodasLasCriticidades" xml:space="preserve">
  <value>All severities</value>
</data>
<data name="EtiquetaCriticidadInformativo" xml:space="preserve">
  <value>Informational</value>
</data>
<data name="EtiquetaCriticidadAdvertencia" xml:space="preserve">
  <value>Warning</value>
</data>
<data name="EtiquetaCriticidadCritico" xml:space="preserve">
  <value>Critical</value>
</data>
<data name="EtiquetaCriticidadGrave" xml:space="preserve">
  <value>Severe</value>
</data>
<data name="MensajeSinRegistrosBitacora" xml:space="preserve">
  <value>No records were found with the selected filters.</value>
</data>
<data name="MensajeResumenPaginadoBitacora" xml:space="preserve">
  <value>Showing {0} to {1} of {2} records</value>
</data>
<data name="EnlaceBitacora" xml:space="preserve">
  <value>Log</value>
</data>
```

---

## 8. Estilos (`Operativ.Web/Estilos/operativ.css`)

Agregar `input[type=date]` al selector compartido de inputs (hoy solo cubre `text`/`password`/`select`):

```css
.campo-formulario input[type=text],
.campo-formulario input[type=date],
.campo-formulario input[type=password],
.campo-formulario select {
    width: 100%;
    padding: 10px;
    border: 1px solid #d1d5db;
    border-radius: 4px;
    font-size: 14px;
    font-family: inherit;
    background-color: #ffffff;
    color: var(--color-texto);
}
```

Agregar los 4 badges de criticidad, cerca de `.badge-activo`/`.badge-bloqueado`:

```css
.badge-informativo {
    background-color: #e3f2fd;
    color: #1565c0;
}

.badge-advertencia {
    background-color: #fdf0d5;
    color: var(--color-advertencia);
}

.badge-critico {
    background-color: #fbe2e2;
    color: var(--color-error);
}

.badge-grave {
    background-color: var(--color-error);
    color: #ffffff;
}
```

Agregar el truncado con tooltip para la columna Descripción:

```css
.descripcion-bitacora {
    display: block;
    max-width: 420px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}
```

---

## 9. Pasos para Claude Code

1. `git checkout main && git pull`
2. `git checkout -b feature/parche-2.2-consultar-bitacora`
3. Aplicar los archivos de las secciones 3 a 6 en orden (BE → DAL → SEC → Web), prestando atención a las notas entre paréntesis si el parche 2.1 (backup/restore) todavía no está aplicado sobre esta rama.
4. Aplicar el cambio de seed (sección 2) y recrear la base local.
5. Aplicar los recursos (sección 7) y el CSS (sección 8).
6. Commits sugeridos:
   - `feat(db): agrega la patente ConsultarBitacora y la asigna a WebMaster`
   - `feat(dal): agrega busqueda paginada de bitacora con filtros`
   - `feat(web): agrega la pagina de consultar bitacora`
7. Build completo: `MSBuild.exe Operativ.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU"`.
8. Pruebas manuales (con `webmaster`):
   - Loguearse como `webmaster` → aparece el enlace "Bitácora" en el navbar → entrar, la grilla muestra actividad reciente (logins, etc.) sin ningún filtro aplicado, ordenada de más reciente a más vieja.
   - Loguearse como `admin` (sin `ConsultarBitacora`) → intentar entrar directo por URL a `ConsultarBitacora.aspx` → redirige a `NoAutorizado.aspx`; no aparece el enlace en su navbar.
   - Filtrar por usuario `admin` → solo aparecen filas con `IdUsuario` de `admin` (login, cambios que hizo, etc.), sin las filas de `Sistema` (reparación, backups).
   - Filtrar por acción "Usuario bloqueado tras N intentos fallidos" (la que tiene el placeholder) → en el combo aparece con la "N" en vez de "{0}"; al aplicarlo, la grilla filtra bien por esa acción puntual y cada fila muestra su descripción completa con el número real de intentos.
   - Filtrar por criticidad "Crítico" → solo aparecen filas de esa criticidad, con el badge en color rojo claro; probar "Grave" también si hay alguna fila con esa criticidad (hoy ninguna acción del catálogo usa `Grave`, así que puede no haber resultados — es esperable).
   - Filtrar por rango de fechas que incluya "hoy" en el campo "Hasta" → deben aparecer los registros de hoy (confirma que el ajuste de fin de día funciona).
   - Combinar varios filtros a la vez (usuario + criticidad + rango de fechas) → la combinación de condiciones se aplica correctamente (AND entre todas).
   - Con más registros que el tamaño de página configurado, probar "Siguiente"/"Anterior" y confirmar que el resumen ("Mostrando X a Y de Z registros") es correcto en cada página.
   - Pasar el mouse sobre una descripción larga truncada → aparece el tooltip nativo con el texto completo.
9. No mergear a `main`. Dejar la rama lista con los commits para que el dueño del repo la revise y haga el push.
