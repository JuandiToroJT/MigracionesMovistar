using System;
using System.Collections.Generic;

namespace ProyectoMigracionMovistarApi.Entities;

public partial class Proceso
{
    public int IdProceso { get; set; }

    public string? Origen { get; set; }

    public string? EstadoProceso { get; set; }

    public DateTime? Fecha { get; set; }

    public int? TotalRegistros { get; set; }

    public int? CantidadExito { get; set; }

    public int? CantidadError { get; set; }

    public int? CantidadDuplicado { get; set; }

    public int? IdUsuario { get; set; }

    public virtual ICollection<Detalle> Detalles { get; set; } = new List<Detalle>();

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
