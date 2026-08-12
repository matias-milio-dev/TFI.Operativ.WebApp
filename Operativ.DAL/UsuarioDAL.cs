using System;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using Operativ.BE;
using Operativ.Comun;

namespace Operativ.DAL
{
    public interface IUsuarioDAL
    {
        Usuario ObtenerPorNombreUsuario(string nombreUsuario);
        Usuario ObtenerPorId(int idUsuario);
        DataTable Listar(string filtro, int numeroPagina, int tamanioPagina);
        int Insertar(Usuario usuario);
        void Modificar(Usuario usuario);
        void Baja(int idUsuario);
        void Desbloquear(int idUsuario);
        void CambiarClave(int idUsuario, byte[] claveHash, byte[] claveSalt, bool claveTemporal);
        ResultadoIntentoLogin RegistrarIntentoFallido(int idUsuario, int maximoIntentos);
        void RegistrarLoginExitoso(int idUsuario);
        List<Patente> ObtenerPatentesDirectasDeUsuario(int idUsuario);
        List<Familia> ObtenerFamilias(int idUsuario);
    }

    public class UsuarioDAL : IUsuarioDAL
    {
        public Usuario ObtenerPorNombreUsuario(string nombreUsuario)
        {
            return DALHelper.EjecutarLector(@"
                SELECT u.IdUsuario, u.NombreUsuario, u.NombreCompleto, u.CorreoElectronico, u.ClaveHash, u.ClaveSalt,
                       u.IdPerfil, p.Codigo AS CodigoPerfil, u.CantidadIntentosFallidos, u.Bloqueado, u.ClaveTemporal,
                       u.Activo, u.IdiomaPreferido, u.FechaCreacion, u.FechaUltimoLogin
                FROM dbo.Usuario u
                INNER JOIN dbo.Perfil p ON p.IdPerfil = u.IdPerfil
                WHERE u.NombreUsuario = @NombreUsuario",
                comando => comando.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = nombreUsuario,
                MapearUsuarioCompleto);
        }

        public Usuario ObtenerPorId(int idUsuario)
        {
            return DALHelper.EjecutarLector(@"
                SELECT u.IdUsuario, u.NombreUsuario, u.NombreCompleto, u.CorreoElectronico, u.IdPerfil,
                       p.Codigo AS CodigoPerfil, u.Bloqueado, u.ClaveTemporal, u.Activo, u.IdiomaPreferido,
                       u.FechaCreacion, u.FechaUltimoLogin
                FROM dbo.Usuario u
                INNER JOIN dbo.Perfil p ON p.IdPerfil = u.IdPerfil
                WHERE u.IdUsuario = @IdUsuario",
                comando => comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario,
                lector => new Usuario
                {
                    IdUsuario = (int)lector["IdUsuario"],
                    NombreUsuario = (string)lector["NombreUsuario"],
                    NombreCompleto = (string)lector["NombreCompleto"],
                    CorreoElectronico = (string)lector["CorreoElectronico"],
                    IdPerfil = (int)lector["IdPerfil"],
                    CodigoPerfil = (string)lector["CodigoPerfil"],
                    Bloqueado = (bool)lector["Bloqueado"],
                    ClaveTemporal = (bool)lector["ClaveTemporal"],
                    Activo = (bool)lector["Activo"],
                    IdiomaPreferido = (string)lector["IdiomaPreferido"],
                    FechaCreacion = (DateTime)lector["FechaCreacion"],
                    FechaUltimoLogin = lector["FechaUltimoLogin"] as DateTime?
                });
        }

        public DataTable Listar(string filtro, int numeroPagina, int tamanioPagina)
        {
            return DALHelper.EjecutarConsulta(@"
                SELECT u.IdUsuario, u.NombreUsuario, u.NombreCompleto, u.CorreoElectronico,
                       p.Nombre AS NombrePerfil, u.Bloqueado, u.Activo, u.FechaCreacion,
                       COUNT(*) OVER() AS TotalRegistros
                FROM dbo.Usuario u
                INNER JOIN dbo.Perfil p ON p.IdPerfil = u.IdPerfil
                WHERE (@Filtro IS NULL OR u.NombreUsuario LIKE '%' + @Filtro + '%' OR u.NombreCompleto LIKE '%' + @Filtro + '%')
                ORDER BY u.NombreCompleto
                OFFSET (@NumeroPagina - 1) * @TamanioPagina ROWS FETCH NEXT @TamanioPagina ROWS ONLY", comando =>
            {
                comando.Parameters.Add("@Filtro", SqlDbType.VarChar, 100).Value = DALHelper.ValorODbNull(filtro);
                comando.Parameters.Add("@NumeroPagina", SqlDbType.Int).Value = numeroPagina;
                comando.Parameters.Add("@TamanioPagina", SqlDbType.Int).Value = tamanioPagina;
            });
        }

        public int Insertar(Usuario usuario)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comandoDuplicado = new SqlCommand(
                    "SELECT COUNT(*) FROM dbo.Usuario WHERE NombreUsuario = @NombreUsuario OR CorreoElectronico = @CorreoElectronico", conexion, transaccion))
                {
                    comandoDuplicado.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = usuario.NombreUsuario;
                    comandoDuplicado.Parameters.Add("@CorreoElectronico", SqlDbType.VarChar, 150).Value = usuario.CorreoElectronico;
                    if ((int)comandoDuplicado.ExecuteScalar() > 0)
                    {
                        throw new ExcepcionNegocio(CodigosError.ErrorUsuarioOCorreoYaRegistrado);
                    }
                }

                int idUsuarioNuevo;
                using (var comando = new SqlCommand(@"
                    INSERT INTO dbo.Usuario (NombreUsuario, NombreCompleto, CorreoElectronico, ClaveHash, ClaveSalt, IdPerfil, IdiomaPreferido, ClaveTemporal)
                    VALUES (@NombreUsuario, @NombreCompleto, @CorreoElectronico, @ClaveHash, @ClaveSalt, @IdPerfil, @IdiomaPreferido, @ClaveTemporal);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);", conexion, transaccion))
                {
                    comando.Parameters.Add("@NombreUsuario", SqlDbType.VarChar, 50).Value = usuario.NombreUsuario;
                    comando.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 150).Value = usuario.NombreCompleto;
                    comando.Parameters.Add("@CorreoElectronico", SqlDbType.VarChar, 150).Value = usuario.CorreoElectronico;
                    comando.Parameters.Add("@ClaveHash", SqlDbType.VarBinary, 64).Value = usuario.ClaveHash;
                    comando.Parameters.Add("@ClaveSalt", SqlDbType.VarBinary, 32).Value = usuario.ClaveSalt;
                    comando.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = usuario.IdPerfil;
                    comando.Parameters.Add("@IdiomaPreferido", SqlDbType.VarChar, 5).Value = usuario.IdiomaPreferido;
                    comando.Parameters.Add("@ClaveTemporal", SqlDbType.Bit).Value = usuario.ClaveTemporal;
                    idUsuarioNuevo = (int)comando.ExecuteScalar();
                }

                string valores = string.Join("|", idUsuarioNuevo.ToString(), usuario.NombreUsuario, usuario.NombreCompleto,
                    usuario.CorreoElectronico, usuario.IdPerfil.ToString(), IntegridadHelper.FormatoBit(true));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Usuario", "IdUsuario", idUsuarioNuevo, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Usuario", "IdUsuario");

                return idUsuarioNuevo;
            });
        }

        public void Modificar(Usuario usuario)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comando = new SqlCommand(@"
                    UPDATE dbo.Usuario
                    SET NombreCompleto = @NombreCompleto, CorreoElectronico = @CorreoElectronico,
                        IdPerfil = @IdPerfil, IdiomaPreferido = @IdiomaPreferido
                    WHERE IdUsuario = @IdUsuario", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = usuario.IdUsuario;
                    comando.Parameters.Add("@NombreCompleto", SqlDbType.NVarChar, 150).Value = usuario.NombreCompleto;
                    comando.Parameters.Add("@CorreoElectronico", SqlDbType.VarChar, 150).Value = usuario.CorreoElectronico;
                    comando.Parameters.Add("@IdPerfil", SqlDbType.Int).Value = usuario.IdPerfil;
                    comando.Parameters.Add("@IdiomaPreferido", SqlDbType.VarChar, 5).Value = usuario.IdiomaPreferido;
                    comando.ExecuteNonQuery();
                }

                bool activo = ObtenerActivo(conexion, transaccion, "Usuario", "IdUsuario", usuario.IdUsuario);
                string valores = string.Join("|", usuario.IdUsuario.ToString(), ObtenerNombreUsuario(conexion, transaccion, usuario.IdUsuario),
                    usuario.NombreCompleto, usuario.CorreoElectronico, usuario.IdPerfil.ToString(), IntegridadHelper.FormatoBit(activo));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Usuario", "IdUsuario", usuario.IdUsuario, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Usuario", "IdUsuario");
            });
        }

        public void Baja(int idUsuario)
        {
            DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comando = new SqlCommand("UPDATE dbo.Usuario SET Activo = 0 WHERE IdUsuario = @IdUsuario", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                    comando.ExecuteNonQuery();
                }

                string valores = string.Join("|", idUsuario.ToString(), ObtenerNombreUsuario(conexion, transaccion, idUsuario),
                    ObtenerCampo(conexion, transaccion, "NombreCompleto", "Usuario", "IdUsuario", idUsuario),
                    ObtenerCampo(conexion, transaccion, "CorreoElectronico", "Usuario", "IdUsuario", idUsuario),
                    ObtenerCampoInt(conexion, transaccion, "IdPerfil", "Usuario", "IdUsuario", idUsuario).ToString(),
                    IntegridadHelper.FormatoBit(false));
                DALHelper.ActualizarDigitoVerificadorFila(conexion, transaccion, "Usuario", "IdUsuario", idUsuario, valores);
                DALHelper.RecalcularDVV(conexion, transaccion, "Usuario", "IdUsuario");
            });
        }

        public void Desbloquear(int idUsuario)
        {
            DALHelper.EjecutarNonQuery("UPDATE dbo.Usuario SET Bloqueado = 0, CantidadIntentosFallidos = 0 WHERE IdUsuario = @IdUsuario", comando =>
            {
                comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
            });
        }

        public void CambiarClave(int idUsuario, byte[] claveHash, byte[] claveSalt, bool claveTemporal)
        {
            DALHelper.EjecutarNonQuery(@"
                UPDATE dbo.Usuario
                SET ClaveHash = @ClaveHash, ClaveSalt = @ClaveSalt, ClaveTemporal = @ClaveTemporal,
                    CantidadIntentosFallidos = 0, Bloqueado = 0
                WHERE IdUsuario = @IdUsuario", comando =>
            {
                comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                comando.Parameters.Add("@ClaveHash", SqlDbType.VarBinary, 64).Value = claveHash;
                comando.Parameters.Add("@ClaveSalt", SqlDbType.VarBinary, 32).Value = claveSalt;
                comando.Parameters.Add("@ClaveTemporal", SqlDbType.Bit).Value = claveTemporal;
            });
        }

        public ResultadoIntentoLogin RegistrarIntentoFallido(int idUsuario, int maximoIntentos)
        {
            return DALHelper.EjecutarEnTransaccion((conexion, transaccion) =>
            {
                using (var comando = new SqlCommand(@"
                    UPDATE dbo.Usuario
                    SET CantidadIntentosFallidos = CantidadIntentosFallidos + 1,
                        Bloqueado = CASE WHEN CantidadIntentosFallidos + 1 >= @MaximoIntentos THEN 1 ELSE Bloqueado END
                    WHERE IdUsuario = @IdUsuario;
                    SELECT CantidadIntentosFallidos, Bloqueado FROM dbo.Usuario WHERE IdUsuario = @IdUsuario;", conexion, transaccion))
                {
                    comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
                    comando.Parameters.Add("@MaximoIntentos", SqlDbType.TinyInt).Value = (byte)maximoIntentos;
                    using (var lector = comando.ExecuteReader(CommandBehavior.SingleRow))
                    {
                        lector.Read();
                        return new ResultadoIntentoLogin
                        {
                            CantidadIntentosFallidos = (byte)lector["CantidadIntentosFallidos"],
                            Bloqueado = (bool)lector["Bloqueado"]
                        };
                    }
                }
            });
        }

        public void RegistrarLoginExitoso(int idUsuario)
        {
            DALHelper.EjecutarNonQuery("UPDATE dbo.Usuario SET CantidadIntentosFallidos = 0, FechaUltimoLogin = SYSDATETIME() WHERE IdUsuario = @IdUsuario", comando =>
            {
                comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario;
            });
        }

        public List<Patente> ObtenerPatentesDirectasDeUsuario(int idUsuario)
        {
            var patentes = new List<Patente>();
            var tabla = DALHelper.EjecutarConsulta(@"
                SELECT pa.IdPatente, pa.Codigo, pa.Nombre, pa.Modulo
                FROM dbo.UsuarioPatente up
                INNER JOIN dbo.Patente pa ON pa.IdPatente = up.IdPatente
                WHERE up.IdUsuario = @IdUsuario AND pa.Activo = 1",
                comando => comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario);

            foreach (DataRow fila in tabla.Rows)
            {
                patentes.Add(new Patente
                {
                    IdPatente = (int)fila["IdPatente"],
                    Codigo = (string)fila["Codigo"],
                    Nombre = (string)fila["Nombre"],
                    Modulo = (string)fila["Modulo"]
                });
            }
            return patentes;
        }

        public List<Familia> ObtenerFamilias(int idUsuario)
        {
            var familias = new List<Familia>();
            var tabla = DALHelper.EjecutarConsulta(@"
                SELECT f.IdFamilia, f.Nombre, f.Descripcion
                FROM dbo.UsuarioFamilia uf
                INNER JOIN dbo.Familia f ON f.IdFamilia = uf.IdFamilia
                WHERE uf.IdUsuario = @IdUsuario AND f.Activo = 1",
                comando => comando.Parameters.Add("@IdUsuario", SqlDbType.Int).Value = idUsuario);

            foreach (DataRow fila in tabla.Rows)
            {
                familias.Add(new Familia
                {
                    IdFamilia = (int)fila["IdFamilia"],
                    Nombre = (string)fila["Nombre"],
                    Descripcion = fila["Descripcion"] as string
                });
            }
            return familias;
        }

        private static Usuario MapearUsuarioCompleto(SqlDataReader lector)
        {
            return new Usuario
            {
                IdUsuario = (int)lector["IdUsuario"],
                NombreUsuario = (string)lector["NombreUsuario"],
                NombreCompleto = (string)lector["NombreCompleto"],
                CorreoElectronico = (string)lector["CorreoElectronico"],
                ClaveHash = (byte[])lector["ClaveHash"],
                ClaveSalt = (byte[])lector["ClaveSalt"],
                IdPerfil = (int)lector["IdPerfil"],
                CodigoPerfil = (string)lector["CodigoPerfil"],
                CantidadIntentosFallidos = (byte)lector["CantidadIntentosFallidos"],
                Bloqueado = (bool)lector["Bloqueado"],
                ClaveTemporal = (bool)lector["ClaveTemporal"],
                Activo = (bool)lector["Activo"],
                IdiomaPreferido = (string)lector["IdiomaPreferido"],
                FechaCreacion = (DateTime)lector["FechaCreacion"],
                FechaUltimoLogin = lector["FechaUltimoLogin"] as DateTime?
            };
        }

        private static string ObtenerNombreUsuario(SqlConnection conexion, SqlTransaction transaccion, int idUsuario)
            => ObtenerCampo(conexion, transaccion, "NombreUsuario", "Usuario", "IdUsuario", idUsuario);

        private static bool ObtenerActivo(SqlConnection conexion, SqlTransaction transaccion, string tabla, string columnaId, int id)
        {
            using (var comando = new SqlCommand($"SELECT Activo FROM dbo.{tabla} WHERE {columnaId} = @Id", conexion, transaccion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                return (bool)comando.ExecuteScalar();
            }
        }

        private static string ObtenerCampo(SqlConnection conexion, SqlTransaction transaccion, string campo, string tabla, string columnaId, int id)
        {
            using (var comando = new SqlCommand($"SELECT {campo} FROM dbo.{tabla} WHERE {columnaId} = @Id", conexion, transaccion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                return comando.ExecuteScalar() as string;
            }
        }

        private static int ObtenerCampoInt(SqlConnection conexion, SqlTransaction transaccion, string campo, string tabla, string columnaId, int id)
        {
            using (var comando = new SqlCommand($"SELECT {campo} FROM dbo.{tabla} WHERE {columnaId} = @Id", conexion, transaccion))
            {
                comando.Parameters.Add("@Id", SqlDbType.Int).Value = id;
                return (int)comando.ExecuteScalar();
            }
        }
    }
}
