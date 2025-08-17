using Microsoft.EntityFrameworkCore;
using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<TipoResiduo> TiposResiduo { get; set; }
        public DbSet<Recoleccion> Recolecciones { get; set; }

        public DbSet<Localidad> Localidades { get; set; }
        public DbSet<EmpresaRecolectora> EmpresasRecolectoras { get; set; }
        public DbSet<SubtipoResiduo> SubtiposResiduo { get; set; }
        public DbSet<Descuento> Descuentos { get; set; }
        public DbSet<Canje> Canjes { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<ReglaValidacion> ReglasValidacion { get; set; }
        public DbSet<ConfiguracionZona> ConfiguracionesZona { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            // Usuario - Localidad (N:1)
            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Localidad)
                .WithMany(l => l.Usuarios)
                .HasForeignKey(u => u.LocalidadId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Usuario>(e =>
{
e.Property(x => x.PasswordResetToken).HasMaxLength(128);
e.HasIndex(x => x.PasswordResetToken);
});


            // Recoleccion - Usuario (N:1)
            modelBuilder.Entity<Recoleccion>()
                .HasOne(r => r.Usuario)
                .WithMany(u => u.Recolecciones)
                .HasForeignKey(r => r.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Recoleccion - TipoResiduo (N:1)
            modelBuilder.Entity<Recoleccion>()
                .HasOne(r => r.TipoResiduo)
                .WithMany(t => t.Recolecciones)
                .HasForeignKey(r => r.TipoResiduoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Recoleccion - SubtipoResiduo (N:1)
            modelBuilder.Entity<Recoleccion>()
                .HasOne(r => r.SubtipoResiduo)
                .WithMany(s => s.Recolecciones)
                .HasForeignKey(r => r.SubtipoResiduoId)
                .OnDelete(DeleteBehavior.SetNull);

            // Recoleccion - EmpresaRecolectora (N:1)
            modelBuilder.Entity<Recoleccion>()
                .HasOne(r => r.EmpresaRecolectora)
                .WithMany(e => e.Recolecciones)
                .HasForeignKey(r => r.EmpresaRecolectoraId)
                .OnDelete(DeleteBehavior.SetNull);

            // SubtipoResiduo - TipoResiduo (N:1)
            modelBuilder.Entity<SubtipoResiduo>()
                .HasOne(s => s.TipoResiduo)
                .WithMany(t => t.Subtipos)
                .HasForeignKey(s => s.TipoResiduoId)
                .OnDelete(DeleteBehavior.Cascade);

            // EmpresaRecolectora - Localidad (N:1)
            modelBuilder.Entity<EmpresaRecolectora>()
                .HasOne(e => e.Localidad)
                .WithMany(l => l.EmpresasRecolectoras)
                .HasForeignKey(e => e.LocalidadId)
                .OnDelete(DeleteBehavior.Restrict);

            // Canje - Usuario (N:1)
            modelBuilder.Entity<Canje>()
                .HasOne(c => c.Usuario)
                .WithMany(u => u.Canjes)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Canje - Descuento (N:1)
            modelBuilder.Entity<Canje>()
                .HasOne(c => c.Descuento)
                .WithMany(d => d.Canjes)
                .HasForeignKey(c => c.DescuentoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notificacion - Usuario (N:1)
            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Usuario)
                .WithMany(u => u.Notificaciones)
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Notificacion - Recoleccion (N:1) - Opcional
            modelBuilder.Entity<Notificacion>()
                .HasOne(n => n.Recoleccion)
                .WithMany(r => r.Notificaciones)
                .HasForeignKey(n => n.RecoleccionId)
                .OnDelete(DeleteBehavior.SetNull);

            // ReglaValidacion - TipoResiduo (N:1) - Opcional
            modelBuilder.Entity<ReglaValidacion>()
                .HasOne(r => r.TipoResiduo)
                .WithMany()
                .HasForeignKey(r => r.TipoResiduoId)
                .OnDelete(DeleteBehavior.SetNull);

            // ReglaValidacion - Localidad (N:1) - Opcional
            modelBuilder.Entity<ReglaValidacion>()
                .HasOne(r => r.Localidad)
                .WithMany()
                .HasForeignKey(r => r.LocalidadId)
                .OnDelete(DeleteBehavior.SetNull);

            // ConfiguracionZona - Localidad (N:1)
            modelBuilder.Entity<ConfiguracionZona>()
                .HasOne(c => c.Localidad)
                .WithMany()
                .HasForeignKey(c => c.LocalidadId)
                .OnDelete(DeleteBehavior.Cascade);

            // ConfiguracionZona - TipoResiduo (N:1)
            modelBuilder.Entity<ConfiguracionZona>()
                .HasOne(c => c.TipoResiduo)
                .WithMany()
                .HasForeignKey(c => c.TipoResiduoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice único para configuración por zona y tipo
            modelBuilder.Entity<ConfiguracionZona>()
                .HasIndex(c => new { c.LocalidadId, c.TipoResiduoId })
                .IsUnique();

            modelBuilder.Entity<Usuario>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Canje>()
                .HasIndex(c => c.CodigoCanje)
                .IsUnique();

            modelBuilder.Entity<Descuento>()
                .Property(d => d.ValorDescuento)
                .HasPrecision(10, 2);
        }
    }
}
