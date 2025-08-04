using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios => Set<Usuario>();
        public DbSet<Recoleccion> Recolecciones => Set<Recoleccion>();
        public DbSet<TipoResiduo> TiposResiduo => Set<TipoResiduo>();


    }
}
