namespace RecoleccionResiduosApi.DTOs
{
   public class DashboardDto
{
    public int TotalUsuarios { get; set; }
    public int TotalRecolecciones { get; set; }
    public int TotalPuntosOtorgados { get; set; }
    public int TotalCanjes { get; set; }
    public int RecoleccionesMesActual { get; set; }
    public int PuntosMesActual { get; set; }
    public int CanjesMesActual { get; set; }
    public double PesoTotalKg { get; set; }
    public List<UsuarioRankingDto> TopUsuariosPuntos { get; set; } = new();
    public List<EstadisticaTipoResiduoDto> RecoleccionesPorTipo { get; set; } = new();
    public List<EstadisticaLocalidadDto> RecoleccionesPorLocalidad { get; set; } = new();
    public List<TendenciaMensualDto> TendenciaMensual { get; set; } = new();

    // Lo que espera tu Angular:
    public List<TendenciaItemDto> TendenciaRecolecciones { get; set; } = new();
    public List<TipoResiduoTopDto> TiposResiduoMasRecolectados { get; set; } = new();

    // 👇 FALTA (volver a agregar)
    public ActividadRecienteDto ActividadReciente { get; set; } = new(); 
}

// 👇 Agrega esta clase (si no la tienes)
public class ActividadRecienteDto
{
    public List<RecoleccionRecienteDto> RecoleccionesRecientes { get; set; } = new();
    public List<UsuarioNuevoDto> NuevosUsuarios { get; set; } = new();
    public List<CanjeRecienteDto> CanjesRecientes { get; set; } = new();
}


    // ====== NUEVO: para el chart simple del FE ======
    public class TendenciaItemDto
    {
        public string Mes { get; set; } = "";   // p.ej. "Ene"
        public int Cantidad { get; set; }
    }

    // ====== NUEVO: para el “Top tipos” del FE ======
    public class TipoResiduoTopDto
    {
        public string Tipo { get; set; } = "";
        public int Cantidad { get; set; }
        public double Porcentaje { get; set; }
    }

    // ====== NUEVO: actividad reciente ======
    public class RecoleccionRecienteDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string TipoResiduo { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    public class UsuarioNuevoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
    }

    public class CanjeRecienteDto
    {
        public string Usuario { get; set; } = string.Empty;
        public string Descuento { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
    }

    public class ReporteUsuarioDto
    {
        public UsuarioReporteDto Usuario { get; set; } = new();
        public int TotalRecolecciones { get; set; }
        public double PesoTotalKg { get; set; }
        public int PuntosTotales { get; set; }
        public int TotalCanjes { get; set; }
        public int PuntosCanjeados { get; set; }
        public List<EstadisticaTipoResiduoDto> RecoleccionesPorTipo { get; set; } = new();
        public List<RecoleccionReporteDto> HistorialRecolecciones { get; set; } = new();
    }

    public class ReporteLocalidadDto
    {
        public LocalidadReporteDto Localidad { get; set; } = new();
        public int TotalUsuarios { get; set; }
        public int UsuariosActivos { get; set; }
        public int TotalRecolecciones { get; set; }
        public double PesoTotalKg { get; set; }
        public int PuntosTotales { get; set; }
        public List<EstadisticaTipoResiduoDto> RecoleccionesPorTipo { get; set; } = new();
        public List<UsuarioRankingDto> TopUsuarios { get; set; } = new();
    }

    public class ReporteEmpresaDto
    {
        public EmpresaReporteDto Empresa { get; set; } = new();
        public int TotalRecolecciones { get; set; }
        public double PesoTotalKg { get; set; }
        public int UsuariosAtendidos { get; set; }
        public List<EstadisticaTipoResiduoDto> RecoleccionesPorTipo { get; set; } = new();
        public List<TendenciaMensualDto> RecoleccionesPorMes { get; set; } = new();
    }

    public class UsuarioReporteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Puntos { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string Localidad { get; set; } = string.Empty;
    }

    public class LocalidadReporteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Ciudad { get; set; } = string.Empty;
        public string Departamento { get; set; } = string.Empty;
    }

    public class EmpresaReporteDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Localidad { get; set; } = string.Empty;
    }

    public class UsuarioRankingDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int Puntos { get; set; }
        public int TotalRecolecciones { get; set; }
    }

    public class EstadisticaTipoResiduoDto
    {
        public string TipoResiduo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double PesoTotal { get; set; }
        public int PuntosTotal { get; set; }
    }

    public class EstadisticaLocalidadDto
    {
        public string Localidad { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public double PesoTotal { get; set; }
        public int UsuariosActivos { get; set; }
    }

    public class RecoleccionReporteDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string TipoResiduo { get; set; } = string.Empty;
        public string? Subtipo { get; set; }
        public double? PesoKg { get; set; }
        public int PuntosGanados { get; set; }
        public string Estado { get; set; } = string.Empty;
    }

    public class TendenciaMensualDto
    {
        public int Mes { get; set; }
        public int Ano { get; set; }
        public int Cantidad { get; set; }
        public double PesoTotal { get; set; }
        public string NombreMes => new DateTime(Ano, Mes, 1).ToString("MMMM yyyy");
    }

    public class EstadisticaGeneralDto
    {
        public string Categoria { get; set; } = string.Empty;
        public string Subcategoria { get; set; } = string.Empty;
        public double Valor { get; set; }
        public string Descripcion { get; set; } = string.Empty;
    }
}
