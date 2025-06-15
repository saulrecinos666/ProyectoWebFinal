document.addEventListener('DOMContentLoaded', function () {
    const loginForm = document.getElementById('loginForm');
    const usernameInput = document.getElementById('username');
    const passwordInput = document.getElementById('password');
    const loginMessage = document.getElementById('loginMessage');
    const rememberMeInput = document.getElementById('rememberMe'); 

    loginForm.addEventListener('submit', async function (event) {
        event.preventDefault();

        loginMessage.textContent = '';
        loginMessage.classList.remove('text-success', 'text-danger');

        const username = usernameInput.value.trim();
        const password = passwordInput.value.trim();
        const rememberMe = rememberMeInput.checked;

        if (!username || !password) {
            loginMessage.textContent = 'Por favor, ingresa usuario y contraseña.';
            loginMessage.classList.add('text-danger');
            return;
        }

        try {
            const response = await fetch('/api/auth/login', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ username: username, password: password, rememberMe: rememberMe })
            });

            const data = await response.json();

            if (response.ok) {
                localStorage.setItem('jwtToken', data.token);
                loginMessage.textContent = data.Message || 'Inicio de sesión exitoso. Redirigiendo...';
                loginMessage.classList.add('text-success');
                window.location.href = '/Chat';
            } else {
                loginMessage.textContent = data.Message || 'Error al iniciar sesión. Credenciales incorrectas.';
                loginMessage.classList.add('text-danger');
            }
        } catch (error) {
            console.error('Error de red o del servidor:', error);
            loginMessage.textContent = 'Ocurrió un error al intentar iniciar sesión. Intenta de nuevo.';
            loginMessage.classList.add('text-danger');
        }
    });
});