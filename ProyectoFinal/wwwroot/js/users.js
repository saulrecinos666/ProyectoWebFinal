document.addEventListener('DOMContentLoaded', function () {
    // Definición de constantes para las URLs de la API
    const API_BASE_URL = "http://localhost:5278/api"; // Asegúrate de que esta URL sea correcta
    const API_USERS_URL = `${API_BASE_URL}/user`;

    const token = localStorage.getItem('jwtToken');

    // Referencias a elementos del DOM
    const usersTableBody = document.querySelector('#usersTable tbody');
    const createUserForm = document.getElementById('createUserForm');
    const editUserForm = document.getElementById('editUserForm');

    // Referencias a los selects de los modales
    const editIsActiveSelect = document.getElementById('editIsActive');

    // Referencias a los modales de Bootstrap
    const createUserModal = new bootstrap.Modal(document.getElementById('createUserModal'));
    const editUserModal = new bootstrap.Modal(document.getElementById('editUserModal'));
    const detailsUserModal = new bootstrap.Modal(document.getElementById('detailsUserModal'));

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

    // --- Funciones CRUD de Usuarios ---

    // Cargar Usuarios en la tabla
    async function loadUsers() {
        if (!token) {
            showToast("No Autorizado", "Su sesión no está iniciada. Redirigiendo al login...", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000);
            return;
        }

        usersTableBody.innerHTML = '<tr><td colspan="5" class="text-center"><div class="spinner-border spinner-border-sm text-primary" role="status"><span class="visually-hidden">Cargando...</span></div> Cargando usuarios...</td></tr>';
        try {
            const response = await fetch(API_USERS_URL, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'Error al cargar usuarios');
            }

            const users = await response.json();
            displayUsers(users);
        } catch (error) {
            console.error('Error al cargar usuarios:', error);
            usersTableBody.innerHTML = `<tr><td colspan="5" class="text-center text-danger">Error al cargar usuarios: ${error.message}</td></tr>`;
            showToast("Error de Carga", `Error al cargar usuarios: ${error.message}`, false);
        }
    }

    // Mostrar Usuarios en la tabla
    function displayUsers(users) {
        usersTableBody.innerHTML = '';
        if (users.length === 0) {
            usersTableBody.innerHTML = '<tr><td colspan="5" class="text-center">No hay usuarios registrados.</td></tr>';
            return;
        }

        users.forEach(user => {
            const row = usersTableBody.insertRow();
            row.dataset.userId = user.userId;

            // Asumo que 'IsActive' se ha añadido al ResponseUserDto
            const isActiveText = getStatusText(user.isActive);

            row.innerHTML = `
                <td>${user.userId || 'N/A'}</td>
                <td>${user.username || 'N/A'}</td>
                <td>${user.email || 'N/A'}</td>
                <td><span class="badge ${getBadgeClass(user.isActive)}">${isActiveText}</span></td>
                <td>
                    <button class="btn btn-info btn-sm me-1 view-btn" data-id="${user.userId}" title="Ver Detalles"><i class="fas fa-eye"></i></button>
                    <button class="btn btn-warning btn-sm me-1 edit-btn" data-id="${user.userId}" title="Editar Usuario"><i class="fas fa-edit"></i></button>
                    ${user.isActive ?
                    `<button class="btn btn-danger btn-sm desactivate-btn" data-id="${user.userId}" title="Desactivar Usuario"><i class="fas fa-times-circle"></i></button>` :
                    `<button class="btn btn-success btn-sm activate-btn" data-id="${user.userId}" title="Activar Usuario"><i class="fas fa-check-circle"></i></button>`
                }
                </td>
            `;
        });
    }

    // Mostrar detalles de un usuario en el modal
    async function showDetails(id) {
        try {
            const response = await fetch(`${API_USERS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudieron obtener los detalles del usuario.');
            }

            const user = await response.json();
            // Asumo que todos los campos del ResponseUserDto (incluyendo IsActive, UserId)
            // son devueltos por el GetUserById en el backend.
            document.getElementById('detailsUserId').textContent = user.userId || 'N/A';
            document.getElementById('detailsUsername').textContent = user.username || 'N/A';
            document.getElementById('detailsEmail').textContent = user.email || 'N/A';
            document.getElementById('detailsIsActive').textContent = getStatusText(user.isActive); // Necesitas que IsActive venga
            document.getElementById('detailsCreatedBy').textContent = user.createdBy || 'N/A';
            document.getElementById('detailsCreatedAt').textContent = formatDate(user.createdAt);
            document.getElementById('detailsModifiedBy').textContent = user.modifiedBy || 'N/A';
            document.getElementById('detailsModifiedAt').textContent = formatDate(user.modifiedAt);

            detailsUserModal.show();
        } catch (error) {
            console.error('Error al obtener detalles del usuario:', error);
            showToast("Error de Carga", `Error al obtener detalles: ${error.message}`, false);
        }
    }

    // Poblar el formulario de edición y mostrar el modal
    async function populateEditForm(id) {
        try {
            const response = await fetch(`${API_USERS_URL}/${id}`, { headers: getAuthHeaders() });

            if (response.status === 401) {
                showToast("Sesión Expirada", "Su sesión ha expirado. Por favor, inicie sesión de nuevo.", false);
                localStorage.removeItem('jwtToken');
                setTimeout(() => window.location.href = '/Home/Login', 2000);
                return;
            }
            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || 'No se pudo cargar el usuario para edición.');
            }

            const user = await response.json();
            document.getElementById('editUserId').value = id;
            document.getElementById('editUsername').value = user.username || '';
            document.getElementById('editEmail').value = user.email || '';
            // No se carga la contraseña por seguridad

            editUserModal.show();
        } catch (error) {
            console.error('Error al poblar formulario de edición:', error);
            showToast("Error de Carga", `Error al poblar formulario: ${error.message}`, false);
        }
    }


    // --- Event Listeners ---

    // Inicializar la carga de usuarios al cargar el DOM
    loadUsers();

    // Event listener para el formulario de Creación
    createUserForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const password = document.getElementById('createPassword').value;
        const confirmPassword = document.getElementById('createConfirmPassword').value;

        if (password !== confirmPassword) {
            showToast("Error de Validación", "Las contraseñas no coinciden.", false);
            return;
        }

        const newUser = {
            username: document.getElementById('createUsername').value,
            email: document.getElementById('createEmail').value,
            password: password
        };

        // Validación básica
        if (!newUser.username || !newUser.email || !newUser.password) {
            showToast("Error de Validación", "Por favor, complete todos los campos requeridos.", false);
            return;
        }

        try {
            // Este endpoint es [AllowAnonymous], por lo que no necesita token para la creación.
            // Sin embargo, si quieres que solo usuarios autenticados puedan crear, quita [AllowAnonymous].
            const response = await fetch(API_USERS_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' }, // No se envía token si es [AllowAnonymous]
                body: JSON.stringify(newUser)
            });

            if (!response.ok) {
                const errorData = await response.json().catch(() => ({ message: response.statusText }));
                throw new Error(errorData.message || JSON.stringify(errorData));
            }

            showToast("Éxito", "Usuario creado correctamente.", true);
            createUserForm.reset();
            createUserModal.hide();
            loadUsers(); // Recargar la tabla
        } catch (error) {
            console.error('Error al crear usuario:', error);
            showToast("Error de Creación", `Error al crear usuario: ${error.message}`, false);
        }
    });

    // Event listener para el formulario de Edición
    editUserForm.addEventListener('submit', async (e) => {
        e.preventDefault();

        const id = document.getElementById('editUserId').value;
        const updatedUser = {
            username: document.getElementById('editUsername').value,
            email: document.getElementById('editEmail').value,
            password: document.getElementById('editPassword').value || null
        };

        // Validación básica
        if (!updatedUser.username || !updatedUser.email) {
            showToast("Error de Validación", "Por favor, complete el nombre de usuario y el email.", false);
            return;
        }

        try {
            const response = await fetch(`${API_USERS_URL}/${id}`, {
                method: 'PATCH', // Tu controlador usa PATCH para UpdateUser
                headers: getAuthHeaders(),
                body: JSON.stringify(updatedUser)
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

            showToast("Éxito", "Usuario actualizado correctamente.", true);
            editUserModal.hide();
            loadUsers(); // Recargar la tabla
        } catch (error) {
            console.error('Error al actualizar usuario:', error);
            showToast("Error de Actualización", `Error al actualizar usuario: ${error.message}`, false);
        }
    });

    // Delegación de eventos para los botones de Ver, Editar, Activar y Desactivar en la tabla
    usersTableBody.addEventListener('click', async function (event) {
        const target = event.target.closest('button');

        if (!target) return;

        const userId = target.dataset.id;
        if (!userId) return;

        if (target.classList.contains('view-btn')) {
            showDetails(userId);
        } else if (target.classList.contains('edit-btn')) {
            populateEditForm(userId);
        } else if (target.classList.contains('desactivate-btn') || target.classList.contains('activate-btn')) {
            const action = target.classList.contains('desactivate-btn') ? 'desactivar' : 'activar';
            const isActive = action === 'activar';
            const confirmMsg = `¿Está seguro de que desea ${action} este usuario?`;

            if (confirm(confirmMsg)) {
                try {
                    // Tu controlador solo tiene un endpoint para 'desactivate'.
                    // Para 'activate', necesitarías añadir un endpoint similar: PATCH /api/user/{id}/activate
                    const endpoint = isActive ? `${API_USERS_URL}/${userId}/activate` : `${API_USERS_URL}/${userId}/desactivate`;

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

                    showToast("Éxito", `Usuario ${action} correctamente.`, true);
                    loadUsers(); // Recargar la tabla
                } catch (error) {
                    console.error(`Error al ${action} usuario:`, error);
                    showToast(`Error al ${action}`, `Error al ${action} usuario: ${error.message}`, false);
                }
            }
        }
    });

    // Opcional: Limpiar formularios al ocultar los modales para una mejor UX
    document.getElementById('createUserModal').addEventListener('hidden.bs.modal', function () {
        createUserForm.reset();
    });
    document.getElementById('editUserModal').addEventListener('hidden.bs.modal', function () {
        editUserForm.reset();
    });
});