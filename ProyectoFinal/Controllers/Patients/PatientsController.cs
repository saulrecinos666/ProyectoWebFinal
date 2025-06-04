using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Patients;
using ProyectoFinal.Models.Patients.Dto;
using ProyectoFinal.Models.Users;

namespace ProyectoFinal.Controllers.Patients
{
    [ApiController]
    [Route("api/patient")]
    [Authorize]
    public class PatientsController : BaseController
    {
        private readonly DbCitasMedicasContext _context;

        public PatientsController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _context.Patients
                .Include(p => p.User)
                .Select(p => new ResponsePatientDto 
                {
                    PatientId = p.PatientId,
                    UserName = p.User != null ? p.User.Username : "NA",
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    SecondLastName = p.SecondLastName,
                    Dui = p.Dui,
                    Address = p.Address,
                    Email = p.Email,
                    Phone = p.Phone,
                    Gender = p.Gender,
                    DateOfBirth = p.DateOfBirth,
                    IsActive = p.IsActive,
                    CreatedBy = p.CreatedBy,
                    CreatedAt = p.CreatedAt,
                    ModifiedBy = p.ModifiedBy,
                    ModifiedAt = p.ModifiedAt
                })
                .ToListAsync();

            return Ok(patients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _context.Patients
                 .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return NotFound();

            var patientDto = new ResponsePatientDto
            {
                PatientId = patient.PatientId,
                UserName = patient.User != null ? patient.User.Username : "NA",
                FirstName = patient.FirstName,
                MiddleName = patient.MiddleName,
                LastName = patient.LastName,
                SecondLastName = patient.SecondLastName,
                Dui = patient.Dui,
                Address = patient.Address,
                Email = patient.Email,
                Phone = patient.Phone,
                Gender = patient.Gender,
                DateOfBirth = patient.DateOfBirth,
                IsActive = patient.IsActive,
                CreatedBy = patient.CreatedBy,
                CreatedAt = patient.CreatedAt,
                ModifiedBy = patient.ModifiedBy,
                ModifiedAt = patient.ModifiedAt
            };

            return Ok(patientDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] CreatePatientDto createPatientDto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var patient = new Patient
            {
                UserId = createPatientDto.UserId,
                FirstName = createPatientDto.FirstName,
                MiddleName = createPatientDto.MiddleName,
                LastName = createPatientDto.LastName,
                SecondLastName = createPatientDto.SecondLastName,
                Dui = createPatientDto.Dui,
                Address = createPatientDto.Address,
                Email = createPatientDto.Email,
                Phone = createPatientDto.Phone,
                Gender = createPatientDto.Gender,
                DateOfBirth = createPatientDto.DateOfBirth,
                IsActive = true,
                CreatedAt = DateTime.Now,
                CreatedBy = GetUserId()
            };

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();

            var createdPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == patient.PatientId);

            var responseDto = new ResponsePatientDto
            {
                UserName = createdPatient.User != null ? createdPatient.User.Username : "NA",
                FirstName = createdPatient.FirstName,
                MiddleName = createdPatient.MiddleName,
                LastName = createdPatient.LastName,
                SecondLastName = createdPatient.SecondLastName,
                Dui = createdPatient.Dui,
                Address = createdPatient.Address,
                Email = createdPatient.Email,
                Phone = createdPatient.Phone,
                Gender = createdPatient.Gender,
                DateOfBirth = createdPatient.DateOfBirth,
                IsActive = createdPatient.IsActive,
                CreatedAt = createdPatient.CreatedAt,
                CreatedBy = createdPatient.CreatedBy
            };

            return CreatedAtAction(nameof(GetPatientById), new { id = patient.PatientId }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] UpdatePatientDto updatePatientDto)
        {
            var existingPatient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (existingPatient == null) return NotFound("Paciente no encontrado");

            existingPatient.UserId = updatePatientDto.UserId;
            existingPatient.FirstName = updatePatientDto.FirstName;
            existingPatient.MiddleName = updatePatientDto.MiddleName;
            existingPatient.LastName = updatePatientDto.LastName;
            existingPatient.SecondLastName = updatePatientDto.SecondLastName;
            existingPatient.Dui = updatePatientDto.Dui;
            existingPatient.DateOfBirth = updatePatientDto.DateOfBirth;
            existingPatient.Gender = updatePatientDto.Gender;
            existingPatient.Address = updatePatientDto.Address;
            existingPatient.Phone = updatePatientDto.Phone;
            existingPatient.Email = updatePatientDto.Email;
            existingPatient.ModifiedAt = DateTime.Now;
            existingPatient.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id}/desactivate")]
        public async Task<IActionResult> DesactivatePatient(int id)
        {
            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null) return NotFound();

            patient.IsActive = false;
            patient.DeletedAt = DateTime.Now;
            patient.DeletedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
