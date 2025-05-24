using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Specialties;

namespace ProyectoFinal.Controllers.Specialties
{
    [ApiController]
    [Route("api/specialty")]
    [Authorize]
    public class SpecialtiesController : BaseController
    {
        private readonly DbCitasMedicasContext _context;

        public SpecialtiesController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSpecialties()
        {
            var specialties = await _context.Specialties.ToListAsync();
            return Ok(specialties);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecialtyById(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null) { return NotFound(); }
            return Ok(specialty);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSpecialty([FromBody] Specialty specialty)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            specialty.IsActive = true;
            specialty.CreatedAt = DateTime.Now;
            specialty.CreatedBy = GetUserId();

            _context.Specialties.Add(specialty);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetSpecialtyById), new { id = specialty.SpecialtyId }, specialty);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSpecialty(int id, [FromBody] Specialty specialty)
        {
            if (id != specialty.SpecialtyId) return BadRequest();

            var existingSpecialty = await _context.Specialties.FindAsync(id);
            if (existingSpecialty == null) return NotFound("Especialidad no encontrada.");

            existingSpecialty.SpecialtyName = specialty.SpecialtyName;
            existingSpecialty.Description = specialty.Description;
            existingSpecialty.IsActive = specialty.IsActive;
            existingSpecialty.ModifiedAt = DateTime.Now;
            existingSpecialty.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpecialty(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty == null) return NotFound();

            specialty.DeletedAt = DateTime.Now;
            specialty.DeletedBy = GetUserId();
            specialty.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
