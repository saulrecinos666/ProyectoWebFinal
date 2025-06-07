namespace ProyectoFinal.Models.Appointments.Dto
{
    public class AppointmentReportRequestDto
    {
        public DateTime? FechaInicio { get; set; }
        public DateTime? FechaFin { get; set; }
        public int? DoctorId { get; set; }
        public int? PatientId { get; set; }
        public int? InstitutionId { get; set; }
    }
}