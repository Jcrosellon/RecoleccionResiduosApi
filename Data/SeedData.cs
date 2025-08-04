using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Data
{
    public static class SeedData
    {
        public static void Inicializar(AppDbContext context)
        {
            if (!context.TiposResiduo.Any())
            {
                context.TiposResiduo.AddRange(
                    new TipoResiduo { Nombre = "Orgánico", Puntos = 15 },
                    new TipoResiduo { Nombre = "Inorgánico", Puntos = 10 },
                    new TipoResiduo { Nombre = "Peligroso", Puntos = 20 }
                );
                context.SaveChanges();
            }
        }
    }
}
