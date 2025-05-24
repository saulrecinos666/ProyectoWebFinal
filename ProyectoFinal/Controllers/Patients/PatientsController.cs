using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Base;
using ProyectoFinal.Models.Patients;

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
            var patients = await _context.Patients.ToListAsync();
            return Ok(patients);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) { return NotFound(); }
            return Ok(patient);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePatient([FromBody] Patient patient)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            patient.IsActive = true;
            patient.CreatedAt = DateTime.Now;
            patient.CreatedBy = GetUserId();

            _context.Patients.Add(patient);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetPatientById), new { id = patient.PatientId }, patient);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatient(int id, [FromBody] Patient patient)
        {
            if (id != patient.PatientId) return BadRequest();

            var existingPatient = await _context.Patients.FindAsync(id);
            if (existingPatient == null) return NotFound("Paciente no encontrado");

            existingPatient.FirstName = patient.FirstName;
            existingPatient.MiddleName = patient.MiddleName;
            existingPatient.LastName = patient.LastName;
            existingPatient.SecondLastName = patient.SecondLastName;
            existingPatient.Dui = patient.Dui;
            existingPatient.DateOfBirth = patient.DateOfBirth;
            existingPatient.Gender = patient.Gender;
            existingPatient.Address = patient.Address;
            existingPatient.Phone = patient.Phone;
            existingPatient.Email = patient.Email;
            existingPatient.IsActive = patient.IsActive;
            existingPatient.ModifiedAt = DateTime.Now;
            existingPatient.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatient(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient == null) return NotFound();

            patient.DeletedAt = DateTime.Now;
            patient.DeletedBy = GetUserId();
            patient.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
