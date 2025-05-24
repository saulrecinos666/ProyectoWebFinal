using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;

namespace ProyectoFinal.Controllers.Districts
{
    public class DistrictsController : BaseController
    {
        private readonly DbCitasMedicasContext _context;

        public DistrictsController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDistricts()
        {
            var districts = await _context.Districts.ToListAsync();
            return Ok(districts);
        }
    }
}
