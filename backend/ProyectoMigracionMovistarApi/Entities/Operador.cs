using System;
using System.Collections.Generic;

namespace ProyectoMigracionMovistarApi.Entities;

public partial class Operador
{
    public int IdOperador { get; set; }

    public string? Nombre { get; set; }

    public virtual ICollection<Detalle> DetalleIdOperadorDestinoNavigations { get; set; } = new List<Detalle>();

    public virtual ICollection<Detalle> DetalleIdOperadorOrigenNavigations { get; set; } = new List<Detalle>();

    public virtual ICollection<Servicio> Servicios { get; set; } = new List<Servicio>();
}
