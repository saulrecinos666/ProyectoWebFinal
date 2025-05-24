using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Doctors;
using System.Security.Claims;

namespace ProyectoFinal.Controllers.Doctors
{
    [ApiController]
    [Route("api/doctor")]
    [Authorize]
    public class DoctorsController : BaseController
    {
        private readonly DbCitasMedicasContext _context;

        public DoctorsController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDoctors()
        {
            var doctors = await _context.Doctors
                .Include(a => a.Specialty)
                .Include(a => a.Institution)
                .Where(a => a.IsActive)
                .ToListAsync();
            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _context.Doctors
                .Include(a => a.Specialty)
                .Include(a => a.Institution)
                .FirstOrDefaultAsync(a => a.DoctorId == id && a.IsActive);

            if (doctor == null) return NotFound();
            return Ok(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor([FromBody] Doctor doctor)
        {
            if (!ModelState.IsValid) return BadRequest();

            if (!_context.Specialties.Any(s => s.SpecialtyId == doctor.SpecialtyId))
                return BadRequest("La Especialidad no existe");

            if (!_context.Institutions.Any(i => i.InstitutionId == doctor.InstitutionId))
                return BadRequest("La Institucion no existe");

            doctor.IsActive = true;
            doctor.CreatedAt = DateTime.Now;
            doctor.CreatedBy = GetUserId();

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetDoctorById), new { id = doctor }, doctor);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] Doctor doctor)
        {
            if (id != doctor.DoctorId) return BadRequest("El Id del Doctor no existe");

            var existingDoctor = await _context.Doctors.FindAsync(id);
            if (existingDoctor == null) return NotFound();

            existingDoctor.FirstName = doctor.FirstName;
            existingDoctor.MiddleName = doctor.MiddleName;
            existingDoctor.LastName = doctor.LastName;
            existingDoctor.SecondLastName = doctor.SecondLastName;
            existingDoctor.Dui = doctor.Dui;
            existingDoctor.Email = doctor.Email;
            existingDoctor.Phone = doctor.Phone;
            existingDoctor.IsActive = doctor.IsActive;
            existingDoctor.SpecialtyId = doctor.SpecialtyId;
            existingDoctor.InstitutionId = doctor.InstitutionId;
            existingDoctor.ModifiedAt = DateTime.Now;
            existingDoctor.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null) return NotFound();

            doctor.DeletedAt = DateTime.Now;
            doctor.DeletedBy = GetUserId();
            doctor.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
