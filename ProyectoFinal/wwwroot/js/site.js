// --- LÓGICA DE CIERRE DE SESIÓN ---
// Este código se debe añadir al final de tu archivo site.js

// Espera a que todo el DOM esté cargado para buscar el botón
document.addEventListener('DOMContentLoaded', function () {
    const logoutButton = document.getElementById('logoutButton');

    // Si el botón de logout existe en la página...
    if (logoutButton) {
        logoutButton.addEventListener('click', async function (event) {
            event.preventDefault(); // Prevenir cualquier comportamiento por defecto

            console.log("Cerrando sesión...");

            // Obtener el token para enviarlo en la cabecera, por si la API lo necesita para identificar al usuario
            const token = localStorage.getItem('jwtToken');

            try {
                // 1. Llama a tu API de logout en segundo plano
                await fetch('/api/auth/logout', {
                    method: 'POST',
                    headers: {
                        'Authorization': `Bearer ${token}`,
                        'Content-Type': 'application/json'
                    }
                });
            } catch (error) {
                // Aunque la llamada falle (ej. por red), procedemos a limpiar el cliente
                console.error("Error al llamar a la API de logout, se limpiará el cliente de todas formas.", error);
            } finally {
                // 2. Limpia el almacenamiento local del navegador
                console.log("Borrando token del localStorage.");
                localStorage.removeItem('jwtToken');

                // Opcional: Borra otros datos que guardes
                // localStorage.clear();
                // sessionStorage.clear();

                // 3. Redirige al usuario a la página de Login
                console.log("Redirigiendo a /Home/Login");
                window.location.href = '/Home/Login';
            }
        });
    }
});
