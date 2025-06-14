using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ProyectoMigracionMovistarApi.Entities;
using ProyectoMigracionMovistarApi.Models;
using ProyectoMigracionMovistarApi.Utils;
using System.Data;
using System.Text;

namespace ProyectoMigracionMovistarApi.Bussines
{
    public class UsuarioBL
    {
        private readonly IDbContextFactory<MigracionDbContext> _dbContextFactory;
        public UsuarioBL(IDbContextFactory<MigracionDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        internal RespuestaTransaccion AutenticarUsuario(DatosAutenticacion body)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            if (string.IsNullOrWhiteSpace(body.Usuario) || string.IsNullOrWhiteSpace(body.Clave))
                throw new ReglasExcepcion("PMAATU001", "Usuario o clave no pueden estar vacíos.");

            var usuario = dbContext.Usuarios
                .FirstOrDefault(u => u.NumeroIdentificacion == body.Usuario);

            if (usuario == null)
                throw new ReglasExcepcion("PMAATU002", "Usuario o contraseña incorrectos.");

            var hasher = new PasswordHasher<Usuario>();
            var result = hasher.VerifyHashedPassword(usuario, usuario.Clave, body.Clave);
            if (result == PasswordVerificationResult.Failed)
                throw new ReglasExcepcion("PMAATU002", "Usuario o contraseña incorrectos.");

            return new RespuestaTransaccion()
            {
                NumeroRegistro = usuario.IdUsuario,
                Notas = "Autenticación exitosa."
            };
        }

        internal RespuestaTransaccion RegistrarUsuario(DatosUsuarioRegistrar body)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            if (string.IsNullOrWhiteSpace(body.TipoIdentificacion) ||
                string.IsNullOrWhiteSpace(body.Identificacion) ||
                string.IsNullOrWhiteSpace(body.Nombre) ||
                string.IsNullOrWhiteSpace(body.Correo) ||
                string.IsNullOrWhiteSpace(body.Celular) ||
                string.IsNullOrWhiteSpace(body.Clave))
            {
                throw new ReglasExcepcion("PMARUR001", "Todos los campos son obligatorios.");
            }

            var existe = dbContext.Usuarios.Any(u => u.NumeroIdentificacion == body.Identificacion);
            if (existe)
                throw new ReglasExcepcion("PMARUR002", "Ya existe un usuario con la misma identificación.");

            var nuevoUsuario = new Usuario
            {
                TipoIdentificacion = body.TipoIdentificacion,
                NumeroIdentificacion = body.Identificacion,
                Nombre = body.Nombre,
                Correo = body.Correo,
                Celular = body.Celular,
                Cuenta = new List<Cuenta>(),
                Rol = "cliente"
            };

            var hasher = new PasswordHasher<Usuario>();
            nuevoUsuario.Clave = hasher.HashPassword(nuevoUsuario, body.Clave);

            dbContext.Usuarios.Add(nuevoUsuario);
            dbContext.SaveChanges();

            return new RespuestaTransaccion
            {
                NumeroRegistro = nuevoUsuario.IdUsuario,
                Notas = "Usuario registrado exitosamente."
            };
        }
    }
}
