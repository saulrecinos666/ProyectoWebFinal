using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Doctors;
using ProyectoFinal.Models.Doctors.Dto;

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
                .Include(d => d.Specialty)
                .Include(d => d.Institution)
                .Select(d => new ResponseDoctorDto
                {
                    FirstName = d.FirstName,
                    LastName = d.LastName,
                    MiddleName = d.MiddleName,
                    SecondLastName = d.SecondLastName,
                    Dui = d.Dui,
                    Email = d.Email,
                    Phone = d.Phone,
                    SpecialtyName = d.Specialty != null ? d.Specialty.Name : "NA",
                    InstitutionName = d.Institution != null ? d.Institution.Name : "NA",
                    CreatedBy = d.CreatedBy,
                    CreatedAt = d.CreatedAt,
                    ModifiedBy = d.ModifiedBy,
                    ModifiedAt = d.ModifiedAt
                })
                .ToListAsync();

            return Ok(doctors);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctorById(int id)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Specialty)
                .Include(d => d.Institution)
                .FirstOrDefaultAsync(a => a.DoctorId == id);

            if (doctor == null) return NotFound();

            var doctorDto = new ResponseDoctorDto
            {
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                MiddleName = doctor.MiddleName,
                SecondLastName = doctor.SecondLastName,
                Dui = doctor.Dui,
                Email = doctor.Email,
                Phone = doctor.Phone,
                SpecialtyName = doctor.Specialty != null ? doctor.Specialty.Name : "NA",
                InstitutionName = doctor.Institution != null ? doctor.Institution.Name : "NA",
                CreatedBy = doctor.CreatedBy,
                CreatedAt = doctor.CreatedAt,
                ModifiedBy = doctor.ModifiedBy,
                ModifiedAt = doctor.ModifiedAt
            };

            return Ok(doctorDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDto createDoctorDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var doctor = new Doctor
            {
                FirstName = createDoctorDto.FirstName,
                MiddleName = createDoctorDto.MiddleName,
                LastName = createDoctorDto.LastName,
                SecondLastName = createDoctorDto.SecondLastName,
                Dui = createDoctorDto.Dui,
                Email = createDoctorDto.Email,
                Phone = createDoctorDto.Phone,
                SpecialtyId = createDoctorDto.SpecialtyId,
                InstitutionId = createDoctorDto.InstitutionId,
                IsActive = true,
                CreatedBy = GetUserId(),
                CreatedAt = DateTime.Now
            };

            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            var createDoctor = await _context.Doctors
                .Include(d => d.Specialty)
                .Include(d => d.Institution)
                .FirstOrDefaultAsync(d => d.DoctorId == doctor.DoctorId);

            var responseDto = new ResponseDoctorDto
            {
                FirstName = createDoctor.FirstName,
                MiddleName = createDoctor.MiddleName,
                LastName = createDoctor.LastName,
                SecondLastName = createDoctor.SecondLastName,
                Dui = createDoctor.Dui,
                Email = createDoctor.Email,
                Phone = createDoctor.Phone,
                SpecialtyName = createDoctor.Specialty != null ? createDoctor.Specialty.Name : "NA",
                InstitutionName = createDoctor.Institution != null ? createDoctor.Institution.Name : "NA",
                CreatedBy = createDoctor.CreatedBy,
                CreatedAt = createDoctor.CreatedAt
            };

            return CreatedAtAction(nameof(GetDoctorById), new { id = doctor.DoctorId }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] UpdateDoctorDto updateDoctorDto)
        {
            var existingDoctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (existingDoctor == null) return NotFound("Doctor no encontrado");

            existingDoctor.FirstName = updateDoctorDto.FirstName;
            existingDoctor.MiddleName = updateDoctorDto.MiddleName;
            existingDoctor.LastName = updateDoctorDto.LastName;
            existingDoctor.SecondLastName = updateDoctorDto.SecondLastName;
            existingDoctor.Dui = updateDoctorDto.Dui;
            existingDoctor.Email = updateDoctorDto.Email;
            existingDoctor.Phone = updateDoctorDto.Phone;
            existingDoctor.SpecialtyId = updateDoctorDto.SpecialtyId;
            existingDoctor.InstitutionId = updateDoctorDto.InstitutionId;
            existingDoctor.ModifiedAt = DateTime.Now;
            existingDoctor.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/desactivate")]
        public async Task<IActionResult> DesactivateDoctor(int id)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == id);

            if (doctor == null) return NotFound();

            doctor.DeletedAt = DateTime.Now;
            doctor.DeletedBy = GetUserId();
            doctor.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
