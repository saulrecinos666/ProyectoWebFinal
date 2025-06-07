using Microsoft.AspNetCore.Authorization; // Agregado
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProyectoFinal.Models.Appointments.Dto;
using ProyectoFinal.Models.Doctors.Dto;
using ProyectoFinal.Models.Institutions.Dto;
using ProyectoFinal.Models.Patients.Dto;
using ProyectoFinal.Models.Specialties.Dto;
using ProyectoFinal.Models.Users.Dto;
using Parcial3.Services; // Tu ReporteService. Verifica el namespace real si lo cambiaste.
using ProyectoFinal.Models; // Para DbCitasMedicasContext y otras entidades de EF Core
using ProyectoFinal.Models.Base; // Para AppointmentStatus, si está aquí
using System.Linq;
using System.Threading.Tasks;

// EL NAMESPACE DEBE COINCIDIR CON LA UBICACIÓN DE TUS OTROS CONTROLADORES UI
namespace ProyectoFinal.Controllers.Reports // Si es donde agrupas los UI controllers de reportes
// O si los tienes en una carpeta "UI" general, podría ser:
// namespace ProyectoFinal.Controllers.UI
{
    // El nombre de la clase debe ser ReportUIController para que coincida con la URL /ReportUI
    [Authorize(Policy = "CanGenerateReports")]
    public class ReportUIController : Controller
    {
        private readonly DbCitasMedicasContext _context;
        private readonly ReportService _reporteService;

        public ReportUIController(DbCitasMedicasContext context, ReportService reporteService)
        {
            _context = context;
            _reporteService = reporteService;
        }

        [Authorize] // Agrega la autorización como en tus otros controladores UI
        public async Task<IActionResult> Index()
        {
            ViewBag.Doctors = await _context.Doctors.Where(d => d.IsActive).Select(d => new { d.DoctorId, FullName = d.FirstName + " " + d.LastName }).ToListAsync();
            ViewBag.Patients = await _context.Patients.Where(p => p.IsActive).Select(p => new { p.PatientId, FullName = p.FirstName + " " + p.LastName }).ToListAsync();
            ViewBag.Institutions = await _context.Institutions.Where(i => i.IsActive).Select(i => new { i.InstitutionId, i.Name }).ToListAsync();
            ViewBag.Specialties = await _context.Specialties.Where(s => s.IsActive).Select(s => new { s.SpecialtyId, s.Name }).ToListAsync();

            return View();
        }

        // --- ACCIONES PARA GENERAR REPORTES ESPECÍFICOS ---

        [HttpGet]
        public async Task<IActionResult> GenerarReporteCitas([FromQuery] AppointmentReportRequestDto request)
        {
            // ... (El resto del código de la acción GenerarReporteCitas es el mismo) ...
            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Institution)
                .AsQueryable();

            if (request.FechaInicio.HasValue)
            {
                query = query.Where(a => a.AppointmentDate >= request.FechaInicio.Value);
            }
            if (request.FechaFin.HasValue)
            {
                query = query.Where(a => a.AppointmentDate <= request.FechaFin.Value.AddDays(1).AddTicks(-1));
            }
            if (request.DoctorId.HasValue)
            {
                query = query.Where(a => a.DoctorId == request.DoctorId.Value);
            }
            if (request.PatientId.HasValue)
            {
                query = query.Where(a => a.PatientId == request.PatientId.Value);
            }
            if (request.InstitutionId.HasValue)
            {
                query = query.Where(a => a.InstitutionId == request.InstitutionId.Value);
            }

            query = query.OrderBy(a => a.AppointmentDate);
            var citasDto = await query.Select(a => new ResponseAppointmentDto
            {
                AppointmentId = a.AppointmentId,
                DoctorName = a.Doctor.FirstName + " " + a.Doctor.LastName,
                PatientName = a.Patient.FirstName + " " + a.Patient.LastName,
                InstitutionName = a.Institution.Name,
                AppointmentDate = a.AppointmentDate,
                Status = a.Status,
                Notes = a.Notes,
                CreatedAt = a.CreatedAt,
                CreatedBy = a.CreatedBy,
                ModifiedAt = a.ModifiedAt,
                ModifiedBy = a.ModifiedBy
            }).ToListAsync();
            byte[] pdfBytes = _reporteService.GenerarReporteCitas(citasDto);
            return File(pdfBytes, "application/pdf", "ReporteDeCitas.pdf");
        }

        // Las demás acciones GenerarReporteDoctores, GenerarReportePacientes, etc.,
        // van aquí exactamente como te las di en el mensaje anterior.
        // Asegúrate de agregar el [Authorize] a cada una si quieres que estén protegidas.

        [HttpGet]
        public async Task<IActionResult> GenerarReporteDoctores()
        {
            var doctores = await _context.Doctors
               .Include(d => d.Specialty)
               .Include(d => d.Institution)
               .OrderBy(d => d.LastName)
               .Select(d => new ResponseDoctorDto
               {
                   DoctorId = d.DoctorId,
                   FirstName = d.FirstName,
                   MiddleName = d.MiddleName,
                   LastName = d.LastName,
                   SecondLastName = d.SecondLastName,
                   Dui = d.Dui,
                   Email = d.Email,
                   Phone = d.Phone,
                   SpecialtyName = d.Specialty.Name,
                   InstitutionName = d.Institution.Name,
                   IsActive = d.IsActive,
                   CreatedAt = d.CreatedAt,
                   CreatedBy = d.CreatedBy,
                   ModifiedAt = d.ModifiedAt,
                   ModifiedBy = d.ModifiedBy
               })
               .ToListAsync();

            byte[] pdfBytes = _reporteService.GenerarReporteDoctores(doctores);

            return File(pdfBytes, "application/pdf", "ReporteDeDoctores.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GenerarReportePacientes()
        {
            var pacientes = await _context.Patients
                .OrderBy(p => p.LastName)
                .Select(p => new ResponsePatientDto
                {
                    PatientId = p.PatientId,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    SecondLastName = p.SecondLastName,
                    Dui = p.Dui,
                    DateOfBirth = p.DateOfBirth,
                    Gender = p.Gender,
                    Address = p.Address,
                    Email = p.Email,
                    Phone = p.Phone,
                    UserName = p.User != null ? p.User.Username : "N/A", // Si Patient tiene un User relacionado
                    IsActive = p.IsActive,
                    CreatedAt = p.CreatedAt,
                    CreatedBy = p.CreatedBy,
                    ModifiedAt = p.ModifiedAt,
                    ModifiedBy = p.ModifiedBy
                })
                .ToListAsync();

            byte[] pdfBytes = _reporteService.GenerarReportePacientes(pacientes);

            return File(pdfBytes, "application/pdf", "ReporteDePacientes.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GenerarReporteInstituciones()
        {
            var instituciones = await _context.Institutions
                .OrderBy(i => i.Name)
                .Select(i => new ResponseInstitutionDto
                {
                    InstitutionId = i.InstitutionId,
                    Name = i.Name,
                    Address = i.Address,
                    DistrictCode = i.DistrictCode,
                    DistrictName = "N/A", // Reemplaza si no hay una propiedad de navegación
                    Phone = i.Phone,
                    Email = i.Email,
                    IsActive = i.IsActive,
                    CreatedAt = i.CreatedAt,
                    CreatedBy = i.CreatedBy,
                    ModifiedAt = i.ModifiedAt,
                    ModifiedBy = i.ModifiedBy
                })
                .ToListAsync();

            byte[] pdfBytes = _reporteService.GenerarReporteInstituciones(instituciones);

            return File(pdfBytes, "application/pdf", "ReporteDeInstituciones.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GenerarReporteEspecialidades()
        {
            var especialidades = await _context.Specialties
                .OrderBy(s => s.Name)
                .Select(s => new ResponseSpecialtyDto
                {
                    SpecialtyId = s.SpecialtyId,
                    Name = s.Name,
                    Description = s.Description,
                    IsActive = s.IsActive,
                    CreatedAt = s.CreatedAt,
                    CreatedBy = s.CreatedBy,
                    ModifiedAt = s.ModifiedAt,
                    ModifiedBy = s.ModifiedBy
                })
                .ToListAsync();

            byte[] pdfBytes = _reporteService.GenerarReporteEspecialidades(especialidades);

            return File(pdfBytes, "application/pdf", "ReporteDeEspecialidades.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> GenerarReporteUsuarios()
        {
            var usuarios = await _context.Users
                .OrderBy(u => u.Username)
                .Select(u => new ResponseUserDto
                {
                    UserId = u.UserId,
                    Username = u.Username,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt,
                    CreatedBy = u.CreatedBy,
                    ModifiedAt = u.ModifiedAt,
                    ModifiedBy = u.ModifiedBy
                })
                .ToListAsync();

            byte[] pdfBytes = _reporteService.GenerarReporteUsuarios(usuarios);

            return File(pdfBytes, "application/pdf", "ReporteDeUsuarios.pdf");
        }
    }
}