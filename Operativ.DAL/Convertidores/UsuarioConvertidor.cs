using System;
using System.Collections.Generic;
using System.Data;
using Operativ.BE.Entidades;

namespace Operativ.DAL.Convertidores
{
    public static class UsuarioConvertidor
    {
        public static Usuario ToUsuario(this DataRow fila)
        {
            Usuario usuario = new Usuario
            {
                IdUsuario = (int)fila["IdUsuario"],
                NombreUsuario = fila["NombreUsuario"].ToString(),
                Contrasena = fila["Contrasena"].ToString(),
                Salt = fila["Salt"].ToString(),
                Email = fila["Email"].ToString(),
                NombreCompleto = fila["NombreCompleto"].ToString(),
                Bloqueado = (bool)fila["Bloqueado"],
                IntentosFallidos = (int)fila["IntentosFallidos"],
                Activo = (bool)fila["Activo"]
            };
            return usuario;
        }

        public static Usuario ToUsuarioConFamilia(this DataRow fila)
        {
            Usuario usuario = fila.ToUsuario();

            if (fila["IdFamilia"] != DBNull.Value)
            {
                Familia familia = new Familia
                {
                    IdFamilia = (int)fila["IdFamilia"],
                    Nombre = fila["NombreFamilia"].ToString()
                };

                usuario.Familias.Add(familia);
            }

            return usuario;
        }

        public static List<Usuario> ToListaUsuariosConFamilia(this DataTable tabla)
        {
            List<Usuario> usuarios = new List<Usuario>();

            foreach (DataRow fila in tabla.Rows)
            {
                usuarios.Add(fila.ToUsuarioConFamilia());
            }

            return usuarios;
        }
    }
}
