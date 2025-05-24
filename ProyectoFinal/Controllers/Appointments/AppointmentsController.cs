using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Appointments;
using ProyectoFinal.Models.Base;

namespace ProyectoFinal.Controllers.Appointments
{
    [ApiController]
    [Route("api/appointment")]
    [Authorize]
    public class AppointmentsController : BaseController
    {
        private readonly DbCitasMedicasContext _context;

        public AppointmentsController(DbCitasMedicasContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            var appointments = await _context.Appointments.ToListAsync();
            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) { return NotFound(); }
            return Ok(appointment);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] Appointment appointment)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            appointment.IsActive = true;
            appointment.CreatedAt = DateTime.Now;
            appointment.CreatedBy = GetUserId();

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.AppointmentId }, appointment);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] Appointment appointment)
        {
            if (id != appointment.AppointmentId) return BadRequest();

            var existingAppointment = await _context.Appointments.FindAsync(id);
            if (existingAppointment == null) return NotFound("Cita no encontrada");

            existingAppointment.AppointmentDate = appointment.AppointmentDate;
            existingAppointment.Status = appointment.Status;
            existingAppointment.Notes = appointment.Notes;
            existingAppointment.IsActive = appointment.IsActive;
            existingAppointment.UserId = appointment.UserId;
            existingAppointment.DoctorId = appointment.DoctorId;
            existingAppointment.PatientId = appointment.PatientId;
            existingAppointment.InstitutionId = appointment.InstitutionId;
            existingAppointment.ModifiedAt = DateTime.Now;
            existingAppointment.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointmen(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            appointment.DeletedAt = DateTime.Now;
            appointment.DeletedBy = GetUserId();
            appointment.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
