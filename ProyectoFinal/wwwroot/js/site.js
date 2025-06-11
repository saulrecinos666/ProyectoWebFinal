// Espera a que todo el DOM esté cargado para buscar los elementos
document.addEventListener('DOMContentLoaded', function () {

    // ===================================================
    // --- CÓDIGO PARA EL WIDGET DE CHAT ---
    // ===================================================
    const chatContainer = document.getElementById('chat-widget-container');
    const toggleButton = document.getElementById('toggleChatBtn');
    const chatIcon = toggleButton ? toggleButton.querySelector('i') : null;

    // Si el botón para abrir/cerrar el chat existe en la página...
    if (toggleButton && chatContainer) {
        // ...le añadimos un "escuchador" de eventos de clic.
        toggleButton.addEventListener('click', function () {
            // Alterna la clase 'chat-closed' en el contenedor principal.
            chatContainer.classList.toggle('chat-closed');

            // Opcional: Cambia el ícono de la flecha arriba/abajo para mejorar la UX
            if (chatContainer.classList.contains('chat-closed')) {
                chatIcon.classList.remove('fa-chevron-down');
                chatIcon.classList.add('fa-chevron-up');
            } else {
                chatIcon.classList.remove('fa-chevron-up');
                chatIcon.classList.add('fa-chevron-down');
            }
        });
    }


    // ===================================================
    // --- LÓGICA DE CIERRE DE SESIÓN ---
    // ===================================================
    const logoutButton = document.getElementById('logoutButton');

    // Si el botón de logout existe en la página...
    if (logoutButton) {
        logoutButton.addEventListener('click', async function (event) {
            event.preventDefault(); // Prevenir cualquier comportamiento por defecto

            console.log("Cerrando sesión...");

            // Obtener el token para enviarlo en la cabecera
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
                // Aunque la llamada falle, procedemos a limpiar el cliente
                console.error("Error al llamar a la API de logout, se limpiará el cliente de todas formas.", error);
            } finally {
                // 2. Limpia el almacenamiento local del navegador
                console.log("Borrando token del localStorage.");
                localStorage.removeItem('jwtToken');

                // 3. Redirige al usuario a la página de Login
                console.log("Redirigiendo a /Home/Login");
                window.location.href = '/Home/Login';
            }
        });
    }

}); // Fin del addEventListener 'DOMContentLoaded'