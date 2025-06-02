using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProyectoMigracionMovistarApi.Entities;

public partial class MigracionDbContext : DbContext
{
    public MigracionDbContext()
    {
    }

    public MigracionDbContext(DbContextOptions<MigracionDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Auditoria> Auditoria { get; set; }

    public virtual DbSet<Cuenta> Cuenta { get; set; }

    public virtual DbSet<Detalle> Detalles { get; set; }

    public virtual DbSet<Operador> Operadors { get; set; }

    public virtual DbSet<Proceso> Procesos { get; set; }

    public virtual DbSet<Servicio> Servicios { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Auditoria>(entity =>
        {
            entity.HasKey(e => e.IdAuditoria).HasName("PK__AUDITORI__9644A3CE0DBE8131");

            entity.ToTable("AUDITORIA");

            entity.Property(e => e.IdAuditoria).HasColumnName("id_auditoria");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.TipoEvento)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("tipo_evento");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Auditoria)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__AUDITORIA__id_us__52593CB8");
        });

        modelBuilder.Entity<Cuenta>(entity =>
        {
            entity.HasKey(e => e.IdCuenta).HasName("PK__CUENTA__C7E2868579DF071E");

            entity.ToTable("CUENTA");

            entity.Property(e => e.IdCuenta).HasColumnName("id_cuenta");
            entity.Property(e => e.IdServicio).HasColumnName("id_servicio");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.NumeroCuenta)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("numero_cuenta");

            entity.Property(e => e.Migrada)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("migrada");

            entity.HasOne(d => d.IdServicioNavigation).WithMany(p => p.Cuenta)
                .HasForeignKey(d => d.IdServicio)
                .HasConstraintName("FK__CUENTA__id_servi__412EB0B6");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Cuenta)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__CUENTA__id_usuar__403A8C7D");
        });

        modelBuilder.Entity<Detalle>(entity =>
        {
            entity.HasKey(e => e.IdDetalle).HasName("PK__DETALLE__4F1332DE797DCE10");

            entity.ToTable("DETALLE");

            entity.Property(e => e.IdDetalle).HasColumnName("id_detalle");
            entity.Property(e => e.Estado)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("estado");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdCuenta).HasColumnName("id_cuenta");
            entity.Property(e => e.IdOperadorDestino).HasColumnName("id_operador_destino");
            entity.Property(e => e.IdOperadorOrigen).HasColumnName("id_operador_origen");
            entity.Property(e => e.IdProceso).HasColumnName("id_proceso");

            entity.Property(e => e.Notas)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("notas");

            entity.Property(e => e.CodigoError)
                .HasMaxLength(10)
                .IsUnicode(false)
                .HasColumnName("codigoError");

            entity.Property(e => e.TipoProceso)
                .HasMaxLength(9)
                .IsUnicode(false)
                .HasColumnName("tipoProceso");

            entity.HasOne(d => d.IdCuentaNavigation).WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdCuenta)
                .HasConstraintName("FK__DETALLE__id_cuen__4CA06362");

            entity.HasOne(d => d.IdOperadorDestinoNavigation).WithMany(p => p.DetalleIdOperadorDestinoNavigations)
                .HasForeignKey(d => d.IdOperadorDestino)
                .HasConstraintName("FK__DETALLE__id_oper__4AB81AF0");

            entity.HasOne(d => d.IdOperadorOrigenNavigation).WithMany(p => p.DetalleIdOperadorOrigenNavigations)
                .HasForeignKey(d => d.IdOperadorOrigen)
                .HasConstraintName("FK__DETALLE__id_oper__4BAC3F29");

            entity.HasOne(d => d.IdProcesoNavigation).WithMany(p => p.Detalles)
                .HasForeignKey(d => d.IdProceso)
                .HasConstraintName("FK__DETALLE__id_proc__4D94879B");
        });

        modelBuilder.Entity<Operador>(entity =>
        {
            entity.HasKey(e => e.IdOperador).HasName("PK__OPERADOR__F46AAEF2270F40BB");

            entity.ToTable("OPERADOR");

            entity.Property(e => e.IdOperador).HasColumnName("id_operador");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
        });

        modelBuilder.Entity<Proceso>(entity =>
        {
            entity.HasKey(e => e.IdProceso).HasName("PK__PROCESO__4D1766E44D552821");

            entity.ToTable("PROCESO");

            entity.Property(e => e.IdProceso).HasColumnName("id_proceso");
            entity.Property(e => e.CantidadDuplicado).HasColumnName("cantidad_duplicado");
            entity.Property(e => e.CantidadError).HasColumnName("cantidad_error");
            entity.Property(e => e.CantidadExito).HasColumnName("cantidad_exito");
            entity.Property(e => e.EstadoProceso)
                .HasMaxLength(3)
                .IsUnicode(false)
                .HasColumnName("estado_proceso");
            entity.Property(e => e.Fecha)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("fecha");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Origen)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("origen");
            entity.Property(e => e.TotalRegistros).HasColumnName("total_registros");

            entity.Property(e => e.Notas)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("notas");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.Procesos)
                .HasForeignKey(d => d.IdUsuario)
                .HasConstraintName("FK__PROCESO__id_usua__45F365D3");
        });

        modelBuilder.Entity<Servicio>(entity =>
        {
            entity.HasKey(e => e.IdServicio).HasName("PK__SERVICIO__6FD07FDC39468FF1");

            entity.ToTable("SERVICIO");

            entity.Property(e => e.IdServicio).HasColumnName("id_servicio");
            entity.Property(e => e.Descripcion)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("descripcion");
            entity.Property(e => e.IdOperador).HasColumnName("id_operador");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.Tipo)
                .HasMaxLength(5)
                .IsUnicode(false)
                .HasColumnName("tipo");

            entity.HasOne(d => d.IdOperadorNavigation).WithMany(p => p.Servicios)
                .HasForeignKey(d => d.IdOperador)
                .HasConstraintName("FK__SERVICIO__id_ope__3D5E1FD2");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__USUARIO__4E3E04ADEBC34554");

            entity.ToTable("USUARIO");

            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Celular)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("celular");
            entity.Property(e => e.Clave)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("clave");
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("correo");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("nombre");
            entity.Property(e => e.NumeroIdentificacion)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("numero_identificacion");
            entity.Property(e => e.Rol)
                .HasMaxLength(7)
                .IsUnicode(false)
                .HasColumnName("rol");
            entity.Property(e => e.TipoIdentificacion)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("tipo_identificacion");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
