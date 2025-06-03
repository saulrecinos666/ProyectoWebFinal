document.addEventListener('DOMContentLoaded', function () {
    // Definición de constantes para las URLs de la API
    const API_BASE_URL = "http://localhost:5278/api"; // Asegúrate de que esta URL sea correcta
    const API_PATIENTS_URL = `${API_BASE_URL}/patient`;

    const token = localStorage.getItem('jwtToken');

    // Referencias a elementos del DOM
    const patientsTableBody = document.querySelector('#patientsTable tbody');
    const createPatientForm = document.getElementById('createPatientForm');
    const editPatientForm = document.getElementById('editPatientForm');

    // Referencias a los selects de los modales
    const editIsActiveSelect = document.getElementById('editIsActive');
    const createGenderSelect = document.getElementById('createGender');
    const editGenderSelect = document.getElementById('editGender');

    // Referencias a los modales de Bootstrap
    const createPatientModal = new bootstrap.Modal(document.getElementById('createPatientModal'));
    const editPatientModal = new bootstrap.Modal(document.getElementById('editPatientModal'));
    const detailsPatientModal = new bootstrap.Modal(document.getElementById('detailsPatientModal'));

    // --- Funciones Auxiliares ---

    // Función para mostrar Toast de Bootstrap
    function showToast(header, body, isSuccess = true) {
        const toastElement = document.getElementById('liveToast');
        const toastHeaderElement = document.getElementById('toastHeader');
        const toastBodyElement = document.getElementById('toastBody');

        toastHeaderElement.textContent = header;
        toastBodyElement.textContent = body;

        const toastHeaderDiv = toastElement.querySelector('.toast-header');
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
    function formatDate(dateString, isDateOnly = false) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
        if (isDateOnly) {
            // Formato YYYY-MM-DD para input type="date"
            return date.toISOString().split('T')[0];
        }
        return date.toLocaleString();
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

    // Función para obtener la clase del badge según el estado (activo/inactivo)
    function getBadgeClass(isActive) {
        if (isActive === true || isActive === 'true') {
            return 'bg-success';
        } else {
            return 'bg-danger';
        }
    }

    // Función para obtener el texto del estado (Activo/Inactivo)
    function getStatusText(isActive) {
        if (isActive === true || isActive === 'true') {
            return 'Activo';
        } else {
            return 'Inactivo';
        }
    }

    // Función para obtener el texto del género
    function getGenderText(genderCode) {
        switch (genderCode) {
            case 'M': return 'Masculino';
            case 'F': return 'Femenino';
            case 'O': return 'Otro';
            default: return 'N/A';
        }
    }

    // --- Funciones CRUD de Pacientes ---

    // Cargar Pacientes en la tabla
    async function loadPatients() {
        if (!token) {
            showToast("No Autorizado", "Su sesión no está iniciada. Redirigiendo al login...", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000);
            return;
        }

        patientsTableBody.innerHTML = '<tr><td colspan="9" class="text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"><span class="visually-hidden">Cargando...</span></div> Cargando pacientes...</td></tr>';
        try {
            const response = await fetch(API_PATIENTS_URL, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'Error al cargar pacientes');
            }

            const patients = await response.json();
            displayPatients(patients);
        } catch (error) {
            console.error('Error al cargar pacientes:', error);
            patientsTableBody.innerHTML = `<tr><td colspan="9" class="text-center text-danger">Error al cargar pacientes: ${error.message}</td></tr>`;
            showToast("Error de Carga", `Error al cargar pacientes: ${error.message}`, false);
        }
    }

    // Mostrar Pacientes en la tabla
    function displayPatients(patients) {
        patientsTableBody.innerHTML = '';
        if (patients.length === 0) {
            patientsTableBody.innerHTML = '<tr><td colspan="9" class="text-center">No hay pacientes registrados.</td></tr>';
            return;
        }

        patients.forEach(patient => {
            const row = patientsTableBody.insertRow();
            row.dataset.patientId = patient.patientId;

            // Asumo que 'IsActive' también se añade al ResponsePatientDto en el backend
            const isActiveText = getStatusText(patient.isActive);
            const fullName = `${patient.firstName || ''} ${patient.middleName || ''} ${patient.lastName || ''} ${patient.secondLastName || ''}`.trim();

            row.innerHTML = `
                <td>${patient.patientId}</td>
                <td>${fullName}</td>
                <td>${patient.dui || 'N/A'}</td>
                <td>${patient.email || 'N/A'}</td>
                <td>${patient.phone || 'N/A'}</td>
                <td>${getGenderText(patient.gender)}</td>
                <td>${formatDate(patient.dateOfBirth, true)}</td>
                <td><span class="badge ${getBadgeClass(patient.isActive)}">${isActiveText}</span></td>
                <td>
                    <button class="btn btn-info btn-sm me-1 view-btn" data-id="${patient.patientId}" title="Ver Detalles"><i class="fas fa-eye"></i></button>
                    <button class="btn btn-warning btn-sm me-1 edit-btn" data-id="${patient.patientId}" title="Editar Paciente"><i class="fas fa-edit"></i></button>
                    ${patient.isActive ?
                    `<button class="btn btn-danger btn-sm desactivate-btn" data-id="${patient.patientId}" title="Desactivar Paciente"><i class="fas fa-times-circle"></i></button>` :
                    `<button class="btn btn-success btn-sm activate-btn" data-id="${patient.patientId}" title="Activar Paciente"><i class="fas fa-check-circle"></i></button>`
                }
                </td>
            `;
        });
    }

    // Mostrar detalles de un paciente en el modal
    async function showDetails(id) {
        try {
            const response = await fetch(`${API_PATIENTS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudieron obtener los detalles del paciente.');
            }

            const patient = await response.json();
            // Asumo que todos los campos del ResponsePatientDto (incluyendo IsActive, CreatedBy, etc.)
            // son devueltos por el GetPatientById en el backend.
            const fullName = `${patient.firstName || ''} ${patient.middleName || ''} ${patient.lastName || ''} ${patient.secondLastName || ''}`.trim();

            document.getElementById('detailsPatientId').textContent = patient.patientId || 'N/A';
            document.getElementById('detailsFullName').textContent = fullName || 'N/A';
            document.getElementById('detailsDui').textContent = patient.dui || 'N/A';
            document.getElementById('detailsDateOfBirth').textContent = formatDate(patient.dateOfBirth, true);
            document.getElementById('detailsGender').textContent = getGenderText(patient.gender);
            document.getElementById('detailsEmail').textContent = patient.email || 'N/A';
            document.getElementById('detailsPhone').textContent = patient.phone || 'N/A';
            document.getElementById('detailsAddress').textContent = patient.address || 'N/A';
            document.getElementById('detailsIsActive').textContent = getStatusText(patient.isActive);
            document.getElementById('detailsCreatedBy').textContent = patient.createdBy || 'N/A';
            document.getElementById('detailsCreatedAt').textContent = formatDate(patient.createdAt);
            document.getElementById('detailsModifiedBy').textContent = patient.modifiedBy || 'N/A';
            document.getElementById('detailsModifiedAt').textContent = formatDate(patient.modifiedAt);

            detailsPatientModal.show();
        } catch (error) {
            console.error('Error al obtener detalles del paciente:', error);
            showToast("Error de Carga", `Error al obtener detalles: ${error.message}`, false);
        }
    }

    // Poblar el formulario de edición y mostrar el modal
    async function populateEditForm(id) {
        try {
            const response = await fetch(`${API_PATIENTS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudo cargar el paciente para edición.');
            }

            const patient = await response.json();
            document.getElementById('editPatientId').value = id;
            document.getElementById('editFirstName').value = patient.firstName || '';
            document.getElementById('editMiddleName').value = patient.middleName || '';
            document.getElementById('editLastName').value = patient.lastName || '';
            document.getElementById('editSecondLastName').value = patient.secondLastName || '';
            document.getElementById('editDui').value = patient.dui || '';
            document.getElementById('editDateOfBirth').value = formatDate(patient.dateOfBirth, true); // Formato para input date
            editGenderSelect.value = patient.gender || '';
            document.getElementById('editEmail').value = patient.email || '';
            document.getElementById('editPhone').value = patient.phone || '';
            document.getElementById('editAddress').value = patient.address || '';
            editIsActiveSelect.value = patient.isActive ? 'true' : 'false'; // Asumo IsActive viene del backend

            editPatientModal.show();
        } catch (error) {
            console.error('Error al poblar formulario de edición:', error);
            showToast("Error de Carga", `Error al poblar formulario: ${error.message}`, false);
        }
    }


    // --- Event Listeners ---

    // Inicializar la carga de pacientes al cargar el DOM
    loadPatients();

    // Event listener para el formulario de Creación
    createPatientForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const newPatient = {
            userId: null, // Si el UserId se obtiene de alguna otra forma o es nulo al crear, ajústalo.
            // Tu controlador tiene GetUserId() para CreatedBy, pero para UserId del paciente no está claro.
            // Si el paciente se asocia a un usuario existente, necesitarás un campo para seleccionarlo.
            firstName: document.getElementById('createFirstName').value,
            middleName: document.getElementById('createMiddleName').value || null,
            lastName: document.getElementById('createLastName').value,
            secondLastName: document.getElementById('createSecondLastName').value || null,
            dui: document.getElementById('createDui').value,
            address: document.getElementById('createAddress').value,
            email: document.getElementById('createEmail').value,
            phone: document.getElementById('createPhone').value || null,
            gender: createGenderSelect.value,
            dateOfBirth: document.getElementById('createDateOfBirth').value, // Formato YYYY-MM-DD
            // IsActive se establece a true por defecto en el controlador
        };

        // Validación básica
        if (!newPatient.firstName || !newPatient.lastName || !newPatient.dui || !newPatient.dateOfBirth || !newPatient.gender || !newPatient.address || !newPatient.email) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }

        try {
            const response = await fetch(API_PATIENTS_URL, {
                method: 'POST',
                headers: getAuthHeaders(),
                body: JSON.stringify(newPatient)
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

            showToast("Éxito", "Paciente creado correctamente.", true);
            createPatientForm.reset();
            createPatientModal.hide();
            loadPatients(); // Recargar la tabla
        } catch (error) {
            console.error('Error al crear paciente:', error);
            showToast("Error de Creación", `Error al crear paciente: ${error.message}`, false);
        }
    });

    // Event listener para el formulario de Edición
    editPatientForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = document.getElementById('editPatientId').value;
        const updatedPatient = {
            firstName: document.getElementById('editFirstName').value,
            middleName: document.getElementById('editMiddleName').value || null,
            lastName: document.getElementById('editLastName').value,
            secondLastName: document.getElementById('editSecondLastName').value || null,
            dui: document.getElementById('editDui').value,
            address: document.getElementById('editAddress').value,
            email: document.getElementById('editEmail').value,
            phone: document.getElementById('editPhone').value || null,
            gender: editGenderSelect.value,
            dateOfBirth: document.getElementById('editDateOfBirth').value, // Formato YYYY-MM-DD
            isActive: editIsActiveSelect.value === 'true' // Se envía el estado
        };

        // Validación básica
        if (!updatedPatient.firstName || !updatedPatient.lastName || !updatedPatient.dui || !updatedPatient.dateOfBirth || !updatedPatient.gender || !updatedPatient.address || !updatedPatient.email) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }

        try {
            const response = await fetch(`${API_PATIENTS_URL}/${id}`, {
                method: 'PUT',
                headers: getAuthHeaders(),
                body: JSON.stringify(updatedPatient)
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

            showToast("Éxito", "Paciente actualizado correctamente.", true);
            editPatientModal.hide();
            loadPatients(); // Recargar la tabla
        } catch (error) {
            console.error('Error al actualizar paciente:', error);
            showToast("Error de Actualización", `Error al actualizar paciente: ${error.message}`, false);
        }
    });

    // Delegación de eventos para los botones de Ver, Editar, Activar y Desactivar en la tabla
    patientsTableBody.addEventListener('click', async function (event) {
        const target = event.target.closest('button');

        if (!target) return;

        const patientId = target.dataset.id;
        if (!patientId) return;

        if (target.classList.contains('view-btn')) {
            showDetails(patientId);
        } else if (target.classList.contains('edit-btn')) {
            populateEditForm(patientId);
        } else if (target.classList.contains('desactivate-btn') || target.classList.contains('activate-btn')) {
            const action = target.classList.contains('desactivate-btn') ? 'desactivar' : 'activar';
            const isActive = action === 'activar';
            const confirmMsg = `¿Está seguro de que desea ${action} este paciente?`;

            if (confirm(confirmMsg)) {
                try {
                    // Tu controlador solo tiene un endpoint para 'desactivate'.
                    // Para 'activate', necesitarías añadir un endpoint similar: PATCH /api/patient/{id}/activate
                    const endpoint = isActive ? `${API_PATIENTS_URL}/${patientId}/activate` : `${API_PATIENTS_URL}/${patientId}/desactivate`;

                    const response = await fetch(endpoint, {
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

                    showToast("Éxito", `Paciente ${action} correctamente.`, true);
                    loadPatients(); // Recargar la tabla
                } catch (error) {
                    console.error(`Error al ${action} paciente:`, error);
                    showToast(`Error al ${action}`, `Error al ${action} paciente: ${error.message}`, false);
                }
            }
        }
    });

    // Opcional: Limpiar formularios al ocultar los modales para una mejor UX
    document.getElementById('createPatientModal').addEventListener('hidden.bs.modal', function () {
        createPatientForm.reset();
    });
    document.getElementById('editPatientModal').addEventListener('hidden.bs.modal', function () {
        editPatientForm.reset();
    });
});