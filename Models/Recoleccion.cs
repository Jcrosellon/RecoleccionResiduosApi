using System;

namespace RecoleccionResiduosApi.Models
{
    public class Recoleccion
{
    public int Id { get; set; }

    public int TipoResiduoId { get; set; }
    public TipoResiduo? TipoResiduo { get; set; }

    public string? Subtipo { get; set; }
    public DateTime Fecha { get; set; }
    public double? PesoKg { get; set; }
    public bool EsValida { get; set; } = false;

    public int UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }

    public int PuntosGanados { get; set; } = 0;
}

}
