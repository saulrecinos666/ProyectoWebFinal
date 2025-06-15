document.addEventListener('DOMContentLoaded', function () {
    // Definición de constantes para las URLs de la API
    const isLocalhost = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";
    const API_BASE_URL = isLocalhost
        ? "http://localhost:5278/api"
        : "https://b4ndxm8brf.us-east-2.awsapprunner.com/api";
    const API_SPECIALTIES_URL = `${API_BASE_URL}/specialty`;

    const token = localStorage.getItem('jwtToken');

    // Referencias a elementos del DOM
    const specialtiesTableBody = document.querySelector('#specialtiesTable tbody');
    const createSpecialtyForm = document.getElementById('createSpecialtyForm');
    const editSpecialtyForm = document.getElementById('editSpecialtyForm');

    // Referencias a los selects de los modales (solo isActive para edición)
    const editIsActiveSelect = document.getElementById('editIsActive');

    // Referencias a los modales de Bootstrap
    const createSpecialtyModal = new bootstrap.Modal(document.getElementById('createSpecialtyModal'));
    const editSpecialtyModal = new bootstrap.Modal(document.getElementById('editSpecialtyModal'));
    const detailsSpecialtyModal = new bootstrap.Modal(document.getElementById('detailsSpecialtyModal'));

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

    // --- Funciones CRUD de Especialidades ---

    // Cargar Especialidades en la tabla
    async function loadSpecialties() {
        if (!token) {
            showToast("No Autorizado", "Su sesión no está iniciada. Redirigiendo al login...", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000);
            return;
        }

        specialtiesTableBody.innerHTML = '<tr><td colspan="5" class="text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"><span class="visually-hidden">Cargando...</span></div> Cargando especialidades...</td></tr>';
        try {
            const response = await fetch(API_SPECIALTIES_URL, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'Error al cargar especialidades');
            }

            const specialties = await response.json();
            displaySpecialties(specialties);
        } catch (error) {
            console.error('Error al cargar especialidades:', error);
            specialtiesTableBody.innerHTML = `<tr><td colspan="5" class="text-center text-danger">Error al cargar especialidades: ${error.message}</td></tr>`;
            showToast("Error de Carga", `Error al cargar especialidades: ${error.message}`, false);
        }
    }

    // Mostrar Especialidades en la tabla
    function displaySpecialties(specialties) {
        specialtiesTableBody.innerHTML = '';
        if (specialties.length === 0) {
            specialtiesTableBody.innerHTML = '<tr><td colspan="5" class="text-center">No hay especialidades registradas.</td></tr>';
            return;
        }

        specialties.forEach(specialty => {
            const row = specialtiesTableBody.insertRow();
            row.dataset.specialtyId = specialty.specialtyId;

            // Tu ResponseSpecialtyDto solo tiene SpecialtyId, Name, Description.
            // Para mostrar el estado (IsActive) y las fechas de auditoría,
            // necesitas añadirlas a tu ResponseSpecialtyDto en C#.
            // Asumiendo que IsActive se ha añadido:
            const isActiveText = getStatusText(specialty.isActive);

            row.innerHTML = `
                <td>${specialty.specialtyId}</td>
                <td>${specialty.name || 'N/A'}</td>
                <td>${specialty.description || 'N/A'}</td>
                <td><span class="badge ${getBadgeClass(specialty.isActive)}">${isActiveText}</span></td>
                <td>
                    <button class="btn btn-info btn-sm me-1 view-btn" data-id="${specialty.specialtyId}" title="Ver Detalles"><i class="fas fa-eye"></i></button>
                    <button class="btn btn-warning btn-sm me-1 edit-btn" data-id="${specialty.specialtyId}" title="Editar Especialidad"><i class="fas fa-edit"></i></button>
                    ${specialty.isActive ?
                    `<button class="btn btn-danger btn-sm desactivate-btn" data-id="${specialty.specialtyId}" title="Desactivar Especialidad"><i class="fas fa-times-circle"></i></button>` :
                    `<button class="btn btn-success btn-sm activate-btn" data-id="${specialty.specialtyId}" title="Activar Especialidad"><i class="fas fa-check-circle"></i></button>`
                }
                </td>
            `;
        });
    }

    // Mostrar detalles de una especialidad en el modal
    async function showDetails(id) {
        try {
            const response = await fetch(`${API_SPECIALTIES_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudieron obtener los detalles de la especialidad.');
            }

            const specialty = await response.json();
            // Tu GetSpecialtyById solo devuelve Name y Description.
            // Para mostrar el ID, IsActive y los campos de auditoría,
            // necesitas añadirlos a tu ResponseSpecialtyDto en C# y devolverlos en GetSpecialtyById.
            document.getElementById('detailsSpecialtyId').textContent = specialty.specialtyId || 'N/A';
            document.getElementById('detailsName').textContent = specialty.name || 'N/A';
            document.getElementById('detailsDescription').textContent = specialty.description || 'N/A';
            document.getElementById('detailsIsActive').textContent = getStatusText(specialty.isActive);
            document.getElementById('detailsCreatedBy').textContent = specialty.createdBy || 'N/A';
            document.getElementById('detailsCreatedAt').textContent = formatDate(specialty.createdAt);
            document.getElementById('detailsModifiedBy').textContent = specialty.modifiedBy || 'N/A';
            document.getElementById('detailsModifiedAt').textContent = formatDate(specialty.modifiedAt);

            detailsSpecialtyModal.show();
        } catch (error) {
            console.error('Error al obtener detalles de la especialidad:', error);
            showToast("Error de Carga", `Error al obtener detalles: ${error.message}`, false);
        }
    }

    // Poblar el formulario de edición y mostrar el modal
    async function populateEditForm(id) {
        try {
            const response = await fetch(`${API_SPECIALTIES_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudo cargar la especialidad para edición.');
            }

            const specialty = await response.json();
            document.getElementById('editSpecialtyId').value = id; // El ID ya lo tenemos
            document.getElementById('editName').value = specialty.name || '';
            document.getElementById('editDescription').value = specialty.description || '';

            editSpecialtyModal.show();
        } catch (error) {
            console.error('Error al poblar formulario de edición:', error);
            showToast("Error de Carga", `Error al poblar formulario: ${error.message}`, false);
        }
    }


    // --- Event Listeners ---

    // Inicializar la carga de especialidades al cargar el DOM
    loadSpecialties();

    // Event listener para el formulario de Creación
    createSpecialtyForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const newSpecialty = {
            name: document.getElementById('createName').value,
            description: document.getElementById('createDescription').value || null
            // IsActive se establece a true por defecto en el controlador
        };

        if (!newSpecialty.name) {
            showToast("Error de Validación", "Por favor, complete el nombre de la especialidad.", false);
            return;
        }

        try {
            const response = await fetch(API_SPECIALTIES_URL, {
                method: 'POST',
                headers: getAuthHeaders(),
                body: JSON.stringify(newSpecialty)
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

            showToast("Éxito", "Especialidad creada correctamente.", true);
            createSpecialtyForm.reset();
            createSpecialtyModal.hide();
            loadSpecialties(); // Recargar la tabla
        } catch (error) {
            console.error('Error al crear especialidad:', error);
            showToast("Error de Creación", `Error al crear especialidad: ${error.message}`, false);
        }
    });

    // Event listener para el formulario de Edición
    editSpecialtyForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = document.getElementById('editSpecialtyId').value;
        const updatedSpecialty = {
            name: document.getElementById('editName').value,
            description: document.getElementById('editDescription').value || null
        };

        if (!updatedSpecialty.name) {
            showToast("Error de Validación", "Por favor, complete el nombre de la especialidad.", false);
            return;
        }

        try {
            const response = await fetch(`${API_SPECIALTIES_URL}/${id}`, {
                method: 'PUT',
                headers: getAuthHeaders(),
                body: JSON.stringify(updatedSpecialty)
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

            showToast("Éxito", "Especialidad actualizada correctamente.", true);
            editSpecialtyModal.hide();
            loadSpecialties(); // Recargar la tabla
        } catch (error) {
            console.error('Error al actualizar especialidad:', error);
            showToast("Error de Actualización", `Error al actualizar especialidad: ${error.message}`, false);
        }
    });

    // Delegación de eventos para los botones de Ver, Editar, Activar y Desactivar en la tabla
    specialtiesTableBody.addEventListener('click', async function (event) {
        const target = event.target.closest('button');

        if (!target) return;

        const specialtyId = target.dataset.id;
        if (!specialtyId) return;

        if (target.classList.contains('view-btn')) {
            showDetails(specialtyId);
        } else if (target.classList.contains('edit-btn')) {
            populateEditForm(specialtyId);
        } else if (target.classList.contains('desactivate-btn') || target.classList.contains('activate-btn')) {
            const action = target.classList.contains('desactivate-btn') ? 'desactivar' : 'activar';
            const isActive = action === 'activar'; // True if activating, false if deactivating
            const confirmMsg = `¿Está seguro de que desea ${action} esta especialidad?`;

            if (confirm(confirmMsg)) {
                try {
                    // Tu controlador solo tiene un endpoint para 'desactivate'.
                    // Si necesitas 'activate', debes añadirlo al controlador.
                    // Para el caso de activar, asumo un endpoint PATCH similar.
                    const endpoint = isActive ? `${API_SPECIALTIES_URL}/${specialtyId}/activate` : `${API_SPECIALTIES_URL}/${specialtyId}/desactivate`;

                    const response = await fetch(endpoint, {
                        method: 'PATCH', // Asumiendo PATCH para activar/desactivar
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

                    showToast("Éxito", `Especialidad ${action} correctamente.`, true);
                    loadSpecialties(); // Recargar la tabla
                } catch (error) {
                    console.error(`Error al ${action} especialidad:`, error);
                    showToast(`Error al ${action}`, `Error al ${action} especialidad: ${error.message}`, false);
                }
            }
        }
    });

    // Opcional: Limpiar formularios al ocultar los modales para una mejor UX
    document.getElementById('createSpecialtyModal').addEventListener('hidden.bs.modal', function () {
        createSpecialtyForm.reset();
    });
    document.getElementById('editSpecialtyModal').addEventListener('hidden.bs.modal', function () {
        editSpecialtyForm.reset();
    });
});