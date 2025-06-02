using System;
using System.Collections.Generic;

namespace ProyectoMigracionMovistarApi.Entities;

public partial class Cuenta
{
    public int IdCuenta { get; set; }

    public string? NumeroCuenta { get; set; }

    public int? IdUsuario { get; set; }

    public int? IdServicio { get; set; }

    public string? Migrada { get; set; }

    public virtual ICollection<Detalle>? Detalles { get; set; } = new List<Detalle>();

    public virtual Servicio? IdServicioNavigation { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
