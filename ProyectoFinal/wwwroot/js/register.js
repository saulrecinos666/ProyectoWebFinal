document.addEventListener('DOMContentLoaded', function () {
    // Definición de constantes para las URLs de la API
    const isLocalhost = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";
    const API_BASE_URL = isLocalhost
        ? "http://localhost:5278/api"
        : "https://b4ndxm8brf.us-east-2.awsapprunner.com/api";

    const API_REGISTER_URL = `${API_BASE_URL}/auth/register`;
    const API_LOGIN_URL = `${API_BASE_URL}/auth/login`;

    const registerForm = document.getElementById('registerForm');
    if (!registerForm) return; // Si no estamos en la página de registro, no hacer nada.

    // Función para mostrar Toast de Bootstrap
    function showToast(header, body, isSuccess = true) {
        const toastElement = document.getElementById('liveToast');
        if (!toastElement) {
            alert(`${header}: ${body}`);
            return;
        }
        const toastHeader = document.getElementById('toastHeader');
        const toastBody = document.getElementById('toastBody');
        const toastHeaderDiv = toastElement.querySelector('.toast-header');

        toastHeader.textContent = header;
        toastBody.textContent = body;

        toastHeaderDiv.classList.remove('bg-danger', 'bg-success', 'text-white');
        if (isSuccess) {
            toastHeaderDiv.classList.add('bg-success', 'text-white');
        } else {
            toastHeaderDiv.classList.add('bg-danger', 'text-white');
        }

        const toast = new bootstrap.Toast(toastElement);
        toast.show();
    }

    registerForm.addEventListener('submit', async (e) => {
        e.preventDefault(); // Previene el envío tradicional del formulario

        const username = document.getElementById('username').value;
        const email = document.getElementById('email').value;
        const password = document.getElementById('password').value;
        const confirmPassword = document.getElementById('confirmPassword').value;

        if (password !== confirmPassword) {
            showToast("Error de Registro", "Las contraseñas no coinciden.", false);
            return;
        }

        const registerData = {
            username: username,
            email: email,
            password: password
        };

        try {
            const response = await fetch(API_REGISTER_URL, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(registerData)
            });

            if (response.ok) {
                showToast("Registro Exitoso", "Usuario registrado. Iniciando sesión...", true);

                // Intenta iniciar sesión automáticamente
                const loginResponse = await fetch(API_LOGIN_URL, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({ username: username, password: password })
                });

                if (loginResponse.ok) {
                    const loginData = await loginResponse.json();
                    localStorage.setItem('jwtToken', loginData.token);
                    showToast("Inicio de Sesión Exitoso", "Redirigiendo...", true);
                    // Redirigir a la página principal del chat
                    setTimeout(() => { window.location.href = '/Chat'; }, 1500);
                } else {
                    const loginErrorData = await loginResponse.json();
                    showToast("Error", loginErrorData.message || "No se pudo iniciar sesión. Inténtelo manualmente.", false);
                    setTimeout(() => { window.location.href = '/Home/Login'; }, 2000);
                }
            } else {
                const errorData = await response.json();
                showToast("Error de Registro", errorData.message || "Ocurrió un error durante el registro.", false);
            }
        } catch (error) {
            console.error('Error en la solicitud de registro:', error);
            showToast("Error de Red", "No se pudo conectar al servidor.", false);
        }
    });
});
