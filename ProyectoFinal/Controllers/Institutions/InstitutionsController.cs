using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Institutions;

namespace ProyectoFinal.Controllers.Institutions
{
    [ApiController]
    [Route("api/institution")]
    [Authorize]
    public class InstitutionsController : BaseController
    {
        private readonly DbCitasMedicasContext _context;

        public InstitutionsController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInstitutions()
        {
            var institutions = await _context.Institutions.ToListAsync();
            return Ok(institutions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInstitutionById(int id)
        {
            var institution = await _context.Institutions.FindAsync(id);
            if (institution == null) { return NotFound(); }
            return Ok(institution);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInstitution([FromBody] Institution institution)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            institution.IsActive = true;
            institution.CreatedAt = DateTime.Now;
            institution.CreatedBy = GetUserId();

            _context.Institutions.Add(institution);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetInstitutionById), new { id = institution.InstitutionId }, institution);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInstitution(int id, [FromBody] Institution institution)
        {
            if (id != institution.InstitutionId) return BadRequest();

            var existingInstitution = await _context.Institutions.FindAsync(id);
            if (existingInstitution == null) return NotFound("Institucion no encontrada.");

            existingInstitution.Name = institution.Name;
            existingInstitution.Address = institution.Address;
            existingInstitution.Phone = institution.Phone;
            existingInstitution.Email = institution.Email;
            existingInstitution.IsActive = institution.IsActive;
            existingInstitution.DistrictCode = institution.DistrictCode;
            existingInstitution.ModifiedAt = DateTime.Now;
            existingInstitution.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteInstitution(int id)
        {
            var institution = await _context.Institutions.FindAsync(id);
            if (institution == null) return NotFound();

            institution.DeletedAt = DateTime.Now;
            institution.DeletedBy = GetUserId();
            institution.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
