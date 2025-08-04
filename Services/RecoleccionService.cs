using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // <== ¡NECESARIO!
using RecoleccionResiduosApi.Models;
using RecoleccionResiduosApi.Data;

namespace RecoleccionResiduosApi.Services
{
    public class RecoleccionService
    {
        private readonly AppDbContext _context;

        public RecoleccionService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Recoleccion>> GetPorUsuario(int usuarioId)
        {
            return await _context.Recolecciones
                .Where(r => r.UsuarioId == usuarioId)
                .ToListAsync();
        }
    }
}
