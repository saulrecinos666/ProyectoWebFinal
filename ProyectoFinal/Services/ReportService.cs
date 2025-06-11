using ProyectoFinal.Models.Appointments.Dto;
using ProyectoFinal.Models.Doctors.Dto;
using ProyectoFinal.Models.Institutions.Dto;
using ProyectoFinal.Models.Patients.Dto;
using ProyectoFinal.Models.Specialties.Dto;
using ProyectoFinal.Models.Users.Dto;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;

namespace Parcial3.Services
{
    public class ReportService
    {
        public byte[] GenerarReporteCitas(List<ResponseAppointmentDto> citas)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);
            var logoPath = "wwwroot/img/logo.jpg";

            if (File.Exists(logoPath))
            {
                var imagenData = ImageDataFactory.Create(logoPath);
                var logo = new Image(imagenData)
                    .ScaleToFit(100, 100)
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                doc.Add(logo);
            }

            doc.Add(new Paragraph("Reporte de Citas Médicas")
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 3, 3, 4, 2 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            table.AddHeaderCell(new Cell().Add(new Paragraph("Paciente").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Doctor").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Institución").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha y Hora").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Estado").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));

            foreach (var cita in citas)
            {
                table.AddCell(cita.PatientName);
                table.AddCell(cita.DoctorName);
                table.AddCell(cita.InstitutionName);
                table.AddCell(cita.AppointmentDate.ToString("dd/MM/yyyy HH:mm"));
                table.AddCell(cita.Status.ToString()); 
            }

            doc.Add(table);

            doc.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            doc.Close();

            return ms.ToArray();
        }

        public byte[] GenerarReporteDoctores(List<ResponseDoctorDto> doctores)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);
            var logoPath = "wwwroot/img/logo.jpg";

            if (File.Exists(logoPath))
            {
                var imagenData = ImageDataFactory.Create(logoPath);
                var logo = new Image(imagenData)
                    .ScaleToFit(100, 100)
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                doc.Add(logo);
            }

            doc.Add(new Paragraph("Reporte de Doctores")
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 3, 2, 3, 3 })) 
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre Completo").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Email").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Teléfono").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Especialidad").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Institución").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));


            foreach (var doctor in doctores)
            {
                string nombreCompleto = $"{doctor.FirstName} {doctor.MiddleName} {doctor.LastName} {doctor.SecondLastName}".Trim();
                table.AddCell(nombreCompleto);
                table.AddCell(doctor.Email);
                table.AddCell(doctor.Phone ?? "N/A");
                table.AddCell(doctor.SpecialtyName);
                table.AddCell(doctor.InstitutionName);
            }

            doc.Add(table);

            doc.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            doc.Close();

            return ms.ToArray();
        }

        public byte[] GenerarReportePacientes(List<ResponsePatientDto> pacientes)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer); 
            var doc = new Document(pdf);
            var logoPath = "wwwroot/img/logo.jpg";

            if (File.Exists(logoPath))
            {
                var imagenData = ImageDataFactory.Create(logoPath);
                var logo = new Image(imagenData)
                    .ScaleToFit(100, 100)
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                doc.Add(logo);
            }

            doc.Add(new Paragraph("Reporte de Pacientes")
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 1, 2, 3 })) 
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre Completo").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha Nacimiento").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Género").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Teléfono").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Email").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));

            foreach (var paciente in pacientes)
            {
                string nombreCompleto = $"{paciente.FirstName} {paciente.MiddleName} {paciente.LastName} {paciente.SecondLastName}".Trim();
                table.AddCell(nombreCompleto);
                table.AddCell(paciente.DateOfBirth.ToString("dd/MM/yyyy"));
                table.AddCell(paciente.Gender);
                table.AddCell(paciente.Phone ?? "N/A");
                table.AddCell(paciente.Email);
            }

            doc.Add(table);

            doc.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            doc.Close();

            return ms.ToArray();
        }

        public byte[] GenerarReporteInstituciones(List<ResponseInstitutionDto> instituciones)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);
            var logoPath = "wwwroot/img/logo.jpg";

            if (File.Exists(logoPath))
            {
                var imagenData = ImageDataFactory.Create(logoPath);
                var logo = new Image(imagenData)
                    .ScaleToFit(100, 100)
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                doc.Add(logo);
            }

            doc.Add(new Paragraph("Reporte de Instituciones")
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 4, 2, 2 })) 
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            // Cabeceras de la tabla para Instituciones
            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Dirección").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Teléfono").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Email").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));

            foreach (var institucion in instituciones)
            {
                table.AddCell(institucion.Name);
                table.AddCell($"{institucion.Address}, {institucion.DistrictName}");
                table.AddCell(institucion.Phone);
                table.AddCell(institucion.Email);
            }

            doc.Add(table);

            doc.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            doc.Close();

            return ms.ToArray();
        }

        public byte[] GenerarReporteEspecialidades(List<ResponseSpecialtyDto> especialidades)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);
            var logoPath = "wwwroot/img/logo.jpg";

            if (File.Exists(logoPath))
            {
                var imagenData = ImageDataFactory.Create(logoPath);
                var logo = new Image(imagenData)
                    .ScaleToFit(100, 100)
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                doc.Add(logo);
            }

            doc.Add(new Paragraph("Reporte de Especialidades")
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 2, 5, 1 }))
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Descripción").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Activa").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));

            foreach (var especialidad in especialidades)
            {
                table.AddCell(especialidad.Name);
                table.AddCell(especialidad.Description ?? "Sin descripción");
                table.AddCell(especialidad.IsActive ? "Sí" : "No");
            }

            doc.Add(table);

            doc.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            doc.Close();

            return ms.ToArray();
        }

        public byte[] GenerarReporteUsuarios(List<ResponseUserDto> usuarios)
        {
            using var ms = new MemoryStream();
            var writer = new PdfWriter(ms);
            var pdf = new PdfDocument(writer);
            var doc = new Document(pdf);
            var logoPath = "wwwroot/img/logo.jpg";

            if (File.Exists(logoPath))
            {
                var imagenData = ImageDataFactory.Create(logoPath);
                var logo = new Image(imagenData)
                    .ScaleToFit(100, 100)
                    .SetHorizontalAlignment(HorizontalAlignment.LEFT);
                doc.Add(logo);
            }

            doc.Add(new Paragraph("Reporte de Usuarios del Sistema")
                .SetFontSize(20)
                .SetTextAlignment(TextAlignment.CENTER)
                .SetMarginBottom(20));

            var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 4, 1, 3 })) 
                .UseAllAvailableWidth()
                .SetMarginBottom(20)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            table.AddHeaderCell(new Cell().Add(new Paragraph("Nombre de Usuario").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Email").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Activo").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Fecha Creación").SetBackgroundColor(ColorConstants.DARK_GRAY).SetFontColor(ColorConstants.WHITE)));

            foreach (var user in usuarios)
            {
                table.AddCell(user.Username);
                table.AddCell(user.Email);
                table.AddCell(user.IsActive ? "Sí" : "No");
                table.AddCell(user.CreatedAt.ToString("dd/MM/yyyy HH:mm"));
            }

            doc.Add(table);

            doc.Add(new Paragraph($"Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.RIGHT)
                .SetMarginTop(30));

            doc.Close();

            return ms.ToArray();
        }
    }
}