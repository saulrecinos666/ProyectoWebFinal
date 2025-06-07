document.addEventListener('DOMContentLoaded', function () {
    // Definición de constantes para las URLs de la API
    const API_BASE_URL = "http://localhost:5278/api";
    const API_ROLES_URL = `${API_BASE_URL}/roles`;
    const API_PERMISSIONS_URL = `${API_ROLES_URL}/permissions`;
    const API_USERS_URL = `${API_BASE_URL}/users`;

    // Referencias a elementos del DOM
    const rolesTableBody = document.querySelector('#rolesTable tbody');
    const createRoleModalElement = document.getElementById('createRoleModal');
    const createRoleForm = document.getElementById('createRoleForm');
    const editRoleModalElement = document.getElementById('editRoleModal');
    const editRoleForm = document.getElementById('editRoleForm');
    const assignUsersModalElement = document.getElementById('assignUsersModal');
    const assignUsersForm = document.getElementById('assignUsersForm');
    const btnOpenCreateModal = document.getElementById('btnOpenCreateModal');
    const createPermissionsCheckboxesContainer = document.getElementById('createPermissionsCheckboxes');
    const editPermissionsCheckboxesContainer = document.getElementById('editPermissionsCheckboxes');

    // --- Funciones Auxiliares Comunes ---
    function getJwtToken() {
        const token = localStorage.getItem('jwtToken');
        if (!token) {
            showGlobalToast("Error de Autenticación", "Su sesión ha expirado.", false);
            setTimeout(() => window.location.href = '/Home/Login', 2000);
            return null;
        }
        return token;
    }

    function getAuthHeaders() {
        const token = getJwtToken();
        if (!token) return {};
        return { 'Authorization': `Bearer ${token}`, 'Content-Type': 'application/json' };
    }

    function showGlobalToast(header, body, isSuccess = true) {
        const toastElement = document.getElementById('liveToast');
        if (!toastElement) { alert(`${header}: ${body}`); return; }
        const toastHeaderElement = document.getElementById('toastHeader');
        const toastBodyElement = document.getElementById('toastBody');
        toastHeaderElement.textContent = header;
        toastBodyElement.textContent = body;
        const toastHeaderDiv = toastElement.querySelector('.toast-header');
        toastHeaderDiv.classList.remove('bg-danger', 'bg-success', 'text-white');
        toastHeaderDiv.classList.add(isSuccess ? 'bg-success' : 'bg-danger', 'text-white');
        const toast = new bootstrap.Toast(toastElement);
        toast.show();
    }

    function getBadgeClass(isActive) { return isActive ? 'bg-success' : 'bg-danger'; }
    function getStatusText(isActive) { return isActive ? 'Sí' : 'No'; }

    // --- Funciones de Carga de Datos ---
    async function loadPermissionsCheckboxes(containerElement, currentPermissionIds = []) {
        if (!containerElement) return;
        containerElement.innerHTML = '<div class="d-flex justify-content-center"><div class="spinner-border spinner-border-sm text-secondary" role="status"></div><span class="ms-2">Cargando permisos...</span></div>';
        try {
            const response = await fetch(API_PERMISSIONS_URL, { headers: getAuthHeaders() });
            if (!response.ok) throw new Error('No se pudieron cargar los permisos');
            const permissions = await response.json();
            containerElement.innerHTML = '';
            if (permissions.length === 0) {
                containerElement.innerHTML = '<div class="alert alert-info">No hay permisos para asignar.</div>';
                return;
            }
            let html = '<div class="row">';
            permissions.forEach(p => {
                const isChecked = currentPermissionIds.includes(p.permissionId) ? 'checked' : '';
                html += `<div class="col-md-6 col-lg-4 mb-2"><div class="form-check"><input type="checkbox" class="form-check-input" name="permissionIds" value="${p.permissionId}" id="p_${p.permissionId}_${containerElement.id}" ${isChecked} /><label class="form-check-label" for="p_${p.permissionId}_${containerElement.id}">${p.permissionName}</label></div></div>`;
            });
            html += '</div>';
            containerElement.innerHTML = html;
        } catch (error) {
            containerElement.innerHTML = `<div class="alert alert-danger">${error.message}</div>`;
        }
    }

    async function loadRoles() {
        if (!rolesTableBody) return;
        rolesTableBody.innerHTML = '<tr><td colspan="7" class="text-center"><div class="spinner-border text-primary" role="status"></div></td></tr>';
        try {
            const response = await fetch(API_ROLES_URL, { headers: getAuthHeaders() });
            if (!response.ok) throw new Error(`Error HTTP ${response.status}`);
            const roles = await response.json();
            displayRoles(roles);
        } catch (error) {
            rolesTableBody.innerHTML = `<tr><td colspan="7" class="text-center text-danger"><b>Error:</b> ${error.message}</td></tr>`;
        }
    }

    function displayRoles(roles) {
        if (!rolesTableBody) return;
        rolesTableBody.innerHTML = '';
        if (!roles || roles.length === 0) {
            rolesTableBody.innerHTML = '<tr><td colspan="7" class="text-center">No hay roles registrados.</td></tr>';
            return;
        }
        roles.forEach(role => {
            const row = rolesTableBody.insertRow();
            row.innerHTML = `
                <td>${role.roleId}</td>
                <td>${role.roleName}</td>
                <td>${role.description || 'N/A'}</td>
                <td><span class="badge ${getBadgeClass(role.isActive)}">${getStatusText(role.isActive)}</span></td>
                <td>${role.numberOfUsers}</td>
                <td>${role.numberOfPermissions}</td>
                <td class="text-nowrap">
                    <button class="btn btn-sm btn-warning me-1 edit-role-btn" data-id="${role.roleId}" title="Editar Rol"><i class="fas fa-edit"></i></button>
                    <button class="btn btn-sm btn-info me-1 assign-users-btn" data-role-id="${role.roleId}" data-role-name="${role.roleName}" title="Asignar Usuarios"><i class="fas fa-users-cog"></i></button>
                    <form action="/RoleUI/Delete/${role.roleId}" method="post" class="d-inline delete-role-form" onsubmit="return confirm('¿Desactivar el rol \\'${role.roleName}\\'?');">
                        <button type="submit" class="btn btn-sm btn-danger" title="Desactivar Rol"><i class="fas fa-trash"></i></button>
                    </form>
                </td>`;
        });
    }

    // --- Lógica de envío de Formularios ---
    if (createRoleForm) {
        createRoleForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const roleName = document.getElementById('createRoleName').value;
            const description = document.getElementById('createDescription').value;
            const permissionCheckboxes = document.querySelectorAll('#createPermissionsCheckboxes input[name="permissionIds"]:checked');
            const permissionIds = Array.from(permissionCheckboxes).map(cb => parseInt(cb.value));
            if (!roleName) { showGlobalToast("Validación", "El nombre del rol es requerido.", false); return; }
            try {
                const response = await fetch(API_ROLES_URL, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify({ roleName, description, permissionIds }) });
                if (!response.ok) throw new Error((await response.json()).message || 'Error al crear el rol');
                showGlobalToast("Éxito", "Rol creado exitosamente.", true);
                createRoleForm.reset();
                bootstrap.Modal.getInstance(createRoleModalElement)?.hide();
                loadRoles();
            } catch (error) { showGlobalToast("Error de Creación", error.message, false); }
        });
    }

    if (editRoleForm) {
        editRoleForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const roleId = parseInt(document.getElementById('editRoleId').value);
            const roleName = document.getElementById('editRoleName').value;
            const description = document.getElementById('editDescription').value;
            const isActive = document.getElementById('editIsActive').checked;
            const permissionCheckboxes = document.querySelectorAll('#editPermissionsCheckboxes input[name="permissionIds"]:checked');
            const selectedPermissionIds = Array.from(permissionCheckboxes).map(cb => parseInt(cb.value));
            if (!roleName) { showGlobalToast("Validación", "El nombre del rol es requerido.", false); return; }
            try {
                await fetch(`${API_ROLES_URL}/${roleId}`, { method: 'PUT', headers: getAuthHeaders(), body: JSON.stringify({ roleName, description, isActive }) });
                await fetch(`${API_ROLES_URL}/${roleId}/permissions`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify({ roleId: roleId, permissionIds: selectedPermissionIds }) });
                showGlobalToast("Éxito", "Rol y permisos actualizados.", true);
                bootstrap.Modal.getInstance(editRoleModalElement)?.hide();
                loadRoles();
            } catch (error) { showGlobalToast("Error de Actualización", error.message, false); }
        });
    }

    if (assignUsersForm) {
        assignUsersForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const roleId = document.getElementById('assignUsersRoleId').value;
            const checkedUsers = document.querySelectorAll('#assignUsersCheckboxesContainer input[name="userIds"]:checked');
            const userIds = Array.from(checkedUsers).map(cb => parseInt(cb.value));
            try {
                const response = await fetch(`${API_ROLES_URL}/${roleId}/assign-users`, { method: 'POST', headers: getAuthHeaders(), body: JSON.stringify(userIds) });
                if (!response.ok) throw new Error((await response.json()).message || 'Error al guardar.');
                showGlobalToast("Éxito", "Asignaciones actualizadas.", true);
                bootstrap.Modal.getInstance(assignUsersModalElement)?.hide();
                loadRoles();
            } catch (error) { showGlobalToast("Error", error.message, false); }
        });
    }

    // --- Listeners para ABRIR Modales Manualmente ---
    if (btnOpenCreateModal) {
        btnOpenCreateModal.addEventListener('click', () => {
            if (createRoleModalElement) {
                const createModal = new bootstrap.Modal(createRoleModalElement);
                createModal.show();
            }
        });
    }

    if (rolesTableBody) {
        rolesTableBody.addEventListener('click', function (event) {
            const editButton = event.target.closest('.edit-role-btn');
            const assignButton = event.target.closest('.assign-users-btn');

            if (editButton && editRoleModalElement) {
                editRoleModalElement.dataset.roleId = editButton.dataset.id;
                const editModal = new bootstrap.Modal(editRoleModalElement);
                editModal.show();
            }
            if (assignButton && assignUsersModalElement) {
                assignUsersModalElement.dataset.roleId = assignButton.dataset.roleId;
                assignUsersModalElement.dataset.roleName = assignButton.dataset.roleName;
                const assignModal = new bootstrap.Modal(assignUsersModalElement);
                assignModal.show();
            }
        });
    }

    // --- Listeners para cuando los modales se MUESTRAN (para cargar datos) ---
    if (createRoleModalElement) {
        createRoleModalElement.addEventListener('show.bs.modal', () => {
            loadPermissionsCheckboxes(document.getElementById('createPermissionsCheckboxes'), []);
        });
    }

    if (editRoleModalElement) {
        editRoleModalElement.addEventListener('show.bs.modal', async () => {
            const roleId = editRoleModalElement.dataset.roleId;
            if (!roleId) return;
            try {
                const response = await fetch(`${API_ROLES_URL}/${roleId}`, { headers: getAuthHeaders() });
                if (!response.ok) throw new Error('No se pudo cargar la información del rol.');
                const role = await response.json();
                document.getElementById('editRoleId').value = role.roleId;
                document.getElementById('editRoleName').value = role.roleName;
                document.getElementById('editDescription').value = role.description || '';
                document.getElementById('editIsActive').checked = role.isActive;
                const currentPermissionIds = role.permissions ? role.permissions.map(p => p.permissionId) : [];
                await loadPermissionsCheckboxes(document.getElementById('editPermissionsCheckboxes'), currentPermissionIds);
            } catch (error) { showGlobalToast("Error de Carga", error.message, false); }
        });
    }

    if (assignUsersModalElement) {
        assignUsersModalElement.addEventListener('show.bs.modal', async () => {
            const roleId = assignUsersModalElement.dataset.roleId;
            const roleName = assignUsersModalElement.dataset.roleName;
            if (!roleId) return;
            document.getElementById('assignUsersRoleName').textContent = roleName;
            document.getElementById('assignUsersRoleId').value = roleId;
            const container = document.getElementById('assignUsersCheckboxesContainer');
            container.innerHTML = '<div class="alert alert-info">Cargando usuarios...</div>';
            document.getElementById('userSearchInput').value = '';
            try {
                const [allUsersResponse, assignedUsersResponse] = await Promise.all([
                    fetch(API_USERS_URL, { headers: getAuthHeaders() }),
                    fetch(`${API_ROLES_URL}/${roleId}/users`, { headers: getAuthHeaders() })
                ]);
                if (!allUsersResponse.ok || !assignedUsersResponse.ok) throw new Error('No se pudo cargar la información.');
                const allUsers = await allUsersResponse.json();
                const assignedUserIds = await assignedUsersResponse.json();
                container.innerHTML = '';
                if (allUsers.length === 0) { container.innerHTML = '<div class="alert alert-warning">No hay usuarios registrados.</div>'; return; }
                let html = '<div class="list-group">';
                allUsers.forEach(user => {
                    const isChecked = assignedUserIds.includes(user.userId) ? 'checked' : '';
                    html += `<label class="list-group-item"><input class="form-check-input me-2" type="checkbox" name="userIds" value="${user.userId}" ${isChecked}>${user.username} <small class="text-muted">(${user.email})</small></label>`;
                });
                html += '</div>';
                container.innerHTML = html;
            } catch (error) { container.innerHTML = `<div class="alert alert-danger">Error: ${error.message}</div>`; }
        });

        document.getElementById('userSearchInput').addEventListener('keyup', function () {
            const filter = this.value.toLowerCase();
            const labels = document.querySelectorAll('#assignUsersCheckboxesContainer .list-group-item');
            labels.forEach(label => { label.style.display = label.textContent.toLowerCase().includes(filter) ? '' : 'none'; });
        });
    }

    // --- Inicialización ---
    const path = window.location.pathname;
    if (rolesTableBody && (path.endsWith('/RoleUI') || path.endsWith('/RoleUI/') || path.endsWith('/RoleUI/Index'))) {
        loadRoles();
    }
});
