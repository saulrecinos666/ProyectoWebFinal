﻿namespace ProyectoFinal.Models.Appointments.Dto
{
    public class UpdateAppointmentDto
    {
        public int UserId { get; set; }
        //public string UserName { get; set; } = null!;
        public int DoctorId { get; set; }
        //public string DoctorName { get; set; } = null!;
        public int PatientId { get; set; }
        //public string PatientName { get; set; } = null!;
        public int InstitutionId { get; set; }
        //public string InstitutionName { get; set; } = null!;
        public DateTime AppointmentDate { get; set; }
        public AppointmentStatus Status { get; set; }
        public string? Notes { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedAt { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedAt { get; set; }
    }
}
