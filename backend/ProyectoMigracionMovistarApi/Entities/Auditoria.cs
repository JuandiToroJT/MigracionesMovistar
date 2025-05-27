using System;
using System.Collections.Generic;

namespace ProyectoMigracionMovistarApi.Entities;

public partial class Auditoria
{
    public int IdAuditoria { get; set; }

    public DateTime? Fecha { get; set; }

    public string? TipoEvento { get; set; }

    public string? Descripcion { get; set; }

    public int? IdUsuario { get; set; }

    public virtual Usuario? IdUsuarioNavigation { get; set; }
}
