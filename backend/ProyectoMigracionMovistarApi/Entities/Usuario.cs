using System;
using System.Collections.Generic;

namespace ProyectoMigracionMovistarApi.Entities;

public partial class Usuario
{
    public int IdUsuario { get; set; }

    public string? TipoIdentificacion { get; set; }

    public string? NumeroIdentificacion { get; set; }

    public string? Nombre { get; set; }

    public string? Correo { get; set; }

    public string? Celular { get; set; }

    public string? Clave { get; set; }

    public string? Rol { get; set; }

    public virtual ICollection<Auditoria>? Auditoria { get; set; } = new List<Auditoria>();

    public virtual ICollection<Cuenta>? Cuenta { get; set; } = new List<Cuenta>();

    public virtual ICollection<Proceso>? Procesos { get; set; } = new List<Proceso>();
}
