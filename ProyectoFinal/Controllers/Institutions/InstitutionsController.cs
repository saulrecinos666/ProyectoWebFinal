using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Institutions;
using ProyectoFinal.Models.Institutions.Dto;

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
            var institutions = await _context.Institutions
                .Include(i => i.District)
                .Select(i => new ResponseInstitutionDto 
                {
                    InstitutionId = i.InstitutionId,
                    Name = i.Name,
                    Address = i.Address,
                    DistrictName = i.District != null ? i.District.DistrictName : "NA",
                    Email = i.Email,
                    Phone = i.Phone,
                    IsActive = i.IsActive,
                    CreatedBy = i.CreatedBy,
                    CreatedAt = i.CreatedAt,
                    ModifiedBy = i.ModifiedBy,
                    ModifiedAt = i.ModifiedAt

                })
                .ToListAsync();

            return Ok(institutions);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetInstitutionById(int id)
        {
            var institution = await _context.Institutions
                .Include(i => i.District)
                .FirstOrDefaultAsync(i => i.InstitutionId == id);

            if (institution == null) { return NotFound(); }

            var institutionDto = new ResponseInstitutionDto
            {
                InstitutionId = institution.InstitutionId,
                Name = institution.Name,
                Address = institution.Address,
                DistrictName = institution.District != null ? institution.District.DistrictName : "NA",
                Email = institution.Email,
                Phone = institution.Phone,
                IsActive = institution.IsActive,
                CreatedBy = institution.CreatedBy,
                CreatedAt = institution.CreatedAt,
                ModifiedBy = institution.ModifiedBy,
                ModifiedAt = institution.ModifiedAt
            };

            return Ok(institutionDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateInstitution([FromBody] CreateInstitutionDto createInstitutionDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var institution = new Institution
            {
                Name = createInstitutionDto.Name,
                Address = createInstitutionDto.Address,
                DistrictCode = createInstitutionDto.DistrictCode,
                Email = createInstitutionDto.Email,
                Phone = createInstitutionDto.Phone,
                IsActive = true,
                CreatedBy = GetUserId(),
                CreatedAt = DateTime.Now
            };

            _context.Institutions.Add(institution);
            await _context.SaveChangesAsync();

            var createdInstitution = await _context.Institutions
                .Include(i => i.District)
                .FirstOrDefaultAsync(i => i.InstitutionId == institution.InstitutionId);

            var responseDto = new ResponseInstitutionDto
            {
                Name = createdInstitution.Name,
                Address = createdInstitution.Address,
                DistrictName = createdInstitution.District != null ? createdInstitution.District.DistrictName : "NA",
                Email = createdInstitution.Email,
                Phone = createdInstitution.Phone,
                CreatedAt = createdInstitution.CreatedAt,
                CreatedBy = createdInstitution.CreatedBy
            };

            return CreatedAtAction(nameof(GetInstitutionById), new { id = institution.InstitutionId }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateInstitution(int id, [FromBody] UpdateInstitutionDto updateInstitutionDto)
        {
            var existingInstitution = await _context.Institutions
                .FirstOrDefaultAsync(i => i.InstitutionId == id);

            if (existingInstitution == null) return NotFound("Institucion no encontrada.");

            existingInstitution.Name = updateInstitutionDto.Name;
            existingInstitution.Address = updateInstitutionDto.Address;
            existingInstitution.DistrictCode = updateInstitutionDto.DistrictCode;
            existingInstitution.Phone = updateInstitutionDto.Phone;
            existingInstitution.Email = updateInstitutionDto.Email;
            existingInstitution.ModifiedAt = DateTime.Now;
            existingInstitution.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/desactivate")]
        public async Task<IActionResult> DesactivateInstitution(int id)
        {
            var institution = await _context.Institutions.
                FirstOrDefaultAsync(i => i.InstitutionId == id);

            if (institution == null) return NotFound();

            institution.DeletedAt = DateTime.Now;
            institution.DeletedBy = GetUserId();
            institution.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
