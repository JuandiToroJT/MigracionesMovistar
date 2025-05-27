using System;
using System.Collections.Generic;

namespace ProyectoMigracionMovistarApi.Entities;

public partial class Servicio
{
    public int IdServicio { get; set; }

    public string? Tipo { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public int? IdOperador { get; set; }

    public virtual ICollection<Cuenta> Cuenta { get; set; } = new List<Cuenta>();

    public virtual Operador? IdOperadorNavigation { get; set; }
}
