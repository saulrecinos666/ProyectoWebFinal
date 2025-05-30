using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Districts.Dto;

namespace ProyectoFinal.Controllers.Districts
{
    [ApiController]
    [Route("api/district")]
    [Authorize]
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
            var districts = await _context.Districts
                .Select(d => new ResponseDistrictDto 
                {
                    DistrictCode = d.DistrictCode,
                    DistrictName = d.DistrictName,
                    MunicipalityCode = d.MunicipalityCode
                })
                .ToListAsync();

            return Ok(districts);
        }
    }
}
