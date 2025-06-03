using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Specialties;
using ProyectoFinal.Models.Specialties.Dto;

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
            var specialties = await _context.Specialties
                .Select(s => new ResponseSpecialtyDto 
                {
                    SpecialtyId = s.SpecialtyId,
                    Name = s.Name,
                    Description = s.Description
                })               
                .ToListAsync();

            return Ok(specialties);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpecialtyById(int id)
        {
            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.SpecialtyId == id);

            if (specialty == null) { return NotFound(); }

            var specialtyDto = new ResponseSpecialtyDto
            {
                Name = specialty.Name,
                Description = specialty.Description
            };

            return Ok(specialtyDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSpecialty([FromBody] CreateSpecialtyDto createSpecialtyDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var specialty = new Specialty
            {
                Name = createSpecialtyDto.Name,
                Description = createSpecialtyDto.Description,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = GetUserId()
            };

            _context.Specialties.Add(specialty);
            await _context.SaveChangesAsync();

            var createdSpecialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.SpecialtyId == specialty.SpecialtyId);

            var responseDto = new ResponseSpecialtyDto
            {
                Name = createdSpecialty.Name,
                Description = createdSpecialty.Description,
                CreatedBy = createdSpecialty.CreatedBy,
                CreatedAt = createdSpecialty.CreatedAt
            };

            return CreatedAtAction(nameof(GetSpecialtyById), new { id = specialty.SpecialtyId }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSpecialty(int id, [FromBody] UpdateSpecialtyDto updateSpecialtyDto)
        {
            var existingSpecialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.SpecialtyId == id);

            if (existingSpecialty == null) return NotFound("Especialidad no encontrada.");

            existingSpecialty.Name = updateSpecialtyDto.Name;
            existingSpecialty.Description = updateSpecialtyDto.Description;
            existingSpecialty.ModifiedAt = DateTime.Now;
            existingSpecialty.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/desactivate")]
        public async Task<IActionResult> DesactivateSpecialty(int id)
        {
            var specialty = await _context.Specialties
                .FirstOrDefaultAsync(s => s.SpecialtyId == id);

            if (specialty == null) return NotFound();

            specialty.DeletedAt = DateTime.Now;
            specialty.DeletedBy = GetUserId();
            specialty.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
