using Microsoft.EntityFrameworkCore;
using ProyectoMigracionMovistarApi.Entities;
using ProyectoMigracionMovistarApi.Models;
using ProyectoMigracionMovistarApi.Utils;

namespace ProyectoMigracionMovistarApi.Bussines
{
    public class ProcesoBL
    {
        private readonly MigracionDbContext dbContext;
        public ProcesoBL(MigracionDbContext context)
        {
            dbContext = context;
        }

        internal List<ProcesoResumen> ObtenerProcesosMigracion()
        {
            var procesos = dbContext.Procesos
                .OrderByDescending(p => p.Fecha)
                .Select(p => new ProcesoResumen
                {
                    IdProceso = p.IdProceso,
                    Estado = p.EstadoProceso,
                    Total = p.TotalRegistros,
                    Exitosos = p.CantidadExito,
                    Errores = p.CantidadError,
                    Duplicados = p.CantidadDuplicado,
                    Fecha = p.Fecha
                })
                .ToList();

            return procesos;
        }

        internal RespuestaTransaccion RealizarMigracionManual(string usuario, DatosMigracionManual body)
        {
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

            if (!personaMigrar.Cuenta.Any(x => x.NumeroCuenta == body.NumeroCuenta))
                throw new ReglasExcepcion("PMARMM007", "El número de cuenta no coincide con ninguna cuenta registrada");

            string notas = "", codigoError = "";
            ProcesarUsuarioMigracion(personaMigrar, personaMigrar.Cuenta.Where(x => x.NumeroCuenta == body.NumeroCuenta).ToList(), null, out notas, out codigoError);
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

        internal async Task<RespuestaTransaccion> RealizarMigracionMasiva(string usuario, IServiceProvider serviceProvider)
        {
            var usuarioRegistra = dbContext.Usuarios
                .FirstOrDefault(x => x.NumeroIdentificacion == usuario);

            if (usuarioRegistra == null)
                throw new ReglasExcepcion("PMARMM002", "Usuario no encontrado");
            if (usuarioRegistra.Rol == "cliente")
                throw new ReglasExcepcion("PMARMM009", "No tiene permisos para realizar migración masiva");

            int totalRegistros = dbContext.Cuenta
                .Where(c => c.IdServicioNavigation.IdOperador == Constantes.CodigoMovistar && !dbContext.Detalles.Any(d => d.IdCuenta == c.IdCuenta && d.Estado == "APL"))
                .Count();

            int idProceso = IniciarProceso("Migracion", totalRegistros, usuarioRegistra.IdUsuario);

            _ = Task.Run(() =>
            {
                try
                {
                    using var scope = serviceProvider.CreateScope();
                    var nuevoDbContext = scope.ServiceProvider.GetRequiredService<MigracionDbContext>();
                    var nuevaInstancia = new ProcesoBL(nuevoDbContext);

                    nuevaInstancia.EjecutarMigracionMasivaPorLotes(usuarioRegistra, idProceso);
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
            var proceso = dbContext.Procesos.FirstOrDefault(p => p.IdProceso == idProceso);
            if (proceso == null)
                return;

            try
            {
                // Buscar todas las cuentas de Movistar que no han sido migradas
                var cuentasMigrables = dbContext.Cuenta
                    .Include(c => c.IdUsuarioNavigation)
                    .Include(c => c.IdServicioNavigation)
                    .Where(c =>
                        c.IdServicioNavigation.IdOperador == Constantes.CodigoMovistar &&
                        !dbContext.Detalles.Any(d => d.IdCuenta == c.IdCuenta && d.Estado == "APL"))
                    .ToList();

                // Agrupar por usuario
                var usuariosConCuentas = cuentasMigrables
                    .GroupBy(c => c.IdUsuarioNavigation)
                    .ToList();

                foreach (var grupo in usuariosConCuentas)
                {
                    var usuario = grupo.Key;
                    var cuentas = grupo.ToList();

                    ProcesarUsuarioMigracion(usuario, cuentas, proceso, out string notas, out string codigoError);
                }

                proceso.EstadoProceso = "FIN";
                proceso.Fecha = DateTime.Now;
                dbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                proceso.EstadoProceso = "ERR";
                proceso.Fecha = DateTime.Now;
                dbContext.SaveChanges();
            }
        }


        private int IniciarProceso(string origen, int totalRegistros, int idUsuario)
        {
            var nuevoProceso = new Proceso
            {
                Origen = origen,
                EstadoProceso = "PRO",
                TotalRegistros = totalRegistros,
                CantidadExito = 0,
                CantidadDuplicado = 0,
                CantidadError = 0,
                IdUsuario = idUsuario
            };

            dbContext.Procesos.Add(nuevoProceso);
            dbContext.SaveChanges();

            return nuevoProceso.IdProceso;
        }

        private void ProcesarUsuarioMigracion(Usuario personaMigrar, List<Cuenta> cuentas, Proceso proceso, out string notas, out string codigoError)
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

            if (string.IsNullOrWhiteSpace(personaMigrar.Celular) || personaMigrar.Celular.Length < 3)
            {
                if (proceso != null)
                    proceso.CantidadError += cuentas.Count;

                notas = "El número celular del usuario es inválido o no está definido.";
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
                        var detalle = dbContext.Detalles
                            .FirstOrDefault(d => d.IdCuenta == cuenta.IdCuenta && d.Estado == "APL");

                        if (detalle != null)
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

                    var nuevoDetalle = new Detalle
                    {
                        Estado = estado,
                        IdOperadorOrigen = origen,
                        IdOperadorDestino = destino,
                        IdCuenta = cuenta.IdCuenta
                    };

                    dbContext.Detalles.Add(nuevoDetalle);
                    dbContext.SaveChanges();

                    string nombreOperadorOrigen = dbContext.Operadors.FirstOrDefault(o => o.IdOperador == origen)?.Nombre ?? "Desconocido";
                    string nombreOperadorDestino = dbContext.Operadors.FirstOrDefault(o => o.IdOperador == destino)?.Nombre ?? "Desconocido";

                    notas += $"La cuenta '{cuenta.NumeroCuenta}' fue migrada de '{nombreOperadorOrigen}' a '{nombreOperadorDestino}' con el servicio '{servicio.Tipo}'";
                    codigoError = "";
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
                        IdCuenta = cuenta.IdCuenta
                    };

                    dbContext.Detalles.Add(nuevoDetalle);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}
