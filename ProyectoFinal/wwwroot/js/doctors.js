document.addEventListener('DOMContentLoaded', function () {
    // Definición de constantes para las URLs de la API
    const API_BASE_URL = "http://localhost:5278/api"; // Asegúrate de que esta URL sea correcta
    const API_DOCTORS_URL = `${API_BASE_URL}/doctor`;
    const API_SPECIALTIES_URL = `${API_BASE_URL}/specialty`;
    const API_INSTITUTIONS_URL = `${API_BASE_URL}/institution`;

    const token = localStorage.getItem('jwtToken');

    // Referencias a elementos del DOM
    const doctorsTableBody = document.querySelector('#doctorsTable tbody');
    const createDoctorForm = document.getElementById('createDoctorForm');
    const editDoctorForm = document.getElementById('editDoctorForm');

    // Referencias a los selects de los modales
    const createSpecialtySelect = document.getElementById('createSpecialtyId');
    const createInstitutionSelect = document.getElementById('createInstitutionId');

    const editSpecialtySelect = document.getElementById('editSpecialtyId');
    const editInstitutionSelect = document.getElementById('editInstitutionId');
    const editIsActiveSelect = document.getElementById('editIsActive');

    // Referencias a los modales de Bootstrap
    const createDoctorModal = new bootstrap.Modal(document.getElementById('createDoctorModal'));
    const editDoctorModal = new bootstrap.Modal(document.getElementById('editDoctorModal'));
    const detailsDoctorModal = new bootstrap.Modal(document.getElementById('detailsDoctorModal'));

    // --- Funciones Auxiliares ---

    // Función para mostrar Toast de Bootstrap (copiada de appointments.js)
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
    function formatDate(dateString) {
        if (!dateString) return 'N/A';
        const date = new Date(dateString);
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
        // isActive será un booleano (true/false) o una cadena "true"/"false" desde la API
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

    // --- Funciones para Cargar Datos en Selects ---

    async function fetchAndPopulateSelect(url, selectElement, idProperty, nameProperties) {
        if (!token) {
            showToast("Error de Autenticación", "No tiene token JWT. Por favor, inicie sesión.", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000);
            return;
        }

        selectElement.innerHTML = '<option value="">Cargando...</option>';

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
            selectElement.innerHTML = '<option value="">Seleccione...</option>';

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

    // Cargar selects para el modal de creación
    async function loadCreateModalSelects() {
        createSpecialtySelect.value = '';
        createInstitutionSelect.value = '';
        createDoctorForm.reset();

        await Promise.all([
            fetchAndPopulateSelect(API_SPECIALTIES_URL, createSpecialtySelect, 'specialtyId', ['specialtyName']),
            fetchAndPopulateSelect(API_INSTITUTIONS_URL, createInstitutionSelect, 'institutionId', ['name'])
        ]).catch(error => {
            console.error("Error al cargar selects del modal de creación de doctor:", error);
            showToast("Error", "No se pudieron cargar todas las opciones para crear un doctor.", false);
        });
    }

    // Cargar selects para el modal de edición
    async function loadEditModalSelects() {
        await Promise.all([
            fetchAndPopulateSelect(API_SPECIALTIES_URL, editSpecialtySelect, 'specialtyId', ['specialtyName']),
            fetchAndPopulateSelect(API_INSTITUTIONS_URL, editInstitutionSelect, 'institutionId', ['name'])
        ]).catch(error => {
            console.error("Error al cargar selects del modal de edición de doctor:", error);
            showToast("Error", "No se pudieron cargar todas las opciones para editar un doctor.", false);
        });
    }

    // --- Funciones CRUD de Doctores ---

    // Cargar Doctores en la tabla
    async function loadDoctors() {
        if (!token) {
            showToast("No Autorizado", "Su sesión no está iniciada. Redirigiendo al login...", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000);
            return;
        }

        doctorsTableBody.innerHTML = '<tr><td colspan="9" class="text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"><span class="visually-hidden">Cargando...</span></div> Cargando doctores...</td></tr>';
        try {
            const response = await fetch(API_DOCTORS_URL, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'Error al cargar doctores');
            }

            const doctors = await response.json();
            displayDoctors(doctors);
        } catch (error) {
            console.error('Error al cargar doctores:', error);
            doctorsTableBody.innerHTML = `<tr><td colspan="9" class="text-center text-danger">Error al cargar doctores: ${error.message}</td></tr>`;
            showToast("Error de Carga", `Error al cargar doctores: ${error.message}`, false);
        }
    }

    // Mostrar Doctores en la tabla
    function displayDoctors(doctors) {
        doctorsTableBody.innerHTML = '';
        if (doctors.length === 0) {
            doctorsTableBody.innerHTML = '<tr><td colspan="9" class="text-center">No hay doctores registrados.</td></tr>';
            return;
        }

        doctors.forEach(doctor => {
            const row = doctorsTableBody.insertRow();
            row.dataset.doctorId = doctor.doctorId;

            const fullName = `${doctor.firstName || ''} ${doctor.middleName || ''} ${doctor.lastName || ''} ${doctor.secondLastName || ''}`.trim().replace(/\s+/g, ' '); // Eliminar espacios extra
            const isActiveText = getStatusText(doctor.isActive);

            row.innerHTML = `
                <td>${doctor.doctorId}</td>
                <td>${fullName}</td>
                <td>${doctor.dui || 'N/A'}</td>
                <td>${doctor.email || 'N/A'}</td>
                <td>${doctor.phone || 'N/A'}</td>
                <td>${doctor.specialtyName || 'N/A'}</td>
                <td>${doctor.institutionName || 'N/A'}</td>
                <td><span class="badge ${getBadgeClass(doctor.isActive)}">${isActiveText}</span></td>
                <td>
                    <button class="btn btn-info btn-sm me-1 view-btn" data-id="${doctor.doctorId}" title="Ver Detalles"><i class="fas fa-eye"></i></button>
                    <button class="btn btn-warning btn-sm me-1 edit-btn" data-id="${doctor.doctorId}" title="Editar Doctor"><i class="fas fa-edit"></i></button>
                    ${doctor.isActive ?
                    `<button class="btn btn-danger btn-sm desactivate-btn" data-id="${doctor.doctorId}" title="Desactivar Doctor"><i class="fas fa-times-circle"></i></button>` :
                    `<button class="btn btn-success btn-sm activate-btn" data-id="${doctor.doctorId}" title="Activar Doctor"><i class="fas fa-check-circle"></i></button>`
                }
                </td>
            `;
        });
    }

    // Mostrar detalles de un doctor en el modal
    async function showDetails(id) {
        try {
            const response = await fetch(`${API_DOCTORS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudieron obtener los detalles del doctor.');
            }

            const doctor = await response.json();
            const fullName = `${doctor.firstName || ''} ${doctor.middleName || ''} ${doctor.lastName || ''} ${doctor.secondLastName || ''}`.trim().replace(/\s+/g, ' ');

            document.getElementById('detailsDoctorId').textContent = doctor.doctorId;
            document.getElementById('detailsFullName').textContent = fullName;
            document.getElementById('detailsDui').textContent = doctor.dui || 'N/A';
            document.getElementById('detailsEmail').textContent = doctor.email || 'N/A';
            document.getElementById('detailsPhone').textContent = doctor.phone || 'N/A';
            document.getElementById('detailsSpecialty').textContent = doctor.specialtyName || 'N/A';
            document.getElementById('detailsInstitution').textContent = doctor.institutionName || 'N/A';
            document.getElementById('detailsIsActive').textContent = getStatusText(doctor.isActive);
            document.getElementById('detailsCreatedBy').textContent = doctor.createdBy || 'N/A';
            document.getElementById('detailsCreatedAt').textContent = formatDate(doctor.createdAt);
            document.getElementById('detailsModifiedBy').textContent = doctor.modifiedBy || 'N/A';
            document.getElementById('detailsModifiedAt').textContent = formatDate(doctor.modifiedAt);

            detailsDoctorModal.show();
        } catch (error) {
            console.error('Error al obtener detalles del doctor:', error);
            showToast("Error de Carga", `Error al obtener detalles: ${error.message}`, false);
        }
    }

    // Poblar el formulario de edición y mostrar el modal
    async function populateEditForm(id) {
        try {
            await loadEditModalSelects();

            const response = await fetch(`${API_DOCTORS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudo cargar el doctor para edición.');
            }

            const doctor = await response.json();
            document.getElementById('editDoctorId').value = doctor.doctorId;
            document.getElementById('editFirstName').value = doctor.firstName || '';
            document.getElementById('editMiddleName').value = doctor.middleName || '';
            document.getElementById('editLastName').value = doctor.lastName || '';
            document.getElementById('editSecondLastName').value = doctor.secondLastName || '';
            document.getElementById('editDui').value = doctor.dui || '';
            document.getElementById('editEmail').value = doctor.email || '';
            document.getElementById('editPhone').value = doctor.phone || '';

            // Asegúrate de que el select tiene la opción antes de intentar seleccionarla
            editSpecialtySelect.value = doctor.specialtyId ? doctor.specialtyId.toString() : '';
            editInstitutionSelect.value = doctor.institutionId ? doctor.institutionId.toString() : '';
            editIsActiveSelect.value = doctor.isActive ? 'true' : 'false'; // 'true' o 'false' como strings

            editDoctorModal.show();
        } catch (error) {
            console.error('Error al poblar formulario de edición:', error);
            showToast("Error de Carga", `Error al poblar formulario: ${error.message}`, false);
        }
    }


    // --- Event Listeners ---

    // Inicializar la carga de doctores al cargar el DOM
    loadDoctors();

    // Event listener para el formulario de Creación
    createDoctorForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const newDoctor = {
            firstName: document.getElementById('createFirstName').value,
            middleName: document.getElementById('createMiddleName').value || null,
            lastName: document.getElementById('createLastName').value,
            secondLastName: document.getElementById('createSecondLastName').value || null,
            dui: document.getElementById('createDui').value,
            email: document.getElementById('createEmail').value,
            phone: document.getElementById('createPhone').value || null,
            specialtyId: parseInt(createSpecialtySelect.value),
            institutionId: parseInt(createInstitutionSelect.value),
            isActive: true // Por defecto, un nuevo doctor es activo
        };

        if (!newDoctor.firstName || !newDoctor.lastName || !newDoctor.dui || !newDoctor.email || isNaN(newDoctor.specialtyId) || isNaN(newDoctor.institutionId)) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }

        try {
            const response = await fetch(API_DOCTORS_URL, {
                method: 'POST',
                headers: getAuthHeaders(),
                body: JSON.stringify(newDoctor)
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

            showToast("Éxito", "Doctor creado correctamente.", true);
            createDoctorForm.reset();
            createDoctorModal.hide();
            loadDoctors(); // Recargar la tabla
        } catch (error) {
            console.error('Error al crear doctor:', error);
            showToast("Error de Creación", `Error al crear doctor: ${error.message}`, false);
        }
    });

    // Event listener para el formulario de Edición
    editDoctorForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = document.getElementById('editDoctorId').value;
        const updatedDoctor = {
            doctorId: parseInt(id),
            firstName: document.getElementById('editFirstName').value,
            middleName: document.getElementById('editMiddleName').value || null,
            lastName: document.getElementById('editLastName').value,
            secondLastName: document.getElementById('editSecondLastName').value || null,
            dui: document.getElementById('editDui').value,
            email: document.getElementById('editEmail').value,
            phone: document.getElementById('editPhone').value || null,
            specialtyId: parseInt(editSpecialtySelect.value),
            institutionId: parseInt(editInstitutionSelect.value),
            isActive: editIsActiveSelect.value === 'true' // Convertir a booleano
        };

        if (!updatedDoctor.firstName || !updatedDoctor.lastName || !updatedDoctor.dui || !updatedDoctor.email || isNaN(updatedDoctor.specialtyId) || isNaN(updatedDoctor.institutionId)) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }

        try {
            const response = await fetch(`${API_DOCTORS_URL}/${id}`, {
                method: 'PUT',
                headers: getAuthHeaders(),
                body: JSON.stringify(updatedDoctor)
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

            showToast("Éxito", "Doctor actualizado correctamente.", true);
            editDoctorModal.hide();
            loadDoctors(); // Recargar la tabla
        } catch (error) {
            console.error('Error al actualizar doctor:', error);
            showToast("Error de Actualización", `Error al actualizar doctor: ${error.message}`, false);
        }
    });

    // Delegación de eventos para los botones de Ver, Editar, Activar y Desactivar en la tabla
    doctorsTableBody.addEventListener('click', async function (event) {
        const target = event.target.closest('button');

        if (!target) return;

        const doctorId = target.dataset.id;
        if (!doctorId) return;

        if (target.classList.contains('view-btn')) {
            showDetails(doctorId);
        } else if (target.classList.contains('edit-btn')) {
            populateEditForm(doctorId);
        } else if (target.classList.contains('desactivate-btn') || target.classList.contains('activate-btn')) {
            const action = target.classList.contains('desactivate-btn') ? 'desactivar' : 'activar';
            const isActive = action === 'activar'; // True if activating, false if deactivating
            const confirmMsg = `¿Está seguro de que desea ${action} este doctor?`;

            if (confirm(confirmMsg)) {
                try {
                    // Si tu API tiene endpoints separados para activar/desactivar:
                    const endpoint = isActive ? `${API_DOCTORS_URL}/${doctorId}/activate` : `${API_DOCTORS_URL}/${doctorId}/desactivate`;

                    const response = await fetch(endpoint, {
                        method: 'PATCH', // Asumiendo que es una operación PATCH
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

                    showToast("Éxito", `Doctor ${action} correctamente.`, true);
                    loadDoctors(); // Recargar la tabla
                } catch (error) {
                    console.error(`Error al ${action} doctor:`, error);
                    showToast(`Error al ${action}`, `Error al ${action} doctor: ${error.message}`, false);
                }
            }
        }
    });

    // Event listeners para los modales, para cargar los selects al abrirlos
    document.getElementById('createDoctorModal').addEventListener('show.bs.modal', loadCreateModalSelects);
    document.getElementById('editDoctorModal').addEventListener('show.bs.modal', loadEditModalSelects);

    // Opcional: Limpiar formularios al ocultar los modales para una mejor UX
    document.getElementById('createDoctorModal').addEventListener('hidden.bs.modal', function () {
        createDoctorForm.reset();
    });
    document.getElementById('editDoctorModal').addEventListener('hidden.bs.modal', function () {
        editDoctorForm.reset();
    });
});