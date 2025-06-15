document.addEventListener('DOMContentLoaded', function () {
    // Definición de constantes para las URLs de la API
    const isLocalhost = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";
    const API_BASE_URL = isLocalhost
        ? "http://localhost:5278/api"
        : "https://b4ndxm8brf.us-east-2.awsapprunner.com/api";
    const API_INSTITUTIONS_URL = `${API_BASE_URL}/institution`;
    const API_DISTRICTS_URL = `${API_BASE_URL}/district`; // Asumo que tienes un endpoint para distritos

    const token = localStorage.getItem('jwtToken');

    // Referencias a elementos del DOM
    const institutionsTableBody = document.querySelector('#institutionsTable tbody');
    const createInstitutionForm = document.getElementById('createInstitutionForm');
    const editInstitutionForm = document.getElementById('editInstitutionForm');

    // Referencias a los selects de los modales
    const createDistrictSelect = document.getElementById('createDistrictCode');
    const editDistrictSelect = document.getElementById('editDistrictCode');
    const editIsActiveSelect = document.getElementById('editIsActive');

    // Referencias a los modales de Bootstrap
    const createInstitutionModal = new bootstrap.Modal(document.getElementById('createInstitutionModal'));
    const editInstitutionModal = new bootstrap.Modal(document.getElementById('editInstitutionModal'));
    const detailsInstitutionModal = new bootstrap.Modal(document.getElementById('detailsInstitutionModal'));

    // --- Funciones Auxiliares ---

    // Función para mostrar Toast de Bootstrap (copiada de appointments.js y doctors.js)
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
        createDistrictSelect.value = '';
        createInstitutionForm.reset();

        await Promise.all([
            fetchAndPopulateSelect(API_DISTRICTS_URL, createDistrictSelect, 'districtCode', ['districtName']) // Asumo 'districtCode' y 'districtName'
        ]).catch(error => {
            console.error("Error al cargar selects del modal de creación de institución:", error);
            showToast("Error", "No se pudieron cargar las opciones para crear una institución.", false);
        });
    }

    // Cargar selects para el modal de edición
    async function loadEditModalSelects() {
        await Promise.all([
            fetchAndPopulateSelect(API_DISTRICTS_URL, editDistrictSelect, 'districtCode', ['districtName']) // Asumo 'districtCode' y 'districtName'
        ]).catch(error => {
            console.error("Error al cargar selects del modal de edición de institución:", error);
            showToast("Error", "No se pudieron cargar las opciones para editar una institución.", false);
        });
    }

    // --- Funciones CRUD de Instituciones ---

    // Cargar Instituciones en la tabla
    async function loadInstitutions() {
        if (!token) {
            showToast("No Autorizado", "Su sesión no está iniciada. Redirigiendo al login...", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000);
            return;
        }

        institutionsTableBody.innerHTML = '<tr><td colspan="8" class="text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"><span class="visually-hidden">Cargando...</span></div> Cargando instituciones...</td></tr>';
        try {
            const response = await fetch(API_INSTITUTIONS_URL, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'Error al cargar instituciones');
            }

            const institutions = await response.json();
            displayInstitutions(institutions);
        } catch (error) {
            console.error('Error al cargar instituciones:', error);
            institutionsTableBody.innerHTML = `<tr><td colspan="8" class="text-center text-danger">Error al cargar instituciones: ${error.message}</td></tr>`;
            showToast("Error de Carga", `Error al cargar instituciones: ${error.message}`, false);
        }
    }

    // Mostrar Instituciones en la tabla
    function displayInstitutions(institutions) {
        institutionsTableBody.innerHTML = '';
        if (institutions.length === 0) {
            institutionsTableBody.innerHTML = '<tr><td colspan="8" class="text-center">No hay instituciones registradas.</td></tr>';
            return;
        }

        institutions.forEach(institution => {
            const row = institutionsTableBody.insertRow();
            row.dataset.institutionId = institution.institutionId;

            // Asumo que el campo 'IsActive' viene en la respuesta, aunque no lo vi en tu ResponseInstitutionDto.
            // Si no viene, deberás añadirlo en tu DTO de C# para mostrar el estado.
            const isActiveText = getStatusText(institution.isActive);

            row.innerHTML = `
                <td>${institution.institutionId}</td>
                <td>${institution.name || 'N/A'}</td>
                <td>${institution.address || 'N/A'}</td>
                <td>${institution.districtName || 'N/A'}</td>
                <td>${institution.email || 'N/A'}</td>
                <td>${institution.phone || 'N/A'}</td>
                <td><span class="badge ${getBadgeClass(institution.isActive)}">${isActiveText}</span></td>
                <td>
                    <button class="btn btn-info btn-sm me-1 view-btn" data-id="${institution.institutionId}" title="Ver Detalles"><i class="fas fa-eye"></i></button>
                    <button class="btn btn-warning btn-sm me-1 edit-btn" data-id="${institution.institutionId}" title="Editar Institución"><i class="fas fa-edit"></i></button>
                    ${institution.isActive ?
                    `<button class="btn btn-danger btn-sm desactivate-btn" data-id="${institution.institutionId}" title="Desactivar Institución"><i class="fas fa-times-circle"></i></button>` :
                    // No hay endpoint de "activar" en el controlador proporcionado, solo "desactivar"
                    // Si necesitas activar, deberás añadir un endpoint PATCH al controlador.
                    // Por ahora, si está inactiva, no se muestra botón de acción.
                    ''
                }
                </td>
            `;
        });
    }

    // Mostrar detalles de una institución en el modal
    async function showDetails(id) {
        try {
            const response = await fetch(`${API_INSTITUTIONS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudieron obtener los detalles de la institución.');
            }

            const institution = await response.json();
            // Asumo que 'isActive' y campos de auditoría están presentes en el DTO de respuesta para detalles
            // Aunque tu GetInstitutionById solo devuelve Name, Address, etc. y no los campos de auditoría ni IsActive
            // Si no los devuelve, estas líneas no mostrarán nada o darán error.
            document.getElementById('detailsInstitutionId').textContent = institution.institutionId || 'N/A';
            document.getElementById('detailsName').textContent = institution.name || 'N/A';
            document.getElementById('detailsAddress').textContent = institution.address || 'N/A';
            document.getElementById('detailsDistrict').textContent = institution.districtName || 'N/A';
            document.getElementById('detailsEmail').textContent = institution.email || 'N/A';
            document.getElementById('detailsPhone').textContent = institution.phone || 'N/A';
            document.getElementById('detailsIsActive').textContent = getStatusText(institution.isActive); // Necesitas que IsActive venga del backend
            document.getElementById('detailsCreatedBy').textContent = institution.createdBy || 'N/A';
            document.getElementById('detailsCreatedAt').textContent = formatDate(institution.createdAt);
            document.getElementById('detailsModifiedBy').textContent = institution.modifiedBy || 'N/A';
            document.getElementById('detailsModifiedAt').textContent = formatDate(institution.modifiedAt);

            detailsInstitutionModal.show();
        } catch (error) {
            console.error('Error al obtener detalles de la institución:', error);
            showToast("Error de Carga", `Error al obtener detalles: ${error.message}`, false);
        }
    }

    // Poblar el formulario de edición y mostrar el modal
    async function populateEditForm(id) {
        try {
            await loadEditModalSelects();

            const response = await fetch(`${API_INSTITUTIONS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudo cargar la institución para edición.');
            }

            const institution = await response.json();
            document.getElementById('editInstitutionId').value = id; // El ID ya lo tenemos
            document.getElementById('editName').value = institution.name || '';
            document.getElementById('editAddress').value = institution.address || '';
            editDistrictSelect.value = institution.districtCode ? institution.districtCode.toString() : ''; // Asumo districtCode en el DTO
            document.getElementById('editEmail').value = institution.email || '';
            document.getElementById('editPhone').value = institution.phone || '';

            editInstitutionModal.show();
        } catch (error) {
            console.error('Error al poblar formulario de edición:', error);
            showToast("Error de Carga", `Error al poblar formulario: ${error.message}`, false);
        }
    }


    // --- Event Listeners ---

    // Inicializar la carga de instituciones al cargar el DOM
    loadInstitutions();

    // Event listener para el formulario de Creación
    createInstitutionForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const newInstitution = {
            name: document.getElementById('createName').value,
            address: document.getElementById('createAddress').value,
            districtCode: createDistrictSelect.value,
            email: document.getElementById('createEmail').value,
            phone: document.getElementById('createPhone').value || null
        };

        if (!newInstitution.name || !newInstitution.address || isNaN(newInstitution.districtCode) || !newInstitution.email) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }

        try {
            const response = await fetch(API_INSTITUTIONS_URL, {
                method: 'POST',
                headers: getAuthHeaders(),
                body: JSON.stringify(newInstitution)
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

            showToast("Éxito", "Institución creada correctamente.", true);
            createInstitutionForm.reset();
            createInstitutionModal.hide();
            loadInstitutions(); // Recargar la tabla
        } catch (error) {
            console.error('Error al crear institución:', error);
            showToast("Error de Creación", `Error al crear institución: ${error.message}`, false);
        }
    });

    // Event listener para el formulario de Edición
    editInstitutionForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = document.getElementById('editInstitutionId').value;
        const updatedInstitution = {
            name: document.getElementById('editName').value,
            address: document.getElementById('editAddress').value,
            districtCode: editDistrictSelect.value, 
            email: document.getElementById('editEmail').value,
            phone: document.getElementById('editPhone').value || null,
        };

        if (!updatedInstitution.name || !updatedInstitution.address || isNaN(updatedInstitution.districtCode) || !updatedInstitution.email) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }

        try {
            const response = await fetch(`${API_INSTITUTIONS_URL}/${id}`, {
                method: 'PUT',
                headers: getAuthHeaders(),
                body: JSON.stringify(updatedInstitution)
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

            showToast("Éxito", "Institución actualizada correctamente.", true);
            editInstitutionModal.hide();
            loadInstitutions(); // Recargar la tabla
        } catch (error) {
            console.error('Error al actualizar institución:', error);
            showToast("Error de Actualización", `Error al actualizar institución: ${error.message}`, false);
        }
    });

    // Delegación de eventos para los botones de Ver, Editar y Desactivar en la tabla
    institutionsTableBody.addEventListener('click', async function (event) {
        const target = event.target.closest('button');

        if (!target) return;

        const institutionId = target.dataset.id;
        if (!institutionId) return;

        if (target.classList.contains('view-btn')) {
            showDetails(institutionId);
        } else if (target.classList.contains('edit-btn')) {
            populateEditForm(institutionId);
        } else if (target.classList.contains('desactivate-btn')) {
            if (confirm('¿Está seguro de que desea desactivar esta institución?')) {
                try {
                    const response = await fetch(`${API_INSTITUTIONS_URL}/${institutionId}/desactivate`, {
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

                    showToast("Éxito", "Institución desactivada correctamente.", true);
                    loadInstitutions(); // Recargar la tabla
                } catch (error) {
                    console.error('Error al desactivar institución:', error);
                    showToast("Error al desactivar", `Error al desactivar institución: ${error.message}`, false);
                }
            }
        }
        // No hay botón de "activar" ya que no se proporcionó un endpoint en el controlador.
    });

    // Event listeners para los modales, para cargar los selects al abrirlos
    document.getElementById('createInstitutionModal').addEventListener('show.bs.modal', loadCreateModalSelects);
    document.getElementById('editInstitutionModal').addEventListener('show.bs.modal', loadEditModalSelects);

    // Opcional: Limpiar formularios al ocultar los modales para una mejor UX
    document.getElementById('createInstitutionModal').addEventListener('hidden.bs.modal', function () {
        createInstitutionForm.reset();
    });
    document.getElementById('editInstitutionModal').addEventListener('hidden.bs.modal', function () {
        editInstitutionForm.reset();
    });
});