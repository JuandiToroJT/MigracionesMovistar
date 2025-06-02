using System;
using System.Collections.Generic;

namespace ProyectoMigracionMovistarApi.Entities;

public partial class Detalle
{
    public int IdDetalle { get; set; }

    public DateTime? Fecha { get; set; }

    public string? Estado { get; set; }

    public int? IdOperadorDestino { get; set; }

    public int? IdOperadorOrigen { get; set; }

    public int? IdCuenta { get; set; }

    public int? IdProceso { get; set; }

    public string? Notas { get; set; }

    public string? CodigoError { get; set; }

    public string? TipoProceso { get; set; }

    public virtual Cuenta? IdCuentaNavigation { get; set; }

    public virtual Operador? IdOperadorDestinoNavigation { get; set; }

    public virtual Operador? IdOperadorOrigenNavigation { get; set; }

    public virtual Proceso? IdProcesoNavigation { get; set; }
}
