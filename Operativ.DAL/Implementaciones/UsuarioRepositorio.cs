using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Operativ.BE.Entidades;
using Operativ.DAL.Contratos;
using Operativ.DAL.Convertidores;
using Operativ.DAL.Conexion;
using Operativ.DAL.Integridad;

namespace Operativ.DAL.Implementaciones;

//Clase que maneja todos los accesos a base de datos relacionados a la entidad usuario
//Implementa una interfaz IUsuarioRepositorio y tambien IVerificable que significa que utiliza DVH
public class UsuarioRepositorio : IUsuarioRepositorio, IVerificable
{
    private readonly AccesoDatos accesoDatos;

    public UsuarioRepositorio()
    {
        accesoDatos = new AccesoDatos();
    }

    //Metodo que ejecuta una consulta SQL usando SqlParameters para evitar la inyeccion SQL. 
    //El cual trae al usuario activo con el nombre que se pasa por parametro
    //Luego se llama al metodo de acceso datos que ejecuta la consulta con un datareader
    //Finalmente el resultado se transforma en un objeto de capa BE usando un metodo de extension ToUsuario
    //Que mapea el restulado del objeto table en su primera fila.
    public Usuario GetPorNombreUsuario(string nombreUsuario)
    {
        string consulta = "SELECT IdUsuario, NombreUsuario, Contrasena, Salt, Email, NombreCompleto, Bloqueado, IntentosFallidos, Activo "
            + "FROM Usuario WHERE NombreUsuario = @NombreUsuario AND Activo = 1";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NombreUsuario", nombreUsuario)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Usuario usuario = null;

        if (tabla.Rows.Count > 0)
        {
            usuario = tabla.Rows[0].ToUsuario();
        }

        return usuario;
    }

    //Metodo que ejecuta una consulta SQL usando SqlParameters para evitar la inyeccion SQL. 
    //El cual trae al usuario activo con el id que se pasa por parametro
    //Luego se llama al metodo de acceso datos que ejecuta la consulta con un datareader
    //Finalmente el resultado se transforma en un objeto de capa BE usando un metodo de extension ToUsuario
    //Que mapea el restulado del objeto table en su primera fila.
    public Usuario GetPorId(int idUsuario)
    {
        string consulta = "SELECT IdUsuario, NombreUsuario, Contrasena, Salt, Email, NombreCompleto, Bloqueado, IntentosFallidos, Activo "
            + "FROM Usuario WHERE IdUsuario = @IdUsuario AND Activo = 1";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        Usuario usuario = null;

        if (tabla.Rows.Count > 0)
        {
            usuario = tabla.Rows[0].ToUsuario();
        }

        return usuario;
    }

    //Metodo que realiza una consulta de tipo UPDATE a la base de datos con el nuevo numero
    //de intentos fallidos o si es bloqueado al id de usuario especificado
    //Actualiza los digitos verificadores dado que cambiaron los campos del registro
    public void ActualizarIntentosFallidos(int idUsuario, int intentosFallidos, bool bloqueado)
    {
        string consulta = "UPDATE Usuario SET IntentosFallidos = @IntentosFallidos, Bloqueado = @Bloqueado WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IntentosFallidos", intentosFallidos),
            new SqlParameter("@Bloqueado", bloqueado),
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    //Metodo que ejecuta una consulta de tipo UPDATE a la entidad usuario para actualizar la contrasena y su salt
    //Actualiza los digitos verificadores dado que cambiaron los campos del registro
    public void ActualizarContrasena(int idUsuario, string contrasena, string salt)
    {
        string consulta = "UPDATE Usuario SET Contrasena = @Contrasena, Salt = @Salt WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Contrasena", contrasena),
            new SqlParameter("@Salt", salt),
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    //Metodo que ejecuta una consulta de tipo UPDATE a la entidad usuario para actualizar los intentos fallidos
    //Actualiza los digitos verificadores dado que cambiaron los campos del registro
    public void ResetearIntentosFallidos(int idUsuario)
    {
        string consulta = "UPDATE Usuario SET IntentosFallidos = 0 WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    //Metodo que ejecuta una consulta de tipo UPDATE a la entidad usuario para actualizar su estado a activo nuevamente
    //Actualiza los digitos verificadores dado que cambiaron los campos del registro
    public void Desbloquear(int idUsuario)
    {
        string consulta = "UPDATE Usuario SET Bloqueado = 0, IntentosFallidos = 0 WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    public int Insertar(Usuario usuario)
    {
        string consulta = "INSERT INTO Usuario (NombreUsuario, Contrasena, Salt, Email, NombreCompleto, Bloqueado, IntentosFallidos, Activo) "
            + "VALUES (@NombreUsuario, @Contrasena, @Salt, @Email, @NombreCompleto, 0, 0, 1); "
            + "SELECT CAST(SCOPE_IDENTITY() AS INT);";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NombreUsuario", usuario.NombreUsuario),
            new SqlParameter("@Contrasena", usuario.Contrasena),
            new SqlParameter("@Salt", usuario.Salt),
            new SqlParameter("@Email", usuario.Email),
            new SqlParameter("@NombreCompleto", usuario.NombreCompleto)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        int idUsuario = Convert.ToInt32(resultado);
        ActualizarDVH(idUsuario);
        return idUsuario;
    }

    public void Modificar(Usuario usuario)
    {
        string consulta = "UPDATE Usuario SET NombreCompleto = @NombreCompleto, Email = @Email WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NombreCompleto", usuario.NombreCompleto),
            new SqlParameter("@Email", usuario.Email),
            new SqlParameter("@IdUsuario", usuario.IdUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(usuario.IdUsuario);
    }

    public void BajaLogica(int idUsuario)
    {
        string consulta = "UPDATE Usuario SET Activo = 0 WHERE IdUsuario = @IdUsuario";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);
        ActualizarDVH(idUsuario);
    }

    public void AsignarFamilia(int idUsuario, int idFamilia)
    {
        string consulta = "INSERT INTO UsuarioFamilia (IdUsuario, IdFamilia) VALUES (@IdUsuario, @IdFamilia)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdFamilia", idFamilia)
        };

        accesoDatos.EjecutarConsulta(consulta, parametros);

        List<SqlParameter> clavesFila = new List<SqlParameter>
        {
            new SqlParameter("@IdUsuario", idUsuario),
            new SqlParameter("@IdFamilia", idFamilia)
        };

        IntegridadHelper.ActualizarIntegridadClaveCompuesta("UsuarioFamilia", clavesFila);
    }

    public void ActualizarDVH(int id)
    {
        IntegridadHelper.ActualizarIntegridad("Usuario", "IdUsuario", id);
    }

    public List<Usuario> Listar(string filtro, int? idFamilia, int numeroPagina, int tamanioPagina)
    {
        string consulta = "SELECT U.IdUsuario, U.NombreUsuario, U.Contrasena, U.Salt, U.Email, U.NombreCompleto, U.Bloqueado, U.IntentosFallidos, U.Activo, "
            + "F.IdFamilia, F.Nombre AS NombreFamilia "
            + "FROM Usuario U "
            + "LEFT JOIN UsuarioFamilia UF ON UF.IdUsuario = U.IdUsuario "
            + "LEFT JOIN Familia F ON F.IdFamilia = UF.IdFamilia "
            + "WHERE U.Activo = 1 "
            + "AND (@Filtro = '' OR U.NombreUsuario LIKE '%' + @Filtro + '%' OR U.Email LIKE '%' + @Filtro + '%') "
            + (idFamilia.HasValue ? "AND UF.IdFamilia = @IdFamilia " : string.Empty)
            + "ORDER BY U.NombreUsuario "
            + "OFFSET @Salteo ROWS FETCH NEXT @TamanioPagina ROWS ONLY";

        int salteo = (numeroPagina - 1) * tamanioPagina;

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty),
            new SqlParameter("@Salteo", salteo),
            new SqlParameter("@TamanioPagina", tamanioPagina)
        };

        if (idFamilia.HasValue)
        {
            parametros.Add(new SqlParameter("@IdFamilia", idFamilia.Value));
        }

        DataTable tabla = accesoDatos.EjecutarReader(consulta, parametros);

        return tabla.ToListaUsuariosConFamilia();
    }

    public int ContarUsuarios(string filtro, int? idFamilia)
    {
        string consulta = "SELECT COUNT(*) FROM Usuario U "
            + (idFamilia.HasValue ? "INNER JOIN UsuarioFamilia UF ON UF.IdUsuario = U.IdUsuario " : string.Empty)
            + "WHERE U.Activo = 1 "
            + "AND (@Filtro = '' OR U.NombreUsuario LIKE '%' + @Filtro + '%' OR U.Email LIKE '%' + @Filtro + '%') "
            + (idFamilia.HasValue ? "AND UF.IdFamilia = @IdFamilia " : string.Empty);

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Filtro", filtro ?? string.Empty)
        };

        if (idFamilia.HasValue)
        {
            parametros.Add(new SqlParameter("@IdFamilia", idFamilia.Value));
        }

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado);
    }

    public bool ExisteNombreUsuario(string nombreUsuario, int? idUsuarioExcluir)
    {
        string consulta = "SELECT COUNT(*) FROM Usuario "
            + "WHERE NombreUsuario = @NombreUsuario "
            + "AND (@IdUsuarioExcluir IS NULL OR IdUsuario <> @IdUsuarioExcluir)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@NombreUsuario", nombreUsuario),
            new SqlParameter("@IdUsuarioExcluir", (object)idUsuarioExcluir ?? DBNull.Value)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado) > 0;
    }

    public bool ExisteEmail(string correoElectronico, int? idUsuarioExcluir)
    {
        string consulta = "SELECT COUNT(*) FROM Usuario "
            + "WHERE Email = @Email "
            + "AND (@IdUsuarioExcluir IS NULL OR IdUsuario <> @IdUsuarioExcluir)";

        List<SqlParameter> parametros = new List<SqlParameter>
        {
            new SqlParameter("@Email", correoElectronico),
            new SqlParameter("@IdUsuarioExcluir", (object)idUsuarioExcluir ?? DBNull.Value)
        };

        object resultado = accesoDatos.EjecutarEscalar(consulta, parametros);
        return Convert.ToInt32(resultado) > 0;
    }
}
