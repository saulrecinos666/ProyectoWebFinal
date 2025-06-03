document.addEventListener('DOMContentLoaded', function () {
    // Definición de constantes para las URLs de la API
    const API_BASE_URL = "http://localhost:5278/api"; // Asegúrate de que esta URL sea correcta para tu backend
    const API_APPOINTMENTS_URL = `${API_BASE_URL}/appointment`;
    const API_DOCTORS_URL = `${API_BASE_URL}/doctor`;
    const API_PATIENTS_URL = `${API_BASE_URL}/patient`;
    const API_INSTITUTIONS_URL = `${API_BASE_URL}/institution`;

    const token = localStorage.getItem('jwtToken');

    // Referencias a elementos del DOM
    const appointmentsTableBody = document.querySelector('#appointmentsTable tbody');
    const createAppointmentForm = document.getElementById('createAppointmentForm');
    const editAppointmentForm = document.getElementById('editAppointmentForm');

    // Referencias a los selects de los modales
    const createDoctorSelect = document.getElementById('createDoctorSelect');
    const createPatientSelect = document.getElementById('createPatientSelect');
    const createInstitutionSelect = document.getElementById('createInstitutionSelect');
    const createStatusSelect = document.getElementById('createStatus');

    const editDoctorSelect = document.getElementById('editDoctorSelect');
    const editPatientSelect = document.getElementById('editPatientSelect');
    const editInstitutionSelect = document.getElementById('editInstitutionSelect');
    const editStatusSelect = document.getElementById('editStatus');

    // Referencias a los modales de Bootstrap
    const createAppointmentModal = new bootstrap.Modal(document.getElementById('createAppointmentModal'));
    const editAppointmentModal = new bootstrap.Modal(document.getElementById('editAppointmentModal'));
    const detailsAppointmentModal = new bootstrap.Modal(document.getElementById('detailsAppointmentModal'));

    // --- Funciones Auxiliares ---

    // Función para mostrar Toast de Bootstrap
    function showToast(header, body, isSuccess = true) {
        const toastElement = document.getElementById('liveToast');
        const toastHeaderElement = document.getElementById('toastHeader');
        const toastBodyElement = document.getElementById('toastBody');

        toastHeaderElement.textContent = header;
        toastBodyElement.textContent = body;

        const toastHeaderDiv = toastElement.querySelector('.toast-header');
        // Quitar clases previas para evitar acumulación
        toastHeaderDiv.classList.remove('bg-danger', 'bg-success', 'text-white');
        if (isSuccess) {
            toastHeaderDiv.classList.add('bg-success', 'text-white');
        } else {
            toastHeaderDiv.classList.add('bg-danger', 'text-white');
        }

        const toast = new bootstrap.Toast(toastElement);
        toast.show();
    }

    // Función auxiliar para formatear fechas
    function formatDate(dateString) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        return date.toLocaleString(); // Formato local de fecha y hora
    }

    // Función para obtener los headers de autorización
    function getAuthHeaders() {
        if (!token) {
            console.error("Error: No se encontró el token JWT en localStorage.");
            showToast("Error de Autenticación", "No tiene token JWT. Por favor, inicie sesión.", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000); // Redirigir al login
            return {};
        }
        return {
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        };
    }

    // Mapeo de estados numéricos a cadenas de texto (si tu API devuelve números)
    const statusMap = {
        0: "Programada",
        1: "Confirmada",
        2: "Completada",
        3: "Cancelada"
    };

    // Función para obtener la clase del badge según el estado (numérico o string)
    function getBadgeClass(status) {
        let statusString = String(status); // Convertir a string para usar toLowerCase()

        // Si es un número, convertir a su representación de texto primero
        if (statusMap[statusString] !== undefined) {
            statusString = statusMap[statusString];
        }

        switch (statusString.toLowerCase()) {
            case 'programada': return 'bg-warning text-dark';
            case 'confirmada': return 'bg-primary';
            case 'cancelada': return 'bg-danger';
            case 'completada': return 'bg-success';
            default: return 'bg-secondary';
        }
    }

    // Función para obtener el texto del estado (útil para la tabla y detalles)
    function getStatusText(status) {
        // Si ya es un string, lo devuelve directamente
        if (typeof status === 'string') {
            return status;
        }
        // Si es un número, lo busca en el mapa
        return statusMap[status] || 'Desconocido';
    }


    // --- Funciones para Cargar Datos en Selects ---

    async function fetchAndPopulateSelect(url, selectElement, idProperty, nameProperties) {
        if (!token) {
            showToast("Error de Autenticación", "No tiene token JWT. Por favor, inicie sesión.", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000);
            return;
        }

        selectElement.innerHTML = '<option value="">Cargando...</option>'; // Mensaje de carga

        try {
            const response = await fetch(url, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                showToast("Error de Carga", `No se pudieron cargar opciones desde ${url}: ${errorData.message || JSON.stringify(errorData)}`, false);
                console.error(`Error al cargar datos desde ${url}:`, response.status, errorData);
                selectElement.innerHTML = '<option value="">Error al cargar</option>';
                return;
            }

            const data = await response.json();
            selectElement.innerHTML = '<option value="">Seleccione...</option>'; // Opción por defecto después de cargar

            data.forEach(item => {
                const option = document.createElement('option');
                option.value = item[idProperty];
                option.textContent = nameProperties.map(prop => item[prop]).filter(Boolean).join(' ');
                selectElement.appendChild(option);
            });

        } catch (error) {
            console.error(`Error de red o CORS al cargar datos desde ${url}:`, error);
            showToast("Error de Red", `No se pudo conectar al servidor para cargar datos de ${url}.`, false);
            selectElement.innerHTML = '<option value="">Error de conexión</option>';
        }
    }

    // Cargar todos los selects al abrir el modal de creación
    async function loadCreateModalSelects() {
        createDoctorSelect.value = '';
        createPatientSelect.value = '';
        createInstitutionSelect.value = '';
        createStatusSelect.value = '0'; // Default a 'Programada'

        createAppointmentForm.reset();

        await Promise.all([
            fetchAndPopulateSelect(API_DOCTORS_URL, createDoctorSelect, 'doctorId', ['firstName', 'lastName']),
            fetchAndPopulateSelect(API_PATIENTS_URL, createPatientSelect, 'patientId', ['firstName', 'lastName']),
            fetchAndPopulateSelect(API_INSTITUTIONS_URL, createInstitutionSelect, 'institutionId', ['name'])
        ]).catch(error => {
            console.error("Error al cargar selects del modal de creación:", error);
            showToast("Error", "No se pudieron cargar todas las opciones para crear una cita.", false);
        });
    }

    // Cargar todos los selects al abrir el modal de edición
    async function loadEditModalSelects() {
        await Promise.all([
            fetchAndPopulateSelect(API_DOCTORS_URL, editDoctorSelect, 'doctorId', ['firstName', 'lastName']),
            fetchAndPopulateSelect(API_PATIENTS_URL, editPatientSelect, 'patientId', ['firstName', 'lastName']),
            fetchAndPopulateSelect(API_INSTITUTIONS_URL, editInstitutionSelect, 'institutionId', ['name'])
        ]).catch(error => {
            console.error("Error al cargar selects del modal de edición:", error);
            showToast("Error", "No se pudieron cargar todas las opciones para editar una cita.", false);
        });
    }

    // --- Funciones CRUD de Citas ---

    // Cargar Citas en la tabla
    async function loadAppointments() {
        if (!token) {
            showToast("No Autorizado", "Su sesión no está iniciada. Redirigiendo al login...", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000);
            return;
        }

        appointmentsTableBody.innerHTML = '<tr><td colspan="8" class="text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"><span class="visually-hidden">Cargando...</span></div> Cargando citas...</td></tr>';
        try {
            const response = await fetch(API_APPOINTMENTS_URL, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'Error al cargar citas');
            }

            const appointments = await response.json();
            displayAppointments(appointments);
        } catch (error) {
            console.error('Error al cargar citas:', error);
            appointmentsTableBody.innerHTML = `<tr><td colspan="8" class="text-center text-danger">Error al cargar citas: ${error.message}</td></tr>`;
            showToast("Error de Carga", `Error al cargar citas: ${error.message}`, false);
        }
    }

    // Mostrar Citas en la tabla
    function displayAppointments(appointments) {
        appointmentsTableBody.innerHTML = ''; // Limpiar la tabla
        if (appointments.length === 0) {
            appointmentsTableBody.innerHTML = '<tr><td colspan="8" class="text-center">No hay citas registradas.</td></tr>';
            return;
        }

        appointments.forEach(appointment => {
            const row = appointmentsTableBody.insertRow();
            row.dataset.appointmentId = appointment.appointmentId; // Para fácil acceso al ID

            // Obtener el texto del estado para mostrarlo
            const appointmentStatusText = getStatusText(appointment.status);

            row.innerHTML = `
                <td>${appointment.appointmentId}</td>
                <td>${appointment.doctorName || 'N/A'}</td>
                <td>${appointment.patientName || 'N/A'}</td>
                <td>${appointment.institutionName || 'N/A'}</td>
                <td>${formatDate(appointment.appointmentDate)}</td>
                <td><span class="badge ${getBadgeClass(appointment.status)}">${appointmentStatusText}</span></td>
                <td>${appointment.notes || 'N/A'}</td>
                <td>
                    <button class="btn btn-info btn-sm me-1 view-btn" data-id="${appointment.appointmentId}" title="Ver Detalles"><i class="fas fa-eye"></i></button>
                    <button class="btn btn-warning btn-sm me-1 edit-btn" data-id="${appointment.appointmentId}" title="Editar Cita"><i class="fas fa-edit"></i></button>
                    ${appointmentStatusText.toLowerCase() !== 'cancelada' && appointmentStatusText.toLowerCase() !== 'completada' ?
                    `<button class="btn btn-danger btn-sm desactivate-btn" data-id="${appointment.appointmentId}" title="Cancelar Cita"><i class="fas fa-times-circle"></i></button>` : ''
                }
                </td>
            `;
        });
    }

    // Mostrar detalles de una cita en el modal
    async function showDetails(id) {
        try {
            const response = await fetch(`${API_APPOINTMENTS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudieron obtener los detalles de la cita.');
            }

            const appointment = await response.json();
            document.getElementById('detailsAppointmentId').textContent = appointment.appointmentId;
            document.getElementById('detailsDoctorName').textContent = appointment.doctorName || 'N/A';
            document.getElementById('detailsPatientName').textContent = appointment.patientName || 'N/A';
            document.getElementById('detailsInstitutionName').textContent = appointment.institutionName || 'N/A';
            document.getElementById('detailsAppointmentDate').textContent = formatDate(appointment.appointmentDate);
            // Usar getStatusText para mostrar el estado
            document.getElementById('detailsStatus').textContent = getStatusText(appointment.status);
            document.getElementById('detailsNotes').textContent = appointment.notes || 'N/A';
            document.getElementById('detailsCreatedBy').textContent = appointment.createdBy || 'N/A';
            document.getElementById('detailsCreatedAt').textContent = formatDate(appointment.createdAt);
            document.getElementById('detailsModifiedBy').textContent = appointment.modifiedBy || 'N/A';
            document.getElementById('detailsModifiedAt').textContent = formatDate(appointment.modifiedAt);

            detailsAppointmentModal.show();
        } catch (error) {
            console.error('Error al obtener detalles de la cita:', error);
            showToast("Error de Carga", `Error al obtener detalles: ${error.message}`, false);
        }
    }

    // Poblar el formulario de edición y mostrar el modal
    async function populateEditForm(id) {
        try {
            // Cargar los selects primero para que las opciones estén disponibles
            await loadEditModalSelects();

            const response = await fetch(`${API_APPOINTMENTS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudo cargar la cita para edición.');
            }

            const appointment = await response.json();
            document.getElementById('editAppointmentId').value = appointment.appointmentId;

            const dateObj = new Date(appointment.appointmentDate);
            if (isNaN(dateObj.getTime())) {
                console.error("Fecha inválida recibida de la API:", appointment.appointmentDate);
                document.getElementById('editAppointmentDate').value = '';
            } else {
                const year = dateObj.getFullYear();
                const month = String(dateObj.getMonth() + 1).padStart(2, '0');
                const day = String(dateObj.getDate()).padStart(2, '0');
                const hours = String(dateObj.getHours()).padStart(2, '0');
                const minutes = String(dateObj.getMinutes()).padStart(2, '0');
                document.getElementById('editAppointmentDate').value = `${year}-${month}-${day}T${hours}:${minutes}`;
            }

            document.getElementById('editNotes').value = appointment.notes || '';

            editDoctorSelect.value = appointment.doctorId ? appointment.doctorId.toString() : '';
            editPatientSelect.value = appointment.patientId ? appointment.patientId.toString() : '';
            editInstitutionSelect.value = appointment.institutionId ? appointment.institutionId.toString() : '';

            // Si el status del API es un número, lo asigna directamente al select (que usa valores numéricos)
            // Si el status del API es un string (ej. "Programada"), necesitas mapearlo a su número (0, 1, 2, 3)
            // Asumo que el backend devuelve el status como un número en este punto o que los valores de option coinciden.
            // Si tu backend devuelve el string "Programada", entonces necesitas un mapeo inverso aquí.
            // Por ejemplo, si appointment.status es "Programada", y tus opciones son 0, 1, 2, 3:
            const reverseStatusMap = {
                "Programada": "0",
                "Confirmada": "1",
                "Completada": "2",
                "Cancelada": "3"
            };
            editStatusSelect.value = typeof appointment.status === 'string' ? reverseStatusMap[appointment.status] : appointment.status;


            editAppointmentModal.show();
        } catch (error) {
            console.error('Error al poblar formulario de edición:', error);
            showToast("Error de Carga", `Error al poblar formulario: ${error.message}`, false);
        }
    }


    // --- Event Listeners ---

    // Inicializar la carga de citas al cargar el DOM
    loadAppointments();

    // Event listener para el formulario de Creación
    createAppointmentForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const newAppointment = {
            doctorId: parseInt(createDoctorSelect.value),
            patientId: parseInt(createPatientSelect.value),
            institutionId: parseInt(createInstitutionSelect.value),
            appointmentDate: document.getElementById('createAppointmentDate').value,
            status: parseInt(createStatusSelect.value),
            notes: document.getElementById('createNotes').value || null
        };

        if (isNaN(newAppointment.doctorId) || isNaN(newAppointment.patientId) || isNaN(newAppointment.institutionId) || !newAppointment.appointmentDate) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }

        try {
            const response = await fetch(API_APPOINTMENTS_URL, {
                method: 'POST',
                headers: getAuthHeaders(),
                body: JSON.stringify(newAppointment)
            });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || JSON.stringify(errorData));
            }

            showToast("Éxito", "Cita creada correctamente.", true);
            createAppointmentForm.reset();
            createAppointmentModal.hide();
            loadAppointments(); // Recargar la tabla
        } catch (error) {
            console.error('Error al crear cita:', error);
            showToast("Error de Creación", `Error al crear cita: ${error.message}`, false);
        }
    });

    // Event listener para el formulario de Edición
    editAppointmentForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = document.getElementById('editAppointmentId').value;
        const updatedAppointment = {
            appointmentId: parseInt(id),
            doctorId: parseInt(editDoctorSelect.value),
            patientId: parseInt(editPatientSelect.value),
            institutionId: parseInt(editInstitutionSelect.value),
            appointmentDate: document.getElementById('editAppointmentDate').value,
            status: parseInt(editStatusSelect.value),
            notes: document.getElementById('editNotes').value || null
        };

        if (isNaN(updatedAppointment.doctorId) || isNaN(updatedAppointment.patientId) || isNaN(updatedAppointment.institutionId) || !updatedAppointment.appointmentDate) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }

        try {
            const response = await fetch(`${API_APPOINTMENTS_URL}/${id}`, {
                method: 'PUT',
                headers: getAuthHeaders(),
                body: JSON.stringify(updatedAppointment)
            });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || JSON.stringify(errorData));
            }

            showToast("Éxito", "Cita actualizada correctamente.", true);
            editAppointmentModal.hide();
            loadAppointments(); // Recargar la tabla
        } catch (error) {
            console.error('Error al actualizar cita:', error);
            showToast("Error de Actualización", `Error al actualizar cita: ${error.message}`, false);
        }
    });

    // Delegación de eventos para los botones de Ver, Editar y Desactivar en la tabla
    appointmentsTableBody.addEventListener('click', async function (event) {
        const target = event.target.closest('button');

        if (!target) return;

        const appointmentId = target.dataset.id;
        if (!appointmentId) return;

        if (target.classList.contains('view-btn')) {
            showDetails(appointmentId);
        } else if (target.classList.contains('edit-btn')) {
            populateEditForm(appointmentId);
        } else if (target.classList.contains('desactivate-btn')) {
            if (confirm('¿Está seguro de que desea cancelar esta cita?')) {
                try {
                    const response = await fetch(`${API_APPOINTMENTS_URL}/${appointmentId}/desactivate`, {
                        method: 'PATCH',
                        headers: getAuthHeaders()
                    });

                    if (response.status === 401) {
                        showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                        localStorage.removeItem('jwtToken');
                        setTimeout(() => window.location.href = '/Home/Login', 2000);
                        return;
                    }
                    if (!response.ok) {
                        const errorData = await response.json().catch(() => ({ message: response.statusText }));
                        throw new Error(errorData.message || JSON.stringify(errorData));
                    }

                    showToast("Éxito", "Cita cancelada correctamente.", true);
                    loadAppointments(); // Recargar la tabla
                } catch (error) {
                    console.error('Error al cancelar cita:', error);
                    showToast("Error al cancelar", `Error al cancelar cita: ${error.message}`, false);
                }
            }
        }
    });

    // Event listeners para los modales, para cargar los selects al abrirlos
    document.getElementById('createAppointmentModal').addEventListener('show.bs.modal', loadCreateModalSelects);
    document.getElementById('editAppointmentModal').addEventListener('show.bs.modal', loadEditModalSelects);

    // Opcional: Limpiar formularios al ocultar los modales para una mejor UX
    document.getElementById('createAppointmentModal').addEventListener('hidden.bs.modal', function () {
        createAppointmentForm.reset();
    });
    document.getElementById('editAppointmentModal').addEventListener('hidden.bs.modal', function () {
        editAppointmentForm.reset();
    });
});