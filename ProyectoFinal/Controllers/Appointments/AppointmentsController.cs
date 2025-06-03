using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Appointments;
using ProyectoFinal.Models.Appointments.Dto;
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
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Institution)
                .Select(a => new ResponseAppointmentDto
                {
                    AppointmentId = a.AppointmentId,
                    DoctorName = a.Doctor != null ? a.Doctor.FirstName + " " + a.Doctor.LastName : "NA",
                    PatientName = a.Patient != null ? a.Patient.FirstName + " " + a.Patient.LastName : "NA",
                    InstitutionName = a.Institution != null ? a.Institution.Name : "NA",
                    AppointmentDate = a.AppointmentDate,
                    Status = a.Status,
                    Notes = a.Notes,
                    CreatedBy = a.CreatedBy,
                    CreatedAt = a.CreatedAt,
                    ModifiedBy = a.ModifiedBy,
                    ModifiedAt = a.ModifiedAt
                })
                .ToListAsync();

            return Ok(appointments);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Institution)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            var appointmentDto = new ResponseAppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                DoctorName = appointment.Doctor != null ? appointment.Doctor.FirstName + " " + appointment.Doctor.LastName : "NA",
                PatientName = appointment.Patient != null ? appointment.Patient.FirstName + " " + appointment.Patient.LastName : "NA",
                InstitutionName = appointment.Institution != null ? appointment.Institution.Name : "NA",
                AppointmentDate = appointment.AppointmentDate,
                Status = appointment.Status,
                Notes = appointment.Notes,
                CreatedBy = appointment.CreatedBy,
                CreatedAt = appointment.CreatedAt,
                ModifiedBy = appointment.ModifiedBy,
                ModifiedAt = appointment.ModifiedAt
            };

            return Ok(appointmentDto);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto appointmentData)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var appointment = new Appointment
            {
                DoctorId = appointmentData.DoctorId,
                PatientId = appointmentData.PatientId,
                InstitutionId = appointmentData.InstitutionId,
                AppointmentDate = appointmentData.AppointmentDate,
                Status = appointmentData.Status,
                Notes = appointmentData.Notes,
                IsActive = true,
                CreatedBy = GetUserId(),
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var createdAppointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Institution)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointment.AppointmentId);

            var responseDto = new ResponseAppointmentDto
            {
                AppointmentId = createdAppointment.AppointmentId,
                AppointmentDate = createdAppointment.AppointmentDate,
                Status = createdAppointment.Status,
                Notes = createdAppointment.Notes,
                DoctorName = createdAppointment.Doctor != null ? createdAppointment.Doctor.FirstName + " " + createdAppointment.Doctor.LastName : "NA",
                PatientName = createdAppointment.Patient != null ? createdAppointment.Patient.FirstName + " " + createdAppointment.Patient.LastName : "NA",
                InstitutionName = createdAppointment.Institution != null ? createdAppointment.Institution.Name : "NA",
                CreatedBy = createdAppointment.CreatedBy,
                CreatedAt = createdAppointment.CreatedAt
            };

            return CreatedAtAction(nameof(GetAppointmentById), new { id = responseDto.AppointmentId }, responseDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDto updateAppointmentDto)
        {
            var existingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (existingAppointment == null) return NotFound("Cita no encontrada");

            existingAppointment.AppointmentDate = updateAppointmentDto.AppointmentDate;
            existingAppointment.Status = updateAppointmentDto.Status;
            existingAppointment.Notes = updateAppointmentDto.Notes;
            existingAppointment.DoctorId = updateAppointmentDto.DoctorId;
            existingAppointment.PatientId = updateAppointmentDto.PatientId;
            existingAppointment.InstitutionId = updateAppointmentDto.InstitutionId;
            existingAppointment.ModifiedAt = DateTime.Now;
            existingAppointment.ModifiedBy = GetUserId();

            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPatch("{id}/desactivate")]
        public async Task<IActionResult> DesactivateAppointmen(int id)
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            appointment.DeletedAt = DateTime.Now;
            appointment.DeletedBy = GetUserId();
            appointment.IsActive = false;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
