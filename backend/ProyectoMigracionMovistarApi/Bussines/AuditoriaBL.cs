using ExcelDataReader;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using ProyectoMigracionMovistarApi.Entities;
using ProyectoMigracionMovistarApi.Models;
using ProyectoMigracionMovistarApi.Utils;
using System.Data;
using System.Text;

namespace ProyectoMigracionMovistarApi.Bussines
{
    public class AuditoriaBL
    {
        private readonly IDbContextFactory<MigracionDbContext> _dbContextFactory;
        public AuditoriaBL(IDbContextFactory<MigracionDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        internal List<AuditoriaItem> ObtenerAuditoria(string usuario)
        {
            using var dbContext = _dbContextFactory.CreateDbContext();

            var query = dbContext.Auditoria.AsQueryable()
                .Include(a => a.IdUsuarioNavigation)
                .Where(a => a.IdUsuarioNavigation.NumeroIdentificacion == usuario);

            var auditoria = query
                .OrderByDescending(p => p.Fecha)
                .Select(p => new AuditoriaItem
                {
                    TipoEvento = p.TipoEvento,
                    Descripcion = p.Descripcion,
                    FechaRegistro = p.Fecha
                })
                .ToList();

            return auditoria;
        }
    }
}
