document.addEventListener('DOMContentLoaded', function () {
    // --- URLs Y REFERENCIAS A ELEMENTOS ---
    const isLocalhost = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";
    const API_BASE_URL = isLocalhost
        ? "http://localhost:5278/api"
        : "https://b4ndxm8brf.us-east-2.awsapprunner.com/api";
    const API_APPOINTMENTS_URL = `${API_BASE_URL}/appointment`;
    const API_DOCTORS_URL = `${API_BASE_URL}/doctor`;
    const API_PATIENTS_URL = `${API_BASE_URL}/patient`;
    const API_INSTITUTIONS_URL = `${API_BASE_URL}/institution`;

    const appointmentsTableBody = document.querySelector('#appointmentsTable tbody');
    const createAppointmentModalElement = document.getElementById('createAppointmentModal');
    const editAppointmentModalElement = document.getElementById('editAppointmentModal');
    const detailsAppointmentModalElement = document.getElementById('detailsAppointmentModal');
    const btnOpenCreateModal = document.getElementById('btnOpenCreateModal');
    const createAppointmentForm = document.getElementById('createAppointmentForm');
    const editAppointmentForm = document.getElementById('editAppointmentForm');

    // --- FUNCIONES AUXILIARES ---
    function getAuthHeaders() {
        const token = localStorage.getItem('jwtToken');
        if (!token) {
            showToast("Error de Autenticación", "Sesión no encontrada. Redirigiendo al login.", false);
            setTimeout(() => window.location.href = '/Home/Login', 1500);
            return null;
        }
        return { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' };
    }

    function showToast(header, body, isSuccess = true) {
        const toastElement = document.getElementById('liveToast');
        if (!toastElement) { alert(`${header}: ${body}`); return; }
        const toastHeaderElement = document.getElementById('toastHeader');
        const toastBodyElement = document.getElementById('toastBody');
        const toastHeaderDiv = toastElement.querySelector('.toast-header');
        toastHeaderElement.textContent = header;
        toastBodyElement.textContent = body;
        toastHeaderDiv.classList.remove('bg-danger', 'bg-success', 'text-white');
        toastHeaderDiv.classList.add(isSuccess ? 'bg-success' : 'bg-danger', 'text-white');
        const toast = new bootstrap.Toast(toastElement);
        toast.show();
    }

    function formatDate(dateString) {
        if (!dateString) return 'N/A';
        return new Date(dateString).toLocaleString('es-SV', { dateStyle: 'long', timeStyle: 'short' });
    }

    const statusMap = { 0: "Programada", 1: "Confirmada", 2: "Completada", 3: "Cancelada" };
    function getStatusText(status) { return statusMap[status] || 'Desconocido'; }

    function getBadgeClass(status) {
        const statusText = getStatusText(status).toLowerCase();
        switch (statusText) {
            case 'programada': return 'bg-primary';
            case 'confirmada': return 'bg-info text-dark';
            case 'cancelada': return 'bg-danger';
            case 'completada': return 'bg-success';
            default: return 'bg-secondary';
        }
    }

    function getUserInfoFromToken() {
        const token = localStorage.getItem('jwtToken');
        if (!token) return null;
        try {
            const payload = JSON.parse(atob(token.split('.')[1]));
            const roles = payload.role;
            return { roles: Array.isArray(roles) ? roles : [roles] };
        } catch (e) {
            console.error("Error al decodificar JWT:", e);
            return null;
        }
    }

    async function handleApiRequest(url, options = {}) {
        const headers = getAuthHeaders();
        if (!headers) throw new Error("No autenticado.");
        options.headers = headers;

        const response = await fetch(url, options);
        if (!response.ok) {
            if (response.status === 401) throw new Error('Sesión expirada. Por favor, inicie sesión de nuevo.');
            if (response.status === 403) throw new Error('No tiene permiso para realizar esta acción.');
            if (response.status === 404) throw new Error('El recurso solicitado no fue encontrado.');
            const errorData = await response.json().catch(() => ({}));
            throw new Error(errorData.message || `Error del servidor: ${response.status}`);
        }
        return response.status === 204 ? null : response.json();
    }

    async function fetchAndPopulateSelect(url, selectElement, idProperty, nameProperties) {
        selectElement.innerHTML = '<option value="">Cargando...</option>';
        try {
            const data = await handleApiRequest(url);
            selectElement.innerHTML = '<option value="">-- Seleccione --</option>';
            data.forEach(item => {
                const option = document.createElement('option');
                option.value = item[idProperty];
                option.textContent = nameProperties.map(prop => item[prop]).filter(Boolean).join(' ');
                selectElement.appendChild(option);
            });
        } catch (error) {
            selectElement.innerHTML = '<option value="">Error al cargar</option>';
            console.error(`Error cargando desde ${url}:`, error);
        }
    }

    // --- LÓGICA PRINCIPAL ---

    async function loadAppointments() {
        if (!appointmentsTableBody) return;
        appointmentsTableBody.innerHTML = '<tr><td colspan="8" class="text-center"><div class="spinner-border text-primary" role="status"></div></td></tr>';
        try {
            const appointments = await handleApiRequest(API_APPOINTMENTS_URL);
            displayAppointments(appointments);
        } catch (error) {
            appointmentsTableBody.innerHTML = `<tr><td colspan="8" class="text-center text-danger"><b>Error:</b> ${error.message}</td></tr>`;
        }
    }

    function displayAppointments(appointments) {
        appointmentsTableBody.innerHTML = '';
        if (!appointments || appointments.length === 0) {
            appointmentsTableBody.innerHTML = '<tr><td colspan="8" class="text-center">No hay citas registradas.</td></tr>';
            return;
        }
        appointments.forEach(appointment => {
            const row = appointmentsTableBody.insertRow();
            row.innerHTML = `
                <td>${appointment.appointmentId}</td>
                <td>${appointment.doctorName || 'N/A'}</td>
                <td>${appointment.patientName || 'N/A'}</td>
                <td>${appointment.institutionName || 'N/A'}</td>
                <td>${formatDate(appointment.appointmentDate)}</td>
                <td><span class="badge ${getBadgeClass(appointment.status)}">${getStatusText(appointment.status)}</span></td>
                <td>${appointment.notes || 'N/A'}</td>
                <td class="text-nowrap">
                    <button class="btn btn-info btn-sm me-1 view-btn" data-id="${appointment.appointmentId}" title="Ver Detalles"><i class="fas fa-eye"></i></button>
                    <button class="btn btn-warning btn-sm me-1 edit-btn" data-id="${appointment.appointmentId}" title="Editar Cita"><i class="fas fa-edit"></i></button>
                    <button class="btn btn-danger btn-sm desactivate-btn" data-id="${appointment.appointmentId}" title="Cancelar Cita"><i class="fas fa-times-circle"></i></button>
                </td>`;
        });
    }

    // --- MANEJO DE EVENTOS Y MODALES ---

    btnOpenCreateModal.addEventListener('click', async () => {
        createAppointmentForm.reset();
        const createStatusContainer = document.getElementById('createStatusContainer');
        const userInfo = getUserInfoFromToken();
        if (userInfo && userInfo.roles.includes('Usuario Estándar')) {
            if (createStatusContainer) createStatusContainer.style.display = 'none';
        } else {
            if (createStatusContainer) createStatusContainer.style.display = 'block';
        }
        await Promise.all([
            fetchAndPopulateSelect(API_DOCTORS_URL, document.getElementById('createDoctorSelect'), 'doctorId', ['firstName', 'lastName']),
            fetchAndPopulateSelect(API_PATIENTS_URL, document.getElementById('createPatientSelect'), 'patientId', ['firstName', 'lastName']),
            fetchAndPopulateSelect(API_INSTITUTIONS_URL, document.getElementById('createInstitutionSelect'), 'institutionId', ['name'])
        ]);
        new bootstrap.Modal(createAppointmentModalElement).show();
    });

    appointmentsTableBody.addEventListener('click', async function (event) {
        const button = event.target.closest('button');
        if (!button) return;
        const id = button.dataset.id;
        if (!id) return;

        if (button.classList.contains('view-btn')) {
            try {
                const appointment = await handleApiRequest(`${API_APPOINTMENTS_URL}/${id}`);
                const modalBody = document.getElementById('detailsAppointmentModalBody');
                modalBody.innerHTML = `<p><strong>ID:</strong> ${appointment.appointmentId}</p>
                                       <p><strong>Doctor:</strong> ${appointment.doctorName}</p>
                                       <p><strong>Paciente:</strong> ${appointment.patientName}</p>
                                       <p><strong>Institución:</strong> ${appointment.institutionName}</p>
                                       <p><strong>Fecha:</strong> ${formatDate(appointment.appointmentDate)}</p>
                                       <p><strong>Estado:</strong> <span class="badge ${getBadgeClass(appointment.status)}">${getStatusText(appointment.status)}</span></p>
                                       <p><strong>Notas:</strong> ${appointment.notes || 'N/A'}</p>`;
                new bootstrap.Modal(detailsAppointmentModalElement).show();
            } catch (error) {
                showToast("Error", error.message, false);
            }

        } else if (button.classList.contains('edit-btn')) {
            try {
                editAppointmentForm.reset();
                document.getElementById('editAppointmentId').value = id;
                // Mostrar un spinner mientras se carga todo
                document.getElementById('editDoctorSelect').innerHTML = '<option>Cargando...</option>';

                const modal = new bootstrap.Modal(editAppointmentModalElement);
                modal.show();

                await Promise.all([
                    fetchAndPopulateSelect(API_DOCTORS_URL, document.getElementById('editDoctorSelect'), 'doctorId', ['firstName', 'lastName']),
                    fetchAndPopulateSelect(API_PATIENTS_URL, document.getElementById('editPatientSelect'), 'patientId', ['firstName', 'lastName']),
                    fetchAndPopulateSelect(API_INSTITUTIONS_URL, document.getElementById('editInstitutionSelect'), 'institutionId', ['name'])
                ]);

                const appointment = await handleApiRequest(`${API_APPOINTMENTS_URL}/${id}`);

                document.getElementById('editDoctorSelect').value = appointment.doctorId;
                document.getElementById('editPatientSelect').value = appointment.patientId;
                document.getElementById('editInstitutionSelect').value = appointment.institutionId;
                document.getElementById('editAppointmentDate').value = new Date(new Date(appointment.appointmentDate).getTime() - (new Date().getTimezoneOffset() * 60000)).toISOString().slice(0, 16);
                document.getElementById('editStatus').value = appointment.status;
                document.getElementById('editNotes').value = appointment.notes || '';

            } catch (error) {
                bootstrap.Modal.getInstance(editAppointmentModalElement)?.hide();
                showToast("Error", `No se pudo abrir el editor: ${error.message}`, false);
            }

        } else if (button.classList.contains('desactivate-btn')) {
            if (confirm('¿Está seguro de que desea cancelar esta cita?')) {
                try {
                    await handleApiRequest(`${API_APPOINTMENTS_URL}/${id}/desactivate`, { method: 'PATCH' });
                    showToast("Éxito", "Cita cancelada correctamente.", true);
                    loadAppointments();
                } catch (error) {
                    showToast("Error", `Error al cancelar cita: ${error.message}`, false);
                }
            }
        }
    });

    createAppointmentForm.addEventListener('submit', async function (e) {
        e.preventDefault();
        const userInfo = getUserInfoFromToken();
        const patientData = {
            doctorId: parseInt(document.getElementById('createDoctorSelect').value),
            patientId: parseInt(document.getElementById('createPatientSelect').value),
            institutionId: parseInt(document.getElementById('createInstitutionSelect').value),
            appointmentDate: document.getElementById('createAppointmentDate').value,
            status: (userInfo && userInfo.roles.includes('Usuario Estándar')) ? 0 : parseInt(document.getElementById('createStatus').value),
            notes: document.getElementById('createNotes').value || null
        };
        if (isNaN(patientData.doctorId) || isNaN(patientData.patientId) || isNaN(patientData.institutionId) || !patientData.appointmentDate) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }
        try {
            await handleApiRequest(API_APPOINTMENTS_URL, { method: 'POST', body: JSON.stringify(patientData) });
            showToast("Éxito", "Cita creada correctamente.", true);
            bootstrap.Modal.getInstance(createAppointmentModalElement)?.hide();
            loadAppointments();
        } catch (error) {
            showToast("Error de Creación", error.message, false);
        }
    });

    editAppointmentForm.addEventListener('submit', async function (e) {
        e.preventDefault();
        const id = document.getElementById('editAppointmentId').value;
        const updatedAppointment = {
            appointmentId: parseInt(id),
            doctorId: parseInt(document.getElementById('editDoctorSelect').value),
            patientId: parseInt(document.getElementById('editPatientSelect').value),
            institutionId: parseInt(document.getElementById('editInstitutionSelect').value),
            appointmentDate: document.getElementById('editAppointmentDate').value,
            status: parseInt(document.getElementById('editStatus').value),
            notes: document.getElementById('editNotes').value || null
        };
        if (isNaN(updatedAppointment.appointmentId) || isNaN(updatedAppointment.doctorId) || isNaN(updatedAppointment.patientId) || isNaN(updatedAppointment.institutionId) || !updatedAppointment.appointmentDate) {
            showToast("Error de Validación", "Todos los campos son requeridos.", false);
            return;
        }
        try {
            await handleApiRequest(`${API_APPOINTMENTS_URL}/${id}`, { method: 'PUT', body: JSON.stringify(updatedAppointment) });
            showToast("Éxito", "Cita actualizada correctamente.", true);
            bootstrap.Modal.getInstance(editAppointmentModalElement)?.hide();
            loadAppointments();
        } catch (error) {
            showToast("Error de Actualización", error.message, false);
        }
    });

    loadAppointments();
});
