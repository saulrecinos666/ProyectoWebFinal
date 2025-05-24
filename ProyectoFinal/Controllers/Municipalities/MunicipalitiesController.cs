using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models.Base;

namespace ProyectoFinal.Controllers.Municipalities
{
    public class MunicipalitiesController : Controller
    {
        private readonly DbCitasMedicasContext _context;

        public MunicipalitiesController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMunipalities()
        {
            var municipalities = await _context.Municipalities.ToListAsync();
            return Ok(municipalities);
        }
    }
}
