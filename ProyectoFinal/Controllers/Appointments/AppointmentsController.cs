using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Controllers.Base;
using ProyectoFinal.Models.Appointments;
using ProyectoFinal.Models.Appointments.Dto;
using ProyectoFinal.Models.Base;
using System.Security.Claims;

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

        // --- MÉTODO GetAllAppointments CORREGIDO ---
        [HttpGet]
        public async Task<IActionResult> GetAllAppointments()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Sesión inválida.");
            }

            var query = _context.Appointments.AsQueryable();

            // Si el usuario NO tiene permiso para gestionar todas las citas...
            if (!User.HasClaim("Permission", "can_manage_appointments"))
            {
                // ...filtramos las citas usando el UserId directo en la tabla de citas.
                query = query.Where(a => a.UserId == userId);
            }

            var appointments = await query
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
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return Ok(appointments);
        }

        // --- MÉTODO GetAppointmentById CON SEGURIDAD CORREGIDA ---
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAppointmentById(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Institution)
                .FirstOrDefaultAsync(a => a.AppointmentId == id);

            if (appointment == null) return NotFound();

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            int.TryParse(userIdStr, out var currentUserId);

            // Verificamos si es admin O si la cita le pertenece directamente.
            if (!User.HasClaim("Permission", "can_manage_appointments") && appointment.UserId != currentUserId)
            {
                return Forbid();
            }

            var appointmentDto = new ResponseAppointmentDto
            {
                AppointmentId = appointment.AppointmentId,
                DoctorId = appointment.DoctorId,
                PatientId = appointment.PatientId,
                InstitutionId = appointment.InstitutionId,
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

        // --- MÉTODO CreateAppointment CON LÓGICA DE STATUS CORREGIDA ---
        [HttpPost]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto appointmentData)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var currentUserId = GetUserId();

            if (User.IsInRole("Usuario Estándar"))
            {
                appointmentData.Status = 0; // 0 = Programada
            }

            var appointment = new Appointment
            {
                UserId = currentUserId, // Asignamos el usuario logueado
                DoctorId = appointmentData.DoctorId,
                PatientId = appointmentData.PatientId,
                InstitutionId = appointmentData.InstitutionId,
                AppointmentDate = appointmentData.AppointmentDate,
                Status = appointmentData.Status,
                Notes = appointmentData.Notes,
                IsActive = true,
                CreatedBy = currentUserId,
                CreatedAt = DateTime.UtcNow
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

        // --- MÉTODO UpdateAppointment CON SEGURIDAD AÑADIDA ---
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, [FromBody] UpdateAppointmentDto updateAppointmentDto)
        {
            var existingAppointment = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == id);
            if (existingAppointment == null) return NotFound("Cita no encontrada");

            var currentUserId = GetUserId();
            if (!User.HasClaim("Permission", "can_manage_appointments") && existingAppointment.UserId != currentUserId)
            {
                return Forbid();
            }

            // Aplicar cambios
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

        // --- MÉTODO DesactivateAppointmen CON SEGURIDAD AÑADIDA ---
        [HttpPatch("{id}/desactivate")]
        public async Task<IActionResult> DesactivateAppointmen(int id)
        {
            var appointment = await _context.Appointments.FirstOrDefaultAsync(a => a.AppointmentId == id);
            if (appointment == null) return NotFound();

            var currentUserId = GetUserId();
            if (!User.HasClaim("Permission", "can_manage_appointments") && appointment.UserId != currentUserId)
            {
                return Forbid();
            }

            appointment.IsActive = false;
            appointment.DeletedAt = DateTime.UtcNow;
            appointment.DeletedBy = currentUserId;

            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
