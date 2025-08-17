using RecoleccionResiduosApi.Models;

namespace RecoleccionResiduosApi.Data
{
    public static class SeedData
    {
        public static void Inicializar(AppDbContext context)
        {
            if (!context.Localidades.Any())
            {
                var localidades = new List<Localidad>
                {
                    new Localidad { Nombre = "Chapinero", Ciudad = "Bogotá", Departamento = "Cundinamarca" },
                    new Localidad { Nombre = "Usaquén", Ciudad = "Bogotá", Departamento = "Cundinamarca" },
                    new Localidad { Nombre = "Suba", Ciudad = "Bogotá", Departamento = "Cundinamarca" },
                    new Localidad { Nombre = "Kennedy", Ciudad = "Bogotá", Departamento = "Cundinamarca" },
                    new Localidad { Nombre = "Engativá", Ciudad = "Bogotá", Departamento = "Cundinamarca" }
                };
                context.Localidades.AddRange(localidades);
                context.SaveChanges();
            }

            // Tipos de residuo existentes
            if (!context.TiposResiduo.Any())
            {
                var tiposResiduo = new List<TipoResiduo>
                {
                    new TipoResiduo { Nombre = "Orgánico", Puntos = 10, Descripcion = "Residuos biodegradables", Color = "#4CAF50" },
                    new TipoResiduo { Nombre = "Inorgánico Reciclable", Puntos = 15, Descripcion = "Papel, cartón, plásticos, vidrio, metales", Color = "#2196F3" },
                    new TipoResiduo { Nombre = "Peligroso", Puntos = 25, Descripcion = "Baterías, pilas, aceites, químicos", Color = "#F44336" }
                };
                context.TiposResiduo.AddRange(tiposResiduo);
                context.SaveChanges();
            }

            if (!context.SubtiposResiduo.Any())
            {
                var tipoOrganico = context.TiposResiduo.First(t => t.Nombre == "Orgánico");
                var tipoInorganico = context.TiposResiduo.First(t => t.Nombre == "Inorgánico Reciclable");
                var tipoPeligroso = context.TiposResiduo.First(t => t.Nombre == "Peligroso");

                var subtipos = new List<SubtipoResiduo>
                {
                    // Subtipos orgánicos
                    new SubtipoResiduo { Nombre = "FO", Descripcion = "Fracción Orgánica", TipoResiduoId = tipoOrganico.Id, PuntosAdicionales = 5 },
                    new SubtipoResiduo { Nombre = "FV", Descripcion = "Fracción Vegetal", TipoResiduoId = tipoOrganico.Id, PuntosAdicionales = 3 },
                    new SubtipoResiduo { Nombre = "Poda", Descripcion = "Residuos de Poda", TipoResiduoId = tipoOrganico.Id, PuntosAdicionales = 2 },
                    
                    // Subtipos inorgánicos
                    new SubtipoResiduo { Nombre = "Papel", Descripcion = "Papel y cartón", TipoResiduoId = tipoInorganico.Id, PuntosAdicionales = 2 },
                    new SubtipoResiduo { Nombre = "Plástico", Descripcion = "Envases plásticos", TipoResiduoId = tipoInorganico.Id, PuntosAdicionales = 3 },
                    new SubtipoResiduo { Nombre = "Vidrio", Descripcion = "Botellas y envases de vidrio", TipoResiduoId = tipoInorganico.Id, PuntosAdicionales = 4 },
                    new SubtipoResiduo { Nombre = "Metal", Descripcion = "Latas y metales", TipoResiduoId = tipoInorganico.Id, PuntosAdicionales = 5 },
                    
                    // Subtipos peligrosos
                    new SubtipoResiduo { Nombre = "Baterías", Descripcion = "Baterías y pilas", TipoResiduoId = tipoPeligroso.Id, PuntosAdicionales = 10 },
                    new SubtipoResiduo { Nombre = "Aceites", Descripcion = "Aceites usados", TipoResiduoId = tipoPeligroso.Id, PuntosAdicionales = 8 },
                    new SubtipoResiduo { Nombre = "Químicos", Descripcion = "Sustancias químicas", TipoResiduoId = tipoPeligroso.Id, PuntosAdicionales = 15 }
                };
                context.SubtiposResiduo.AddRange(subtipos);
                context.SaveChanges();
            }

            if (!context.EmpresasRecolectoras.Any())
            {
                var localidades = context.Localidades.ToList();
                var empresas = new List<EmpresaRecolectora>
                {
                    new EmpresaRecolectora { Nombre = "EcoRecolecta Norte", Telefono = "3001234567", Email = "norte@ecorecolecta.com", LocalidadId = localidades.First(l => l.Nombre == "Usaquén").Id },
                    new EmpresaRecolectora { Nombre = "Verde Limpio Centro", Telefono = "3007654321", Email = "centro@verdelimpio.com", LocalidadId = localidades.First(l => l.Nombre == "Chapinero").Id },
                    new EmpresaRecolectora { Nombre = "Residuos Suba", Telefono = "3009876543", Email = "info@residuossuba.com", LocalidadId = localidades.First(l => l.Nombre == "Suba").Id }
                };
                context.EmpresasRecolectoras.AddRange(empresas);
                context.SaveChanges();
            }

            if (!context.Descuentos.Any())
            {
                var descuentos = new List<Descuento>
                {
                    new Descuento 
                    { 
                        Nombre = "10% Descuento Supermercado", 
                        Descripcion = "10% de descuento en compras del supermercado", 
                        PuntosRequeridos = 100, 
                        ValorDescuento = 10, 
                        EsPorcentaje = true,
                        FechaInicio = DateTime.UtcNow,
                        FechaFin = DateTime.UtcNow.AddMonths(6),
                        CantidadDisponible = 50
                    },
                    new Descuento 
                    { 
                        Nombre = "$5000 Descuento Restaurante", 
                        Descripcion = "$5000 pesos de descuento en restaurantes aliados", 
                        PuntosRequeridos = 150, 
                        ValorDescuento = 5000, 
                        EsPorcentaje = false,
                        FechaInicio = DateTime.UtcNow,
                        FechaFin = DateTime.UtcNow.AddMonths(3),
                        CantidadDisponible = 30
                    },
                    new Descuento 
                    { 
                        Nombre = "15% Descuento Transporte", 
                        Descripcion = "15% de descuento en transporte público", 
                        PuntosRequeridos = 200, 
                        ValorDescuento = 15, 
                        EsPorcentaje = true,
                        FechaInicio = DateTime.UtcNow,
                        FechaFin = DateTime.UtcNow.AddMonths(12),
                        CantidadDisponible = -1 // Ilimitado
                    }
                };
                context.Descuentos.AddRange(descuentos);
                context.SaveChanges();
            }
        }
    }
}
