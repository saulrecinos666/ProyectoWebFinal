using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Municipalities.Dto;

namespace ProyectoFinal.Controllers.Municipalities
{
    [ApiController]
    [Route("api/municipality")]
    [Authorize]
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
            var municipalities = await _context.Municipalities
                .Select(s => new ResponseMunicipalityDto 
                {
                    MunicipalityCode = s.MunicipalityCode,
                    MunicipalityName = s.MunicipalityName, 
                    DepartmentCode = s.DepartmentCode
                })
                .ToListAsync();

            return Ok(municipalities);
        }
    }
}
