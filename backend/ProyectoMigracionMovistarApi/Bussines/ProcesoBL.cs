using ExcelDataReader;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using ProyectoMigracionMovistarApi.Entities;
using ProyectoMigracionMovistarApi.Models;
using ProyectoMigracionMovistarApi.Utils;
using System;
using System.Collections.Concurrent;
using System.Data;
using System.Diagnostics;
using System.Text;

namespace ProyectoMigracionMovistarApi.Bussines
{
    public class ProcesoBL
    {
        private readonly IDbContextFactory<MigracionDbContext> _dbContextFactory;
        private readonly string _bdExternaMovistar;
        public ProcesoBL(IDbContextFactory<MigracionDbContext> dbContextFactory, string bdExternaMovistar)
        {
            _dbContextFactory = dbContextFactory;
            _bdExternaMovistar = bdExternaMovistar;
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

        internal List<DetalleProcesoItem> ObtenerDetalleProcesos(int? idProceso, string tipo)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var query = dbContext.Detalles
                .Include(d => d.IdCuentaNavigation)
                    .ThenInclude(c => c.IdUsuarioNavigation)
                .Include(d => d.IdCuentaNavigation)
                    .ThenInclude(c => c.IdServicioNavigation)
                        .ThenInclude(s => s.IdOperadorNavigation)
                .AsQueryable();

            if (idProceso.HasValue)
                query = query.Where(p => p.IdProceso == idProceso.Value);
            if (!string.IsNullOrWhiteSpace(tipo))
                query = query.Where(p => p.TipoProceso == tipo);

            var detalles = query
                .OrderByDescending(d => d.Fecha)
                .Select(d => new DetalleProcesoItem
                {
                    IdProceso = d.IdProceso,
                    Tipo = d.TipoProceso,
                    Estado = d.Estado,
                    Fecha = d.Fecha,
                    Notas = d.Notas,
                    TipoIdentificacion = d.IdCuentaNavigation.IdUsuarioNavigation.TipoIdentificacion,
                    Identificacion = d.IdCuentaNavigation.IdUsuarioNavigation.NumeroIdentificacion,
                    NombreCliente = d.IdCuentaNavigation.IdUsuarioNavigation.Nombre,
                    IdCuenta = d.IdCuenta,
                    NumeroCuenta = d.IdCuentaNavigation.NumeroCuenta,
                    TipoServicio = d.IdCuentaNavigation.IdServicioNavigation.Tipo,
                    IdOperador = d.IdCuentaNavigation.IdServicioNavigation.IdOperador,
                    Operador = d.IdCuentaNavigation.IdServicioNavigation.IdOperadorNavigation.Nombre
                })
                .ToList();

            return detalles;
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

        private int IniciarProceso(string origen, int idUsuario, MigracionDbContext dbContext, int totalRegistros)
        {
            if (dbContext.Procesos.Any(p => p.EstadoProceso == "PRO"))
                throw new ReglasExcepcion("PMIGR007", "Ya existe un proceso en curso. Por favor, espere a que finalice antes de iniciar uno nuevo");

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

        internal async Task<RespuestaTransaccion> RealizarMigracionMasiva(string usuario)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var usuarioRegistra = dbContext.Usuarios
                .FirstOrDefault(x => x.NumeroIdentificacion == usuario);

            if (usuarioRegistra == null)
                throw new ReglasExcepcion("PMARMM002", "Usuario no encontrado");
            if (usuarioRegistra.Rol == "cliente")
                throw new ReglasExcepcion("PMARMM009", "No tiene permisos para realizar migración masiva");

            int totalRegistros = dbContext.Cuenta
                    .AsNoTracking()
                    .Count(c => c.Migrada != "S");

            int idProceso = IniciarProceso("Migracion", usuarioRegistra.IdUsuario, dbContext, totalRegistros);

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

        internal async Task<RespuestaTransaccion> RealizarCargueUsuarios(string usuario, DatosCargueMasivo body)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var usuarioRegistra = dbContext.Usuarios
                .FirstOrDefault(x => x.NumeroIdentificacion == usuario);

            if (usuarioRegistra == null)
                throw new ReglasExcepcion("PMARCU001", "Usuario no encontrado");
            if (usuarioRegistra.Rol == "cliente")
                throw new ReglasExcepcion("PMARCU002", "No tiene permisos para realizar el cargue masivo");

            int totalRegistros = 0;
            List<Usuario> usuariosCargados = new List<Usuario>();
            if (body.Archivo != null && body.Archivo.Length > 0 && !string.IsNullOrWhiteSpace(body.Formato))
            {
                DataTable datosUsuarios = CargarDatosDesdeArchivo(body.Archivo, body.Formato);
                Dictionary<string, Usuario> mapaUsuarios = new Dictionary<string, Usuario>();

                foreach (DataRow fila in datosUsuarios.Rows)
                {
                    string numeroIdentificacion = fila["NumeroIdentificacion"]?.ToString()?.Trim() ?? "";

                    if (!mapaUsuarios.TryGetValue(numeroIdentificacion, out var usuarioitem))
                    {
                        var hasher = new PasswordHasher<Usuario>();
                        usuarioitem = new Usuario
                        {
                            TipoIdentificacion = fila["TipoIdentificacionId"]?.ToString()?.Trim(),
                            NumeroIdentificacion = numeroIdentificacion,
                            Nombre = fila["NombreCompleto"]?.ToString()?.Trim(),
                            Correo = fila["CorreoElectronico"]?.ToString()?.Trim(),
                            Celular = fila["NumeroCelular"]?.ToString()?.Trim(),
                            Clave = null,
                            Rol = "cliente",
                            Cuenta = new List<Cuenta>()
                        };

                        usuarioitem.Clave = hasher.HashPassword(usuarioitem, "123");

                        mapaUsuarios[numeroIdentificacion] = usuarioitem;
                    }

                    var cuenta = new Cuenta
                    {
                        NumeroCuenta = fila["NumeroCuenta"]?.ToString()?.Trim(),
                        IdServicio = int.TryParse(fila["PlanId"]?.ToString(), out var idPlan) ? idPlan : (int?)null,
                        Migrada = "N"
                    };

                    usuarioitem.Cuenta.Add(cuenta);
                    totalRegistros++;
                }

                usuariosCargados = mapaUsuarios.Values.ToList();
            }

            int totalExterno = ObtenerTotalDesdeMySQL();

            int idProceso = IniciarProceso("Cargue", usuarioRegistra.IdUsuario, dbContext, totalRegistros + totalExterno);

            _ = Task.Run(async () =>
            {
                try
                {
                    await EjecutarCargueMasivo(usuarioRegistra, idProceso, usuariosCargados, totalExterno);
                }
                catch
                {
                }
            });

            if (usuarioRegistra.Rol == "admin")
            {
                var nuevoAuditoria = new Auditoria
                {
                    TipoEvento = "Cargue",
                    Descripcion = $"Cargue masivo realizado por el administrador {usuarioRegistra.NumeroIdentificacion}",
                    IdUsuario = usuarioRegistra.IdUsuario
                };

                dbContext.Auditoria.Add(nuevoAuditoria);
                dbContext.SaveChanges();
            }

            return new RespuestaTransaccion()
            {
                NumeroRegistro = idProceso,
                Notas = $"Proceso de cargue masivo iniciado. ID del proceso: {idProceso}"
            };
        }

        private DataTable CargarDatosDesdeArchivo(byte[] archivo, string formato)
        {
            var tipo = formato.ToUpper().Trim();

            if (tipo == "CSV")
                return LeerCsvDesdeBytes(archivo);
            else if (tipo == "XLS" || tipo == "XLSX")
                return LeerExcelDesdeBytes(archivo);
            else
                throw new ReglasExcepcion("PMARCU007", "Formato de archivo no soportado: " + formato);
        }

        private DataTable LeerCsvDesdeBytes(byte[] archivo)
        {
            var dt = new DataTable();
            using (var ms = new MemoryStream(archivo))
            using (var reader = new StreamReader(ms, Encoding.UTF8))
            {
                bool columnasCargadas = false;

                while (!reader.EndOfStream)
                {
                    var linea = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(linea)) continue;

                    var valores = linea.Split(',');

                    if (!columnasCargadas)
                    {
                        foreach (var columna in valores)
                            dt.Columns.Add(columna.Trim());
                        columnasCargadas = true;
                    }
                    else
                    {
                        dt.Rows.Add(valores);
                    }
                }
            }
            return dt;
        }

        private DataTable LeerExcelDesdeBytes(byte[] archivo)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var ms = new MemoryStream(archivo))
            using (var reader = ExcelReaderFactory.CreateReader(ms))
            {
                var result = reader.AsDataSet(new ExcelDataSetConfiguration()
                {
                    ConfigureDataTable = (_) => new ExcelDataTableConfiguration
                    {
                        UseHeaderRow = true
                    }
                });

                return result.Tables[0];
            }
        }

        private int ObtenerTotalDesdeMySQL()
        {
            using var con = new MySqlConnection(_bdExternaMovistar);
            con.Open();

            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM users", con);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        private async Task EjecutarCargueMasivo(Usuario usuarioRegistra, int idProceso, List<Usuario> usuariosCargados, int totalExterno)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var proceso = dbContext.Procesos.FirstOrDefault(p => p.IdProceso == idProceso);
            if (proceso == null)
                return;

            try
            {
                var dictOperadores = dbContext.Operadors
                    .ToDictionary(o => o.Nombre.ToLower(), o => o.IdOperador);
                var listaServicios = dbContext.Servicios.ToList();

                await ProcesarMySQLAsync(usuarioRegistra, totalExterno, proceso, dictOperadores, listaServicios);
                foreach (var item in usuariosCargados)
                {
                    using var loteContext = _dbContextFactory.CreateDbContext();
                    ProcesarUsuarioCargue(item, loteContext, proceso);
                }

                proceso.EstadoProceso = "FIN";
                proceso.Notas = "Cargue masivo finalizado correctamente";
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

        private async Task ProcesarMySQLAsync(Usuario usuarioRegistra, int totalExterno, Proceso proceso, Dictionary<string, int> dictOperadores, List<Servicio> listaServicios)
        {
            int batchSize = Constantes.TamañoLote;
            int lotes = (int)Math.Ceiling((double)totalExterno / batchSize);

            var semaphore = new SemaphoreSlim(Constantes.CantidadHilos);
            var tareas = new List<Task>();

            for (int i = 0; i < lotes; i++)
            {
                int offset = i * batchSize;

                await semaphore.WaitAsync();

                var tarea = Task.Run(async () =>
                {
                    try
                    {
                        await ProcesarBatchDesdeMySQL(offset, batchSize, usuarioRegistra, proceso, dictOperadores, listaServicios);
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                tareas.Add(tarea);
            }

            await Task.WhenAll(tareas);
        }

        private async Task ProcesarBatchDesdeMySQL(int offset, int limit, Usuario usuarioRegistra, Proceso proceso, Dictionary<string, int> dictOperadores, List<Servicio> listaServicios)
        {
            var usuarios = new Dictionary<string, Usuario>();

            string query = @"
                SELECT nombrecompleto, tipo_identificacion, numero_identificacion,
                correo_electronico, numero_celular, numero_cuenta, plan, operador_actual
                FROM users ORDER BY id LIMIT @limit OFFSET @offset";

            using var con = new MySqlConnection(_bdExternaMovistar);
            await con.OpenAsync();

            using var cmd = new MySqlCommand(query, con);
            cmd.Parameters.AddWithValue("@limit", limit);
            cmd.Parameters.AddWithValue("@offset", offset);

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string id = reader["numero_identificacion"]?.ToString()?.Trim() ?? "";
                if (!usuarios.TryGetValue(id, out var u))
                {
                    u = new Usuario
                    {
                        Nombre = reader["nombrecompleto"]?.ToString()?.Trim(),
                        TipoIdentificacion = reader["tipo_identificacion"]?.ToString()?.Trim(),
                        NumeroIdentificacion = id,
                        Correo = reader["correo_electronico"]?.ToString()?.Trim(),
                        Celular = reader["numero_celular"]?.ToString()?.Trim(),
                        Clave = null,
                        Rol = "cliente",
                        Cuenta = new List<Cuenta>()
                    };

                    var hasher = new PasswordHasher<Usuario>();
                    u.Clave = hasher.HashPassword(u, "123");

                    usuarios[id] = u;
                }

                int? idServicio = ConvertirPlanAId(reader["plan"]?.ToString()?.Trim(), reader["operador_actual"]?.ToString()?.Trim(), dictOperadores, listaServicios);

                u.Cuenta.Add(new Cuenta
                {
                    NumeroCuenta = reader["numero_cuenta"]?.ToString()?.Trim(),
                    IdServicio = idServicio,
                    Migrada = "N"
                });
            }

            var usuariosCargados = usuarios.Values.ToList();
            foreach (var item in usuariosCargados)
            {
                using var loteContext = _dbContextFactory.CreateDbContext();
                ProcesarUsuarioCargue(item, loteContext, proceso);
            }
        }

        private int? ConvertirPlanAId(string plan, string operador, Dictionary<string, int> dictOperadores, List<Servicio> listaServicios)
        {
            if (string.IsNullOrWhiteSpace(plan) || string.IsNullOrWhiteSpace(operador))
                return null;

            string tipo = plan.ToLower() switch
            {
                "empresarial" => "Fibra",
                "prepago" => "Movil",
                "pospago" => "Movil",
                "datos" => "Banda",
                _ => null
            };

            if (tipo == null) return null;

            if (!dictOperadores.TryGetValue(operador.ToLower(), out var idOperador))
                return null;

            return listaServicios
                .FirstOrDefault(s =>
                    s.IdOperador == idOperador &&
                    s.Tipo.Equals(tipo, StringComparison.OrdinalIgnoreCase))
                ?.IdServicio;
        }

        private void ProcesarUsuarioCargue(Usuario item, MigracionDbContext dbContext, Proceso proceso)
        {
            int? numeroProceso = null;
            if (proceso != null)
                numeroProceso = proceso.IdProceso;

            if (item.Cuenta == null || !item.Cuenta.Any())
            {
                if (proceso != null)
                    proceso.CantidadError ++;

                RegistrarDetalleCargue(dbContext, numeroProceso, "ERR", "El usuario no tiene cuentas asociadas.", "PMARCU003");
                dbContext.SaveChanges();
                return;
            }

            if (string.IsNullOrWhiteSpace(item.TipoIdentificacion) ||
                    string.IsNullOrWhiteSpace(item.NumeroIdentificacion) ||
                    string.IsNullOrWhiteSpace(item.Nombre) ||
                    string.IsNullOrWhiteSpace(item.Correo) ||
                    string.IsNullOrWhiteSpace(item.Celular))
            {
                foreach (var cuenta in item.Cuenta)
                {
                    if (proceso != null)
                        proceso.CantidadError++;

                    RegistrarDetalleCargue(dbContext, numeroProceso, "ERR", $"Faltan campos obligatorios para el usuario '{item.NumeroIdentificacion}'.", "PMARCU005");
                }

                dbContext.SaveChanges();
                return;
            }

            var usuarioExistente = dbContext.Usuarios
                        .Include(u => u.Cuenta)
                        .FirstOrDefault(u => u.NumeroIdentificacion == item.NumeroIdentificacion);

            foreach (var cuenta in item.Cuenta)
            {
                string estado = "ERR";

                try
                {
                    if (string.IsNullOrWhiteSpace(cuenta.NumeroCuenta) || !cuenta.IdServicio.HasValue)
                        throw new ReglasExcepcion("PMARCU006", $"La cuenta del usuario '{item.NumeroIdentificacion}' no tiene todos los datos requeridos.");

                    if (usuarioExistente == null)
                    {
                        var nuevoUsuario = new Usuario
                        {
                            TipoIdentificacion = item.TipoIdentificacion,
                            NumeroIdentificacion = item.NumeroIdentificacion,
                            Nombre = item.Nombre,
                            Correo = item.Correo,
                            Celular = item.Celular,
                            Clave = item.Clave,
                            Rol = item.Rol,
                            Cuenta = new List<Cuenta>()
                        };

                        dbContext.Usuarios.Add(nuevoUsuario);
                        dbContext.SaveChanges();
                        usuarioExistente = nuevoUsuario;
                    }

                    if (usuarioExistente.Cuenta != null && usuarioExistente.Cuenta.Any(c => c.NumeroCuenta == cuenta.NumeroCuenta))
                    {
                        estado = "DUP";
                        throw new ReglasExcepcion("PMARCU004", $"La cuenta {cuenta.NumeroCuenta} ya está registrada para el usuario '{item.NumeroIdentificacion}'.");
                    }

                    cuenta.IdUsuario = usuarioExistente.IdUsuario;
                    usuarioExistente.Cuenta.Add(cuenta);
                    dbContext.Cuenta.Add(cuenta);

                    dbContext.SaveChanges();

                    estado = "APL";

                    if (proceso != null)
                        proceso.CantidadExito++;

                    RegistrarDetalleCargue(dbContext, numeroProceso, estado, $"Cuenta '{cuenta.NumeroCuenta}' se registro correctamente para el usuario '{item.NumeroIdentificacion}'.", "", cuenta.IdCuenta);
                    dbContext.SaveChanges();
                }
                catch (Exception ex)
                {
                    var mensajeError = APIUtil.RecuperarExcepcionSistema(ex);

                    if (proceso != null)
                    {
                        if (estado == "DUP")
                            proceso.CantidadDuplicado++;
                        else
                            proceso.CantidadError++;
                    }

                    int? idCuenta = null;
                    if (estado == "DUP")
                        idCuenta = usuarioExistente.Cuenta.FirstOrDefault(c => c.NumeroCuenta == cuenta.NumeroCuenta)?.IdCuenta;

                    RegistrarDetalleCargue(dbContext, numeroProceso, estado, mensajeError.MensajeError, mensajeError.CodigoError, idCuenta);
                    dbContext.SaveChanges();
                }
            }
        }

        private void RegistrarDetalleCargue(MigracionDbContext dbContext, int? idProceso, string estado, string notas, string codigoError, int? idCuenta = null)
        {
            dbContext.Detalles.Add(new Detalle
            {
                Estado = estado,
                TipoProceso = "Cargue",
                Notas = notas,
                CodigoError = codigoError,
                IdProceso = idProceso,
                IdCuenta = idCuenta
            });
        }

        private void ProcesarUsuarioMigracion(Usuario personaMigrar, List<Cuenta> cuentas, MigracionDbContext dbContext, Proceso proceso, out string notas, out string codigoError)
        {
            notas = "";
            codigoError = "";

            int? numeroProceso = null;
            if (proceso != null)
                numeroProceso = proceso.IdProceso;

            if (personaMigrar == null)
            {
                if (proceso != null)
                {
                    proceso.CantidadError += cuentas.Count;
                    dbContext.SaveChanges();
                }

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
                        {
                            cuenta.Migrada = "S";
                            dbContext.SaveChanges();
                            throw new ReglasExcepcion("PMIGR003", $"La cuenta {cuenta.NumeroCuenta} no requiere migración; ya pertenece a otro operador.");
                        }
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
                    cuenta.Migrada = "S";
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
                        IdCuenta = cuenta.IdCuenta,
                        IdProceso = numeroProceso
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
                        IdCuenta = cuenta.IdCuenta,
                        IdProceso = numeroProceso
                    };

                    dbContext.Detalles.Add(nuevoDetalle);
                    dbContext.SaveChanges();
                }
            }
        }
    }
}
