using Microsoft.EntityFrameworkCore;
using ProyectoMigracionMovistarApi.Entities;
using ProyectoMigracionMovistarApi.Models;
using ProyectoMigracionMovistarApi.Utils;

namespace ProyectoMigracionMovistarApi.Bussines
{
    public class ProcesoBL
    {
        private readonly IDbContextFactory<MigracionDbContext> _dbContextFactory;
        public ProcesoBL(IDbContextFactory<MigracionDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        internal List<ProcesoResumen> ObtenerProcesos(string tipo)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var query = dbContext.Procesos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(p => p.Origen == tipo);

            var procesos = query
                .OrderByDescending(p => p.Fecha)
                .Select(p => new ProcesoResumen
                {
                    IdProceso = p.IdProceso,
                    Tipo = p.Origen,
                    Estado = p.EstadoProceso,
                    Total = p.TotalRegistros,
                    Exitosos = p.CantidadExito,
                    Errores = p.CantidadError,
                    Duplicados = p.CantidadDuplicado,
                    Fecha = p.Fecha,
                    Notas = p.Notas
                })
                .ToList();

            return procesos;
        }

        internal RespuestaTransaccion RealizarMigracionManual(string usuario, DatosMigracionManual body)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            if (string.IsNullOrEmpty(body.Identificacion) ||
                string.IsNullOrEmpty(body.NumeroTelefono) ||
                string.IsNullOrEmpty(body.NumeroCuenta) ||
                string.IsNullOrEmpty(body.Correo))
            {
                throw new ReglasExcepcion("PMARMM001", "Datos de migración incompletos");
            }

            var usuarioRegistra = dbContext.Usuarios
                .FirstOrDefault(x => x.NumeroIdentificacion == usuario);

            if (usuarioRegistra == null)
                throw new ReglasExcepcion("PMARMM002", "Usuario no encontrado");

            var personaMigrar = dbContext.Usuarios
                .Include(x => x.Cuenta)
                .FirstOrDefault(x => x.NumeroIdentificacion == body.Identificacion);

            if (personaMigrar == null)
                throw new ReglasExcepcion("PMARMM003", "Persona a migrar no encontrada");
            if (personaMigrar.Rol == "admin")
                throw new ReglasExcepcion("PMARMM004", "No se puede migrar un usuario administrador");
            if (usuarioRegistra.Rol == "cliente")
            {
                if (usuarioRegistra.IdUsuario != personaMigrar.IdUsuario)
                    throw new ReglasExcepcion("PMARMM008", "No tiene permisos para migrar a otro usuario");
            }

            if (!string.Equals(personaMigrar.Correo, body.Correo, StringComparison.OrdinalIgnoreCase))
                throw new ReglasExcepcion("PMARMM005", "El correo no coincide con el registrado");

            if (!string.Equals(personaMigrar.Celular, body.NumeroTelefono, StringComparison.OrdinalIgnoreCase))
                throw new ReglasExcepcion("PMARMM006", "El número de teléfono no coincide con el registrado");

            if (personaMigrar.Cuenta == null || personaMigrar.Cuenta.Count == 0)
                throw new ReglasExcepcion("PMARMM010", "No tiene ningún servicio contratado con nosotros actualmente");

            if (!personaMigrar.Cuenta.Any(x => x.NumeroCuenta == body.NumeroCuenta))
                throw new ReglasExcepcion("PMARMM007", "El número de cuenta no coincide con ninguna cuenta registrada");

            string notas = "", codigoError = "";
            ProcesarUsuarioMigracion(personaMigrar, personaMigrar.Cuenta.Where(x => x.NumeroCuenta == body.NumeroCuenta).ToList(), dbContext, null, out notas, out codigoError);
            if (!string.IsNullOrEmpty(codigoError))
                throw new ReglasExcepcion(codigoError, notas);

            if (usuarioRegistra.Rol == "admin")
            {
                var nuevoAuditoria = new Auditoria
                {
                    TipoEvento = "Migracion",
                    Descripcion = $"Migración manual realizada por el administrador {usuarioRegistra.NumeroIdentificacion} para el usuario {personaMigrar.NumeroIdentificacion}",
                    IdUsuario = usuarioRegistra.IdUsuario
                };

                dbContext.Auditoria.Add(nuevoAuditoria);
                dbContext.SaveChanges();
            }

            return new RespuestaTransaccion()
            {
                NumeroRegistro = personaMigrar.IdUsuario,
                Notas = notas
            };
        }

        internal async Task<RespuestaTransaccion> RealizarMigracionMasiva(string usuario)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var usuarioRegistra = dbContext.Usuarios
                .FirstOrDefault(x => x.NumeroIdentificacion == usuario);

            if (usuarioRegistra == null)
                throw new ReglasExcepcion("PMARMM002", "Usuario no encontrado");
            if (usuarioRegistra.Rol == "cliente")
                throw new ReglasExcepcion("PMARMM009", "No tiene permisos para realizar migración masiva");

            int idProceso = IniciarProceso("Migracion", usuarioRegistra.IdUsuario, dbContext);

            _ = Task.Run(() =>
            {
                try
                {
                    EjecutarMigracionMasivaPorLotes(usuarioRegistra, idProceso);
                }
                catch
                {
                }
            });

            if (usuarioRegistra.Rol == "admin")
            {
                var nuevoAuditoria = new Auditoria
                {
                    TipoEvento = "Migracion",
                    Descripcion = $"Migración automatica realizada por el administrador {usuarioRegistra.NumeroIdentificacion}",
                    IdUsuario = usuarioRegistra.IdUsuario
                };

                dbContext.Auditoria.Add(nuevoAuditoria);
                dbContext.SaveChanges();
            }

            return new RespuestaTransaccion()
            {
                NumeroRegistro = idProceso,
                Notas = $"Proceso de migración masiva iniciado. ID del proceso: {idProceso}"
            };
        }

        private void EjecutarMigracionMasivaPorLotes(Usuario usuarioRegistra, int idProceso)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var proceso = dbContext.Procesos.FirstOrDefault(p => p.IdProceso == idProceso);
            if (proceso == null)
                return;

            try
            {
                const int batchSize = 100;
                int skip = 0;
                bool hayMas = true;

                while (hayMas)
                {
                    var cuentasMigrables = dbContext.Cuenta
                        .Include(c => c.IdUsuarioNavigation)
                        .Where(c => c.Migrada != "S")
                        .OrderBy(c => c.IdCuenta)
                        .Skip(skip)
                        .Take(batchSize)
                        .ToList();

                    hayMas = cuentasMigrables.Count == batchSize;
                    skip += batchSize;

                    if (!cuentasMigrables.Any())
                        break;

                    var usuariosConCuentas = cuentasMigrables
                        .GroupBy(c => c.IdUsuarioNavigation)
                        .ToList();

                    foreach (var grupo in usuariosConCuentas)
                    {
                        using var loteContext = _dbContextFactory.CreateDbContext();
                        var usuario = grupo.Key;
                        var cuentas = grupo.ToList();

                        ProcesarUsuarioMigracion(usuario, cuentas, loteContext, proceso, out string notas, out string codigoError);
                    }
                }

                proceso.EstadoProceso = "FIN";
                proceso.Notas = "Migración masiva finalizada correctamente";
                proceso.Fecha = DateTime.Now;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                var mensajeError = APIUtil.RecuperarExcepcionSistema(ex);

                proceso.EstadoProceso = "ERR";
                proceso.Notas = mensajeError.CodigoError + ": " + mensajeError.MensajeError;
                proceso.Fecha = DateTime.Now;
                dbContext.SaveChanges();
            }
        }


        private int IniciarProceso(string origen, int idUsuario, MigracionDbContext dbContext)
        {
            if (dbContext.Procesos.Any(p => p.EstadoProceso == "PRO"))
                throw new ReglasExcepcion("PMIGR007", "Ya existe un proceso en curso. Por favor, espere a que finalice antes de iniciar uno nuevo");

            int totalRegistros = 0;
            if (origen == "Migracion")
            {
                totalRegistros = dbContext.Cuenta
                    .AsNoTracking()
                    .Count(c => c.Migrada != "S");
            }

            var nuevoProceso = new Proceso
            {
                Origen = origen,
                EstadoProceso = "PRO",
                TotalRegistros = totalRegistros,
                CantidadExito = 0,
                CantidadDuplicado = 0,
                CantidadError = 0,
                Notas = "Proceso iniciado correctamente",
                IdUsuario = idUsuario
            };

            dbContext.Procesos.Add(nuevoProceso);
            dbContext.SaveChanges();

            return nuevoProceso.IdProceso;
        }

        private void ProcesarUsuarioMigracion(Usuario personaMigrar, List<Cuenta> cuentas, MigracionDbContext dbContext, Proceso proceso, out string notas, out string codigoError)
        {
            notas = "";
            codigoError = "";

            if (personaMigrar == null)
            {
                if (proceso != null)
                    proceso.CantidadError += cuentas.Count;

                notas = "El usuario a migrar no puede ser nulo.";
                codigoError = "PMIGR000";
                return;
            }

            if (cuentas == null || !cuentas.Any())
            {
                notas = "El usuario no tiene cuentas asociadas para migrar.";
                codigoError = "PMIGR000";
                return;
            }

            foreach (var cuenta in cuentas)
            {
                string estado = "ERR";
                try
                {
                    var servicio = dbContext.Servicios.FirstOrDefault(s => s.IdServicio == cuenta.IdServicio);
                    if (servicio == null)
                        throw new ReglasExcepcion("PMIGR001", $"No se encontró el servicio para la cuenta {cuenta.NumeroCuenta}.");

                    if (servicio.IdOperador != Constantes.CodigoMovistar)
                    {
                        if (cuenta.Migrada == "S")
                        {
                            estado = "DUP";
                            throw new ReglasExcepcion("PMIGR002", $"La cuenta {cuenta.NumeroCuenta} ya fue migrada anteriormente.");
                        }
                        else
                            throw new ReglasExcepcion("PMIGR003", $"La cuenta {cuenta.NumeroCuenta} no requiere migración; ya pertenece a otro operador.");
                    }

                    int codigoDestino = 0;
                    if (servicio.Tipo == "Movil")
                    {
                        if (string.IsNullOrWhiteSpace(personaMigrar.Celular) || personaMigrar.Celular.Length < 3)
                            throw new ReglasExcepcion("PMIGR006", "El número celular del usuario es inválido o no está definido.");

                        string prefijo = personaMigrar.Celular.Substring(0, 3);
                        if (Constantes.PrefijosParaTigo.Contains(prefijo))
                            codigoDestino = Constantes.CodigoTigo;
                        else
                            throw new ReglasExcepcion("PMIGR004", $"El prefijo {prefijo} no corresponde a migración automática para Movil.");
                    }
                    else if (servicio.Tipo == "Fibra")
                        codigoDestino = Constantes.CodigoClaro;
                    else if (servicio.Tipo == "Banda")
                        codigoDestino = Constantes.CodigoTigo;

                    var servicioOperadorDestino = dbContext.Servicios.FirstOrDefault(s => s.IdOperador == codigoDestino && s.Tipo == servicio.Tipo);
                    if (servicioOperadorDestino == null)
                        throw new ReglasExcepcion("PMIGR005", $"El operador destino {codigoDestino} no tiene servicio del tipo {servicio.Tipo} para la cuenta {cuenta.NumeroCuenta}.");

                    int origen = servicio.IdOperador.Value, destino = codigoDestino;

                    cuenta.IdServicio = servicioOperadorDestino.IdServicio;
                    dbContext.SaveChanges();

                    estado = "APL";

                    if (proceso != null)
                        proceso.CantidadExito++;

                    string nombreOperadorOrigen = dbContext.Operadors.FirstOrDefault(o => o.IdOperador == origen)?.Nombre ?? "Desconocido";
                    string nombreOperadorDestino = dbContext.Operadors.FirstOrDefault(o => o.IdOperador == destino)?.Nombre ?? "Desconocido";

                    notas += $"La cuenta '{cuenta.NumeroCuenta}' fue migrada de '{nombreOperadorOrigen}' a '{nombreOperadorDestino}' con el servicio '{servicio.Tipo}'";
                    codigoError = "";

                    var nuevoDetalle = new Detalle
                    {
                        Estado = estado,
                        TipoProceso = "Migracion",
                        Notas = notas,
                        CodigoError = codigoError,
                        IdOperadorOrigen = origen,
                        IdOperadorDestino = destino,
                        IdCuenta = cuenta.IdCuenta
                    };

                    dbContext.Detalles.Add(nuevoDetalle);
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    var mensajeError = APIUtil.RecuperarExcepcionSistema(ex);
                    codigoError = mensajeError.CodigoError;
                    notas = mensajeError.MensajeError;

                    if (proceso != null)
                    {
                        if (estado == "DUP")
                            proceso.CantidadDuplicado++;
                        else
                            proceso.CantidadError++;
                    }

                    var nuevoDetalle = new Detalle
                    {
                        Estado = estado,
                        TipoProceso = "Migracion",
                        Notas = notas,
                        CodigoError = codigoError,
                        IdCuenta = cuenta.IdCuenta
                    };

                    dbContext.Detalles.Add(nuevoDetalle);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}
